using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MissileGuidanceModule : GuidanceModule
{
    private float lastActualDistance = -1f;
    private float lastPredictDistance = -1f;

    public float loftCoefficient = 0f;

    public float enableDistance = 20000f;
    public bool selfDestructAfterMissingTarget = true;
    public bool selfDestructAfterLostTrack = true;
    private bool hasInitiallyDisabled = false;
    private bool hasEnabled = false;

    private int selfDestructCounter = 50;

    public override void SetupGuidance(Track target, object guidanceParam)
    {
        base.SetupGuidance(target, guidanceParam);
        lastActualDistance = -1f;
        lastPredictDistance = -1f;
        selfDestructCounter = UnityEngine.Random.Range(20, 150);
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        if (hasInitiallyDisabled == false)
        {
            self.sensorCtrl.ToggleAll(false, true);
            hasInitiallyDisabled = true;
        }

        if (targetTrack != null && targetTrack.isLost == false)
        {
            if (lastActualDistance == -1f)
                lastActualDistance = Vector3.Distance(targetTrack.target.position, self.position);

            float actualDistance = Vector3.Distance(targetTrack.target.position, self.position);

            float distance = Vector3.Distance(targetTrack.predictedPosition, self.position);
            if (distance < enableDistance)
            {
                if (hasEnabled == false)
                {
                    self.sensorCtrl.ToggleAll(true, true);
                    hasEnabled = true;
                }
            }

            float eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(0)) / (self.fuel > 0 ? self.maxSpeed : self.speed);
            eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / (self.fuel > 0 ? self.maxSpeed : self.speed);
            eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / (self.fuel > 0 ? self.maxSpeed : self.speed);
            eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / (self.fuel > 0 ? self.maxSpeed : self.speed);

            Vector3 predictedPosition = targetTrack.predictedPositionAtTime(eta);
            float horizontalDistance = Mathf.Sqrt(Mathf.Pow(predictedPosition.z - self.position.z, 2) + Mathf.Pow(predictedPosition.x - self.position.x, 2));
            predictedPosition.y += loftCoefficient * horizontalDistance * horizontalDistance / Vehicle.sVehicleRanges[self.typeName];

            // Pitch Guidance.
            float expectedPitch = Mathf.Rad2Deg * Mathf.Atan2(predictedPosition.y - self.position.y, horizontalDistance);
            self.locomotor.orderedPitch = expectedPitch;

            // Yaw Guidance.
            float expectedCourse = Mathf.Rad2Deg * Mathf.Atan2(predictedPosition.x - self.position.x, predictedPosition.z - self.position.z);
            self.locomotor.orderedCourse = expectedCourse;

            WarheadModule warhead = self.GetComponent<WarheadModule>();
            if (lastActualDistance < actualDistance && Time.time - warhead.timeOfLaunch > warhead.safetyTimer && distance < warhead.proximityRange * 100f)
            {
                if (lastActualDistance < warhead.damageRadius * 5f || UnityEngine.Random.Range(0, 100) > 75)
                {
                    self.GetComponent<WarheadModule>().Ignite();
                }
                else
                {
                    self.GetComponent<WarheadModule>().Shutdown();
                }
            }
            else if (self.fuel <= 0 && self.speed < targetTrack.velocity.magnitude * 0.9f)
            {
                self.GetComponent<WarheadModule>().Shutdown();
            }

            lastActualDistance = actualDistance;
        }
        else if(guidanceParameter != null)
        {   
            Vector3 predictedPosition = (Vector3)guidanceParameter;
            if (lastPredictDistance == -1f)
                lastPredictDistance = Vector3.Distance(predictedPosition, self.position);
            
            float horizontalDistance = Mathf.Sqrt(Mathf.Pow(predictedPosition.z - self.position.z, 2) + Mathf.Pow(predictedPosition.x - self.position.x, 2));
            predictedPosition.y += loftCoefficient * horizontalDistance * horizontalDistance / Vehicle.sVehicleRanges[self.typeName];

            // Pitch Guidance.
            float expectedPitch = Mathf.Rad2Deg * Mathf.Atan2(predictedPosition.y - self.position.y, horizontalDistance);
            self.locomotor.orderedPitch = expectedPitch;

            // Yaw Guidance.
            float expectedCourse = Mathf.Rad2Deg * Mathf.Atan2(predictedPosition.x - self.position.x, predictedPosition.z - self.position.z);
            self.locomotor.orderedCourse = expectedCourse;

            float predictDistance = Vector3.Distance(predictedPosition, self.position);
            if (predictDistance < enableDistance)
            {
                if (hasEnabled == false)
                {
                    self.sensorCtrl.ToggleAll(true, true);
                    hasEnabled = true;
                }
            }
            if (lastPredictDistance < predictDistance && predictDistance < self.speed)
            {
                if (selfDestructAfterMissingTarget)
                    self.GetComponent<WarheadModule>().Ignite();
            }
            lastPredictDistance = predictDistance;
        }
        else
        {
            if (selfDestructAfterLostTrack || targetTrack == null)
            {
                if (selfDestructCounter == 0)
                    self.GetComponent<WarheadModule>().Shutdown();
                else
                    selfDestructCounter--;
            }
            else
            {
                // We have a track but it's lost so we just keep moving towards the predicted rendezvous point.
                float eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(0)) / (self.fuel > 0 ? self.maxSpeed : self.speed);
                eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / (self.fuel > 0 ? self.maxSpeed : self.speed);
                eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / (self.fuel > 0 ? self.maxSpeed : self.speed);
                eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / (self.fuel > 0 ? self.maxSpeed : self.speed);

                SetupGuidance(null, targetTrack.predictedPositionAtTime(eta));
            }
        }
    }
}
