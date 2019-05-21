using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class JammerModule : MonoBehaviour
{
    public enum JamType
    {
        DistanceJamming,
        PositionJamming,
        FalseTarget,
        DatalinkJamming,
    }

    private Vehicle self
    {
        get { return this.gameObject.GetComponent<Vehicle>(); }
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
    private Vehicle falseVehicle = null;
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
                            if (falseTrack == null || falseTrack.age > maxFalseTrackAge)
                            {
                                Vector3 falsePos = self.position + UnityEngine.Random.insideUnitSphere * jamStrength;
                                Vector2 randCircle = UnityEngine.Random.insideUnitCircle;
                                falsePos.x += randCircle.x * jamStrength * 10f;
                                falsePos.z += randCircle.y * jamStrength * 10f;

                                Vector3 falseVel = UnityEngine.Random.insideUnitCircle.normalized * 200f;

                                falseVehicle = new Vehicle();
                                falseVehicle.position = falsePos;
                                falseVehicle.speed = 200f;
                                falseVehicle.pitch = 0f;
                                falseVehicle.course = UnityEngine.Random.Range(0f, 1f) * 360f;
                                falseVehicle.typeName = self.typeName;

                                falseTrack = new Track(accessor, falseVehicle, TrackId.Unknown);
                                falseTrack.position = falsePos;
                                falseTrack.velocity = falseVehicle.velocity;
                            }
                            else
                            {
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
		
	}
}
