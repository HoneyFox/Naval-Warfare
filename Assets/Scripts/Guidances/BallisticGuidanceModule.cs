using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BallisticGuidanceModule : GuidanceModule
{
    public float loftCoefficient = 1f;
    public float altitudeThreshold = 100f;
    public float distanceThreshold = 400f;
    public float CEP = 10f;

    private float lastDistance = -1f;
    private int farAwayCounter = 0;

    private bool hasGeneratedRandomError = false;

    public override void SetupGuidance(Track target, object guidanceParam)
    {
        base.SetupGuidance(target, guidanceParam);
        lastDistance = -1f;
        farAwayCounter = UnityEngine.Random.Range(2, 8);
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        Vector3 targetPosition = (Vector3)guidanceParameter;

        if(hasGeneratedRandomError == false)
        {
            float error = CEP * Vector3.Distance(targetPosition, self.position) / VehicleDatabase.sVehicleRanges[self.typeName] * 1.2f;
            guidanceParameter = targetPosition + new Vector3(UnityEngine.Random.Range(-error, error), 0.0f, UnityEngine.Random.Range(-error, error));
            hasGeneratedRandomError = true;
        }

        if (lastDistance == -1f)
            lastDistance = Vector3.Distance(targetPosition, self.position);

        float distance = Vector3.Distance(targetPosition, self.position);
        float horizontalDistance = Mathf.Sqrt((targetPosition.x - self.position.x) * (targetPosition.x - self.position.x) + (targetPosition.z - self.position.z) * (targetPosition.z - self.position.z));
        targetPosition.y += loftCoefficient * horizontalDistance * horizontalDistance / VehicleDatabase.sVehicleRanges[self.typeName];

        // Pitch Guidance.
        float expectedPitch = Mathf.Rad2Deg * Mathf.Atan2(targetPosition.y - self.position.y, Mathf.Sqrt(Mathf.Pow(targetPosition.z - self.position.z, 2) + Mathf.Pow(targetPosition.x - self.position.x, 2)));
        self.locomotor.orderedPitch = expectedPitch;

        // Yaw Guidance.
        float expectedCourse = Mathf.Rad2Deg * Mathf.Atan2(targetPosition.x - self.position.x, targetPosition.z - self.position.z);
        self.locomotor.orderedCourse = expectedCourse;

        if (self.position.y < altitudeThreshold && distance < distanceThreshold && self.velocity.y < 0 && self.fuel <= 0)
        {
            if (self.launcherCtrl != null)
            {
                if (self.launcherCtrl.Attack(targetPosition))
                {
                    SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1].speed = self.speed;
                    if(SceneManager.instance.GetComponent<VehicleSelector>().selectedVehicle == self)
                        SceneManager.instance.GetComponent<VehicleSelector>().SelectVehicle(SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1]);
                }
            }
            if (self.GetComponent<WarheadModule>() != null)
            {
                self.GetComponent<WarheadModule>().Ignite();
            }
        }

        if (lastDistance < distance)
        {
            if (farAwayCounter == 0)
            {
                if (self.GetComponent<WarheadModule>() != null)
                {
                    self.GetComponent<WarheadModule>().Ignite();
                }
            }
            else
            {
                farAwayCounter--;
            }
        }

        lastDistance = distance;
    }
}
