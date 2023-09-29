using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ArmorModule : MonoBehaviour
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

    public float maxArmorPoint;
    public float armorPoint;
    public bool isArmorPointInvulnerable = false;
    public bool isLauncherInvulnerable = false;
    public bool isSensorInvulnerable = false;
    public bool isAirstripInvulnerable = false;

    void Update()
    {

    }

    public void DoDamage(float amount)
    {
        if(isArmorPointInvulnerable == false)
            armorPoint -= amount;

        if(isLauncherInvulnerable == false)
            if (self.launcherCtrl != null)
                self.launcherCtrl.OnDamage(amount);
        if(isSensorInvulnerable == false)
            if (self.sensorCtrl != null)
                self.sensorCtrl.OnDamage(amount);
        if(isAirstripInvulnerable == false)
            if (self.airstripCtrl != null)
                self.airstripCtrl.OnDamage(amount);

        if (armorPoint <= 0f)
        {
            armorPoint = 0f;
            Debug.Log(self.typeName + " is destroyed.");
            
            // Make sure all its subsystems are disabled.
            if(self.launcherCtrl != null) self.launcherCtrl.OnDamage(maxArmorPoint);
            if (self.sensorCtrl != null) self.sensorCtrl.OnDamage(maxArmorPoint);
            if (self.airstripCtrl != null) self.airstripCtrl.OnDamage(maxArmorPoint);
        }
    }
}
