using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AAM : Vehicle
{
    public override void OnNewTrack(Track track, string source)
    {
        if (VehicleDatabase.sVehicleTypes[track.vehicleTypeName] == VehicleType.Air
            && VehicleDatabase.sVehicleCanEngage[typeName][(int)VehicleDatabase.sVehicleTypes[track.vehicleTypeName]]
            && VehicleDatabase.sVehicleCanBeEngaged[track.vehicleTypeName][(int)VehicleDatabase.sVehicleTypes[typeName]])
        {
            base.OnNewTrack(track, source);
            this.StartCoroutine(TrackTactic(track));
        }
        else
        {
            sensorCtrl.tracksDetected.Remove(track);
        }
    }

    private IEnumerator TrackTactic(Track track)
    {
        GuidanceModule guidance = GetComponent<GuidanceModule>();
        if (guidance != null)
        {
            while (track != null && track.isLost == false && track.identification != TrackId.Neutrual)
            {
                // If original track is lost or missing, we will change our target.
                // If original track is identified as neutral (counter-measures will all be neutral).
                // If the new track is very close to the original track, the missile might consider that as the true target.
                if (guidance.targetTrack == null || guidance.targetTrack.isLost || guidance.targetTrack.identification == TrackId.Neutrual)
                {
                    if (guidance.targetTrack != null)
                    {
                        // Sort priority by distance error.
                        yield return new WaitForSeconds(Mathf.Min(Vector3.Distance(guidance.targetTrack.predictedPosition, track.predictedPosition) * 0.0004f, 4f));
                    }
                    if (guidance.targetTrack == null || guidance.targetTrack.isLost || guidance.targetTrack.identification == TrackId.Neutrual)
                    {
                        if (UnityEngine.Random.Range(0f, 100f) > 20f)
                        {
                            guidance.SetupGuidance(track, null);
                            WarheadModule warhead = GetComponent<WarheadModule>();
                            if (warhead != null)
                                warhead.SetupTarget(track);
                            yield return new WaitForSeconds(0.2f);
                        }
                    }
                }
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.5f));
            }
        }
    }
}
