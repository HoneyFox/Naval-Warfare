/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 1/5/2012
 * Time: 8:46 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
//using System.Windows.Forms;
using SharpLua.LuaTypes;

namespace SharpLua
{
    /// <summary>
    /// Converts the object to the Lua equivalent (Credit to Arjen Douma)
    /// </summary>
    public class ObjectToLua
    {

        private static void SetPropertyValue(object obj, object value, PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.FullName == "System.Int32")
            {
                propertyInfo.SetValue(obj, (int)(double)value, null);
            }
            else if (propertyInfo.PropertyType.IsEnum)
            {
                object enumValue = Enum.Parse(propertyInfo.PropertyType, (string)value);
                propertyInfo.SetValue(obj, enumValue, null);
            }
            //else if (propertyInfo.PropertyType.FullName == "System.Drawing.Image")
            //{
            //    LuaTable enviroment = LuaRuntime.GlobalEnvironment.GetValue("_G") as LuaTable;
            //    LuaString workDir = enviroment.GetValue("_WORKDIR") as LuaString;
            //    var image = System.Drawing.Image.FromFile(Path.Combine(workDir.Text, (string)value));
            //    propertyInfo.SetValue(obj, image, null);
            //}
            else
            {
                propertyInfo.SetValue(obj, value, null);
            }
        }

        private static void SetMemberValue(object control, Type type, string member, object value)
        {
            PropertyInfo propertyInfo = type.GetProperty(member);
            FieldInfo fieldInfo = type.GetField(member);
            if (propertyInfo != null)
            {
                SetPropertyValue(control, value, propertyInfo);
            }
            else if (fieldInfo != null)
            {
                if (fieldInfo.FieldType.FullName == "System.Int32")
                {
                    fieldInfo.SetValue(control, (int)(double)value);
                }
                else if (fieldInfo.FieldType.FullName == "System.Single")
                {
                    fieldInfo.SetValue(control, (float)(double)value);
                }
                else if (fieldInfo.FieldType.FullName == "System.Int64")
                {
                    fieldInfo.SetValue(control, (long)(double)value);
                }
                else if (fieldInfo.FieldType.IsEnum)
                {
                    object enumValue = Enum.Parse(propertyInfo.PropertyType, (string)value);
                    fieldInfo.SetValue(control, enumValue);
                }
                else
                {
                    fieldInfo.SetValue(control, value);
                }
            }
            else
            {
                EventInfo eventInfo = type.GetEvent(member);
                if (eventInfo != null)
                {
                    switch (eventInfo.EventHandlerType.FullName)
                    {
                        case "System.EventHandler":
                            eventInfo.AddEventHandler(control, new EventHandler((sender, e) =>
                                                                                {
                                                                                    (value as LuaFunc).Invoke(new LuaValue[] { new LuaUserdata(sender), new LuaUserdata(e) });
                                                                                }));
                            break;
                        //case "System.Windows.Forms.TreeViewEventHandler":
                        //    eventInfo.AddEventHandler(control, new TreeViewEventHandler((sender, e) =>
                        //                                                                {
                        //                                                                    (value as LuaFunc).Invoke(new LuaValue[] { new LuaUserdata(sender), new LuaUserdata(e) });
                        //                                                                }));
                        //    break;
                        default:
                            throw new NotImplementedException(eventInfo.EventHandlerType.FullName + " type not implemented.");
                    }
                }
            }
        }

        private static LuaValue GetMemberValue(object control, Type type, string member)
        {
            PropertyInfo propertyInfo = type.GetProperty(member);
            if (propertyInfo != null)
            {
                object value = propertyInfo.GetValue(control, null);
                return ToLuaValue(value);
            }
            FieldInfo fi = type.GetField(member);
            if (fi != null)
            {
                return ToLuaValue(fi.GetValue(control));
            }
            MethodInfo[] miarr = type.GetMethods();
            MethodInfo mi = null;
            foreach (MethodInfo m in miarr)
                if (m.Name == member)
                    mi = m;
            if (mi != null)
            {
                return new LuaFunction((LuaValue[] args) =>
                {
                    mi = null;
                    List<object> args2 = new List<object>();
                    foreach (LuaValue v in args)
                        args2.Add(v.Value);

                    foreach (MethodInfo m in miarr)
                        if (m.Name == member)
                            if (m.GetGenericArguments().Length == 0)
                            {
                                if (m.GetParameters().Length == args.Length)
                                {
                                    bool isParameterMatched = true;
                                    ParameterInfo[] pis = m.GetParameters();
                                    for (int i = 0; i < pis.Length; ++i)
                                    {
                                        if (args[i] is LuaNumber)
                                        {
                                            if (!(pis[i].ParameterType.Equals(typeof(int))
                                                || pis[i].ParameterType.Equals(typeof(uint))
                                                || pis[i].ParameterType.Equals(typeof(long))
                                                || pis[i].ParameterType.Equals(typeof(ulong))
                                                || pis[i].ParameterType.Equals(typeof(float))
                                                || pis[i].ParameterType.Equals(typeof(double))
                                                )
                                            )
                                            {
                                                isParameterMatched = false; break;
                                            }
                                            else
                                            {
                                                args2[i] = Convert.ChangeType(args2[i], pis[i].ParameterType);
                                            }
                                        }
                                        else if (args[i] is LuaBoolean)
                                        {
                                            if (!(pis[i].ParameterType.Equals(typeof(bool))))
                                            { isParameterMatched = false; break; }
                                        }
                                        else if (args[i] is LuaString)
                                        {
                                            if (!(pis[i].ParameterType.Equals(typeof(string))))
                                            { isParameterMatched = false; break; }
                                        }
                                        else if (args[i] is LuaUserdata)
                                        {
                                            if (!(pis[i].ParameterType.Equals(args2[i].GetType())))
                                            { isParameterMatched = false; break; }
                                        }
                                    }

                                    if (isParameterMatched)
                                    {
                                        mi = m;
                                        break;
                                    }
                                    else
                                        continue;
                                }
                            }

                    object result = null;
                    if (mi != null)
                        result = mi.Invoke(control, args2.ToArray());
                    return ToLuaValue(result);
                });
            }

            throw new Exception(string.Format("Cannot get {0} from {1}", member, control));
        }

