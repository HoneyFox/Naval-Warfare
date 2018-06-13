using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Airstrip : MonoBehaviour
{
    public Vehicle self;
    
    public Vehicle vehicleAttached = null;
    public float vehicleProgress = 0;

    public bool vehicleIsDeploying = false;
    public bool vehicleIsLaunching = false;
    public bool vehicleIsLanding = false;
    public bool vehicleIsUndeploying = false;

    public string takeOffMethod;
    public string landMethod;

    public bool canLaunchAir = false;
    public bool canLandAir = false;
    
    public bool canLaunchSurf = false;
    public bool canLandSurf = false;

    public bool canLaunchSub = false;
    public bool canLandSub = false;

    public Transform deployStartPoint;
    public float deployRate = 10f;

    public float approachDistanceThreshold = 100f;
    public Transform launchStartPoint;
    public Transform launchEndPoint;
    public float launchRate = 10f;
    
    public Transform landStartPoint;
    public Transform landEndPoint;
    public float landRate = 10f;

    public Transform undeployEndPoint;
    public float undeployRate = 10f;

    void Start()
    {

    }

    public bool CanTakeOff(string vehicleName, bool considerOccupation = true)
    {
        if (vehicleAttached != null && considerOccupation) return false;
        if (enabled == false) return false;

        if (self.airstripCtrl.vehicles.Contains(vehicleName) == false || self.airstripCtrl.vehicleCounts[self.airstripCtrl.vehicles.IndexOf(vehicleName)] == 0) return false;
        if (canLaunchAir == false && Vehicle.sVehicleTypes[vehicleName] == Vehicle.VehicleType.Air) return false;
        if (canLaunchSurf == false && Vehicle.sVehicleTypes[vehicleName] == Vehicle.VehicleType.Surf) return false;
        if (canLaunchSub == false && Vehicle.sVehicleTypes[vehicleName] == Vehicle.VehicleType.Sub) return false;
        if (Vehicle.sVehicleTakeOffMethods.ContainsKey(vehicleName) == false || Vehicle.sVehicleTakeOffMethods[vehicleName].Any((string value) => value == takeOffMethod) == false) return false;

        return true;
    }

    public bool PrepareForTakeOff(string vehicleName)
    {
        if (vehicleAttached != null) return false;
        if (enabled == false) return false;

        if(self.airstripCtrl.vehicles.Contains(vehicleName) == false || self.airstripCtrl.vehicleCounts[self.airstripCtrl.vehicles.IndexOf(vehicleName)] == 0) return false;
        if (canLaunchAir == false && Vehicle.sVehicleTypes[vehicleName] == Vehicle.VehicleType.Air) return false;
        if (canLaunchSurf == false && Vehicle.sVehicleTypes[vehicleName] == Vehicle.VehicleType.Surf) return false;
        if (canLaunchSub == false && Vehicle.sVehicleTypes[vehicleName] == Vehicle.VehicleType.Sub) return false;
        if (Vehicle.sVehicleTakeOffMethods.ContainsKey(vehicleName) == false || Vehicle.sVehicleTakeOffMethods[vehicleName].Any((string value) => value == takeOffMethod) == false) return false;

        GameObject vehicle = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths[vehicleName]);

        vehicleAttached = vehicle.GetComponent<Vehicle>();
        vehicleAttached.enabled = false;
        vehicle.transform.parent = self.transform;
        vehicle.transform.position = deployStartPoint.position;

        vehicleAttached.side = self.side;
        self.airstripCtrl.vehicleCounts[self.airstripCtrl.vehicles.IndexOf(vehicleName)]--;

        vehicleIsDeploying = true;
        vehicleProgress = 0f;

        return true;
    }

    public void TakeOff()
    {
        if (enabled == false) return;
        if (vehicleAttached == null) return;
        if (Vehicle.sVehicleIsDeployOnly.ContainsKey(vehicleAttached.typeName) && Vehicle.sVehicleIsDeployOnly[vehicleAttached.typeName] == true) return;
        if (vehicleIsDeploying == true && vehicleIsUndeploying == false && vehicleProgress == 100f)
        {
            vehicleProgress = 0f;
            vehicleIsDeploying = false;
            vehicleIsLaunching = true;
        }
    }

    public void CancelTakeOff()
    {
        if (vehicleAttached == null) return;

        vehicleIsUndeploying = true;
    }

    public bool CanLand(Vehicle vehicle, bool considerOccupation = true)
    {
        if (vehicleAttached != null && vehicleAttached != vehicle && considerOccupation) return false;
        if (enabled == false) return false;

        if (canLandAir == false && Vehicle.sVehicleTypes[vehicle.typeName] == Vehicle.VehicleType.Air) return false;
        if (canLandSurf == false && Vehicle.sVehicleTypes[vehicle.typeName] == Vehicle.VehicleType.Surf) return false;
        if (canLandSub == false && Vehicle.sVehicleTypes[vehicle.typeName] == Vehicle.VehicleType.Sub) return false;
        if (Vehicle.sVehicleLandMethods.ContainsKey(vehicle.typeName) == false || Vehicle.sVehicleLandMethods[vehicle.typeName].Any((string value) => value == landMethod) == false) return false;

        return true;
    }

    public bool Land(Vehicle vehicle)
    {
        if (vehicleAttached != null && vehicleAttached != vehicle) return false;
        if (enabled == false) return false;

        if (canLandAir == false && Vehicle.sVehicleTypes[vehicle.typeName] == Vehicle.VehicleType.Air) return false;
        if (canLandSurf == false && Vehicle.sVehicleTypes[vehicle.typeName] == Vehicle.VehicleType.Surf) return false;
        if (canLandSub == false && Vehicle.sVehicleTypes[vehicle.typeName] == Vehicle.VehicleType.Sub) return false;
        if (Vehicle.sVehicleLandMethods.ContainsKey(vehicle.typeName) == false || Vehicle.sVehicleLandMethods[vehicle.typeName].Any((string value) => value == landMethod) == false) return false;

        vehicleAttached = vehicle;
        if(Vector3.Distance(vehicle.transform.position, landStartPoint.position) < approachDistanceThreshold)
        {
            vehicleAttached.enabled = false;
            vehicleAttached.GetComponentsInChildren<LineRenderer>().Any((LineRenderer lr) => { MonoBehaviour.Destroy(lr); return false; });
            vehicleIsLanding = true;
            vehicleProgress = 0f;
            return true;
        }
        else
        {
            // The aircraft should move towards the start point first.
            return false;
        }
    }

    void FixedUpdate()
    {
        if (vehicleAttached == null)
        {
            vehicleIsDeploying = vehicleIsLaunching = vehicleIsLanding = vehicleIsUndeploying = false;
            return;
        }

        if (vehicleIsDeploying == true && vehicleIsUndeploying == false)
        {
            vehicleProgress += deployRate * Time.fixedDeltaTime;
            vehicleProgress = Mathf.Clamp(vehicleProgress, 0f, 100f);
            vehicleAttached.transform.position = Vector3.Lerp(deployStartPoint.position, launchStartPoint.position, vehicleProgress / 100f);
            vehicleAttached.transform.rotation = Quaternion.Lerp(deployStartPoint.rotation, launchStartPoint.rotation, vehicleProgress / 100f);
            /*
            if(vehicleProgress == 100.0f)
            {
                // Prepare to launch the vehicle.
                vehicleProgress = 0f;
                vehicleIsDeploying = false;
                vehicleIsLaunching = true;
            }
            */
        }
        else if (vehicleIsLaunching)
        {
            vehicleProgress += launchRate * Time.fixedDeltaTime;
            vehicleProgress = Mathf.Clamp(vehicleProgress, 0f, 100f);
            vehicleAttached.transform.position = Vector3.Lerp(launchStartPoint.position, launchEndPoint.position, vehicleProgress / 100f);
            vehicleAttached.transform.rotation = Quaternion.Lerp(launchStartPoint.rotation, launchEndPoint.rotation, vehicleProgress / 100f);
            if(vehicleProgress == 100.0f)
            {
                // The vehicle is taking off. Detach the vehicle.
                vehicleAttached.transform.parent = self.transform.parent;
                vehicleAttached.position = vehicleAttached.transform.position;
                Vector3 launchVector = launchEndPoint.position + launchEndPoint.forward * 100f;
                float course = Mathf.Rad2Deg * Mathf.Atan2(launchVector.x - launchEndPoint.position.x, launchVector.z - launchEndPoint.position.z);
                float pitch = Mathf.Rad2Deg * Mathf.Atan2(launchVector.y - launchEndPoint.position.y, Mathf.Sqrt(Mathf.Pow(launchVector.z - launchEndPoint.position.z, 2) + Mathf.Pow(launchVector.x - launchEndPoint.position.x, 2)));
                vehicleAttached.course = vehicleAttached.locomotor.orderedCourse = course;
                vehicleAttached.pitch = pitch;
                vehicleAttached.speed = (Vector3.Distance(launchEndPoint.position, launchStartPoint.position) / (100f / launchRate));
                vehicleAttached.enabled = true;
                if(SceneManager.instance.vehicles.Contains(vehicleAttached) == false)
                    SceneManager.instance.vehicles.Add(vehicleAttached);
                vehicleAttached.OnTakeOff(self);
                vehicleAttached = null;
                vehicleIsLaunching = false;
                vehicleProgress = 0f;
            }
        }
        else if (vehicleIsLanding)
        {
            vehicleProgress += landRate * Time.fixedDeltaTime;
            vehicleProgress = Mathf.Clamp(vehicleProgress, 0f, 100f);
            vehicleAttached.transform.position = Vector3.Lerp(landStartPoint.position, landEndPoint.position, vehicleProgress / 100f);
            vehicleAttached.transform.rotation = Quaternion.Lerp(landStartPoint.rotation, landEndPoint.rotation, vehicleProgress / 100f);
            if(vehicleProgress == 100.0f)
            {
                vehicleIsLanding = false;
                vehicleIsUndeploying = true;
                vehicleProgress = 0f;
            }
        }
        else if (vehicleIsUndeploying == true && vehicleIsDeploying == false)
        {
            vehicleProgress += undeployRate * Time.fixedDeltaTime;
            vehicleProgress = Mathf.Clamp(vehicleProgress, 0f, 100f);
            vehicleAttached.transform.position = Vector3.Lerp(landEndPoint.position, undeployEndPoint.position, vehicleProgress / 100f);
            vehicleAttached.transform.rotation = Quaternion.Lerp(landEndPoint.rotation, undeployEndPoint.rotation, vehicleProgress / 100f);
            if (vehicleProgress == 100f)
            {
                if (self.airstripCtrl.vehicles.Contains(vehicleAttached.typeName))
                {
                    self.airstripCtrl.vehicleCounts[self.airstripCtrl.vehicles.IndexOf(vehicleAttached.typeName)]++;
                }
                else
                {
                    self.airstripCtrl.vehicles.Add(vehicleAttached.typeName);
                    self.airstripCtrl.vehicleCounts.Add(1);
                }
                SceneManager.instance.GetComponent<VehicleSelector>().OnVehicleDead(vehicleAttached);
                if (SceneManager.instance.vehicles.Contains(vehicleAttached) == true)
                    SceneManager.instance.vehicles.Remove(vehicleAttached);
                GameObject.Destroy(vehicleAttached.gameObject);
                vehicleAttached = null;
                vehicleIsUndeploying = false;
                vehicleProgress = 0f;
            }
        }
        else if (vehicleIsDeploying == true && vehicleIsUndeploying == true)
        {
            vehicleProgress -= undeployRate * Time.fixedDeltaTime;
            vehicleProgress = Mathf.Clamp(vehicleProgress, 0f, 100f);
            vehicleAttached.transform.position = Vector3.Lerp(undeployEndPoint.position, launchStartPoint.position, vehicleProgress / 100f);
            vehicleAttached.transform.rotation = Quaternion.Lerp(undeployEndPoint.rotation, launchStartPoint.rotation, vehicleProgress / 100f);
            if (vehicleProgress == 0f)
            {
                if (self.airstripCtrl.vehicles.Contains(vehicleAttached.typeName))
                {
                    self.airstripCtrl.vehicleCounts[self.airstripCtrl.vehicles.IndexOf(vehicleAttached.typeName)]++;
                }
                else
                {
                    self.airstripCtrl.vehicles.Add(vehicleAttached.typeName);
                    self.airstripCtrl.vehicleCounts.Add(1);
                }
                if (SceneManager.instance.vehicles.Contains(vehicleAttached) == true)
                    SceneManager.instance.vehicles.Remove(vehicleAttached);
                GameObject.Destroy(vehicleAttached.gameObject);
                vehicleAttached = null;
                vehicleIsDeploying = false;
                vehicleIsUndeploying = false;
            }
        }
    }
}
