using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SensorController : MonoBehaviour 
{
    private Vehicle self
    {
        get { return this.gameObject.GetComponent<Vehicle>(); }
    }

    public List<Sensor> sensors = new List<Sensor>();
    public List<Track> tracksDetected = new List<Track>();
    public List<Track> tracksToBeAssignedToTracker = new List<Track>();

    public int maxTracks = 8;
    public float ageFactor = 1f;

    public void AddTrack(Track track)
    {
        if (tracksDetected.Exists((Track trk) => trk.target == track.target))
            return;

        if(tracksDetected.Count < maxTracks)
            tracksDetected.Add(track);
        else
        {
            Track oldestTrack = null;
            float oldestAge = 0f;
            foreach(Track trk in tracksDetected)
            {
                if (trk.age > oldestAge)
                {
                    oldestTrack = trk;
                    oldestAge = trk.age;
                }
            }

            if(oldestTrack != null)
            {
                tracksDetected.Remove(oldestTrack);
                tracksDetected.Add(track);
            }
        }
    }

    // Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    for(int i = 0; i < tracksDetected.Count; ++i)
        {
            if(tracksDetected[i].isLost)
            {
                Debug.Log(self.typeName + " lost track of " + tracksDetected[i].vehicleTypeName + ".");
                tracksDetected.RemoveAt(i);
                --i;
            }
        }

        for (int i = 0; i < tracksToBeAssignedToTracker.Count; ++i)
        {
            if (tracksToBeAssignedToTracker[i].isLost)
            {
                tracksToBeAssignedToTracker.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < tracksToBeAssignedToTracker.Count; ++i)
        {
            Track track = tracksToBeAssignedToTracker[i];
            foreach (Sensor sensor in sensors)
            {
                if (sensor.enabled == false) continue;
                if (sensor.isToggled && sensor.isTracker && (sensor.tracks.Count < sensor.maxTrackAllowed))
                {
                    if (sensor.CheckIfDetected(track.target))
                    {
                        sensor.tracks.Add(track);
                        tracksToBeAssignedToTracker.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
        }
	}

    public void OnDamage(float amount)
    {
        foreach (Sensor sensor in sensors)
        {
            float percent = amount / self.GetComponent<ArmorModule>().maxArmorPoint;
            if (UnityEngine.Random.Range(0f, 1.0f) < percent)
            {
                sensor.OnDamage();
            }
        }
    }

    public void Toggle(string name, bool enabled, bool forcibly = false)
    {
        foreach(Sensor sensor in sensors)
        {
            if(sensor.sensorName == name)
            {
                if(sensor.isToggleable == true || forcibly == true)
                    sensor.isToggled = enabled;

                if (sensor.isTracker && sensor.isToggled == false)
                {
                    tracksToBeAssignedToTracker.AddRange(sensor.tracks);
                    sensor.tracks.Clear();
                }
            }
        }
    }

    public void ToggleAll(bool enabled, bool forcibly = false)
    {
        foreach (Sensor sensor in sensors)
        {
            Toggle(sensor.sensorName, enabled, forcibly);
        }
    }
}
