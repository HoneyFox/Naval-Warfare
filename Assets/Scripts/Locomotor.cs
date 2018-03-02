using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Locomotor : MonoBehaviour
{
    public float orderedSpeed = 0f;
    public float orderedCourse = 0f;
    public float orderedPitch = 0f;
    private float orderedBank = 0f;
    public float turnRadius = 1000f;
    public bool useBTT = false;
    public float bttFactor = 1f;
    public float maxBankAngle = 0f;
    public float rollRate = 300f;
    public float maxTurnAcceleration = 100f;
    public float linearAcceleration = 10f;
    public float dragAcceleration = 1f;

    public Vehicle self 
    {
        get { return this.gameObject.GetComponent<Vehicle>(); }
    }

    void FixedUpdate() 
    {
        Steer(Time.fixedDeltaTime);
    }

    public virtual void Steer(float deltaTime)
    {
        if (orderedSpeed > self.speed)
            self.speed = Mathf.MoveTowards(self.speed, Mathf.Clamp(orderedSpeed, self.minSpeed, self.maxSpeed), linearAcceleration * deltaTime);
        else
            self.speed = Mathf.MoveTowards(self.speed, Mathf.Clamp(orderedSpeed, self.minSpeed, self.maxSpeed), dragAcceleration * SceneManager.instance.altitudeDragCurve.Evaluate(self.position.y) * deltaTime);

        float oldCourse = self.course;
        self.course = Mathf.MoveTowardsAngle(self.course, orderedCourse, Mathf.Rad2Deg * Mathf.Min(self.speed / turnRadius, maxTurnAcceleration / self.speed) * deltaTime);
        if (self.course > 360f) 
            self.course -= 360f;
        if (self.course < 0f)
            self.course += 360f;

        float courseDiff = self.course - oldCourse;
        if (courseDiff > 180f) courseDiff -= 360f;
        if (courseDiff < -180f) courseDiff += 360f;
        if (useBTT)
            orderedBank = -Mathf.Atan2(self.speed * (courseDiff / 180 * Mathf.PI) / deltaTime, 9.8f) * bttFactor * Mathf.Rad2Deg * Mathf.Sign(maxBankAngle);
        else
            orderedBank = 0f;

        self.pitch = Mathf.MoveTowardsAngle(self.pitch, Mathf.Clamp(orderedPitch, -90f, 90f), Mathf.Rad2Deg * Mathf.Min(self.speed / turnRadius, maxTurnAcceleration / self.speed) * deltaTime);
        if (self.pitch > 180f)
            self.pitch -= 360f;
        self.pitch = Mathf.Clamp(self.pitch, -90f, 90f);
        
        self.bank = Mathf.LerpAngle(self.bank, Mathf.MoveTowardsAngle(self.bank, Mathf.Clamp(orderedBank, -Mathf.Abs(maxBankAngle), Mathf.Abs(maxBankAngle)), rollRate * deltaTime), 0.3f);
        if (self.bank > 180f)
            self.bank -= 360f;
        self.bank = Mathf.Clamp(self.bank, -90f, 90f);

        self.position += self.velocity * deltaTime;
        self.fuel -= self.fuelConsumptionRate * self.fuelConsumptionCurve.Evaluate(Mathf.InverseLerp(self.minSpeed, self.maxSpeed, self.speed)) * deltaTime;
        if (self.fuel <= 0)
        {
            self.fuel = 0f;
            OnFuelExhausted();
        }
        if (self.isDead)
        {
            OnDead();
        }
    }

    public virtual void OnFuelExhausted()
    {
        orderedSpeed = 0f;
    }

    public virtual void OnDead()
    {

    }
}
