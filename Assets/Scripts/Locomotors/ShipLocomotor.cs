using UnityEngine;
using System.Collections;

public class ShipLocomotor : Locomotor
{
    public override void Steer(float deltaTime)
    {
        base.Steer(deltaTime);
        if (self.isDead == false)
            self.position.y = 0f;
    }

    public override void OnFuelExhausted()
    {
        orderedSpeed = 0f; // Slow-down to be floating on water.
    }

    public override void OnDead()
    {
        orderedSpeed = 3f;
        orderedPitch = -90f;
    }
}
