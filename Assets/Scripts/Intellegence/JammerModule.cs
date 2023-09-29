using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static JammerModule;

public class JammerModule : MonoBehaviour
{
    public enum JamType
    {
        DistanceJamming,
        PositionJamming,
        FalseTarget,
        DatalinkJamming,
    }

    private Vehicle _cachedSelf;
    private Vehicle self
    {
        get
        {
            if (_cachedSelf == null) _cachedSelf = this.gameObject.GetComponent<Vehicle>();
            return _cachedSelf;
        }
    }

    public bool isSelfDefenseJamming;

    public JamType jamType;
    public float jamStrength;

    public float GetJamStrengthAtDistance(float distanceSqr)
    {
        return jamStrength * distanceSqr;
    }
    public float GetJamStrengthAtDistance(Vector3 losVector)
    {
        return jamStrength * losVector.sqrMagnitude;
    }

    public Sensor.SensorType counterSensorType;
    private Vehicle falseVehicleToAdd = null;
    private Track falseTrack = null;
    public float maxFalseTrackAge = 10;

    public void OnReceivingSensorAccess(Vehicle accessor, Sensor sensor, Track track)
    {
        if (sensor.sensorType == this.counterSensorType)
        {
            if (isSelfDefenseJamming == false || track.target == self)
            {
                switch (jamType)
                {
                    case JamType.DistanceJamming:
                        {
                            float jamStrengthAtDistance = GetJamStrengthAtDistance(accessor.position - self.position);
                            if (sensor.IsJamSuccessful(jamStrengthAtDistance))
                            {
                                var sensorPosition = accessor.position;
                                var losVec = track.position - sensorPosition;
                                float randomDistanceCoeff = UnityEngine.Random.Range(-jamStrength, jamStrength) + 1f;
                                var falsePos = losVec.magnitude * randomDistanceCoeff * losVec.normalized + sensorPosition;

                                track.position = falsePos;
                            }
                            break;
                        }

                    case JamType.PositionJamming:
                        {
                            float jamStrengthAtDistance = GetJamStrengthAtDistance(accessor.position - self.position);
                            if (sensor.IsJamSuccessful(jamStrengthAtDistance))
                            {
                                float randomDistanceCoeff = UnityEngine.Random.Range(0, GetJamStrengthAtDistance(accessor.position - self.position)) + 1f;
                                var falsePos = track.position - self.velocity.magnitude * randomDistanceCoeff * self.velocity.normalized;

                                track.position = falsePos;
                            }
                            break;
                        }

                    case JamType.DatalinkJamming:
                        {
                            var datalinkModules = accessor.GetComponents<DatalinkModule>();
                            if (datalinkModules != null && datalinkModules.Length > 0)
                            {
                                for(int i = 0; i < datalinkModules.Length; ++i)
                                {
                                    DatalinkModule datalinkModule = datalinkModules[i];
                                    datalinkModule.jamState += GetJamStrengthAtDistance(accessor.position - self.position);
                                }
                            }
                            break;
                        }

                    case JamType.FalseTarget:
                        {
                            if (falseTrack == null)
                            {
                                Vector3 falsePos = self.position + UnityEngine.Random.insideUnitSphere * jamStrength;
                                Vector2 randCircle = UnityEngine.Random.insideUnitCircle;
                                falsePos.x += randCircle.x * jamStrength * 10f;
                                falsePos.z += randCircle.y * jamStrength * 10f;
                                falsePos.y = Mathf.Max(falsePos.y, 5f);

                                Vector3 falseVel = UnityEngine.Random.insideUnitCircle.normalized * 200f;

                                GameObject falseVehicleGO = new GameObject("False Target");
                                GameObject falseVehicleModelGO = new GameObject("Model");
                                falseVehicleModelGO.transform.SetParent(falseVehicleGO.transform, false);
                                ArmorModule armorModule = falseVehicleGO.AddComponent<ArmorModule>();
                                armorModule.armorPoint = 1.0f;
                                armorModule.isArmorPointInvulnerable = true;
                                AeroLocomotor locomotor = falseVehicleGO.AddComponent<AeroLocomotor>();
                                falseVehicleToAdd = falseVehicleGO.AddComponent<Vehicle>();
                                falseVehicleToAdd.side = -1;
                                falseVehicleToAdd.locomotor = locomotor;
                                falseVehicleToAdd.position = falsePos;
                                falseVehicleToAdd.speed = self.maxSpeed * UnityEngine.Random.Range(0.8f, 1.2f);
                                falseVehicleToAdd.minSpeed = self.maxSpeed * 0.8f;
                                falseVehicleToAdd.maxSpeed = self.maxSpeed * 1.2f;
                                falseVehicleToAdd.pitch = 0f;
                                falseVehicleToAdd.course = self.course + UnityEngine.Random.Range(-1f, 1f) * 60f;
                                falseVehicleToAdd.fuel = 1f;
                                falseVehicleToAdd.fuelConsumptionRate = 0f;
                                falseVehicleToAdd.fuelConsumptionCurve = new AnimationCurve();
                                falseVehicleToAdd.onlyVisibleTo = accessor;
                                falseVehicleToAdd.typeName = self.typeName;

                                falseTrack = new Track(accessor, falseVehicleToAdd, TrackId.Unknown);
                                falseTrack.position = falsePos;
                                falseTrack.velocity = falseVehicleToAdd.velocity;

                                falseVehicleGO.transform.parent = self.transform.parent.parent.Find("Side -1");
                            }
                            else
                            {
                                if (accessor == falseTrack.owner)
                                    falseTrack.timeOfLastUpdate = Time.time;
                            }
                            break;
                        }
                }
            }
        }
    }

    void Start () {
		
	}
	
	void FixedUpdate ()
    {
        if (falseVehicleToAdd != null)
        {
            SceneManager.instance.vehicles.Add(falseVehicleToAdd);
            falseVehicleToAdd = null;
        }

        if (jamType == JamType.FalseTarget && falseTrack != null && falseTrack.age > maxFalseTrackAge)
        {
            DestroyFalseTrack();
        }
	}

    void DestroyFalseTrack()
    {
        // Destroy falseTrack.
        SceneManager.instance?.vehicleSelector?.OnVehicleDead(falseTrack.target);
        SceneManager.instance?.vehicles.Remove(falseTrack.target);
        GameObject.Destroy(falseTrack.target.gameObject);
        falseTrack = null;
    }

    private void OnDestroy()
    {
        if (falseTrack != null)
        {
            DestroyFalseTrack();
        }
    }
}
