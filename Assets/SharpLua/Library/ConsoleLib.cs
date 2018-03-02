/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/21/2011
 * Time: 9:15 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    /// <summary>
    /// Description of ConsoleLib.
    /// </summary>
    public class ConsoleLib
    {
        public static void RegisterModule(LuaTable mod)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            mod.SetNameValue("console", module);
        }
        
        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("write", Write);
            module.Register("writeline", WriteLine);
            module.Register("clear", new LuaFunc(delegate (LuaValue[] args) { 
#if UNITY_EDITOR
                                                     UnityEngine.Debug.ClearDeveloperConsole(); 
#endif
                                                     return LuaNil.Nil; 
                                                 }));
        }
        
        public static LuaValue Write(LuaValue[] args)
        {
            string[] result = new string[args.Length];
            for (int i = 0; i < args.Length; ++i)
                result[i] = args[i].ToString();
            Debug.Log(string.Join("    ", result));
            return LuaNil.Nil;
        }
        
        public static LuaValue WriteLine(LuaValue[] args)
        {
            return Write(args);
        }
    }
}
