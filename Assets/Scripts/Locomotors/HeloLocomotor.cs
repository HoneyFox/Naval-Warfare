using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class HeloLocomotor : AeroLocomotor
{
    public float kP = 0.02f;
    public float kD = 0.2f;
    public float maxPitch = 30f;
    public float kC = 0.8f;

    float lastSpeed = 0f;

    public override void Steer(float deltaTime)
    {
        base.Steer(deltaTime);

        if (self.enabled)
        {
            float acc = (self.speed - lastSpeed) / deltaTime;
            Transform model = self.transform.FindChild("Model");
            if (model != null)
                model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.Euler(Mathf.Clamp(kP * self.speed + kD * acc, -maxPitch, maxPitch) + self.pitch * kC, 0f, 0f), 0.08f);
            Transform sensor = self.transform.FindChild("Sensors");
            if (sensor != null)
                sensor.localRotation = Quaternion.Slerp(sensor.localRotation, Quaternion.Euler(Mathf.Clamp(kP * self.speed + kD * acc, -maxPitch, maxPitch) + self.pitch * kC, 0f, 0f), 0.08f);
            Transform launcher = self.transform.FindChild("Launchers");
            if (launcher != null)
                launcher.localRotation = Quaternion.Slerp(launcher.localRotation, Quaternion.Euler(Mathf.Clamp(kP * self.speed + kD * acc, -maxPitch, maxPitch) + self.pitch * kC, 0f, 0f), 0.08f);
            lastSpeed = self.speed;
        }
        else
        {
            Transform model = self.transform.FindChild("Model");
            if (model != null)
                model.localRotation = Quaternion.Slerp(model.localRotation, Quaternion.Euler(0f, 0f, 0f), 0.08f);
            Transform sensor = self.transform.FindChild("Sensors");
            if (sensor != null)
                sensor.localRotation = Quaternion.Slerp(sensor.localRotation, Quaternion.Euler(0f, 0f, 0f), 0.08f);
            Transform launcher = self.transform.FindChild("Launchers");
            if (launcher != null)
                launcher.localRotation = Quaternion.Slerp(launcher.localRotation, Quaternion.Euler(0f, 0f, 0f), 0.08f);
        }
    }
}
