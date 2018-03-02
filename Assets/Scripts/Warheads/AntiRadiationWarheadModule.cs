using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AntiRadiationWarheadModule : WarheadModule
{
    public float antiSensorBonus = 4.0f;

    public override void Explode()
    {
        if (explosionFX != null)
        {
            if (Vector3.Distance(SceneManager.instance.GetComponent<VehicleSelector>().vehicleCamera.transform.position, self.transform.position) < explosionFXVisibleRange)
            {
                GameObject explosionFXObj = (GameObject)GameObject.Instantiate(explosionFX);
                explosionFXObj.transform.parent = self.transform;
                explosionFXObj.transform.localPosition = Vector3.zero;
                explosionFXObj.transform.parent = SceneManager.instance.transform;
                explosionFXObj.GetComponent<ParticleSystem>().Play(true);
            }
        }

        if (target != null)
        {
            if (target.target != null)
            {
                float actualDistance = Vector3.Distance(target.target.position, this.transform.position);
                float damage = damageAmount * damageCurve.Evaluate(actualDistance / damageRadius);
                ArmorModule armor = target.target.GetComponent<ArmorModule>();

                if (self.isDead == false)
                    Debug.Log(self.typeName + " does " + Mathf.RoundToInt(damage).ToString() + " damage to " + target.vehicleTypeName);

                armor.DoDamage(damage);

                // This will generate specific damage for sensors that this weapon is countering.
                if (armor.isSensorInvulnerable == false)
                {
                    SensorController sensorCtrl = target.target.GetComponent<SensorController>();
                    if (sensorCtrl != null)
                    {
                        foreach (Sensor sensor in sensorCtrl.sensors)
                        {
                            if (sensor.enabled == false) continue;

                            if (self.sensorCtrl.sensors.Any((Sensor s) => s.sensorType == sensor.counterSensorType))
                            {
                                // The sensor on the target vehicle is countered by self sensor.
                                float percent = damage / armor.maxArmorPoint;
                                if (UnityEngine.Random.Range(0f, 1.0f) < percent * antiSensorBonus)
                                {
                                    sensor.OnDamage();
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            foreach (Vehicle v in SceneManager.instance.vehicles)
            {
                if (v != self && Vector3.Distance(v.position, this.transform.position) < proximityRange)
                {
                    float actualDistance = Vector3.Distance(v.position, this.transform.position);
                    float damage = damageAmount * damageCurve.Evaluate(actualDistance / damageRadius);
                    ArmorModule armor = v.GetComponent<ArmorModule>();

                    if (self.isDead == false)
                        Debug.Log(self.typeName + " does " + Mathf.RoundToInt(damage).ToString() + " damage to " + v.typeName);

                    armor.DoDamage(damage);
                }
            }
        }

        Shutdown();
    }
}
