using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TorpedoGuidanceModule : GuidanceModule
{
    public float diveAlt = 50f;
    public float kP = 1.0f;
    public float enableDistance = 12000f;

    private float origMaxSpeed;
    private float origTurnRadius;

    private bool hasInitiallyDisabled = false;
    private bool hasEnabled = false;
    private bool hasEnteredWater = false;

    private float lastUpdatePosY;

    private bool isBeingRemotelyControlled = false;

    void Start()
    {
        origMaxSpeed = self.maxSpeed;
        origTurnRadius = self.locomotor.turnRadius;
        lastUpdatePosY = self.position.y;
    }

    public override void OnFixedUpdate(float deltaTime)
    {
        if (self.fuel <= 0) return;

        // Guidance Logic.
        if (targetTrack == null)
        {
            self.locomotor.orderedPitch = Mathf.Clamp((diveAlt - self.position.y) / self.speed * kP, -30f, 30f);
            if (isBeingRemotelyControlled == false)
            {
                self.locomotor.orderedSpeed = self.maxSpeed * 0.8f;
                if (guidanceParameter is Vector3)
                {
                    Vector3 targetPos = (Vector3)guidanceParameter;
                    self.locomotor.orderedCourse = Mathf.Rad2Deg * Mathf.Atan2(targetPos.x - self.position.x, targetPos.z - self.position.z);
                }
            }
        }
        else
        {
            float eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(0)) / self.speed;
            eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / self.speed;
            eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / self.speed;
            eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / self.speed;
            eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / self.speed;
            eta = Vector3.Distance(self.position, targetTrack.predictedPositionAtTime(eta)) / self.speed;

            float expectedPitch = Mathf.Rad2Deg * Mathf.Atan2(Mathf.Min(0f, targetTrack.predictedPositionAtTime(eta).y) - self.position.y, Mathf.Sqrt(Mathf.Pow(targetTrack.predictedPositionAtTime(eta).z - self.position.z, 2) + Mathf.Pow(targetTrack.predictedPositionAtTime(eta).x - self.position.x, 2)));
            self.locomotor.orderedPitch = expectedPitch;
            float expectedCourse = Mathf.Rad2Deg * Mathf.Atan2(targetTrack.predictedPositionAtTime(eta).x - self.position.x, targetTrack.predictedPositionAtTime(eta).z - self.position.z);
            self.locomotor.orderedCourse = expectedCourse;
            self.locomotor.orderedSpeed =
                (targetTrack.identification == TrackId.Hostile)
                    ? self.maxSpeed * 1.0f
                    : (targetTrack.identification == TrackId.AssumedHostile)
                        ? self.maxSpeed * 0.95f : self.maxSpeed * 0.85f;
        }

        self.GetComponentsInChildren<ParticleEmitter>().ToList().ForEach((ParticleEmitter pe) => { pe.emit = (self.position.y < 0); });

        // Auto Enable Logic.
        if (hasInitiallyDisabled == false)
        {
            self.sensorCtrl.ToggleAll(false);
            hasInitiallyDisabled = true;
        }

        if (targetTrack == null)
        {
            if (guidanceParameter is Vector3)
            {
                if (Vector3.Distance((Vector3)guidanceParameter, self.position) < enableDistance)
                {
                    if (hasEnabled == false)
                    {
                        self.sensorCtrl.ToggleAll(true);
                        hasEnabled = true;
                    }
                }
            }
        }
        else
        {
            if (Vector3.Distance(targetTrack.predictedPosition, self.position) < enableDistance)
            {
                if (hasEnabled == false)
                {
                    self.sensorCtrl.ToggleAll(true);
                    hasEnabled = true;
                }
            }            
        }

        // Entering Water Logic.
        if (self.position.y < 0 && lastUpdatePosY >= 0)
        {
            self.sensorCtrl.ToggleAll(true, true);
            self.maxSpeed = origMaxSpeed;
            self.locomotor.turnRadius = origTurnRadius;
            if (self.speed > self.maxSpeed)
                self.speed -= self.locomotor.dragAcceleration * deltaTime * 10f;
        }

        // Free Falling Logic / Swim Out of Water.
        if (self.position.y >= 0)
        {
            self.maxSpeed = 20f;
            if (self.speed > self.maxSpeed)
                self.speed -= self.locomotor.dragAcceleration * deltaTime * 60f;
            self.locomotor.turnRadius = 100f;
            self.locomotor.orderedPitch = -90;
            self.locomotor.orderedSpeed = self.maxSpeed;
            self.fuel = self.maxFuel;
            self.sensorCtrl.ToggleAll(false, true);
        }

        lastUpdatePosY = self.position.y;
    }

    private string newDiveAltStr = null;

    public override void OnGuidanceControllerGUI()
    {
        if (newDiveAltStr == null)
            newDiveAltStr = diveAlt.ToString("F0");

        GUILayout.Label("Control:");
        if(isBeingRemotelyControlled)
            isBeingRemotelyControlled = GUILayout.Toggle(isBeingRemotelyControlled, "Remote Control", "button", GUILayout.Height(15f));
        else
            GUILayout.Label("Remote Control", "button", GUILayout.Height(15f));
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Course (" + self.course.ToString("F0") + "->" + (self.locomotor.orderedCourse < 0 ? self.locomotor.orderedCourse + 360f : (self.locomotor.orderedCourse > 360f ? self.locomotor.orderedCourse - 360f : self.locomotor.orderedCourse)).ToString("F0") + ") :");
            float origOrderedCourse = self.locomotor.orderedCourse;
            self.locomotor.orderedCourse = GUILayout.HorizontalSlider(self.locomotor.orderedCourse, self.locomotor.orderedCourse - 180f, self.locomotor.orderedCourse + 180f);
            if (origOrderedCourse != self.locomotor.orderedCourse)
                isBeingRemotelyControlled = true;
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Speed (" + self.speed.ToString("F0") + "->" + self.locomotor.orderedSpeed.ToString("F0") + ") :");
            float origOrderedSpeed = self.locomotor.orderedSpeed;
            self.locomotor.orderedSpeed = GUILayout.HorizontalSlider(self.locomotor.orderedSpeed, Mathf.Max(0f, self.locomotor.orderedSpeed - 10f), Mathf.Min(self.maxSpeed, self.locomotor.orderedSpeed + 10f));
            if (origOrderedSpeed != self.locomotor.orderedSpeed)
                isBeingRemotelyControlled = true;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            newDiveAltStr = GUILayout.TextField(newDiveAltStr);
            bool updateDiveAlt = GUILayout.Button("Set Alt", "button", GUILayout.Width(60f));
            if (updateDiveAlt)
            {
                diveAlt = Convert.ToSingle(newDiveAltStr);
            }
        }
        GUILayout.EndHorizontal();

        if(GUILayout.Button("Drop Target", GUILayout.Height(15f)))
        {
            targetTrack = null;
            self.sensorCtrl.tracksDetected.Clear();
            self.sensorCtrl.tracksToBeAssignedToTracker.Clear();
            GetComponent<Vehicle>().StopAllCoroutines();
        }
    }
}