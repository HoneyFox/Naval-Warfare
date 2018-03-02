using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SceneManager : MonoSingleton<SceneManager> 
{

    public List<Vehicle> vehicles = new List<Vehicle>();

    public AnimationCurve altitudeDragCurve;

	// Use this for initialization
	void Start () 
    {
	    // Test.
        GameObject obj1 = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths["Arleigh Burke DDG"]);
        Vehicle v1 = obj1.GetComponent<Vehicle>();
        v1.side = 0;
        obj1.transform.parent = SceneManager.instance.transform.Find("Side 0");
        v1.course = 270f;
        obj1.GetComponent<Locomotor>().orderedCourse = 270f;
        v1.position = new Vector3(42000f, 0f, 1000f);
        obj1.transform.localPosition = new Vector3(42000f, 0f, 1000f);
        obj1.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);

        GameObject obj2 = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths["Arleigh Burke DDG"]);
        Vehicle v2 = obj2.GetComponent<Vehicle>();
        v2.side = 1;
        obj2.transform.parent = SceneManager.instance.transform.Find("Side 1");
        v2.course = 90f;
        obj2.GetComponent<Locomotor>().orderedCourse = 90f;
        v2.position = new Vector3(-42000f, 0f, -1000f);
        obj2.transform.localPosition = new Vector3(-42000f, 0f, -1000f);
        obj2.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        v2.autoEngageAirTracks = v2.autoEngageSurfTracks = v2.autoEngageSubTracks = true;

        vehicles.Add(v1);
        vehicles.Add(v2);

        GameObject obj3 = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths["OH Perry FFG"]);
        Vehicle v3 = obj3.GetComponent<Vehicle>();
        v3.side = 0;
        obj3.transform.parent = SceneManager.instance.transform.Find("Side 0");
        v3.course = 270f;
        obj3.GetComponent<Locomotor>().orderedCourse = 270f;
        v3.position = new Vector3(28000f, 0f, 2000f);
        obj3.transform.localPosition = new Vector3(28000f, 0f, 2000f);
        obj3.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);

        GameObject obj4 = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths["OH Perry FFG"]);
        Vehicle v4 = obj4.GetComponent<Vehicle>();
        v4.side = 1;
        obj4.transform.parent = SceneManager.instance.transform.Find("Side 1");
        v4.course = 90f;
        obj4.GetComponent<Locomotor>().orderedCourse = 90f;
        v4.position = new Vector3(-28000f, 0f, -2000f);
        obj4.transform.localPosition = new Vector3(-28000f, 0f, -2000f);
        obj4.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        v4.autoEngageAirTracks = v4.autoEngageSurfTracks = v4.autoEngageSubTracks = true;

        vehicles.Add(v3);
        vehicles.Add(v4);
        
        GameObject obj5 = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths["Kilo SSK"]);
        Vehicle v5 = obj5.GetComponent<Vehicle>();
        v5.side = 0;
        obj5.transform.parent = SceneManager.instance.transform.Find("Side 0");
        v5.course = 270f;
        obj5.GetComponent<Locomotor>().orderedCourse = 270f;
        v5.position = new Vector3(9000f, -50f, 4000f);
        obj5.transform.localPosition = new Vector3(9000f, -50f, 4000f);
        obj5.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);

        GameObject obj6 = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths["Kilo SSK"]);
        Vehicle v6 = obj6.GetComponent<Vehicle>();
        v6.side = 1;
        obj6.transform.parent = SceneManager.instance.transform.Find("Side 1");
        v6.course = 90f;
        obj6.GetComponent<Locomotor>().orderedCourse = 90f;
        v6.position = new Vector3(-9000f, -50f, -4000f);
        obj6.transform.localPosition = new Vector3(-9000f, -50f, -4000f);
        obj6.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        v6.autoEngageAirTracks = v6.autoEngageSurfTracks = v6.autoEngageSubTracks = true;

        vehicles.Add(v5);
        vehicles.Add(v6);

        GameObject obj7 = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths["Wasp LHD"]);
        Vehicle v7 = obj7.GetComponent<Vehicle>();
        v7.side = 0;
        obj7.transform.parent = SceneManager.instance.transform.Find("Side 0");
        v7.course = 270f;
        obj7.GetComponent<Locomotor>().orderedCourse = 270f;
        v7.position = new Vector3(44000f, 0f, 1500f);
        obj7.transform.localPosition = new Vector3(44000f, 0f, 1500f);
        obj7.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);

        GameObject obj8 = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths["Wasp LHD"]);
        Vehicle v8 = obj8.GetComponent<Vehicle>();
        v8.side = 1;
        obj8.transform.parent = SceneManager.instance.transform.Find("Side 1");
        v8.course = 90f;
        obj8.GetComponent<Locomotor>().orderedCourse = 90f;
        v8.position = new Vector3(-44000f, 0f, -1500f);
        obj8.transform.localPosition = new Vector3(-44000f, 0f, -1500f);
        obj8.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        v8.autoEngageAirTracks = v8.autoEngageSurfTracks = v8.autoEngageSubTracks = true;

        vehicles.Add(v7);
        vehicles.Add(v8);

        GameObject obj9 = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths["Arleigh Burke DDG"]);
        Vehicle v9 = obj9.GetComponent<Vehicle>();
        v9.side = 0;
        obj9.transform.parent = SceneManager.instance.transform.Find("Side 0");
        v9.course = 270f;
        obj9.GetComponent<Locomotor>().orderedCourse = 270f;
        v9.position = new Vector3(30000f, 0f, 1000f);
        obj9.transform.localPosition = new Vector3(43000f, 0f, 2000f);
        obj9.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);

        GameObject obj10 = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths["Arleigh Burke DDG"]);
        Vehicle v10 = obj10.GetComponent<Vehicle>();
        v10.side = 1;
        obj10.transform.parent = SceneManager.instance.transform.Find("Side 1");
        v10.course = 90f;
        obj10.GetComponent<Locomotor>().orderedCourse = 90f;
        v10.position = new Vector3(-30000f, 0f, -1000f);
        obj10.transform.localPosition = new Vector3(-43000f, 0f, -2000f);
        obj10.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        v10.autoEngageAirTracks = v10.autoEngageSurfTracks = v10.autoEngageSubTracks = true;

        vehicles.Add(v9);
        vehicles.Add(v10);


        vehicles[0].GetComponents<DatalinkModule>().Any((DatalinkModule dl) =>
        {
            dl.AddReceiver(vehicles[2]);
            dl.AddReceiver(vehicles[4]);
            dl.AddReceiver(vehicles[6]);
            dl.AddReceiver(vehicles[8]);
            return false;
        });
        vehicles[1].GetComponents<DatalinkModule>().Any((DatalinkModule dl) =>
        {
            dl.AddReceiver(vehicles[3]);
            dl.AddReceiver(vehicles[5]);
            dl.AddReceiver(vehicles[7]);
            dl.AddReceiver(vehicles[9]);
            return false;
        });

        vehicles[2].GetComponents<DatalinkModule>().Any((DatalinkModule dl) =>
        {
            dl.AddReceiver(vehicles[0]);
            dl.AddReceiver(vehicles[4]);
            dl.AddReceiver(vehicles[6]);
            dl.AddReceiver(vehicles[8]);
            return false;
        });
        vehicles[3].GetComponents<DatalinkModule>().Any((DatalinkModule dl) =>
        {
            dl.AddReceiver(vehicles[1]);
            dl.AddReceiver(vehicles[5]);
            dl.AddReceiver(vehicles[7]);
            dl.AddReceiver(vehicles[9]);
            return false;
        });

        vehicles[4].GetComponents<DatalinkModule>().Any((DatalinkModule dl) =>
        {
            dl.AddReceiver(vehicles[0]);
            dl.AddReceiver(vehicles[2]);
            dl.AddReceiver(vehicles[6]);
            dl.AddReceiver(vehicles[8]);
            return false;
        });
        vehicles[5].GetComponents<DatalinkModule>().Any((DatalinkModule dl) =>
        {
            dl.AddReceiver(vehicles[1]);
            dl.AddReceiver(vehicles[3]);
            dl.AddReceiver(vehicles[7]);
            dl.AddReceiver(vehicles[9]);
            return false;
        });

        vehicles[6].GetComponents<DatalinkModule>().Any((DatalinkModule dl) =>
        {
            dl.AddReceiver(vehicles[0]);
            dl.AddReceiver(vehicles[2]);
            dl.AddReceiver(vehicles[4]);
            dl.AddReceiver(vehicles[8]);
            return false;
        });
        vehicles[7].GetComponents<DatalinkModule>().Any((DatalinkModule dl) =>
        {
            dl.AddReceiver(vehicles[1]);
            dl.AddReceiver(vehicles[3]);
            dl.AddReceiver(vehicles[5]);
            dl.AddReceiver(vehicles[9]);
            return false;
        });

        vehicles[8].GetComponents<DatalinkModule>().Any((DatalinkModule dl) =>
        {
            dl.AddReceiver(vehicles[0]);
            dl.AddReceiver(vehicles[2]);
            dl.AddReceiver(vehicles[4]);
            dl.AddReceiver(vehicles[6]);
            return false;
        });
        vehicles[9].GetComponents<DatalinkModule>().Any((DatalinkModule dl) =>
        {
            dl.AddReceiver(vehicles[1]);
            dl.AddReceiver(vehicles[3]);
            dl.AddReceiver(vehicles[5]);
            dl.AddReceiver(vehicles[7]);
            return false;
        });
	}
	
	// Update is called once per frame
	void Update () 
    {
        // Since those FFGs are far away from each other, they cannot detect each other by themselves.
        // Here we simulate the military satelite data-link by continuously providing hostile FFG track information after T+10s.

        /*
        if (Time.time > 10f)
        {
            // Fill tracks 1 & 3 to vehicle 0.
            if (vehicles[0].sensorCtrl.tracksDetected.Exists((Track trk) => trk.target == vehicles[1]) == false)
            {
                Track newTrack = new Track(vehicles[1], TrackId.Unknown);
                newTrack.UpdateTrack(vehicles[1].position, vehicles[1].velocity, TrackId.Hostile);
                vehicles[0].sensorCtrl.AddTrack(newTrack);
                vehicles[0].OnNewTrack(newTrack, "Satelite Uplink");
            }
            else
            {
                Track existingTrack = vehicles[0].sensorCtrl.tracksDetected.Find((Track trk) => trk.target == vehicles[1]);
                existingTrack.UpdateTrack(vehicles[1].position, vehicles[1].velocity, existingTrack.identification);
            }
            if (vehicles[0].sensorCtrl.tracksDetected.Exists((Track trk) => trk.target == vehicles[3]) == false)
            {
                Track newTrack = new Track(vehicles[3], TrackId.Unknown);
                newTrack.UpdateTrack(vehicles[3].position, vehicles[3].velocity, TrackId.Hostile);
                vehicles[0].sensorCtrl.AddTrack(newTrack);
                vehicles[0].OnNewTrack(newTrack, "Satelite Uplink");
            }
            else
            {
                Track existingTrack = vehicles[0].sensorCtrl.tracksDetected.Find((Track trk) => trk.target == vehicles[3]);
                existingTrack.UpdateTrack(vehicles[3].position, vehicles[3].velocity, existingTrack.identification);
            }

            // Fill tracks 0 & 2 to vehicle 1.
            if (vehicles[1].sensorCtrl.tracksDetected.Exists((Track trk) => trk.target == vehicles[0]) == false)
            {
                Track newTrack = new Track(vehicles[0], TrackId.Unknown);
                newTrack.UpdateTrack(vehicles[0].position, vehicles[0].velocity, TrackId.Hostile);
                vehicles[1].sensorCtrl.AddTrack(newTrack);
                vehicles[1].OnNewTrack(newTrack, "Satelite Uplink");
            }
            else
            {
                Track existingTrack = vehicles[1].sensorCtrl.tracksDetected.Find((Track trk) => trk.target == vehicles[0]);
                existingTrack.UpdateTrack(vehicles[0].position, vehicles[0].velocity, existingTrack.identification);
            }
            if (vehicles[1].sensorCtrl.tracksDetected.Exists((Track trk) => trk.target == vehicles[2]) == false)
            {
                Track newTrack = new Track(vehicles[2], TrackId.Unknown);
                newTrack.UpdateTrack(vehicles[2].position, vehicles[2].velocity, TrackId.Hostile);
                vehicles[1].sensorCtrl.AddTrack(newTrack);
                vehicles[1].OnNewTrack(newTrack, "Satelite Uplink");
            }
            else
            {
                Track existingTrack = vehicles[1].sensorCtrl.tracksDetected.Find((Track trk) => trk.target == vehicles[2]);
                existingTrack.UpdateTrack(vehicles[2].position, vehicles[2].velocity, existingTrack.identification);
            }
        }
        */
	}
}
