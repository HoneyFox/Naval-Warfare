﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditorInternal;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public static bool IsPureCounterSensorType(SensorType type)
    {
        if (type == SensorType.ESM) return true;
        else return false;
    }

    public Vehicle self;

    public enum SensorType
    {
        None,
        Visual,
        Radar,
        ESM,
        MAD,
        ActiveSonar,
        PassiveSonar,
        IR,
    }

    public string sensorName;
    public bool affectedByEarthCurve;
    public float detectMaxDistance;
    public AnimationCurve detectCurveDistance;
    public float detectMaxDepth;
    public float detectMaxHeight;
    public float detectorMaxDepth;
    public float detectorMaxHeight;
    public AnimationCurve detectCurveDepth;
    public bool isToggleable;
    public bool isToggled = true;
    public bool isDestructable = true;
    public SensorType sensorType;
    public SensorType counterSensorType;
    public Vector3 centerVector = Vector3.forward;
    public float halfAngle = 180f;
    public float lastUpdateTime = 0f;
    public float updatePeriod = 0.5f;
    public float error = 0f;
    public float identifyCapability = 20f;
    public float counterJamStrength = 5f;
    public bool IsJamSuccessful(float jamStrength) { return UnityEngine.Random.Range(0f, Mathf.Log10(jamStrength)) >= counterJamStrength; }


    public bool canSearchNewTrack = true;
    public bool canUpdateOldTrack = true;
    public bool isTracker = false;
    public int maxTrackAllowed = 1;
    public List<Track> tracks = new List<Track>();

    private List<Track> _newTracks = new List<Track>();
    private List<Track> _tracksLostByTracker = new List<Track>();

    void Start()
    {

    }

    void FixedUpdate()
    {
        if (isToggled == false) return;

        if (Time.time > lastUpdateTime + updatePeriod)
        {
            _newTracks.Clear();
            _tracksLostByTracker.Clear(); 
            foreach (Vehicle vehicle in SceneManager.instance.vehicles)
            {
                if (vehicle == null) continue;
                if (VehicleDatabase.sVehicleCanBeTracked[vehicle.typeName] == false) continue;
                if (vehicle.side == self.side) continue;
                if (vehicle == self) continue;

                if (CheckIfDetected(vehicle))
                {
                    // This sensor can detect the vehicle.

                    Track existingTrack = null;
                    foreach (Track trk in self.sensorCtrl.tracksDetected)
                    {
                        if (trk.target == vehicle)
                        {
                            existingTrack = trk;
                            break;
                        }
                    }

                    bool noExistingTrack = (existingTrack == null);
                    if (noExistingTrack)
                    {
                        foreach (Track trk in _newTracks)
                        {
                            if (trk.target == vehicle)
                            {
                                noExistingTrack = false;
                                break;
                            }
                        }
                    }

                    if (noExistingTrack)
                    {
                        // It's a new track that is not detected by any sensor (not just this sensor).
                        if (canSearchNewTrack)
                        {
                            Track newTrack = new Track(self, vehicle, TrackId.Unknown);
                            _newTracks.Add(newTrack);

                            float distance = Vector3.Distance(vehicle.position, this.transform.position);
                            Vector3 detectError = new Vector3(UnityEngine.Random.Range(-error * distance / detectMaxDistance, error * distance / detectMaxDistance), UnityEngine.Random.Range(-error * distance / detectMaxDistance, error * distance / detectMaxDistance), UnityEngine.Random.Range(-error * distance / detectMaxDistance, error * distance / detectMaxDistance));
                            newTrack.position = vehicle.position + detectError;
                            newTrack.velocity = Vector3.zero;
                        }
                    }
                    else
                    {
                        // It's an existing track and this sensor can contribute to it.
                        if (canUpdateOldTrack)
                        {
                            float distance = Vector3.Distance(vehicle.position, this.transform.position);
                            Vector3 detectError = new Vector3(UnityEngine.Random.Range(-error * distance / detectMaxDistance, error * distance / detectMaxDistance), UnityEngine.Random.Range(-error * distance / detectMaxDistance, error * distance / detectMaxDistance), UnityEngine.Random.Range(-error * distance / detectMaxDistance, error * distance / detectMaxDistance));
                            if ((Time.time - existingTrack.timeOfDetection) * identifyCapability * existingTrack.target.identifyFactor >= 100f)
                            {
                                TrackId oldId = existingTrack.identification;
                                if (vehicle.side == -1)
                                    existingTrack.UpdateTrack(vehicle.position + detectError, vehicle.velocity + detectError * 0.1f, TrackId.Neutrual);
                                else if (vehicle.side != self.side)
                                    existingTrack.UpdateTrack(vehicle.position + detectError, vehicle.velocity + detectError * 0.1f, TrackId.Hostile);
                                else if (vehicle.side == self.side)
                                    existingTrack.UpdateTrack(vehicle.position + detectError, vehicle.velocity + detectError * 0.1f, TrackId.Friendly);
                                else
                                    existingTrack.UpdateTrack(vehicle.position + detectError, vehicle.velocity + detectError * 0.1f, existingTrack.identification);

                                if (oldId != existingTrack.identification)
                                {
                                    //Debug.Log(self.typeName + "'s " + sensorName + " has identified " + existingTrack.vehicleTypeName + " from " + oldId.ToString() + " to " + existingTrack.identification.ToString());
                                }
                            }
                            else if ((Time.time - existingTrack.timeOfDetection) * identifyCapability * existingTrack.target.identifyFactor >= 75f)
                            {
                                TrackId oldId = existingTrack.identification;
                                if (vehicle.side == -1 && existingTrack.identification == TrackId.Unknown)
                                    existingTrack.UpdateTrack(vehicle.position + detectError, vehicle.velocity + detectError * 0.1f, TrackId.Neutrual);
                                else if (vehicle.side != self.side && existingTrack.identification == TrackId.Unknown)
                                    existingTrack.UpdateTrack(vehicle.position + detectError, vehicle.velocity + detectError * 0.1f, TrackId.AssumedHostile);
                                else if (vehicle.side == self.side && existingTrack.identification == TrackId.Unknown)
                                    existingTrack.UpdateTrack(vehicle.position + detectError, vehicle.velocity + detectError * 0.1f, TrackId.AssumedFriendly);
                                else
                                    existingTrack.UpdateTrack(vehicle.position + detectError, vehicle.velocity + detectError * 0.1f, existingTrack.identification);
                                if (oldId != existingTrack.identification)
                                {
                                    //Debug.Log(self.typeName + "'s " + sensorName + " has identified " + existingTrack.vehicleTypeName + " from " + oldId.ToString() + " to " + existingTrack.identification.ToString()); 
                                }
                            }
                            else
                            {
                                existingTrack.UpdateTrack(vehicle.position + detectError, vehicle.velocity + detectError * 0.1f, existingTrack.identification);
                            }
                            JammerModule[] jammers = existingTrack.target.GetComponents<JammerModule>();
                            if (jammers != null && jammers.Length > 0)
                            {
                                for (int i = 0; i < jammers.Length; ++i)
                                {
                                    jammers[i].OnReceivingSensorAccess(self, this, existingTrack);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // This sensor cannot detect the vehicle.
                    Track lostTrack = null;
                    foreach (Track trk in tracks)
                    {
                        if (trk.target == vehicle)
                        {
                            lostTrack = trk;
                            break;
                        }
                    }
                    if(lostTrack != null)
                    {
                        // It's an existing track but cannot be detected by this sensor. Send it back to sensorCtrl and see if other sensors can maintain the track.
                        tracks.Remove(lostTrack);
                        _tracksLostByTracker.Add(lostTrack);
                    }
                }
            }

            foreach (Track newTrack in _newTracks)
            {
                self.sensorCtrl.AddTrack(newTrack);
                if (isTracker && tracks.Count < maxTrackAllowed && isToggled)
                    tracks.Add(newTrack);
                else
                    self.sensorCtrl.tracksToBeAssignedToTracker.Add(newTrack);
                self.OnNewTrack(newTrack, sensorName);
            }

            for (int i = 0; i < tracks.Count; ++i)
            {
                if(tracks[i].target == null || tracks[i].isLost == true)
                {
                    // Vehicle is destroyed or track age is too high.
                    tracks.RemoveAt(i);
                    i--;
                }
            }

            lastUpdateTime = Time.time;
        }
    }

    public virtual bool CheckIfDetected(Vehicle vehicle)
    {
        if (vehicle == null) return false;
        if (vehicle.onlyVisibleTo != null && vehicle.onlyVisibleTo != self) return false;
        if (this.transform.position.y < detectorMaxDepth || this.transform.position.y > detectorMaxHeight) return false;
        float distance = Vector3.Distance(vehicle.position, this.transform.position);
        if (vehicle.position.y < detectMaxDepth || vehicle.position.y > detectMaxHeight) return false;
        bool canBeDetected = (Sensor.IsPureCounterSensorType(this.sensorType) == false && distance * (1.0f - vehicle.stealthFactor) < detectMaxDistance);
        if (!canBeDetected)
        {
            if (vehicle.sensorCtrl != null)
            {
                foreach (Sensor sensor in vehicle.sensorCtrl.sensors)
                {
                    if (sensor.enabled && sensor.isToggled 
                        && sensor.counterSensorType == this.sensorType 
                        && distance < detectMaxDistance * detectMaxDistance * 0.5f 
                        && sensor.transform.position.y < sensor.detectMaxHeight 
                        && sensor.transform.position.y > sensor.detectorMaxDepth)
                    {
                        canBeDetected = true;
                        break;
                    }
                }
            }
        }
        if (canBeDetected)
        {
            float angleDiff = Vector3.Angle(vehicle.position - this.transform.position, this.transform.TransformDirection(centerVector));
            if (angleDiff < halfAngle)
            {
                if (affectedByEarthCurve)
                {
                    double earthRadius = 6360000f;
                    double angle = Math.Acos(earthRadius / (vehicle.position.y + vehicle.mastHeight + earthRadius)) + Math.Acos(earthRadius / (this.transform.position.y + earthRadius));

                    if (distance < angle * earthRadius)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public virtual void OnDamage()
    {
        if (isDestructable == false) return;
        enabled = false;
        if (isTracker)
        {
            self.sensorCtrl.tracksToBeAssignedToTracker.AddRange(tracks);
            tracks.Clear();
        }
    }
}
