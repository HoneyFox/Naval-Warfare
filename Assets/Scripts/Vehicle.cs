using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Vehicle : MonoBehaviour 
{
    public enum VehicleType
    {
        Surf = 0,
        Sub,
        Air,
    }

    public static Dictionary<string, string> sVehiclePrefabPaths = new Dictionary<string, string>();
    public static Dictionary<string, VehicleType> sVehicleTypes = new Dictionary<string, VehicleType>();
    public static Dictionary<string, bool[]> sVehicleCanEngage = new Dictionary<string, bool[]>();
    public static Dictionary<string, bool[]> sVehicleCanBeEngaged = new Dictionary<string, bool[]>();
    public static Dictionary<string, float> sVehicleRanges = new Dictionary<string, float>();
    public static Dictionary<string, float> sVehicleMaxSpeed = new Dictionary<string, float>();
    public static Dictionary<string, bool> sVehicleCanBeTracked = new Dictionary<string, bool>();
    public static Dictionary<string, string[]> sVehicleTakeOffMethods = new Dictionary<string, string[]>();
    public static Dictionary<string, string[]> sVehicleLandMethods = new Dictionary<string, string[]>();
    public static Dictionary<string, VehicleType[]> sVehicleTaskTypes = new Dictionary<string, VehicleType[]>();
    public static Dictionary<string, bool> sVehicleIsDeployOnly = new Dictionary<string, bool>();

    static Vehicle()
    {
        sVehiclePrefabPaths["OH Perry FFG"] = "Prefab/Vessels/OH Perry FFG";
        sVehiclePrefabPaths["Arleigh Burke DDG"] = "Prefab/Vessels/Arleigh Burke DDG";
        sVehiclePrefabPaths["Wasp LHD"] = "Prefab/Vessels/Wasp LHD";
        sVehiclePrefabPaths["MR Shell"] = "Prefab/Vessels/MR Shell";
        sVehiclePrefabPaths["SM-1 Standard"] = "Prefab/Vessels/SM-1 Standard";
        sVehiclePrefabPaths["SM-2 Standard"] = "Prefab/Vessels/SM-2 Standard";
        sVehiclePrefabPaths["SM-6 Standard"] = "Prefab/Vessels/SM-6 Standard";
        sVehiclePrefabPaths["AGM-84 Harpoon"] = "Prefab/Vessels/AGM-84 Harpoon";
        sVehiclePrefabPaths["SS-N-27 Club"] = "Prefab/Vessels/SS-N-27 Club";
        sVehiclePrefabPaths["SS-N-27 Warhead"] = "Prefab/Vessels/SS-N-27 Warhead";
        sVehiclePrefabPaths["RGM-100 LRASM"] = "Prefab/Vessels/RGM-100 LRASM";
        sVehiclePrefabPaths["RUR-5 ASROC"] = "Prefab/Vessels/RUR-5 ASROC";
        sVehiclePrefabPaths["Mk-46 Torpedo"] = "Prefab/Vessels/Mk-46 Torpedo";
        sVehiclePrefabPaths["SR Shell"] = "Prefab/Vessels/SR Shell";
        sVehiclePrefabPaths["Kilo SSK"] = "Prefab/Vessels/Kilo SSK";
        sVehiclePrefabPaths["ESSM"] = "Prefab/Vessels/ESSM";
        sVehiclePrefabPaths["SH-60 SeaHawk"] = "Prefab/Vessels/SH-60 SeaHawk";
        sVehiclePrefabPaths["AV-8 Harrier"] = "Prefab/Vessels/AV-8 Harrier";
        sVehiclePrefabPaths["AGM-119 Penguin"] = "Prefab/Vessels/AGM-119 Penguin";
        sVehiclePrefabPaths["UGST Torpedo"] = "Prefab/Vessels/UGST Torpedo";
        sVehiclePrefabPaths["AIM-120C AMRAAM"] = "Prefab/Vessels/AIM-120C AMRAAM";
        sVehiclePrefabPaths["RIM-7 Sea Sparrow"] = "Prefab/Vessels/RIM-7 Sea Sparrow";
        sVehiclePrefabPaths["AGM-88 HARM"] = "Prefab/Vessels/AGM-88 HARM";
        sVehiclePrefabPaths["Sub Decoy"] = "Prefab/Vessels/Sub Decoy";
        sVehiclePrefabPaths["Towed Sonar Decoy"] = "Prefab/Vessels/Towed Sonar Decoy";

        sVehicleTypes["OH Perry FFG"] = VehicleType.Surf;
        sVehicleTypes["Arleigh Burke DDG"] = VehicleType.Surf;
        sVehicleTypes["Wasp LHD"] = VehicleType.Surf;
        sVehicleTypes["MR Shell"] = VehicleType.Air;
        sVehicleTypes["SM-1 Standard"] = VehicleType.Air;
        sVehicleTypes["SM-2 Standard"] = VehicleType.Air;
        sVehicleTypes["SM-6 Standard"] = VehicleType.Air;
        sVehicleTypes["AGM-84 Harpoon"] = VehicleType.Air;
        sVehicleTypes["SS-N-27 Club"] = VehicleType.Air;
        sVehicleTypes["SS-N-27 Warhead"] = VehicleType.Air;
        sVehicleTypes["RGM-100 LRASM"] = VehicleType.Air;
        sVehicleTypes["RUR-5 ASROC"] = VehicleType.Air;
        sVehicleTypes["Mk-46 Torpedo"] = VehicleType.Sub;
        sVehicleTypes["SR Shell"] = VehicleType.Air;
        sVehicleTypes["Kilo SSK"] = VehicleType.Sub;
        sVehicleTypes["ESSM"] = VehicleType.Air;
        sVehicleTypes["SH-60 SeaHawk"] = VehicleType.Air;
        sVehicleTypes["AV-8 Harrier"] = VehicleType.Air;
        sVehicleTypes["AGM-119 Penguin"] = VehicleType.Air;
        sVehicleTypes["UGST Torpedo"] = VehicleType.Sub;
        sVehicleTypes["AIM-120C AMRAAM"] = VehicleType.Air;
        sVehicleTypes["RIM-7 Sea Sparrow"] = VehicleType.Air;
        sVehicleTypes["AGM-88 HARM"] = VehicleType.Air;
        sVehicleTypes["Sub Decoy"] = VehicleType.Sub;
        sVehicleTypes["Towed Sonar Decoy"] = VehicleType.Sub;

        sVehicleCanEngage["OH Perry FFG"] = new bool[4] { false, false, false, false };
        sVehicleCanEngage["Arleigh Burke DDG"] = new bool[4] { false, false, false, false};
        sVehicleCanEngage["Wasp LHD"] = new bool[4] { false, false, false, false };
        sVehicleCanEngage["MR Shell"] = new bool[4] { true, false, true, true };
        sVehicleCanEngage["SM-1 Standard"] = new bool[4] { false, false, true, false };
        sVehicleCanEngage["SM-2 Standard"] = new bool[4] { false, false, true, false };
        sVehicleCanEngage["SM-6 Standard"] = new bool[4] { false, false, true, false };
        sVehicleCanEngage["AGM-84 Harpoon"] = new bool[4] { true, false, false, true };
        sVehicleCanEngage["SS-N-27 Club"] = new bool[4] { true, false, false, true };
        sVehicleCanEngage["SS-N-27 Warhead"] = new bool[4] { true, false, false, true };
        sVehicleCanEngage["RGM-100 LRASM"] = new bool[4] { true, false, false, true };
        sVehicleCanEngage["RUR-5 ASROC"] = new bool[4] { true, true, false, true };
        sVehicleCanEngage["Mk-46 Torpedo"] = new bool[4] { true, true, false, true };
        sVehicleCanEngage["SR Shell"] = new bool[4] { true, false, true, true };
        sVehicleCanEngage["Kilo SSK"] = new bool[4] { false, false, false, false };
        sVehicleCanEngage["ESSM"] = new bool[4] { false, false, true, false };
        sVehicleCanEngage["SH-60 SeaHawk"] = new bool[4] { false, false, false, false };
        sVehicleCanEngage["AV-8 Harrier"] = new bool[4] { false, false, false, false };
        sVehicleCanEngage["AGM-119 Penguin"] = new bool[4] { true, false, false, true };
        sVehicleCanEngage["UGST Torpedo"] = new bool[4] { true, true, false, true };
        sVehicleCanEngage["AIM-120C AMRAAM"] = new bool[4] { false, false, true, false };
        sVehicleCanEngage["RIM-7 Sea Sparrow"] = new bool[4] { false, false, true, false };
        sVehicleCanEngage["AGM-88 HARM"] = new bool[4] { true, false, false, true };
        sVehicleCanEngage["Sub Decoy"] = new bool[4] { false, false, false, false };
        sVehicleCanEngage["Towed Sonar Decoy"] = new bool[4] { false, false, false, false };

        sVehicleCanBeEngaged["OH Perry FFG"] = new bool[3] { false, true, true };
        sVehicleCanBeEngaged["Arleigh Burke DDG"] = new bool[3] { false, true, true };
        sVehicleCanBeEngaged["Wasp LHD"] = new bool[3] { false, true, true };
        sVehicleCanBeEngaged["MR Shell"] = new bool[3] { false, false, false };
        sVehicleCanBeEngaged["SM-1 Standard"] = new bool[3] { false, false, false };
        sVehicleCanBeEngaged["SM-2 Standard"] = new bool[3] { false, false, false };
        sVehicleCanBeEngaged["SM-6 Standard"] = new bool[3] { false, false, false };
        sVehicleCanBeEngaged["AGM-84 Harpoon"] = new bool[3] { false, false, true };
        sVehicleCanBeEngaged["SS-N-27 Club"] = new bool[3] { false, false, true };
        sVehicleCanBeEngaged["SS-N-27 Warhead"] = new bool[3] { false, false, true };
        sVehicleCanBeEngaged["RGM-100 LRASM"] = new bool[3] { false, false, true };
        sVehicleCanBeEngaged["RUR-5 ASROC"] = new bool[3] { false, false, true };
        sVehicleCanBeEngaged["Mk-46 Torpedo"] = new bool[3] { false, false, false };
        sVehicleCanBeEngaged["SR Shell"] = new bool[3] { false, false, false };
        sVehicleCanBeEngaged["Kilo SSK"] = new bool[3] { false, true, true };
        sVehicleCanBeEngaged["ESSM"] = new bool[3] { false, false, false };
        sVehicleCanBeEngaged["SH-60 SeaHawk"] = new bool[3] { false, false, true };
        sVehicleCanBeEngaged["AV-8 Harrier"] = new bool[3] { false, false, true };
        sVehicleCanBeEngaged["AGM-119 Penguin"] = new bool[3] { false, false, true };
        sVehicleCanBeEngaged["UGST Torpedo"] = new bool[3] { false, false, false };
        sVehicleCanBeEngaged["AIM-120C AMRAAM"] = new bool[3] { false, false, false };
        sVehicleCanBeEngaged["RIM-7 Sea Sparrow"] = new bool[3] { false, false, false };
        sVehicleCanBeEngaged["AGM-88 HARM"] = new bool[3] { false, false, true };
        sVehicleCanBeEngaged["Sub Decoy"] = new bool[3] { false, true, true };
        sVehicleCanBeEngaged["Towed Sonar Decoy"] = new bool[3] { false, true, true };

        sVehicleRanges["OH Perry FFG"] = float.MaxValue;
        sVehicleRanges["Arleigh Burke DDG"] = float.MaxValue;
        sVehicleRanges["Wasp LHD"] = float.MaxValue;
        sVehicleRanges["MR Shell"] = 48000;
        sVehicleRanges["SM-1 Standard"] = 45000;
        sVehicleRanges["SM-2 Standard"] = 125000;
        sVehicleRanges["SM-6 Standard"] = 220000;
        sVehicleRanges["AGM-84 Harpoon"] = 220000;
        sVehicleRanges["SS-N-27 Club"] = 270000;
        sVehicleRanges["SS-N-27 Warhead"] = 35000;
        sVehicleRanges["RGM-100 LRASM"] = 600000;
        sVehicleRanges["RUR-5 ASROC"] = 20000;
        sVehicleRanges["Mk-46 Torpedo"] = 12000;
        sVehicleRanges["SR Shell"] = 2000;
        sVehicleRanges["Kilo SSK"] = float.MaxValue;
        sVehicleRanges["ESSM"] = 32000;
        sVehicleRanges["SH-60 SeaHawk"] = 300000;
        sVehicleRanges["AV-8 Harrier"] = 600000;
        sVehicleRanges["AGM-119 Penguin"] = 65000;
        sVehicleRanges["UGST Torpedo"] = 45000;
        sVehicleRanges["AIM-120C AMRAAM"] = 85000;
        sVehicleRanges["RIM-7 Sea Sparrow"] = 25000;
        sVehicleRanges["AGM-88 HARM"] = 125000;
        sVehicleRanges["Sub Decoy"] = 600;
        sVehicleRanges["Towed Sonar Decoy"] = 10000;

        sVehicleMaxSpeed["OH Perry FFG"] = 16;
        sVehicleMaxSpeed["Arleigh Burke DDG"] = 17;
        sVehicleMaxSpeed["Wasp LHD"] = 15;
        sVehicleMaxSpeed["MR Shell"] = 760;
        sVehicleMaxSpeed["SM-1 Standard"] = 850;
        sVehicleMaxSpeed["SM-2 Standard"] = 1000;
        sVehicleMaxSpeed["SM-6 Standard"] = 1200;
        sVehicleMaxSpeed["AGM-84 Harpoon"] = 320;
        sVehicleMaxSpeed["SS-N-27 Club"] = 320;
        sVehicleMaxSpeed["SS-N-27 Warhead"] = 980;
        sVehicleMaxSpeed["RGM-100 LRASM"] = 320;
        sVehicleMaxSpeed["RUR-5 ASROC"] = 700;
        sVehicleMaxSpeed["Mk-46 Torpedo"] = 24;
        sVehicleMaxSpeed["SR Shell"] = 1098;
        sVehicleMaxSpeed["Kilo SSK"] = 12;
        sVehicleMaxSpeed["ESSM"] = 850;
        sVehicleMaxSpeed["SH-60 SeaHawk"] = 60f;
        sVehicleMaxSpeed["AV-8 Harrier"] = 310f;
        sVehicleMaxSpeed["AGM-119 Penguin"] = 310;
        sVehicleMaxSpeed["UGST Torpedo"] = 30;
        sVehicleMaxSpeed["AIM-120C AMRAAM"] = 1250;
        sVehicleMaxSpeed["RIM-7 Sea Sparrow"] = 850;
        sVehicleMaxSpeed["AGM-88 HARM"] = 800;
        sVehicleMaxSpeed["Sub Decoy"] = 30;
        sVehicleMaxSpeed["Towed Sonar Decoy"] = 30;

        sVehicleCanBeTracked["OH Perry FFG"] = true;
        sVehicleCanBeTracked["Arleigh Burke DDG"] = true;
        sVehicleCanBeTracked["Wasp LHD"] = true;
        sVehicleCanBeTracked["MR Shell"] = false;
        sVehicleCanBeTracked["SM-1 Standard"] = true;
        sVehicleCanBeTracked["SM-2 Standard"] = true;
        sVehicleCanBeTracked["SM-6 Standard"] = true;
        sVehicleCanBeTracked["AGM-84 Harpoon"] = true;
        sVehicleCanBeTracked["SS-N-27 Club"] = true;
        sVehicleCanBeTracked["SS-N-27 Warhead"] = true;
        sVehicleCanBeTracked["RGM-100 LRASM"] = true;
        sVehicleCanBeTracked["RUR-5 ASROC"] = true;
        sVehicleCanBeTracked["Mk-46 Torpedo"] = true;
        sVehicleCanBeTracked["SR Shell"] = false;
        sVehicleCanBeTracked["Kilo SSK"] = true;
        sVehicleCanBeTracked["ESSM"] = true;
        sVehicleCanBeTracked["SH-60 SeaHawk"] = true;
        sVehicleCanBeTracked["AV-8 Harrier"] = true;
        sVehicleCanBeTracked["AGM-119 Penguin"] = true;
        sVehicleCanBeTracked["UGST Torpedo"] = true;
        sVehicleCanBeTracked["AIM-120C AMRAAM"] = true;
        sVehicleCanBeTracked["RIM-7 Sea Sparrow"] = true;
        sVehicleCanBeTracked["AGM-88 HARM"] = true;
        sVehicleCanBeTracked["Sub Decoy"] = true;
        sVehicleCanBeTracked["Towed Sonar Decoy"] = true;
        
        sVehicleTaskTypes["SH-60 SeaHawk"] = new VehicleType[] { VehicleType.Sub };
        sVehicleTaskTypes["AV-8 Harrier"] = new VehicleType[] { VehicleType.Air, VehicleType.Surf };
        sVehicleTaskTypes["Sub Decoy"] = new VehicleType[] { VehicleType.Sub };
        sVehicleTaskTypes["Towed Sonar Decoy"] = new VehicleType[] { VehicleType.Sub };

        sVehicleTakeOffMethods["SH-60 SeaHawk"] = new string[] { "HeloStrip", "LargeHeloStrip" };
        sVehicleTakeOffMethods["AV-8 Harrier"] = new string[] { "Catapult", "LargeHeloStrip" };
        sVehicleTakeOffMethods["Sub Decoy"] = new string[] { "CounterMeasureLauncher" };
        sVehicleTakeOffMethods["Towed Sonar Decoy"] = new string[] { "CounterMeasureDeployer" };
        
        sVehicleLandMethods["SH-60 SeaHawk"] = new string[] { "HeloStrip", "LargeHeloStrip" };
        sVehicleLandMethods["AV-8 Harrier"] = new string[] { "LargeHeloStrip", "ArrestCable" };

        sVehicleIsDeployOnly["Towed Sonar Decoy"] = true;
    }

    public string typeName;

    [NonSerialized]
    public int side;

    public Vector3 position;
    public float speed;
    public float course;
    public float pitch;
    public float bank;
    public Vector3 velocity
    {
        get
        {
            return Quaternion.Euler(-pitch, course, bank) * new Vector3(0f, 0f, speed);
        }
    }
    public Locomotor locomotor;
    public float maxFuel;
    public float fuel;
    public int FuelPercentage { get { return Mathf.RoundToInt(fuel / maxFuel * 100f); } }
    public float minSpeed;
    public float maxSpeed;
    public float fuelConsumptionRate;
    public AnimationCurve fuelConsumptionCurve;
    
    public int maxAge = 30; // Track will be lost if not detected by this long period of time.
    public float stealthFactor = 0f;
    public float identifyFactor = 1f;

    public bool[] canEngage { get { return sVehicleCanEngage[typeName]; } }
    public bool[] canBeEngaged { get { return sVehicleCanBeEngaged[typeName]; } }

    public LauncherController launcherCtrl { get { return this.gameObject.GetComponent<LauncherController>(); } }
    public SensorController sensorCtrl { get { return this.gameObject.GetComponent<SensorController>(); } }
    public AirstripController airstripCtrl { get { return this.gameObject.GetComponent<AirstripController>(); } }
    public ArmorModule armorModule { get { return this.gameObject.GetComponent<ArmorModule>(); } }
    public GuidanceModule guidanceModule { get { return this.gameObject.GetComponent<GuidanceModule>(); } }
    public WarheadModule warheadModule { get { return this.gameObject.GetComponent<WarheadModule>(); } }

    public bool isDead { get { return armorModule.armorPoint <= 0f; } }

    public bool autoEngageAirTracks = false;
    public bool autoDealWithAirThreats = true;
    public bool autoEngageSurfTracks = false;
    public bool autoEngageSubTracks = false;
    public bool autoDealWithSubThreats = true;

    public Vehicle takeOffFrom = null;
    public float mastHeight = 0f;
    public float size = 1f;
    
    private LineRenderer plumbLineRenderer = null;
    private LineRenderer orderedVectorLineRenderer = null;

	// Use this for initialization
	void Start () 
    {

	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        this.transform.localPosition = position;
        this.transform.localRotation = Quaternion.Euler(-pitch, course, bank);
        OnFixedUpdate();

        Camera cameraOrbit = SceneManager.instance.GetComponent<VehicleSelector>().vehicleCamera;
        float cameraScale = Vector3.Distance(cameraOrbit.transform.position, position) * 0.01f;
        float defaultWidth = 0.2f;
        defaultWidth *= cameraScale;
        if (plumbLineRenderer != null)
        {
            if (SceneManager.instance.GetComponent<VehicleSelector>().showPlumbLine)
            {
                plumbLineRenderer.enabled = true;
                plumbLineRenderer.SetWidth(defaultWidth, defaultWidth);
                plumbLineRenderer.SetPosition(0, position);
                plumbLineRenderer.SetPosition(1, new Vector3(position.x, 0f, position.z));
            }
            else
            {
                plumbLineRenderer.enabled = false;
            }
        }
        else
        {
            if (sVehicleCanBeTracked[typeName])
            {
                plumbLineRenderer = this.gameObject.AddComponent<LineRenderer>();
                plumbLineRenderer.SetColors(new Color(1f, 1f, 0f, 0.25f), Color.yellow);
                plumbLineRenderer.SetVertexCount(2);
                plumbLineRenderer.SetWidth(defaultWidth, defaultWidth);
                plumbLineRenderer.useWorldSpace = true;
                plumbLineRenderer.material = ResourceManager.LoadMaterial("Materials/BulletTrailMaterial");
                plumbLineRenderer.castShadows = false;
                plumbLineRenderer.receiveShadows = false;
            }
        }

        if (orderedVectorLineRenderer != null)
        {
            if (SceneManager.instance.GetComponent<VehicleSelector>().showIntentionLine)
            {
                orderedVectorLineRenderer.enabled = true;
                orderedVectorLineRenderer.SetWidth(defaultWidth, 0f);
                orderedVectorLineRenderer.SetPosition(0, position);
                orderedVectorLineRenderer.SetPosition(1, position + (Quaternion.Euler(-locomotor.orderedPitch, locomotor.orderedCourse, 0f) * Vector3.forward) * 200f);
            }
            else
            {
                orderedVectorLineRenderer.enabled = false;
            }
        }
        else
        {
            if (sVehicleCanBeTracked[typeName])
            {
                orderedVectorLineRenderer = this.transform.FindChild("Model").gameObject.AddComponent<LineRenderer>();
                orderedVectorLineRenderer.SetColors(new Color(1f, 0, 0f, 0.25f), Color.red);
                orderedVectorLineRenderer.SetVertexCount(2);
                orderedVectorLineRenderer.SetWidth(defaultWidth, 0f);
                orderedVectorLineRenderer.useWorldSpace = true;
                orderedVectorLineRenderer.material = ResourceManager.LoadMaterial("Materials/BulletTrailMaterial");
                orderedVectorLineRenderer.castShadows = false;
                orderedVectorLineRenderer.receiveShadows = false;
            }
        }
	}

    public virtual void OnFixedUpdate()
    {
        // Do your logic here.
    }

    public virtual void OnNewTrack(Track track, string source)
    {
        //Debug.Log(typeName + " has a new track " + track.vehicleTypeName + " via " + source + ".");
    }

    public virtual void OnTakeOff(Vehicle fromVehicle)
    {
        if (fromVehicle != null)
        {
            takeOffFrom = fromVehicle;
            Debug.Log(typeName + " takes-off from " + fromVehicle.typeName + ".");
        }
    }

    public virtual void ReturnToBase()
    {
        // For vehicles that are launched from air-strips.
        if(takeOffFrom != null)
            Debug.Log(typeName + " is RTB to " + takeOffFrom.typeName + ".");
    }

    public virtual List<string> GetVehicleInfo()
    {
        return null;
    }

    public virtual void OnVehicleControllerGUI()
    {
        return;
    }
}