        private static LuaValue GetIndexerValue(object control, Type type, double index)
        {
            MemberInfo[] members = type.GetMember("Item");

            if (members.Length == 0)
            {
                throw new InvalidOperationException(string.Format("Indexer is not defined in {0}", type.FullName));
            }

            foreach (MemberInfo memberInfo in members)
            {
                PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo != null)
                {
                    try
                    {
                        object result = propertyInfo.GetValue(control, new object[] { (int)index - 1 });
                        return ToLuaValue(result);
                    }
                    catch (TargetParameterCountException)
                    {
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (MethodAccessException)
                    {
                    }
                    catch (TargetInvocationException)
                    {
                    }
                }
            }

            return LuaNil.Nil;
        }

        public static LuaValue ToLuaValue(object value)
        {
            if ((value as LuaValue) != null)
                return value as LuaValue;
            if (value is int || value is long || value is double || value is float)
            {
                return new LuaNumber(Convert.ToDouble(value));
            }
            else if (value is string)
            {
                return new LuaString((string)value);
            }
            else if (value is bool)
            {
                return LuaBoolean.From((bool)value);
            }
            else if (value == null)
            {
                return LuaNil.Nil;
            }
            else if (value.GetType().IsEnum)
            {
                return new LuaString(value.ToString());
            }
            else if (value.GetType().IsArray)
            {
                LuaTable table = new LuaTable();
                foreach (var item in (value as Array))
                {
                    table.AddValue(ToLuaValue(item));
                }
                return table;
            }
            else if (value is LuaValue)
            {
                return (LuaValue)value;
            }
            else
            {
                LuaUserdata data = new LuaUserdata(value);
                data.MetaTable = GetControlMetaTable();
                return data;
            }
        }

        public static LuaValue StaticClassToLuaTable(Type type)
        {
            LuaUserdata data = new LuaUserdata(null);
            data.MetaTable = GetControlMetaTable(true, type);
            data.MetaTable.SetNameValue("staticClassType", ToLuaValue(type));
            return data;
        }

        static Dictionary<Type, LuaTable> controlMetaTables = new Dictionary<Type, LuaTable>();
        static LuaTable instanceControlMetaTable;
        public static LuaTable GetControlMetaTable(bool isStatic = false, Type classType = null)
        {
            LuaTable controlMetaTable = null;
            if (isStatic == false && instanceControlMetaTable == null || isStatic == true && controlMetaTables.ContainsKey(classType) == false)
            {
                controlMetaTable = new LuaTable();

                controlMetaTable.SetNameValue("__index", new LuaFunction((values) =>
                {
                    bool isStaticClass = false;
                    LuaUserdata control = values[0] as LuaUserdata;
                    Type type;
                    if (control.MetaTable.ContainsKey(new LuaString("staticClassType")))
                    {
                        isStaticClass = true;
                        type = values[0].MetaTable.GetValue("staticClassType").Value as Type;
                    }
                    else
                        type = control.Value.GetType();

                    LuaString member = values[1] as LuaString;
                    if (member != null)
                    {
                        return GetMemberValue((isStaticClass == false ? control.Value : null), type, member.Text);
                    }

                    LuaNumber index = values[1] as LuaNumber;
                    if (index != null)
                    {
                        return GetIndexerValue((isStaticClass == false ? control.Value : null), type, index.Number);
                    }

                    return LuaNil.Nil;
                }));

                controlMetaTable.SetNameValue("__newindex", new LuaFunction((values) =>
                {
                    bool isStaticClass = false;
                    LuaUserdata control = values[0] as LuaUserdata;
                    Type type;
                    if (control.MetaTable.ContainsKey(new LuaString("staticClassType")))
                    {
                        isStaticClass = true;
                        type = values[0].MetaTable.GetValue("staticClassType").Value as Type;
                    }
                    else
                        type = control.Value.GetType();

                    LuaString member = values[1] as LuaString;
                    LuaValue value = values[2];

                    SetMemberValue((isStaticClass == false ? control.Value : null), type, member.Text, value.Value);
                    return null;
                }));

                if (isStatic == false)
                    instanceControlMetaTable = controlMetaTable;
                else
                    controlMetaTables[classType] = controlMetaTable;
            }

            if (isStatic == false)
                return instanceControlMetaTable;
            else
                return controlMetaTables[classType];
        }

        public static LuaTable ToLuaTable(object o)
        {
            LuaTable ret = new LuaTable();

            // check if Dictionary...
            System.Collections.IDictionary dict = o as System.Collections.IDictionary;
            if (dict != null)
            {
                foreach (object obj in dict.Keys)
                {
                    ret.SetKeyValue(ToLuaValue(obj), ToLuaValue(dict[obj]));
                }
                return ret;
            }

            System.Collections.IEnumerable ie = (o as System.Collections.IEnumerable);
            if (ie != null)
            {
                foreach (object obj in ie)
                {
                    ret.AddValue(ToLuaValue(obj));
                }
                return ret;
            }

            // check if <type>...
            // not an array type
            ret.AddValue(ToLuaValue(o));
            return ret;
        }
    }
}
