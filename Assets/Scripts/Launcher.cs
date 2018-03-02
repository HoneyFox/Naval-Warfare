using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public class FireControlChannel
    {
        public Track target;
        public Vehicle attacker;

        public FireControlChannel(Vehicle attacker, Track target)
        {
            this.target = target;
            this.attacker = attacker;
        }
    }

    public Vehicle self;

    public string launcherName;
    public List<string> vehicleNames = new List<string>();
    public List<int> vehicleCounts = new List<int>();
    public bool isYawFixed;
    public bool isPitchFixed;

    public Vector2 fixedDirection; // pitch, yaw.

    public float launchHalfAngle = 90;

    public float roundPerMinute;
    public float secondsPerRound { get { return 60f / roundPerMinute; } }
    public float lastTimeOfFiring = -float.MaxValue;
    public bool isLoaded { get { return lastTimeOfFiring < Time.time - secondsPerRound; } }
    public int salvoCount = 1;
    private int currentSalvoCount = 0;
    public int salvoInterval = 1;

    public int maxFireControlChannels = 1;
    public List<FireControlChannel> fireControlChannels = new List<FireControlChannel>();

    public GameObject muzzleFX;
    
    void Start()
    {

    }

    public bool Launch(string vehicleName, Track target)
    {
        if (enabled == false) return false;
        if (isLoaded == false && currentSalvoCount == 0) return false;
        if (vehicleNames.Contains(vehicleName) == false) return false;
        if (vehicleCounts[vehicleNames.IndexOf(vehicleName)] <= 0) return false;
        GameObject vehicle = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths[vehicleName]);

        float eta = Vector3.Distance(this.transform.position, target.predictedPositionAtTime(0)) / Vehicle.sVehicleMaxSpeed[vehicleName] / 0.9f;
        eta = Vector3.Distance(this.transform.position, target.predictedPositionAtTime(eta)) / Vehicle.sVehicleMaxSpeed[vehicleName] / 0.9f;
        eta = Vector3.Distance(this.transform.position, target.predictedPositionAtTime(eta)) / Vehicle.sVehicleMaxSpeed[vehicleName] / 0.9f;

        GuidanceModule guidance = vehicle.GetComponent<GuidanceModule>();
        if (guidance == null)
        {
            Debug.Log("Vehicle that can be launched should have GuidanceModule.");
        }
        else
        {
            if (guidance.requiresLockBeforeFiring == true)
            {
                guidance.SetupGuidance(target, null);

                if (vehicle.GetComponent<Vehicle>().sensorCtrl != null)
                    vehicle.GetComponent<Vehicle>().sensorCtrl.AddTrack(target);

                WarheadModule warhead = vehicle.GetComponent<WarheadModule>();
                if (warhead != null)
                    warhead.SetupTarget(target);

                // Fill track information via data-link if possible.
                // (even if this weapon already requires locking, which means the track information will be provided by the launcher).
                if (vehicle.GetComponent<DatalinkModule>() != null)
                {
                    if (vehicle.GetComponent<DatalinkModule>().isDuplex)
                        vehicle.GetComponent<DatalinkModule>().AddReceiver(self);

                    foreach (DatalinkModule dl in self.GetComponents<DatalinkModule>())
                    {
                        if (dl.limitTrackedVehicleType == false || dl.trackedVehicleType == Vehicle.sVehicleTypes[target.vehicleTypeName])
                        {
                            dl.AddReceiver(vehicle.GetComponent<Vehicle>());
                        }
                    }
                }
            }
            else
            {
                GameObject.Destroy(vehicle);

                bool isLaunched = Launch(vehicleName, target.predictedPositionAtTime(eta), target);
                if (isLaunched)
                {
                    Vehicle launchedVehicle = SceneManager.instance.vehicles[SceneManager.instance.vehicles.Count - 1];
                    launchedVehicle.GetComponent<WarheadModule>().SetupTarget(target);

                    // Fill track information via data-link if possible.
                    if(launchedVehicle.GetComponent<DatalinkModule>() != null)
                    {
                        Track launchedVehicleOwnTrack = new Track(launchedVehicle, target.target, TrackId.Unknown);
                        launchedVehicleOwnTrack.UpdateTrack(target.position, target.velocity, target.timeOfLastUpdate, target.identification);
                        launchedVehicle.sensorCtrl.AddTrack(launchedVehicleOwnTrack);
                        launchedVehicle.OnNewTrack(launchedVehicleOwnTrack, "Fire Control");

                        if(launchedVehicle.GetComponent<DatalinkModule>().isDuplex)
                            launchedVehicle.GetComponent<DatalinkModule>().AddReceiver(self);

                        foreach(DatalinkModule dl in self.GetComponents<DatalinkModule>())
                        {
                            if (dl.limitTrackedVehicleType == false || dl.trackedVehicleType == Vehicle.sVehicleTypes[target.vehicleTypeName])
                            {
                                dl.AddReceiver(launchedVehicle);
                            }
                        }
                    }
                }
                return isLaunched;
            }
        }

        Vector3 launcherDirection;
        float angleDiff;
        
        if (isPitchFixed == false && isYawFixed == true)
        {
            float directCourse = Mathf.Rad2Deg * Mathf.Atan2(target.predictedPositionAtTime(eta).x - vehicle.transform.localPosition.x, target.predictedPositionAtTime(eta).z - vehicle.transform.localPosition.z);
            angleDiff = directCourse - (self.course + fixedDirection.y);
        }
        else if (isPitchFixed == true && isYawFixed == false)
        {
            float directPitch = Mathf.Rad2Deg * Mathf.Atan2(target.predictedPositionAtTime(eta).y - vehicle.transform.localPosition.y, Mathf.Sqrt(Mathf.Pow(target.predictedPositionAtTime(eta).z - vehicle.transform.localPosition.z, 2) + Mathf.Pow(target.predictedPositionAtTime(eta).x - vehicle.transform.localPosition.x, 2)));
            angleDiff = directPitch - (self.pitch + fixedDirection.x);
        }
        else if (isPitchFixed == true && isYawFixed == true)
        {
            launcherDirection = this.transform.forward;
            Quaternion rotation = Quaternion.Euler(new Vector3(-fixedDirection.x, fixedDirection.y, 0f));
            launcherDirection = rotation * launcherDirection;
            angleDiff = Vector3.Angle(target.predictedPositionAtTime(eta) - this.transform.position, launcherDirection);
        }
        else
        {
            angleDiff = 0f;
        }

        if (angleDiff > launchHalfAngle)
        {
            // Launcher half angle constraint not met.
            GameObject.Destroy(vehicle);
            return false;
        }

        if(guidance.requiresFireControl)
        {
            if (fireControlChannels.Count < maxFireControlChannels)
            {
                vehicle.transform.parent = SceneManager.instance.transform.Find("Side " + self.side.ToString());
                vehicle.transform.position = this.transform.position;

                float expectedCourse = Mathf.Rad2Deg * Mathf.Atan2(target.predictedPositionAtTime(eta).x - vehicle.transform.localPosition.x, target.predictedPositionAtTime(eta).z - vehicle.transform.localPosition.z);
                vehicle.GetComponent<Locomotor>().orderedCourse = expectedCourse;
                if (this.isYawFixed)
                    vehicle.GetComponent<Vehicle>().course = this.transform.eulerAngles.y + fixedDirection.y;
                else
                    vehicle.GetComponent<Vehicle>().course = expectedCourse;

                float expectedPitch = Mathf.Rad2Deg * Mathf.Atan2(target.predictedPositionAtTime(eta).y - vehicle.transform.localPosition.y, Mathf.Sqrt(Mathf.Pow(target.predictedPositionAtTime(eta).z - vehicle.transform.localPosition.z, 2) + Mathf.Pow(target.predictedPositionAtTime(eta).x - vehicle.transform.localPosition.x, 2)));
                vehicle.GetComponent<Locomotor>().orderedPitch = Mathf.Max(0f, expectedPitch);
                if (this.isPitchFixed)
                    vehicle.GetComponent<Vehicle>().pitch = this.transform.eulerAngles.x + fixedDirection.x;
                else
                    vehicle.GetComponent<Vehicle>().pitch = expectedPitch;

                vehicle.GetComponent<Vehicle>().speed = self.speed;

                vehicle.GetComponent<Locomotor>().orderedSpeed = vehicle.GetComponent<Vehicle>().maxSpeed;
                vehicle.GetComponent<Vehicle>().position = this.transform.position;
        
                SceneManager.instance.vehicles.Add(vehicle.GetComponent<Vehicle>());
                vehicle.GetComponent<Vehicle>().side = self.side;
                fireControlChannels.Add(new FireControlChannel(vehicle.GetComponent<Vehicle>(), target));
                lastTimeOfFiring = Time.time;
                vehicleCounts[vehicleNames.IndexOf(vehicleName)]--;

                currentSalvoCount++;
                if(muzzleFX != null)
                {
                    if (currentSalvoCount == 1)
                    {
                        GameObject muzzleFXInstance = (GameObject)GameObject.Instantiate(muzzleFX);
                        muzzleFXInstance.transform.parent = SceneManager.instance.transform;
                        muzzleFXInstance.transform.position = this.transform.position;
                        muzzleFXInstance.GetComponent<ParticleSystem>().Play(true);
                    }
                }
                if (currentSalvoCount == salvoCount)
                    currentSalvoCount = 0;
                else
                    StartCoroutine(DelayedLaunch(vehicleName, target, salvoInterval));

                return true;
            }
            else
            {
                // Fire Control Channels are full.
                GameObject.Destroy(vehicle);
                return false;
            }
        }
        else
        {
            vehicle.transform.parent = SceneManager.instance.transform.Find("Side " + self.side.ToString());
            vehicle.transform.position = this.transform.position;

            float expectedCourse = Mathf.Rad2Deg * Mathf.Atan2(target.predictedPositionAtTime(eta).x - vehicle.transform.localPosition.x, target.predictedPositionAtTime(eta).z - vehicle.transform.localPosition.z);
            vehicle.GetComponent<Locomotor>().orderedCourse = expectedCourse;
            if (this.isYawFixed)
                vehicle.GetComponent<Vehicle>().course = self.course + fixedDirection.y;
            else
                vehicle.GetComponent<Vehicle>().course = expectedCourse;

            float expectedPitch = Mathf.Rad2Deg * Mathf.Atan2(target.predictedPositionAtTime(eta).y - vehicle.transform.localPosition.y, Mathf.Sqrt(Mathf.Pow(target.predictedPositionAtTime(eta).z - vehicle.transform.localPosition.z, 2) + Mathf.Pow(target.predictedPositionAtTime(eta).x - vehicle.transform.localPosition.x, 2)));
            vehicle.GetComponent<Locomotor>().orderedPitch = Mathf.Max(0f, expectedPitch);
            if (this.isPitchFixed)
                vehicle.GetComponent<Vehicle>().pitch = self.pitch + fixedDirection.x;
            else
                vehicle.GetComponent<Vehicle>().pitch = expectedPitch;

            vehicle.GetComponent<Vehicle>().speed = self.speed;

            vehicle.GetComponent<Locomotor>().orderedSpeed = vehicle.GetComponent<Vehicle>().maxSpeed;
            vehicle.GetComponent<Vehicle>().position = this.transform.position;

            SceneManager.instance.vehicles.Add(vehicle.GetComponent<Vehicle>());
            vehicle.GetComponent<Vehicle>().side = self.side;
            lastTimeOfFiring = Time.time;
            vehicleCounts[vehicleNames.IndexOf(vehicleName)]--;
            
            currentSalvoCount++;
            if (muzzleFX != null)
            {
                if (currentSalvoCount == 1)
                {
                    GameObject muzzleFXInstance = (GameObject)GameObject.Instantiate(muzzleFX);
                    muzzleFXInstance.transform.parent = SceneManager.instance.transform;
                    muzzleFXInstance.transform.position = this.transform.position;
                    muzzleFXInstance.GetComponent<ParticleSystem>().Play(true);
                }
            }
            if (currentSalvoCount == salvoCount)
                currentSalvoCount = 0;
            else
                StartCoroutine(DelayedLaunch(vehicleName, target, salvoInterval));
            return true;
        }

    }

    public bool Launch(string vehicleName, Vector3 position, Track actualTrack = null)
    {
        if (enabled == false) return false; 
        if (isLoaded == false && currentSalvoCount == 0) return false; 
        if (vehicleNames.Contains(vehicleName) == false) return false;
        if (vehicleCounts[vehicleNames.IndexOf(vehicleName)] <= 0) return false;
        GameObject vehicle = ResourceManager.LoadPrefab(Vehicle.sVehiclePrefabPaths[vehicleName]);

        GuidanceModule guidance = vehicle.GetComponent<GuidanceModule>();
        if (guidance == null)
            Debug.Log("Vehicle that can be launched should have GuidanceModule.");


        Vector3 launcherDirection;
        float angleDiff;

        if (isPitchFixed == false && isYawFixed == true)
        {
            float directCourse = Mathf.Rad2Deg * Mathf.Atan2(position.x - vehicle.transform.localPosition.x, position.z - vehicle.transform.localPosition.z);
            angleDiff = directCourse - (self.course + fixedDirection.y);
        }
        else if (isPitchFixed == true && isYawFixed == false)
        {
            float directPitch = Mathf.Rad2Deg * Mathf.Atan2(position.y - vehicle.transform.localPosition.y, Mathf.Sqrt(Mathf.Pow(position.z - vehicle.transform.localPosition.z, 2) + Mathf.Pow(position.x - vehicle.transform.localPosition.x, 2)));
            angleDiff = directPitch - (self.pitch + fixedDirection.x);
        }
        else if (isPitchFixed == true && isYawFixed == true)
        {
            launcherDirection = this.transform.forward;
            Quaternion rotation = Quaternion.Euler(new Vector3(-fixedDirection.x, fixedDirection.y, 0f));
            launcherDirection = rotation * launcherDirection;
            angleDiff = Vector3.Angle(position - this.transform.position, launcherDirection);
        }
        else
        {
            angleDiff = 0f;
        }

        if (angleDiff > launchHalfAngle)
        {
            // Launcher half angle constraint not met.
            GameObject.Destroy(vehicle);
            return false;
        }

        guidance.SetupGuidance(null, position);

        WarheadModule warhead = vehicle.GetComponent<WarheadModule>();
        if (warhead != null)
            warhead.SetupTarget(position);

        vehicle.transform.parent = SceneManager.instance.transform.Find("Side " + self.side.ToString());
        vehicle.transform.position = this.transform.position;

        float expectedCourse = Mathf.Rad2Deg * Mathf.Atan2(position.x - vehicle.transform.localPosition.x, position.z - vehicle.transform.localPosition.z);
        vehicle.GetComponent<Locomotor>().orderedCourse = expectedCourse;
        if (this.isYawFixed)
            vehicle.GetComponent<Vehicle>().course = self.course + fixedDirection.y;
        else
            vehicle.GetComponent<Vehicle>().course = expectedCourse;

        float expectedPitch = Mathf.Rad2Deg * Mathf.Atan2(position.y - vehicle.transform.localPosition.y, Mathf.Sqrt(Mathf.Pow(position.z - vehicle.transform.localPosition.z, 2) + Mathf.Pow(position.x - vehicle.transform.localPosition.x, 2)));
        vehicle.GetComponent<Locomotor>().orderedPitch = Mathf.Max(0f, expectedPitch);
        if (this.isPitchFixed)
            vehicle.GetComponent<Vehicle>().pitch = self.pitch + fixedDirection.x;
        else
            vehicle.GetComponent<Vehicle>().pitch = expectedPitch;

        vehicle.GetComponent<Vehicle>().speed = self.speed;

        vehicle.GetComponent<Locomotor>().orderedSpeed = vehicle.GetComponent<Vehicle>().maxSpeed;
        vehicle.GetComponent<Vehicle>().position = this.transform.position;
        
        SceneManager.instance.vehicles.Add(vehicle.GetComponent<Vehicle>());
        vehicle.GetComponent<Vehicle>().side = self.side;
        lastTimeOfFiring = Time.time;
        vehicleCounts[vehicleNames.IndexOf(vehicleName)]--;

        currentSalvoCount++;
        if (muzzleFX != null)
        {
            if (currentSalvoCount == 1)
            {
                GameObject muzzleFXInstance = (GameObject)GameObject.Instantiate(muzzleFX);
                muzzleFXInstance.transform.parent = SceneManager.instance.transform;
                muzzleFXInstance.transform.position = this.transform.position;
                muzzleFXInstance.GetComponent<ParticleSystem>().Play(true);
            }
        }
        if (currentSalvoCount == salvoCount)
        {
            currentSalvoCount = 0;
        }
        else
        {
            if (actualTrack == null)
                StartCoroutine(DelayedLaunch(vehicleName, position, salvoInterval));
            else
                StartCoroutine(DelayedLaunch(vehicleName, actualTrack, salvoInterval));
        }

        return true;
    }

    private IEnumerator DelayedLaunch(string vehicleName, Track target, int frames)
    {
        while (frames > 0)
        {
            yield return null;
            frames--;
        }

        if (target.target == null || (target.isLost || target.target.isDead))
        {
            currentSalvoCount = 0;
            yield break;
        }
        else
        {
            Launch(vehicleName, target);
        }
    }

    private IEnumerator DelayedLaunch(string vehicleName, Vector3 position, int frames)
    {
        while(frames > 0)
        {
            yield return null;
            frames--;
        }

        Launch(vehicleName, position);
    }

    public void UpdateFireControlChannels()
    {
        for(int i = 0; i < fireControlChannels.Count; ++i)
        {
            if (fireControlChannels[i].attacker == null || fireControlChannels[i].target == null || fireControlChannels[i].target.isLost)
            {
                fireControlChannels.RemoveAt(i);
                --i;
            }
        }
    }
}
