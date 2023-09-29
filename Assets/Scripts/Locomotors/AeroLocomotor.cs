﻿using UnityEngine;
using System.Collections;

public class AeroLocomotor : Locomotor
{
    public override void Steer(float deltaTime)
    {
        base.Steer(deltaTime);
        if ((self.position.y < 0 && self.velocity.y < 0) || (self.position.y < 0 && self.fuel <= 0))
        {
            if (self.warheadModule != null)
            {
                if(Time.time - self.warheadModule.timeOfLaunch > self.warheadModule.safetyTimer)
                    self.warheadModule.Ignite();
            }
            else
            {
                SceneManager.instance.vehicleSelector?.OnVehicleDead(self);
                SceneManager.instance.vehicles.Remove(self);
                GameObject.Destroy(self.gameObject);
            }
        }
    }

    public override void OnFuelExhausted()
    {
        orderedSpeed = 60f;
        orderedPitch = -90f;
        // Slow-down and dive to ground.
    }

    public override void OnDead()
    {
        if (self.warheadModule != null)
        {
            self.warheadModule.Explode();
        }
        else
        {
            self.fuel = 0f;
            OnFuelExhausted();
        }
    }
}
