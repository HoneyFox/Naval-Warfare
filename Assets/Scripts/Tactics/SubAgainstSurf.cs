using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SubAgainstSurf : Vehicle
{
    private float origMaxSpeed = 0f;
    private float origTurnRadius = 0f;
    private float origOrderedSpeed = 0f;
    private float origOrderedPitch = 0f;
    private float lastUpdatePosY = 0f;

    private bool forcedLocomotor = false;

    void Start()
    {
        origMaxSpeed = maxSpeed;
        origTurnRadius = locomotor.turnRadius;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (position.y > 0 && lastUpdatePosY < 0)
        {
            forcedLocomotor = true;
            origOrderedSpeed = locomotor.orderedSpeed;
            origOrderedPitch = locomotor.orderedPitch;
            maxSpeed = 20f;
            if (speed > maxSpeed)
                speed -= locomotor.dragAcceleration * Time.fixedDeltaTime * 20f;
            locomotor.turnRadius = 100f;
            locomotor.orderedPitch = -90f;
        }
        else if (position.y < 0 && lastUpdatePosY > 0)
        {
            forcedLocomotor = false;
            maxSpeed = origMaxSpeed;
            locomotor.turnRadius = origTurnRadius;
            locomotor.orderedSpeed = origOrderedSpeed;
            locomotor.orderedPitch = origOrderedPitch;
        }
        lastUpdatePosY = position.y;

        if (useAltCommand)
            if (forcedLocomotor == false && isDead == false)
                locomotor.orderedPitch = Mathf.Clamp(kP * (newOrderedAlt - position.y) / speed, -40f, 40f);
    }

    public override void OnNewTrack(Track track, string source)
    {
        base.OnNewTrack(track, source);
        switch (Vehicle.sVehicleTypes[track.vehicleTypeName])
        {
            case VehicleType.Surf:
                this.StartCoroutine(SurfTrackTactic(track));
                break;
            case VehicleType.Sub:
                this.StartCoroutine(SubTrackTactic(track));
                break;
        }

        if (this.airstripCtrl != null)
            this.StartCoroutine(TakeOffVehicleTactic(track));
    }

    private IEnumerator TakeOffVehicleTactic(Track track)
    {
        Vehicle interceptor = null;
        Airstrip interceptorTookOffFrom = null;

        while (track.target != null && track.isLost == false)
        {
            bool isThreat = (Vehicle.sVehicleCanEngage[track.vehicleTypeName][(int)Vehicle.VehicleType.Surf] == true
                || Vehicle.sVehicleCanEngage[track.vehicleTypeName][(int)Vehicle.VehicleType.Sub] == true);
            if ((isThreat && Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Air && autoDealWithAirThreats == false)
                || (isThreat && Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Sub && autoDealWithSubThreats == false)
                || (isThreat == false && Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Air && autoEngageAirTracks == false)
                || (isThreat == false && Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Surf && autoEngageSurfTracks == false)
                || (isThreat == false && Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Sub && autoEngageSubTracks == false))
            {
                yield return new WaitForSeconds(1.0f);
                continue;
            }

            if (UnityEngine.Random.Range(0f, 100f) < 50f)
            {
                if (interceptorTookOffFrom != null && interceptorTookOffFrom.vehicleAttached == interceptor)
                {
                    // We've got the interceptor on the deck awaiting take-off.
                    interceptorTookOffFrom.TakeOff();
                    if (interceptorTookOffFrom.vehicleIsLaunching)
                        interceptorTookOffFrom = null;
                }
            }
            if (UnityEngine.Random.Range(0f, 100f) < 40f)
            {
                if (interceptor == null || interceptor.isDead || (interceptor is Aircraft && (interceptor as Aircraft).isRTB))
                {
                    string chosenInterceptor = airstripCtrl.FindBestVehicleFor(track);
                    if (chosenInterceptor != "")
                    {
                        Airstrip airStrip;
                        if (airstripCtrl.PrepareForTakeOff(chosenInterceptor, out airStrip))
                        {
                            interceptor = airStrip.vehicleAttached;
                            interceptorTookOffFrom = airStrip;
                            yield return new WaitForSeconds(UnityEngine.Random.Range(0.4f, 0.5f));
                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds(5f);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }

        // The track is dead/lost. We should recover our interceptor back to the hangar if it has not taken-off yet.
        if (interceptor != null && interceptorTookOffFrom != null)
        {
            while (true)
            {
                if (interceptorTookOffFrom.vehicleIsDeploying && interceptorTookOffFrom.vehicleIsUndeploying == false && interceptorTookOffFrom.vehicleProgress == 100f)
                {
                    interceptorTookOffFrom.CancelTakeOff();
                    break;
                }

                yield return new WaitForSeconds(1f);
            }
        }
    }

    private IEnumerator SurfTrackTactic(Track track)
    {
        List<Vehicle> attackers = new List<Vehicle>();
        yield return new WaitForSeconds(UnityEngine.Random.Range(2.0f, 4.0f));
        while (track.isLost == false && (track.target == null || track.target.armorModule.armorPoint > 0))
        {
            if (autoEngageSurfTracks == false)
            {
                yield return new WaitForSeconds(1.0f);
                continue;
            }

            if (track.identification <= TrackId.Hostile)
            {
                for (int i = 0; i < attackers.Count; ++i)
                {
                    if (attackers[i] == null || attackers[i].isDead)
                    {
                        attackers.RemoveAt(i);
                        --i;
                    }
                }
                if (attackers.Count < 3)
                {
                    if (launcherCtrl.Attack(track))
                    {
                        attackers.Add(SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1]);
                        yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 2.0f));
                    }
                    yield return null;
                }
                else
                {
                    yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 2.0f));
                }
            }
            else
            {
                if (track.identification == TrackId.Friendly || track.identification == TrackId.Neutrual)
                    yield break;
                else
                    yield return new WaitForSeconds(1.0f);
            }
        }
        yield break;
    }

    private IEnumerator SubTrackTactic(Track track)
    {
        Vehicle attacker = null;
        float timeOfLaunchOfAttacker = 0f;
        while (track.isLost == false && (track.target == null || track.target.armorModule.armorPoint > 0))
        {
            if (track.target != null && track.target.GetComponent<WarheadModule>() != null)
            {
                // This is a weapon.
                yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 2.0f));
                if (autoDealWithSubThreats == false)
                {
                    yield return new WaitForSeconds(1.0f);
                    continue;
                }
                
                if (Vector3.Distance(track.predictedPosition, position) > 15000f)
                {
                    float bearing = Mathf.Rad2Deg * Mathf.Atan2(track.predictedPosition.x - position.x, track.predictedPosition.z - position.z);
                    locomotor.orderedCourse = bearing + (UnityEngine.Random.Range(0.0f, 100.0f) > 50.0f ? 90.0f : -90.0f);
                }
                else
                {
                    float bearing = Mathf.Rad2Deg * Mathf.Atan2(track.predictedPosition.x - position.x, track.predictedPosition.z - position.z);
                    locomotor.orderedCourse = bearing + 180.0f;
                }
                locomotor.orderedSpeed = maxSpeed;
                yield return new WaitForSeconds(UnityEngine.Random.Range(30.0f, 40.0f));
            }
            else
            {
                // This is a submarine.
                yield return new WaitForSeconds(UnityEngine.Random.Range(4.0f, 8.0f));
                if (autoEngageSubTracks == false)
                {
                    yield return new WaitForSeconds(1.0f);
                    continue;
                }

                if (track.identification <= TrackId.AssumedHostile)
                {
                    if (attacker == null || Time.time - timeOfLaunchOfAttacker > 60f)
                    {
                        if (launcherCtrl.Attack(track))
                        {
                            attacker = SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1];
                            timeOfLaunchOfAttacker = Time.time;
                            yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 2.0f));
                        }
                        yield return new WaitForSeconds(1f);
                        yield return null;
                    }
                    else
                    {
                        yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 2.0f));
                    }
                }
                else
                {
                    if (track.identification == TrackId.Friendly || track.identification == TrackId.Neutrual)
                        yield break;
                    else
                        yield return new WaitForSeconds(1.0f);
                }
            }
        }
        yield break;
    }

    public float kP = 10f;
    float newOrderedAlt = -20f;
    string newOrderedAltStr = "-20";
    bool useAltCommand = false;

    public override void OnVehicleControllerGUI()
    {
        if(isDead)
            return;

        GUILayout.Label("Control:");
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Course (" + course.ToString("F0") + "->" + (locomotor.orderedCourse < 0 ? locomotor.orderedCourse + 360f : (locomotor.orderedCourse > 360f ? locomotor.orderedCourse - 360f : locomotor.orderedCourse)).ToString("F0") + ") :");
            if (forcedLocomotor == false)
                locomotor.orderedCourse = GUILayout.HorizontalSlider(locomotor.orderedCourse, locomotor.orderedCourse - 180f, locomotor.orderedCourse + 180f);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Speed (" + speed.ToString("F0") + "->" + locomotor.orderedSpeed.ToString("F0") + ") :");
            if (forcedLocomotor == false)
                locomotor.orderedSpeed = GUILayout.HorizontalSlider(locomotor.orderedSpeed, Mathf.Max(0f, locomotor.orderedSpeed - 10f), Mathf.Min(maxSpeed, locomotor.orderedSpeed + 10f));
        }
        GUILayout.EndHorizontal();
        float newPitch = 0f;
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Pitch (" + pitch.ToString("F0") + "->" + locomotor.orderedPitch.ToString("F0") + ") :");
            newPitch = GUILayout.HorizontalSlider(locomotor.orderedPitch, Mathf.Max(-40f, locomotor.orderedPitch - 20f), Mathf.Min(40f, locomotor.orderedPitch + 20f));
            if (newPitch != locomotor.orderedPitch)
                useAltCommand = false;
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            newOrderedAltStr = GUILayout.TextField(newOrderedAltStr);
            bool newValue = GUILayout.Toggle(useAltCommand, "Set Alt", "button", GUILayout.Width(60f));
            if ((useAltCommand == false && newValue == true) || (useAltCommand == true && newValue == false))
            {
                newOrderedAlt = Convert.ToSingle(newOrderedAltStr);
                useAltCommand = true;
            }
        }
        GUILayout.EndHorizontal();

        if (!useAltCommand)
            if (forcedLocomotor == false)
                locomotor.orderedPitch = newPitch;
    }
}