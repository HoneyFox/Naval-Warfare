using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AircraftLocomotor : AeroLocomotor
{
    public float stdSpd = 80f;
    public float stdAoA = 12f;
    public float maxAoA = 20f;

    float lastSpeed = 0f;

    public override void Steer(float deltaTime)
    {
        base.Steer(deltaTime);

        if (self.enabled)
        {
            float aoa = Mathf.Clamp((stdSpd * stdSpd / self.speed / self.speed) * stdAoA, 0f, maxAoA);
            Transform model = self.transform.Find("Model");
            if (model != null)
                model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.Euler(-aoa, 0f, 0f), 0.08f);
            Transform sensor = self.transform.Find("Sensors");
            if (sensor != null)
                sensor.localRotation = Quaternion.Slerp(sensor.localRotation, Quaternion.Euler(-aoa, 0f, 0f), 0.08f);
            Transform launcher = self.transform.Find("Launchers");
            if (launcher != null)
                launcher.localRotation = Quaternion.Slerp(launcher.localRotation, Quaternion.Euler(-aoa, 0f, 0f), 0.08f);
        }
        else
        {
            Transform model = self.transform.Find("Model");
            if (model != null)
                model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.Euler(0f, 0f, 0f), 0.08f);
            Transform sensor = self.transform.Find("Sensors");
            if (sensor != null)
                sensor.localRotation = Quaternion.Slerp(sensor.localRotation, Quaternion.Euler(0f, 0f, 0f), 0.08f);
            Transform launcher = self.transform.Find("Launchers");
            if (launcher != null)
                launcher.localRotation = Quaternion.Slerp(launcher.localRotation, Quaternion.Euler(0f, 0f, 0f), 0.08f);
        }
    }
}
