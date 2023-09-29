using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SharpLua;
using SharpLua.LuaTypes;
using SharpLua.AST;

public class ScriptModule : MonoBehaviour
{
    private Vehicle _cachedSelf;
    private Vehicle self
    {
        get
        {
            if (_cachedSelf == null) _cachedSelf = this.gameObject.GetComponent<Vehicle>();
            return _cachedSelf;
        }
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

    private float lastFixedDeltaTime = -1;
    private string lastFixedUpdateCodes = "";
    private Chunk codeChunk = null;
    void FixedUpdate()
    {
        if (fixedUpdateCodes != "")
        {
            if (Time.fixedDeltaTime != lastFixedDeltaTime)
            {
                lastFixedDeltaTime = Time.fixedDeltaTime;
                var dtLuaNumber = new LuaNumber(Time.fixedDeltaTime);
                t.SetNameValue("deltaTime", dtLuaNumber);
            }
            if (lastFixedUpdateCodes != fixedUpdateCodes)
            {
                lastFixedUpdateCodes += fixedUpdateCodes;
                codeChunk = LuaRuntime.Parse(fixedUpdateCodes);
            }
            codeChunk.Enviroment = t;
            codeChunk.Execute();
        }
    }
}
