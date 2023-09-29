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

    public bool[] canEngage { get { return VehicleDatabase.sVehicleCanEngage[typeName]; } }
    public bool[] canBeEngaged { get { return VehicleDatabase.sVehicleCanBeEngaged[typeName]; } }

    private LauncherController cachedLauncherCtrl;
    public LauncherController launcherCtrl {
        get { 
            if (cachedLauncherCtrl == null) cachedLauncherCtrl = this.gameObject.GetComponent<LauncherController>();
            return cachedLauncherCtrl;
        }
    }
    private SensorController cachedSensorCtrl;
    public SensorController sensorCtrl
    {
        get
        {
            if (cachedSensorCtrl == null) cachedSensorCtrl = this.gameObject.GetComponent<SensorController>();
            return cachedSensorCtrl;
        }
    }
    private AirstripController cachedAirstripCtrl;
    public AirstripController airstripCtrl
    {
        get
        {
            if (cachedAirstripCtrl == null) cachedAirstripCtrl = this.gameObject.GetComponent<AirstripController>();
            return cachedAirstripCtrl;
        }
    }
    private ArmorModule cachedArmorModule;
    public ArmorModule armorModule
    {
        get
        {
            if (cachedArmorModule == null) cachedArmorModule = this.gameObject.GetComponent<ArmorModule>();
            return cachedArmorModule;
        }
    }
    private GuidanceModule cachedGuidanceModule;
    public GuidanceModule guidanceModule
    {
        get
        {
            if (cachedGuidanceModule == null) cachedGuidanceModule = this.gameObject.GetComponent<GuidanceModule>();
            return cachedGuidanceModule;
        }
    }
    private WarheadModule cachedWarheadModule;
    public WarheadModule warheadModule
    {
        get
        {
            if (cachedWarheadModule == null) cachedWarheadModule = this.gameObject.GetComponent<WarheadModule>();
            return cachedWarheadModule;
        }
    }

    public bool isDead { get { return armorModule != null ? armorModule.armorPoint <= 0f : false; } }

    public bool autoEngageAirTracks = false;
    public bool autoDealWithAirThreats = true;
    public bool autoEngageSurfTracks = false;
    public bool autoEngageSubTracks = false;
    public bool autoDealWithSubThreats = true;

    public Vehicle takeOffFrom = null;
    public float mastHeight = 0f;
    public float size = 1f;
    
    public Vehicle onlyVisibleTo = null;

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

        Camera cameraOrbit = SceneManager.instance.vehicleSelector.vehicleCamera;
        float distance = Vector3.Distance(cameraOrbit.transform.position, position);
        float cameraScale = distance * 0.01f;
        float defaultWidth = 0.2f;
        defaultWidth *= cameraScale;
        if (plumbLineRenderer != null)
        {
            if (SceneManager.instance.vehicleSelector.showPlumbLine && distance <= 5000f)
            {
                plumbLineRenderer.enabled = true;
                plumbLineRenderer.startWidth = plumbLineRenderer.endWidth = defaultWidth;
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
            if (VehicleDatabase.sVehicleCanBeTracked[typeName])
            {
                plumbLineRenderer = this.gameObject.AddComponent<LineRenderer>();
                plumbLineRenderer.startColor = new Color(1f, 1f, 0f, 0.25f); plumbLineRenderer.endColor = Color.yellow;
                plumbLineRenderer.positionCount = 2;
                plumbLineRenderer.startWidth = plumbLineRenderer.endWidth = defaultWidth;
                plumbLineRenderer.useWorldSpace = true;
                plumbLineRenderer.material = ResourceManager.LoadMaterial("Materials/BulletTrailMaterial");
                plumbLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                plumbLineRenderer.receiveShadows = false;
            }
        }

        if (orderedVectorLineRenderer != null)
        {
            if (SceneManager.instance.vehicleSelector.showIntentionLine && distance <= 5000f)
            {
                orderedVectorLineRenderer.enabled = true;
                orderedVectorLineRenderer.startWidth = defaultWidth; orderedVectorLineRenderer.endWidth = 0f;
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
            if (VehicleDatabase.sVehicleCanBeTracked[typeName])
            {
                orderedVectorLineRenderer = this.transform.Find("Model").gameObject.AddComponent<LineRenderer>();
                orderedVectorLineRenderer.startColor = new Color(1f, 0, 0f, 0.25f); orderedVectorLineRenderer.endColor = Color.red;
                orderedVectorLineRenderer.positionCount = 2;
                orderedVectorLineRenderer.startWidth = defaultWidth; orderedVectorLineRenderer.endWidth = 0f;
                orderedVectorLineRenderer.useWorldSpace = true;
                orderedVectorLineRenderer.material = ResourceManager.LoadMaterial("Materials/BulletTrailMaterial");
                orderedVectorLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
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
