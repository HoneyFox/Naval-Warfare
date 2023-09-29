using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AirstripController : MonoBehaviour
{
    private Vehicle _cachedSelf;
    private Vehicle self
    {
        get
        {
            if (_cachedSelf == null) _cachedSelf = this.gameObject.GetComponent<Vehicle>();
            return _cachedSelf;
        }
    }

    public List<string> vehicles = new List<string>();
    public List<int> vehicleCounts = new List<int>();
    public List<Airstrip> airStrips = new List<Airstrip>();

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
	    foreach(Airstrip airStrip in airStrips)
        {
            if(airStrip.vehicleAttached != null)
            {
                if (airStrip.vehicleAttached is Aircraft)
                {
                    Aircraft ac = airStrip.vehicleAttached as Aircraft;
                    if (ac.enabled && ac.isRTB == false)
                        airStrip.vehicleAttached = null;
                }
            }
        }
	}

    public bool PrepareForTakeOff(string vehicleName, out Airstrip airstrip)
    {
        List<Airstrip> availableStrips = new List<Airstrip>();
        foreach(Airstrip airStrip in airStrips)
        {
            if (airStrip.CanTakeOff(vehicleName))
                availableStrips.Add(airStrip);
        }

        if (availableStrips.Count == 0)
        {
            airstrip = null;
            return false;
        }

        int index = UnityEngine.Random.Range(0, availableStrips.Count);
        bool hasTookOff = availableStrips[index].PrepareForTakeOff(vehicleName);
        if (hasTookOff)
            airstrip = availableStrips[index];
        else
            airstrip = null;
        return hasTookOff;
    }

    public void TakeOff(Airstrip airStrip)
    {
        airStrip.TakeOff();
    }

    public void CancelTakeOff(Airstrip airStrip)
    {
        if (airStrip.vehicleAttached == null || airStrip.vehicleIsDeploying == false) return;

        airStrip.CancelTakeOff();
    }

    public Airstrip Land(Vehicle vehicle)
    {
        List<Airstrip> availableStrips = new List<Airstrip>();
        foreach (Airstrip airStrip in airStrips)
        {
            if (airStrip.CanLand(vehicle, true))
            {
                availableStrips.Add(airStrip);
            }
        }

        if(availableStrips.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, availableStrips.Count);
        return availableStrips[index];
    }

    public string FindBestVehicleFor(Track track, bool considerRange = true)
    {
        List<string> candidates = new List<string>();

        string trackTypeName = track.vehicleTypeName;
        Vehicle.VehicleType trackVehicleType = VehicleDatabase.sVehicleTypes[trackTypeName];
        foreach (string vehicle in vehicles)
        {
            if (vehicleCounts[vehicles.IndexOf(vehicle)] <= 0 || (airStrips.Any((Airstrip a) => a.CanTakeOff(vehicle, false)) == false)) continue;

            bool canEngage = VehicleDatabase.sVehicleTaskTypes[vehicle].Any((Vehicle.VehicleType t) => t == trackVehicleType);
            if (canEngage)
            {
                float eta = Vector3.Distance(self.position, track.predictedPositionAtTime(0)) / VehicleDatabase.sVehicleMaxSpeed[vehicle] / 0.9f;
                eta = Vector3.Distance(self.position, track.predictedPositionAtTime(eta)) / VehicleDatabase.sVehicleMaxSpeed[vehicle] / 0.9f;
                eta = Vector3.Distance(self.position, track.predictedPositionAtTime(eta)) / VehicleDatabase.sVehicleMaxSpeed[vehicle] / 0.9f;
                eta = Vector3.Distance(self.position, track.predictedPositionAtTime(eta)) / VehicleDatabase.sVehicleMaxSpeed[vehicle] / 0.9f;
                eta = Vector3.Distance(self.position, track.predictedPositionAtTime(eta)) / VehicleDatabase.sVehicleMaxSpeed[vehicle] / 0.9f;
                eta = Vector3.Distance(self.position, track.predictedPositionAtTime(eta)) / VehicleDatabase.sVehicleMaxSpeed[vehicle] / 0.9f;

                float range = Vector3.Distance(self.position, track.predictedPositionAtTime(eta));

                if (range <= VehicleDatabase.sVehicleRanges[vehicle] / 2.5f || considerRange == false)
                {
                    if (candidates.Contains(vehicle) == false)
                        candidates.Add(vehicle);
                }
            }
        }

        if (candidates.Count == 0)
        {
            return "";
        }
        else
        {
            int index = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[index];
        }
    }
    public void OnDamage(float amount)
    {
        float percent = amount / self.GetComponent<ArmorModule>().maxArmorPoint;
        foreach(Airstrip airStrip in airStrips)
        {
            if (UnityEngine.Random.Range(0f, 1.0f) < percent)
            {
                if(airStrip.vehicleAttached != null)
                {
                    if (airStrip.vehicleIsDeploying || airStrip.vehicleIsUndeploying)
                    {
                        // The vehicle that is staying on the deck will be destroyed.
                        airStrip.vehicleAttached.armorModule.DoDamage(9999f);
                    }
                    else if (airStrip.vehicleIsLaunching)
                    {
                        airStrip.vehicleAttached.armorModule.DoDamage(airStrip.vehicleAttached.armorModule.maxArmorPoint * UnityEngine.Random.Range(0f, 1.0f));
                    }
                    else if (airStrip.vehicleIsLanding)
                    {
                        // Abort the landing! Pull up! Pull up!
                        airStrip.vehicleAttached.OnTakeOff(null);
                    }
                }
                airStrip.enabled = false;
            }
        }

        if (UnityEngine.Random.Range(0f, 1.0f) < percent * 4.0f)
        {
            for (int i = 0; i < vehicleCounts.Count; ++i)
            {
                if (UnityEngine.Random.Range(0f, 1.0f) < percent * 2.0f)
                {
                    vehicleCounts[i] -= (int)(UnityEngine.Random.Range(0f, percent) * vehicleCounts[i]);
                }
            }
        }
    }
}
