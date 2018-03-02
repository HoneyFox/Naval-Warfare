using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LauncherController : MonoBehaviour 
{
    private Vehicle self
    {
        get { return this.gameObject.GetComponent<Vehicle>(); }
    }

    public List<Launcher> launchers = new List<Launcher>();
    public List<List<string>> vehicleNames = new List<List<string>>();
    public List<List<int>> vehicleCount = new List<List<int>>();

	// Use this for initialization
	void Start ()
    {
	    
	}
	
	// Update is called once per frame
	void Update () 
    {
	    foreach(Launcher launcher in launchers)
        {
            launcher.UpdateFireControlChannels();
        }
	}

    public void OnDamage(float amount)
    {
        foreach (Launcher launcher in launchers)
        {
            float percent = amount / self.GetComponent<ArmorModule>().maxArmorPoint;
            if (UnityEngine.Random.Range(0f, 1.0f) < percent)
            {
                launcher.enabled = false;
            }
        }
    }

    public bool Launch(string vehicleName, Track track)
    {
        if (track.isLost || (track.target == null || track.target.isDead))
            return false;

        for(int i = 0; i < launchers.Count; ++i)
        {
            bool hasLaunched = launchers[i].Launch(vehicleName, track);
            if (hasLaunched)
            {
                Debug.Log(self.typeName + " launches a " + vehicleName + " against " + track.vehicleTypeName);
                return true;
            }
        }
        return false;
    }

    public bool Launch(string vehicleName, Vector3 position)
    {
        for (int i = 0; i < launchers.Count; ++i)
        {
            bool hasLaunched = launchers[i].Launch(vehicleName, position);
            if (hasLaunched)
            {
                Debug.Log(self.typeName + " launches a " + vehicleName + " against Location" + position.ToString());
                return true;
            }
        }
        return false;
    }

    public bool Attack(Track track)
    {
        if (track.isLost || (track.target == null || track.target.isDead))
            return false;
        
        string vehicleName = FindBestVehicleFor(track);
        if (vehicleName != "")
        {
            return Launch(vehicleName, track);
        }
        else
            return false;
    }

    public bool Attack(Vector3 position)
    {
        string vehicleName = FindBestVehicleFor(position);
        if (vehicleName != "")
            return Launch(vehicleName, position);
        else
            return false;
    }

    public bool CanEngageWith(Track track, string vehicleName, bool considerLoaded = true, bool considerRange = true)
    {
        string trackTypeName = track.vehicleTypeName;
        Vehicle.VehicleType trackVehicleType = Vehicle.sVehicleTypes[trackTypeName];
        foreach (Launcher launcher in launchers)
        {
            if (launcher.vehicleNames.Contains(vehicleName) == false) continue;
            if (launcher.enabled == false) continue;
            if (launcher.vehicleCounts[launcher.vehicleNames.IndexOf(vehicleName)] <= 0 || (launcher.isLoaded == false && considerLoaded == true)) continue;

            bool canEngage = Vehicle.sVehicleCanEngage[vehicleName][(int)trackVehicleType];
            bool canBeEngaged = Vehicle.sVehicleCanBeEngaged[track.vehicleTypeName][(int)Vehicle.sVehicleTypes[vehicleName]];
            if (canEngage && canBeEngaged)
            {
                float range = Vector3.Distance(self.position, track.predictedPosition);

                if (range <= Vehicle.sVehicleRanges[vehicleName] || considerRange == false)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public string FindBestVehicleFor(Track track, bool considerReload = true, bool considerRange = true)
    {
        Dictionary<string, float> candidates = new Dictionary<string,float>();

        string trackTypeName = track.vehicleTypeName;
        Vehicle.VehicleType trackVehicleType = Vehicle.sVehicleTypes[trackTypeName];
        foreach(Launcher launcher in launchers)
        {
            foreach(string vehicle in launcher.vehicleNames)
            {
                if (launcher.vehicleCounts[launcher.vehicleNames.IndexOf(vehicle)] <= 0 || (launcher.isLoaded == false && considerReload)) continue;
                if (launcher.enabled == false) continue;

                if (Vehicle.sVehicleCanEngage.ContainsKey(vehicle) == false)
                    Debug.LogError("Error: No Engage List for " + vehicle);
                bool canEngage = Vehicle.sVehicleCanEngage[vehicle][(int)trackVehicleType];
                bool canBeEngaged = Vehicle.sVehicleCanBeEngaged[track.vehicleTypeName][(int)Vehicle.sVehicleTypes[vehicle]];

                if(canEngage && canBeEngaged)
                {
                    float eta = Vector3.Distance(self.position, track.predictedPositionAtTime(0)) / Vehicle.sVehicleMaxSpeed[vehicle] / 0.9f;
                    eta = Vector3.Distance(self.position, track.predictedPositionAtTime(eta)) / Vehicle.sVehicleMaxSpeed[vehicle] / 0.9f;
                    eta = Vector3.Distance(self.position, track.predictedPositionAtTime(eta)) / Vehicle.sVehicleMaxSpeed[vehicle] / 0.9f;
                    eta = Vector3.Distance(self.position, track.predictedPositionAtTime(eta)) / Vehicle.sVehicleMaxSpeed[vehicle] / 0.9f;
                    eta = Vector3.Distance(self.position, track.predictedPositionAtTime(eta)) / Vehicle.sVehicleMaxSpeed[vehicle] / 0.9f;
                    eta = Vector3.Distance(self.position, track.predictedPositionAtTime(eta)) / Vehicle.sVehicleMaxSpeed[vehicle] / 0.9f;

                    float range = Vector3.Distance(self.position, track.predictedPositionAtTime(eta));
                    
                    if (range <= Vehicle.sVehicleRanges[vehicle] || considerReload == false)
                    {
                        if (candidates.ContainsKey(vehicle) == false)
                            candidates.Add(vehicle, Vehicle.sVehicleRanges[vehicle]);
                    }
                }
            }
        }

        if (candidates.Count == 0)
        {
            return "";
        }
        else
        {
            float minRange = candidates.Min<KeyValuePair<string, float>>((KeyValuePair<string, float> kv) => { return kv.Value; });
            string result = candidates.First((KeyValuePair<string, float> kv2) => { return kv2.Value == minRange; }).Key;
            return result;
        }
    }

    public string FindBestVehicleFor(Vector3 position, bool considerReload = true, bool considerRange = true)
    {
        Dictionary<string, float> candidates = new Dictionary<string, float>();
        
        foreach(Launcher launcher in launchers)
        {
            foreach(string vehicle in launcher.vehicleNames)
            {
                if (launcher.vehicleCounts[launcher.vehicleNames.IndexOf(vehicle)] <= 0 || (launcher.isLoaded == false && considerReload)) continue;
                if (launcher.enabled == false) continue;

                bool canEngage = Vehicle.sVehicleCanEngage[vehicle][3];
                if(canEngage)
                {
                    float range = Vector3.Distance(this.GetComponent<Vehicle>().position, position);
                    if (range <= Vehicle.sVehicleRanges[vehicle] || considerRange == false)
                    {
                        if (candidates.ContainsKey(vehicle) == false)
                            candidates.Add(vehicle, Vehicle.sVehicleRanges[vehicle]);
                    }
                }
            }
        }

        if (candidates.Count == 0)
        {
            return "";
        }
        else
        {
            float minRange = candidates.Min<KeyValuePair<string, float>>((KeyValuePair<string, float> kv) => { return kv.Value; });
            string result = candidates.First((KeyValuePair<string, float> kv2) => { return kv2.Value == minRange; }).Key;
            return result;
        }
    }
}
