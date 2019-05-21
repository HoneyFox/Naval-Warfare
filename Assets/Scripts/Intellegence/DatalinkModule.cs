using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DatalinkModule : MonoBehaviour
{
    private Vehicle self
    {
        get { return this.gameObject.GetComponent<Vehicle>(); }
    }

    public float syncInterval = 1f;
    public float lastSyncTime = -9999f;

    public bool receivesNewTrack = true;
    public bool useAsTrackUpdate = true;

    public bool limitTrackedVehicleType = false;
    public Vehicle.VehicleType trackedVehicleType = Vehicle.VehicleType.Air;

    // Normally data-link node is receiver-only. Only duplex data-link nodes can send to other nodes.
    public bool isDuplex = false;

    public List<Vehicle> receivers = new List<Vehicle>();

    public float minCommunicateDepth = -20f;

    public string datalinkTypeName = "Universal";

    public float counterJamStrength = 4f;
    public float jamState;

    private bool isJamSuccessful() { return UnityEngine.Random.Range(0f, Mathf.Log10(jamState)) >= counterJamStrength; }

    void Start()
    {

    }

    void FixedUpdate()
    {
        jamState *= 0.9f;

        if (self.isDead) return;

        if (Time.time > lastSyncTime + syncInterval)
        {
            if (self.position.y < minCommunicateDepth) return;

            for (int i = 0; i < receivers.Count; ++i)
            {
                if (receivers[i] == null || receivers[i].isDead)
                {
                    receivers.RemoveAt(i);
                    i--;
                }
            }

            if (isDuplex == false) return;

            // Send tracks detected by self to all other units of same side that have data-link installed as well.
            foreach (Vehicle v in receivers)
            {
                if (v.isDead) continue;
                if (v.side != self.side) continue;
                if (v.GetComponent<DatalinkModule>() == null) continue;
                if (v == self) continue;

                foreach (Track track in self.sensorCtrl.tracksDetected)
                {
                    // Skip tracks that are considered lost (but yet to be removed from the list)
                    if (track.isLost) continue;
                    if (limitTrackedVehicleType && Vehicle.sVehicleTypes[track.vehicleTypeName] != trackedVehicleType) continue;

                    // Sender side jam check.
                    if (isJamSuccessful() == false)
                        v.GetComponent<DatalinkModule>().ReceiveTrack(track, self);
                }
            }

            lastSyncTime = Time.time;
        }
    }

    public void AddReceiver(Vehicle vehicle)
    {
        if (receivers.Contains(vehicle) == false)
        {
            if(vehicle.GetComponents<DatalinkModule>().Any((DatalinkModule dlm) => dlm.datalinkTypeName == datalinkTypeName))
                receivers.Add(vehicle);
        }
    }
    
    public void ReceiveTrack(Track track, Vehicle fromVehicle, bool forciblyReceiveNewTrack = false)
    {
        if (self.isDead) return;
        if (self.position.y < minCommunicateDepth) return;
        
        // Receiver side jam check.
        if (isJamSuccessful()) return;

        Track ownTrack = self.sensorCtrl.tracksDetected.Find((Track trk) => trk.target == track.target && trk.target != null);
        if (ownTrack != null)
        {
            // We already have the same track. See if we should update the track by checking which one is newer.
            TrackId combinedTrackId = TrackId.Unknown;
            if (Math.Abs((int)track.identification) > Math.Abs((int)ownTrack.identification))
                combinedTrackId = track.identification;
            else
                combinedTrackId = ownTrack.identification;

            if (useAsTrackUpdate && track.age < ownTrack.age)
                ownTrack.UpdateTrack(track.position, track.velocity, track.timeOfLastUpdate, combinedTrackId);
            else
                ownTrack.identification = combinedTrackId;
        }
        else
        {
            if (receivesNewTrack || forciblyReceiveNewTrack)
            {
                Track ownNewTrack = new Track(self, track.target, TrackId.Unknown);
                ownNewTrack.UpdateTrack(track.position, track.velocity, track.timeOfLastUpdate, track.identification);
                self.sensorCtrl.AddTrack(ownNewTrack);
                self.OnNewTrack(ownNewTrack, "Data Link by " + fromVehicle.typeName);
            }
        }
    }

    public void CleanOwnTracks()
    {
        if (self.sensorCtrl != null)
        {
            self.sensorCtrl.tracksDetected.Clear();
            self.sensorCtrl.tracksToBeAssignedToTracker.Clear();
        }
        if (self.guidanceModule != null)
        {
            self.guidanceModule.targetTrack = null;
        }
    }

    public void RedirectTarget(Track newTrack, Vehicle fromVehicle)
    {
        ReceiveTrack(newTrack, fromVehicle, true);
    }
}
