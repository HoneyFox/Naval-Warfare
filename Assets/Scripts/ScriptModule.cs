using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SharpLua;
using SharpLua.LuaTypes;

public class ScriptModule : MonoBehaviour
{
    private Vehicle self
    {
        get { return this.gameObject.GetComponent<Vehicle>(); }
    }

    [HideInInspector]
    public string initCodes = "";
    [HideInInspector]
    public string fixedUpdateCodes = "";
    
    private LuaTable t;
    
    void Start()
    {
        t = LuaRuntime.CreateGlobalEnviroment();
        t.SetNameValue("selfVehicle", new LuaUserdata(self, true));
        t.SetNameValue("sceneManager", new LuaUserdata(SceneManager.instance, true));
        if (initCodes != "")
        {
            LuaRuntime.Run(initCodes, t);
        }
    }

    void FixedUpdate()
    {
        if (fixedUpdateCodes != "")
        {
            t.SetNameValue("deltaTime", new LuaNumber(Time.fixedDeltaTime));
            LuaRuntime.Run(fixedUpdateCodes, t);
        }
    }
}
