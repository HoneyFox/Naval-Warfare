using UnityEngine;
using System.Collections;

public class SubLocomotor : Locomotor
{
    public override void Steer(float deltaTime)
    {
        if (self.position.y < 0)
        {
            base.Steer(deltaTime);
        }
        else
        {
            if (orderedSpeed > self.speed)
                self.speed = Mathf.MoveTowards(self.speed, Mathf.Clamp(orderedSpeed, self.minSpeed, self.maxSpeed), linearAcceleration * deltaTime);
            else
                self.speed = Mathf.MoveTowards(self.speed, Mathf.Clamp(orderedSpeed, self.minSpeed, self.maxSpeed), dragAcceleration * deltaTime);
            
            self.pitch = Mathf.MoveTowardsAngle(self.pitch, Mathf.Clamp(orderedPitch, -90f, 90f), Mathf.Rad2Deg * Mathf.Min(self.speed / turnRadius, maxTurnAcceleration / self.speed) * deltaTime);
            self.pitch = Mathf.Clamp(self.pitch, -90f, 90f);

            self.position += self.velocity * deltaTime;
            self.fuel -= self.fuelConsumptionRate * self.fuelConsumptionCurve.Evaluate(Mathf.InverseLerp(self.minSpeed, self.maxSpeed, self.speed)) * deltaTime;
            if (self.fuel <= 0)
            {
                self.fuel = 0f;
                OnFuelExhausted();
            }
            if (self.isDead)
            {
                OnDead();
            }
        }

        if (self.fuel <= 0)
        {
            if (self.GetComponent<WarheadModule>() != null)
            {
                self.GetComponent<WarheadModule>().Ignite();
            }
        }
    }

    public override void OnFuelExhausted()
    {
        orderedSpeed = 4f;
        orderedPitch = -90f;
        // Slow-down and dive to sea bed.
    }

    public override void OnDead()
    {
        if (self.GetComponent<WarheadModule>() != null)
        {
            self.GetComponent<WarheadModule>().Explode();
        }
        else
        {
            self.fuel = 0f;
            OnFuelExhausted();
        }
    }
}
