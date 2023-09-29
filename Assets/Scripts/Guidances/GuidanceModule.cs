﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GuidanceModule : MonoBehaviour
{
    public Track targetTrack;
    private Vehicle _cachedSelf;
    public Vehicle self
    {
        get
        {
            if (_cachedSelf == null) _cachedSelf = this.gameObject.GetComponent<Vehicle>();
            return _cachedSelf;
        }
    }
    public bool requiresFireControl = false;
    public bool requiresLockBeforeFiring = true;

    public object guidanceParameter = null;

    public bool canBeRemotelyControlled = false;
    
    public virtual void SetupGuidance(Track target, object guidanceParam)
    {
        this.targetTrack = target;
        this.guidanceParameter = guidanceParam;
    }

    public void FixedUpdate()
    {
        OnFixedUpdate(Time.fixedDeltaTime);
    }

    public virtual void OnFixedUpdate(float deltaTime)
    {

    }

    public virtual void OnGuidanceControllerGUI()
    {

    }
}
