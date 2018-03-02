using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Aircraft : Vehicle
{
    public bool autoRTBWhenOutOfWeapon = true;

    public bool isRTB = false;
    public bool isAligned = false;

    public float alignDistance = 4000f;
    public float approachDistance = 1000f;
    public float landDistance = 200f;
    public float landSpeedFactor = 0.03f;
    public float landSpeedConstant = 0f;

    Track mainTrack = null;

    public override void OnTakeOff(Vehicle fromVehicle)
    {
        base.OnTakeOff(fromVehicle);

        foreach(DatalinkModule dlModule in this.GetComponents<DatalinkModule>())
        {
            if(dlModule.isDuplex)
            {
                dlModule.AddReceiver(fromVehicle);
            }
        }

        foreach (DatalinkModule fromDlModule in fromVehicle.GetComponents<DatalinkModule>())
        {
            if
            (
                this.GetComponents<DatalinkModule>().Any
                (
                    (DatalinkModule dlm) =>
                    dlm.limitTrackedVehicleType == false || dlm.trackedVehicleType == fromDlModule.trackedVehicleType
                )
            )
                fromDlModule.AddReceiver(this);
        }

        locomotor.orderedSpeed = maxSpeed * 0.6f;
        locomotor.orderedCourse = course + (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f ? -15f  : 15f);

        StartCoroutine(ReturnToBaseWhenOutOfWeapon());
    }

    public override void OnNewTrack(Track track, string source)
    {
        if (enabled == false) return;
        if (isRTB) return;

        base.OnNewTrack(track, source);

        // Only need to concern about tracks that might threaten me or about tracks that I can engage with.
        if (Vehicle.sVehicleCanEngage[track.vehicleTypeName][(int)Vehicle.VehicleType.Air] || (launcherCtrl != null && launcherCtrl.FindBestVehicleFor(track, false, false) != ""))
        {
            if (mainTrack == null)
            {
                mainTrack = track;
            }
            else
            {
                if (Vector3.Distance(track.predictedPosition, position) < Vector3.Distance(mainTrack.predictedPosition, position))
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.25f)
                    {
                        mainTrack = track;
                    }
                }
            }
            this.StartCoroutine(TrackTactic(track));
        }
    }

    private IEnumerator TrackTactic(Track track)
    {
        Vehicle attacker = null;
        float timeOfAttack = 0f;
        while (track.isLost == false && track.target != null && track.target.isDead == false)
        {
            // Only deal with this track if it's selected as the main track.
            if(mainTrack == track)
            {
                float distance = Vector3.Distance(track.predictedPosition, position);
                
                if (Vehicle.sVehicleCanEngage[track.vehicleTypeName][(int)Vehicle.VehicleType.Air])
                {
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.2f));
                    if (autoDealWithAirThreats == false)
                    {
                        mainTrack = null;
                        yield return new WaitForSeconds(1.0f);
                        continue;
                    }

                    // This is an anti-air weapon.

                    if (distance <= Vehicle.sVehicleRanges[track.vehicleTypeName] * 1.1f)
                    {
                        if (SceneManager.instance.GetComponent<VehicleSelector>().selectedVehicle == this)
                            Debug.Log("Anti-air weapon detected: " + track.vehicleTypeName + ". Evading!");

                        float directCourse = Mathf.Rad2Deg * Mathf.Atan2(track.predictedPosition.x - position.x, track.predictedPosition.z - position.z);
                        float directPitch = Mathf.Rad2Deg * Mathf.Atan2(Mathf.Max(10.0f, track.predictedPosition.y) - position.y, Mathf.Sqrt(Mathf.Pow(track.predictedPosition.z - position.z, 2) + Mathf.Pow(track.predictedPosition.x - position.x, 2)));
                        locomotor.orderedCourse = directCourse + 180f;
                        locomotor.orderedSpeed = maxSpeed;
                    }
                    else
                    {
                        mainTrack = null;
                        yield return new WaitForSeconds(2.0f);
                        continue;
                    }
                    // TODO: Countermeasure Logic. (Will be implemented later)
                }
                else
                {
                    // This is an aircraft.
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.2f));
                    if (Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Air && autoEngageAirTracks == false
                        || Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Surf && autoEngageSurfTracks == false
                        || Vehicle.sVehicleTypes[track.vehicleTypeName] == VehicleType.Sub && autoEngageSubTracks == false)
                    {
                        mainTrack = null;
                        yield return new WaitForSeconds(1.0f);
                        continue;
                    }

                    // Locomotor Logic.
                    if (distance > 2000)
                    {
                        // Fly towards target.
                        float directCourse = Mathf.Rad2Deg * Mathf.Atan2(track.predictedPosition.x - position.x, track.predictedPosition.z - position.z);
                        float directPitch = Mathf.Rad2Deg * Mathf.Atan2(track.predictedPosition.y - position.y, Mathf.Sqrt(Mathf.Pow(track.predictedPosition.z - position.z, 2) + Mathf.Pow(track.predictedPosition.x - position.x, 2)));
                        locomotor.orderedCourse = directCourse;
                        locomotor.orderedPitch = directPitch;
                        locomotor.orderedSpeed = Mathf.Min(maxSpeed * 0.6f, locomotor.orderedSpeed);
                    }
                    else if (distance < 1000)
                    {
                        // Fly away from target.
                        float directCourse = Mathf.Rad2Deg * Mathf.Atan2(track.predictedPosition.x - position.x, track.predictedPosition.z - position.z);
                        float directPitch = Mathf.Rad2Deg * Mathf.Atan2(track.predictedPosition.y - position.y, Mathf.Sqrt(Mathf.Pow(track.predictedPosition.z - position.z, 2) + Mathf.Pow(track.predictedPosition.x - position.x, 2)));
                        locomotor.orderedCourse = directCourse + 180f;
                        locomotor.orderedPitch = 0f;
                        locomotor.orderedSpeed = Mathf.Min(maxSpeed * 0.6f, locomotor.orderedSpeed);
                    }

                    // Launcher Logic.
                    if (track.identification <= TrackId.Hostile)
                    {
                        string bestWeapon = launcherCtrl.FindBestVehicleFor(track);
                        if (bestWeapon != "")
                        {
                            float rangePercent = distance / Vehicle.sVehicleRanges[bestWeapon];
                            if (rangePercent < 0.8f && (attacker == null || Time.time - timeOfAttack > 60f))
                            {
                                if (launcherCtrl.Attack(track))
                                {
                                    attacker = SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1];
                                    timeOfAttack = Time.time;
                                    // We can give other tracks some opportunity.
                                    mainTrack = null;
                                    yield return new WaitForSeconds(5f);
                                }
                                yield return new WaitForSeconds(0.2f);
                                yield return null;
                            }
                            else
                            {
                                // This will give higher priority for closer targets. (which are more dangerous)
                                yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 5f) * rangePercent);
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
                        {
                            mainTrack = null;
                            locomotor.orderedSpeed = Mathf.Min(maxSpeed * 0.6f, locomotor.orderedSpeed);
                            yield break;
                        }
                        else if (launcherCtrl.FindBestVehicleFor(track, false, true) == "")
                        {
                            yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 10f));
                            mainTrack = null;
                            locomotor.orderedSpeed = Mathf.Min(maxSpeed * 0.6f, locomotor.orderedSpeed);
                            yield break;
                        }
                        else
                            yield return new WaitForSeconds(1.0f);
                    }
                }
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(2.0f, 4.0f));
            if (mainTrack != null)
            {
                foreach (Track trk in sensorCtrl.tracksDetected)
                {
                    if (Vehicle.sVehicleCanEngage[trk.vehicleTypeName][(int)Vehicle.VehicleType.Air] || (launcherCtrl != null && launcherCtrl.FindBestVehicleFor(trk, false, false) != ""))
                    {
                        if (Vector3.Distance(trk.predictedPosition, position) < Vector3.Distance(mainTrack.predictedPosition, position))
                        {
                            if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f)
                            {
                                mainTrack = trk;
                                locomotor.orderedSpeed = Mathf.Min(maxSpeed * 0.6f, locomotor.orderedSpeed);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.1f)
                {
                    mainTrack = track;
                    locomotor.orderedSpeed = Mathf.Min(maxSpeed * 0.6f, locomotor.orderedSpeed);
                    continue;
                }
            }

            if (isRTB)
                yield return new WaitForSeconds(10f);

            yield return new WaitForSeconds(0.2f);
            yield return null;
        }

        if (mainTrack == track)
            mainTrack = null;

        locomotor.orderedSpeed = Mathf.Min(maxSpeed * 0.6f, locomotor.orderedSpeed);

        yield break;
    }

    public override void ReturnToBase()
    {
        StopAllCoroutines();
        isRTB = true;
        isAligned = false;
        useAltCommand = false;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (isDead) return;
        if (fuel == 0) return;
        if (isRTB)
        {
            Vehicle target = null;
            if (takeOffFrom == null || takeOffFrom.isDead || takeOffFrom.airstripCtrl.airStrips.Any((Airstrip airStrip) => airStrip.enabled && airStrip.canLandAir) == false)
            {
                Dictionary<Vehicle, float> distances = new Dictionary<Vehicle, float>();
                // We need to find another vessel to land.
                foreach(Vehicle v in SceneManager.instance.vehicles)
                {
                    if (v == null) continue;
                    if (v.isDead) continue;
                    if (v.side != side) continue;
                    if (v.airstripCtrl == null) continue;
                    if (v.airstripCtrl.airStrips.Count == 0) continue;
                    if (v.airstripCtrl.airStrips.Any((Airstrip airStrip) => airStrip.enabled && airStrip.canLandAir) == false) continue;

                    distances.Add(v, Vector3.Distance(position, v.position));
                }

                if (distances.Count > 0)
                {
                    float minDistance = distances.Values.Min();
                    Vehicle nearestVehicle = distances.First((KeyValuePair<Vehicle, float> kv) => kv.Value == minDistance).Key;
                    target = nearestVehicle;
                }
                else
                {
                    // Cannot return to base.
                    isRTB = false;
                    return;
                }
            }
            else
            {
                target = takeOffFrom;
            }
            

            if(isAligned == false)
            {
                //Debug.Log("Align...");
                // Fly away to make sufficient space.
                float distance = Vector3.Distance(position, target.position - Vector3.Normalize(target.velocity) * alignDistance);
                if (distance > approachDistance)
                {
                    //Debug.Log("Aligning...");
                    Vector3 approachTarget = target.position - Vector3.Normalize(target.velocity) * alignDistance;
                    float expectedCourse = Mathf.Rad2Deg * Mathf.Atan2(approachTarget.x - position.x, approachTarget.z - position.z);
                    locomotor.orderedCourse = expectedCourse;
                    if (locomotor.orderedCourse > 360f) locomotor.orderedCourse -= 360f;
                    if (locomotor.orderedCourse < 0f) locomotor.orderedCourse += 360f;
                    locomotor.orderedPitch = Mathf.Clamp(0.2f * ((20f + approachDistance * 0.06f) - position.y) / speed, -30f, 30f);
                    locomotor.orderedSpeed = maxSpeed * 0.6f;
                }
                else
                {
                    isAligned = true;
                }
            }
            else
            {
                float distance = Vector3.Distance(position, target.position);
                Vector3 approachTarget;
                if (distance > landDistance)
                {
                    //Debug.Log("Approaching...");
                    approachTarget = target.position - Vector3.Normalize(target.velocity) * distance * 0.75f;
                    approachTarget.y = 20f + distance * 0.06f;
                }
                else
                {
                    //Debug.Log("Trying to land...");
                    if(target.airstripCtrl.airStrips.Any((Airstrip airStrip) => airStrip.enabled && airStrip.vehicleAttached == this))
                    {
                        // We've already been assigned to an airstrip.
                        Airstrip assignedAirStrip = target.airstripCtrl.airStrips.Find((Airstrip a) => a.enabled && a.vehicleAttached == this);
                        approachTarget = assignedAirStrip.landStartPoint.position;
                        if (assignedAirStrip.Land(this) == true)
                        {
                            // A success landing! The movement control is handed-over to the vehicle.
                            isRTB = false;
                            isAligned = false;
                            return;
                        }
                    }
                    else
                    {
                        // We have no airStrip assigned yet.
                        Airstrip availableAirstrip = target.airstripCtrl.Land(this);
                        if (availableAirstrip != null)
                        {
                            //Debug.Log("Airstrip available!");
                            availableAirstrip.vehicleAttached = this;
                            approachTarget = availableAirstrip.landStartPoint.position;
                            if (availableAirstrip.Land(this) == true)
                            {
                                // A success landing! The movement control is handed-over to the vehicle.
                                isRTB = false;
                                isAligned = false;
                                return;
                            }
                        }
                        else
                        {
                            //Debug.Log("Go around!");
                            // No airstrip is available now. Go around!
                            isAligned = false;
                            return;
                        }
                    }
                }

                float expectedCourse = Mathf.Rad2Deg * Mathf.Atan2(approachTarget.x - position.x, approachTarget.z - position.z);
                locomotor.orderedCourse = expectedCourse;
                if (locomotor.orderedCourse > 360f) locomotor.orderedCourse -= 360f;
                if (locomotor.orderedCourse < 0f) locomotor.orderedCourse += 360f;
                float expectedPitch = Mathf.Rad2Deg * Mathf.Atan2(approachTarget.y - position.y, Mathf.Sqrt(Mathf.Pow(approachTarget.z - position.z, 2) + Mathf.Pow(approachTarget.x - position.x, 2)));
                locomotor.orderedPitch = expectedPitch;
                locomotor.orderedSpeed = Mathf.Clamp(distance * landSpeedFactor + landSpeedConstant, minSpeed, maxSpeed * 0.4f) + target.speed;
            }
        }
        else
        {
            if (useAltCommand)
                locomotor.orderedPitch = Mathf.Clamp(kP * (newOrderedAlt - position.y) / speed, -40f, 40f);

            if(position.y < 10f || (velocity.y < 0 && position.y / velocity.y < 20f))
            {
                locomotor.orderedPitch = Mathf.Max(Mathf.Clamp((10f - position.y) / speed, -5f, 20f), locomotor.orderedPitch);
            }

            if(mainTrack != null && (mainTrack.target == null || (mainTrack.target != null && mainTrack.target.isDead == true)))
                mainTrack = null;
        }
    }

    private IEnumerator ReturnToBaseWhenOutOfWeapon()
    {
        while(true)
        {
            if (enabled)
            {
                if (autoRTBWhenOutOfWeapon && isRTB == false)
                {
                    if (launcherCtrl == null || launcherCtrl.launchers.All((Launcher l) => l.vehicleCounts.All((int c) => c == 0)))
                    {
                        ReturnToBase();
                    }
                }
                yield return new WaitForSeconds(5f);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    public override List<string> GetVehicleInfo()
    {
        List<string> result = new List<string>();
        if(mainTrack != null)
        {
            result.Add("Main Track: " + mainTrack.vehicleTypeName + "(" + mainTrack.age.ToString("F0") + "): " + mainTrack.identification.ToString() + ", " + Vector3.Distance(mainTrack.predictedPosition, position).ToString("F0"));
        }
        else
        {
            result.Add("No Main Track.");
        }
        result.Add("Tracks of Interest:");
        foreach(Track track in sensorCtrl.tracksDetected)
        {
            if (Vehicle.sVehicleCanEngage[track.vehicleTypeName][(int)Vehicle.VehicleType.Air] || (launcherCtrl != null && launcherCtrl.FindBestVehicleFor(track, false, false) != ""))
            {
                result.Add(track.vehicleTypeName + "(" + track.age.ToString("F0") + "): " + track.identification.ToString() + ", " + Vector3.Distance(track.predictedPosition, position).ToString("F0"));
            }
        }
        return result;
    }

    public float kP = 10f;
    float newOrderedAlt = 1000f;
    string newOrderedAltStr = "1000";
    bool useAltCommand = false;

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
            locomotor.orderedPitch = newPitch;
    }
}