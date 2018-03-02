using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SkimmingAntishipGuidanceModule : GuidanceModule
{
    public float cruiseAlt = 5000f;
    public float cruiseDistance = 80000f;
    public float skimDistance = 40000f;
    public float skimAlt = 50f;
    public float skimCurvePower = 1.5f;
    public float kP = 1.0f;
    public float enableDistance = 5000f;
    public bool selfDestructAfterMissingTarget = true;
    public float diveDistance = 2000f;
    public bool terminalPopup = false;
    public float popupDistance = 4000f;
    public float popupPitch = 20f;

    public float fireDistanceThreshold = 10000f;
    public bool selfDestructAfterFire = true;

    private bool hasInitiallyDisabled = false;
    private bool hasEnabled = false;
    private bool hasDisabled = false;

    public override void OnFixedUpdate(float deltaTime)
    {
        if (self.fuel <= 0) return;
        
        if(hasInitiallyDisabled == false)
        {
            self.sensorCtrl.ToggleAll(false, true);
            hasInitiallyDisabled = true;
        }

        if (targetTrack == null)
        {
            if(guidanceParameter is Vector3)
            {
                if(Vector3.Distance((Vector3)guidanceParameter, self.position) < enableDistance)
                {
                    if (hasEnabled == false)
                    {
                        self.sensorCtrl.ToggleAll(true, true);
                        hasEnabled = true;
                    }
                }
                else
                {
                    if (hasDisabled == false && hasEnabled == true)
                    {
                        self.sensorCtrl.ToggleAll(false, true);
                        hasDisabled = true;

                        if (selfDestructAfterMissingTarget)
                            self.GetComponent<WarheadModule>().Ignite();
                    }
                }

                if(Vector3.Distance((Vector3)guidanceParameter, self.position) < fireDistanceThreshold)
                {
                    if(self.launcherCtrl != null)
                    {
                        if (self.launcherCtrl.Attack((Vector3)guidanceParameter))
                        {
                            SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1].speed = self.speed;
                            if (SceneManager.instance.GetComponent<VehicleSelector>().selectedVehicle == self)
                                SceneManager.instance.GetComponent<VehicleSelector>().SelectVehicle(SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1]);

                            if(selfDestructAfterFire)
                                self.GetComponent<WarheadModule>().Shutdown();
                        }
                    }
                }
            }
        }
        else
        {
            if (Vector3.Distance(targetTrack.predictedPosition, self.position) < enableDistance)
            {
                if (hasEnabled == false)
                {
                    self.sensorCtrl.ToggleAll(true, true);
                    hasEnabled = true;
                }
            }
            else
            {
                if (hasDisabled == false && hasEnabled == true && Vector3.Angle(targetTrack.predictedPosition - self.position, self.velocity) > 90f)
                {
                    self.sensorCtrl.ToggleAll(false, true);
                    hasDisabled = true;
                }
            }

            if (Vector3.Distance((Vector3)targetTrack.predictedPosition, self.position) < fireDistanceThreshold)
            {
                if (self.launcherCtrl != null)
                {
                    if (self.launcherCtrl.Attack(targetTrack.predictedPosition))
                    {
                        SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1].speed = self.speed;
                        if (SceneManager.instance.GetComponent<VehicleSelector>().selectedVehicle == self)
                            SceneManager.instance.GetComponent<VehicleSelector>().SelectVehicle(SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1]);

                        if (selfDestructAfterFire)
                            self.GetComponent<WarheadModule>().Shutdown();
                    }
                }
            }
        }

        if(targetTrack == null)
        {
            // Skim Pitch Logic.
            self.locomotor.orderedPitch = Mathf.Clamp((skimAlt - self.position.y) / self.speed * kP, -30f, 30f);
        }
        else
        {
            if (Vector3.Distance(self.position, targetTrack.predictedPosition) < diveDistance)
            {
                // Dive Logic.
                float expectedPitch = Mathf.Rad2Deg * Mathf.Atan2(targetTrack.predictedPosition.y - self.position.y, Mathf.Sqrt(Mathf.Pow(targetTrack.predictedPosition.z - self.position.z, 2) + Mathf.Pow(targetTrack.predictedPosition.x - self.position.x, 2)));
                self.locomotor.orderedPitch = expectedPitch;

                if (self.position.y < -100 && self.velocity.y < 0)
                    self.GetComponent<WarheadModule>().Ignite();
            }
            else
            {
                if(terminalPopup == true && Vector3.Distance(self.position, targetTrack.predictedPosition) <= popupDistance)
                {
                    self.locomotor.orderedPitch = popupPitch;
                }
                else
                {
                    float factor = Mathf.InverseLerp(skimDistance, cruiseDistance, Vector3.Distance(self.position, targetTrack.predictedPosition));
                    float expectedAlt = Mathf.Lerp(skimAlt, cruiseAlt, Mathf.Pow(factor, skimCurvePower));
                    self.locomotor.orderedPitch = Mathf.Clamp((expectedAlt - self.position.y) / self.speed * kP, -60f, 30f);
                }
            }
        }

        if(targetTrack != null)
        {
            // Course Guidance.
            float eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(0)) / self.speed;
            eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / self.speed;
            eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / self.speed;
            eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / self.speed;

            float expectedCourse = Mathf.Rad2Deg * Mathf.Atan2(targetTrack.predictedPositionAtTime(eta).x - self.position.x, targetTrack.predictedPositionAtTime(eta).z - self.position.z);
            self.locomotor.orderedCourse = expectedCourse;
        }
    }
}
