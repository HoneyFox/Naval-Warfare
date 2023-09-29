using UnityEngine;
using System.Collections;

public class ShellLocomotor : Locomotor 
{
    public float CEP = 100;

    private LineRenderer cachedTrail = null;
    private bool hasTrail = true;
    private bool hasGeneratedRandomError = false;

    public override void Steer(float deltaTime)
    {
        base.Steer(deltaTime);

        if (cachedTrail == null && hasTrail == true)
        {
            cachedTrail = self.GetComponentInChildren<LineRenderer>();
            hasTrail = (cachedTrail != null);
        }
        if (self.fuel <= 0f)
        {
            if (hasTrail)
            {
                cachedTrail.enabled = true;
            }
        }
        
        if (self.position.y < 0)
            OnDead();
    }

    public override void OnFuelExhausted()
    {
        orderedSpeed = 120f;

        if (hasGeneratedRandomError == false)
        {
            if (self.GetComponent<GuidanceModule>() != null)
            {
                Vector3 origTargetPos = (Vector3)(self.GetComponent<GuidanceModule>().guidanceParameter);
                float error = CEP * Vector3.Distance(origTargetPos, self.position) / VehicleDatabase.sVehicleRanges[self.typeName] * 1.2f;
                self.GetComponent<GuidanceModule>().guidanceParameter = origTargetPos + new Vector3(UnityEngine.Random.Range(-error, error), 0.0f, UnityEngine.Random.Range(-error, error));
                hasGeneratedRandomError = true;
            }
        }

        turnRadius = 500f;
        maxTurnAcceleration = 30f;
    }

    public override void OnDead()
    {
        if (self.GetComponent<WarheadModule>() != null)
        {
            self.GetComponent<WarheadModule>().Shutdown();
        }
    }
}
