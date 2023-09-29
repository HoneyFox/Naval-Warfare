using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ASM : Vehicle
{
    public override void OnNewTrack(Track track, string source)
    {
        if (VehicleDatabase.sVehicleTypes[track.vehicleTypeName] != VehicleType.Air
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
                if (guidance.targetTrack == null || guidance.targetTrack.isLost || guidance.targetTrack.identification == TrackId.Neutrual ||
                    (UnityEngine.Random.Range(0f, 100f) > 25f && Vector3.Distance(guidance.targetTrack.predictedPosition, track.predictedPosition) < 200f))
                {
                    if (guidance.guidanceParameter != null)
                    {
                        Vector3 predictedPosition = (Vector3)guidance.guidanceParameter;
                        float positionError = Vector3.Distance(predictedPosition, track.predictedPosition);
                        yield return new WaitForSeconds(Mathf.Min(positionError * 0.0002f, 5.0f));
                        if (guidance.targetTrack == null || guidance.targetTrack.isLost || guidance.targetTrack.identification == TrackId.Neutrual)
                        {
                            guidance.SetupGuidance(track, null);
                            WarheadModule warhead = GetComponent<WarheadModule>();
                            if (warhead != null)
                                warhead.SetupTarget(track);
                            yield return new WaitForSeconds(4f);
                            yield return null;
                        }
                    }
                    else
                    {
                        guidance.SetupGuidance(track, null);
                        WarheadModule warhead = GetComponent<WarheadModule>();
                        if (warhead != null)
                            warhead.SetupTarget(track);
                        yield return new WaitForSeconds(4f);
                        yield return null;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
                yield return null;
            }
        }
    }

    public override void OnVehicleControllerGUI()
    {
        if (isDead)
            return;

        if (this.GetComponent<GuidanceModule>() == null || this.GetComponent<GuidanceModule>().canBeRemotelyControlled == false)
            return;

        this.GetComponent<GuidanceModule>().OnGuidanceControllerGUI();
    }
}
