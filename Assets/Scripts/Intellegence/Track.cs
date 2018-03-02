using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum TrackId
{
    Hostile = -2,
    AssumedHostile = -1,
    Unknown = 0,
    AssumedFriendly = 1,
    Neutrual = 2,
    Friendly = 2,
}

public class Track
{
    public Vehicle owner;
    public Vehicle target;
    public float timeOfDetection;
    public float timeOfLastUpdate;
    public float age
    {
        get { return Time.time - timeOfLastUpdate; }
    }
    public Vector3 position;
    public Vector3 velocity;

    public bool isLost { get { return age > target.maxAge * owner.sensorCtrl.ageFactor; } }

    public Track(Vehicle owner, Vehicle target, TrackId identification = TrackId.Unknown)
    {
        this.owner = owner;
        this.target = target;
        this.identification = identification;
        vehicleTypeName = target.typeName;
        timeOfLastUpdate = Time.time;
        timeOfDetection = Time.time;
    }

    public void UpdateTrack(Vector3 position, Vector3 velocity, TrackId identification)
    {
        timeOfLastUpdate = Time.time;
        this.position = position;
        this.velocity = velocity;
        this.identification = identification;
    }

    public void UpdateTrack(Vector3 position, Vector3 velocity, float timeOfUpdate, TrackId identification)
    {
        timeOfLastUpdate = timeOfUpdate;
        this.position = position;
        this.velocity = velocity;
        this.identification = identification;
    }

    public Vector3 predictedPosition { get { return position + velocity * age; } }

    public Vector3 predictedPositionAtTime(float time) { return predictedPosition + velocity * time; }

    public TrackId identification = TrackId.Unknown;

    public string vehicleTypeName = "";
}
