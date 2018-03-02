using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SurfAgainstAir : Vehicle
{
    public override void OnNewTrack(Track track, string source)
    {
        base.OnNewTrack(track, source);
        switch(Vehicle.sVehicleTypes[track.vehicleTypeName])
        {
            case VehicleType.Air:
                this.StartCoroutine(AirTrackTactic(track));
                break;
            case VehicleType.Surf:
                this.StartCoroutine(SurfTrackTactic(track));
                break;
            case VehicleType.Sub:
                this.StartCoroutine(SubTrackTactic(track));
                break;
        }

        if(this.airstripCtrl != null)
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
                || (isThreat && Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Sub)
                || (isThreat == false && Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Air && autoEngageAirTracks == false)
                || (isThreat == false && Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Surf && autoEngageSurfTracks == false)
                || (isThreat == false && Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Sub && autoEngageSubTracks == false))
            {
                yield return new WaitForSeconds(1.0f);
                continue;
            }

            if(UnityEngine.Random.Range(0f, 100f) < 10f)
            {
                if (interceptorTookOffFrom != null && interceptorTookOffFrom.vehicleAttached == interceptor)
                {
                    // We've got the interceptor on the deck awaiting take-off.
                    interceptorTookOffFrom.TakeOff();
                    if(interceptorTookOffFrom.vehicleIsLaunching)
                        interceptorTookOffFrom = null;
                }
            }
            if (UnityEngine.Random.Range(0f, 100f) < 5f)
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
                        yield return new WaitForSeconds(10f);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }

        // The track is dead/lost. We should recover our interceptor back to the hangar if it has not taken-off yet.
        if(interceptor != null && interceptorTookOffFrom != null)
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

    private IEnumerator AirTrackTactic(Track track)
    {
        Vehicle attacker = null;
        Vehicle attacker2 = null;

        // System's initial response delay.
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.8f, 1.0f));
        while (track.isLost == false && (track.target == null || track.target.armorModule.armorPoint > 0))
        {
            if (Vehicle.sVehicleCanEngage[track.vehicleTypeName].Any((bool value) => value == true))
            {
                // This is a missile/rocket.
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.2f));
                if (autoDealWithAirThreats == false)
                {
                    yield return new WaitForSeconds(1.0f);
                    continue;
                }

                if (track.identification <= TrackId.AssumedHostile)
                {
                    float eta = Vector3.Distance(track.predictedPosition, position) / track.velocity.magnitude;
                    if (attacker2 == null && eta < 15f)
                    {
                        if (launcherCtrl.Attack(track))
                        {
                            attacker2 = SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1];
                            yield return new WaitForSeconds(UnityEngine.Random.Range(0.4f, 0.5f));
                        }
                    }
                    if (attacker == null)
                    {
                        if (launcherCtrl.Attack(track))
                        {
                            attacker = SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1];
                        }
                        yield return new WaitForSeconds(0.2f);
                        yield return null;
                    }
                    else
                    {
                        // This will give higher priority for closer targets. (which are more dangerous)
                        yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.1f) * eta);
                    }
                }
                else
                {
                    if (track.identification == TrackId.Friendly || track.identification == TrackId.Neutrual)
                        yield break;
                    else
                        yield return new WaitForSeconds(2.0f);
                }
            }
            else
            {
                // This is an aircraft.
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.2f));
                if (autoEngageAirTracks == false)
                {
                    yield return new WaitForSeconds(1.0f);
                    continue;
                }

                if (track.identification <= TrackId.Hostile)
                {
                    float distance = Vector3.Distance(track.predictedPosition, position);
                    string bestWeapon = launcherCtrl.FindBestVehicleFor(track);
                    if (bestWeapon != "")
                    {
                        float rangePercent = distance / Vehicle.sVehicleRanges[bestWeapon];
                        if (attacker2 == null && rangePercent < 0.4f)
                        {
                            // The aircraft is too close and we should launch an extra missile to engage it.
                            if (launcherCtrl.Attack(track))
                            {
                                attacker2 = SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1];
                                yield return new WaitForSeconds(UnityEngine.Random.Range(0.4f, 0.5f));
                            }
                        }
                        if (attacker == null)
                        {
                            if (launcherCtrl.Attack(track))
                            {
                                attacker = SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1];
                            }
                            yield return new WaitForSeconds(0.2f);
                            yield return null;
                        }
                        else
                        {
                            // This will give higher priority for closer targets. (which are more dangerous)
                            yield return new WaitForSeconds(UnityEngine.Random.Range(8f, 10f) * rangePercent);
                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 3f));
                    }
                }
                else
                {
                    if (track.identification == TrackId.Friendly || track.identification == TrackId.Neutrual)
                        yield break;
                    else
                        yield return new WaitForSeconds(2.0f);
                }
            }
        }
        yield break;
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

            for (int i = 0; i < attackers.Count; ++i)
            {
                if (attackers[i] == null || attackers[i].isDead)
                {
                    attackers.RemoveAt(i);
                    --i;
                }
            }
            if (track.identification <= TrackId.Hostile)
            {
                if (attackers.Count < 4)
                {
                    if (launcherCtrl.Attack(track))
                    {
                        if (Vehicle.sVehicleCanBeTracked[SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1].typeName])
                        {
                            attackers.Add(SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1]);
                        }
                        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.2f));
                    }
                    yield return new WaitForSeconds(1f);
                    yield return null;
                }
                else if(launcherCtrl.CanEngageWith(track, "MR Shell"))
                {
                    launcherCtrl.Launch("MR Shell", track);
                    yield return new WaitForSeconds(0.5f);
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
                    yield return new WaitForSeconds(2.0f);
            }
            yield return new WaitForSeconds(1f);
            yield return null;
        }

        // The target is lost/dead, we should try to redirect weapons here.
        for (int i = 0; i < attackers.Count; ++i)
        {
            if (attackers[i] == null || attackers[i].isDead)
            {
                attackers.RemoveAt(i);
                --i;
            }
        }
        List<Track> alternateTracks = sensorCtrl.tracksDetected.Where
        (
            (Track trk) =>
            Vehicle.sVehicleTypes[trk.vehicleTypeName] == VehicleType.Surf
            && (trk.target != null && trk.target.isDead == false)
            && trk.identification <= TrackId.AssumedHostile
        ).ToList();
        Debug.LogWarning("Alternate Tracks: " + alternateTracks.Count.ToString());
        if (alternateTracks.Count > 0)
        {
            foreach (Vehicle attacker in attackers)
            {
                if (attacker.GetComponent<DatalinkModule>() != null)
                {
                    Debug.LogWarning("Redirecting " + attacker.typeName + " to alternate tracks.");
                    attacker.GetComponent<DatalinkModule>().CleanOwnTracks();
                    attacker.GetComponent<DatalinkModule>().RedirectTarget(alternateTracks[UnityEngine.Random.Range(0, alternateTracks.Count)], this);
                }
            }
        }

        yield break;
    }

    private IEnumerator SubTrackTactic(Track track)
    {
        Vehicle attacker = null;
        while (track.isLost == false && (track.target == null || track.target.armorModule.armorPoint > 0))
        {
            if(track.target != null && track.target.GetComponent<WarheadModule>() != null)
            {
                // This is a torpedo.
                yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 2.0f));
                if (autoDealWithSubThreats == false)
                {
                    yield return new WaitForSeconds(1.0f);
                    continue;
                }

                if (Vector3.Distance(track.predictedPosition, position) <= Vehicle.sVehicleRanges[track.vehicleTypeName] * 1.1f)
                {
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
                    yield return new WaitForSeconds(2.0f);
                    continue;
                }
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

                if(track.identification <= TrackId.AssumedHostile)
                {
                    if (attacker == null)
                    {
                        if (launcherCtrl.Attack(track))
                        {
                            attacker = SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1];
                        }
                        yield return new WaitForSeconds(0.5f);
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

    public override void OnVehicleControllerGUI()
    {
        if (isDead)
            return;

        GUILayout.Label("Control:");
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Course (" + course.ToString("F0") + "->" + (locomotor.orderedCourse < 0 ? locomotor.orderedCourse + 360f : (locomotor.orderedCourse > 360f ? locomotor.orderedCourse - 360f : locomotor.orderedCourse)).ToString("F0") + ") :");
            locomotor.orderedCourse = GUILayout.HorizontalSlider(locomotor.orderedCourse, locomotor.orderedCourse - 90f, locomotor.orderedCourse + 90f);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Speed (" + speed.ToString("F0") + "->" + locomotor.orderedSpeed.ToString("F0") + ") :");
            locomotor.orderedSpeed = GUILayout.HorizontalSlider(locomotor.orderedSpeed, Mathf.Max(0f, locomotor.orderedSpeed - 10f), Mathf.Min(maxSpeed, locomotor.orderedSpeed + 10f));
        }
        GUILayout.EndHorizontal();
    }
}
