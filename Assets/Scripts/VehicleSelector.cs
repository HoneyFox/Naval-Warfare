using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class VehicleSelector : MonoBehaviour
{
    public Vehicle selectedVehicle = null;
    public Camera vehicleCamera;
    private MouseOrbit mouseOrbit;

    public string selectedLauncherVehicle = "";

    public bool viewSide0 = true;
    public bool viewSide1 = true;
    public bool viewSideNeutral = true;
    public bool viewAir = true;
    public bool viewSurf = true;
    public bool viewSub = true;
    public bool viewNonWeapon = true;
    public bool viewWeapon = true;

    public bool showPlumbLine = true;
    public bool showIntentionLine = true;
    public bool showName = true;

    public Vector2 scrollPosition = Vector2.zero;

    void Start()
    {
        mouseOrbit = vehicleCamera.GetComponent<MouseOrbit>();
    }

    public void OnVehicleDead(Vehicle vehicle)
    { 
        if(selectedVehicle == vehicle)
        {
            // Move it back to root.
            //vehicleCamera.transform.parent = null;
            vehicleCamera.GetComponent<MouseOrbit>().target = null;
            selectedLauncherVehicle = "";
        }
    }
    
    public void SelectVehicle(Vehicle vehicle)
    {
        if (VehicleDatabase.sVehicleCanBeTracked[vehicle.typeName])
        {
            bool selected = (selectedVehicle == vehicle);
            bool nowSelected = (vehicle != null);
            if (selected && nowSelected == false)
            {
                //vehicleCamera.transform.parent = null;
                selectedVehicle = null;
                selectedLauncherVehicle = "";
                mouseOrbit.target = null;
            }
            else if (selected == false && nowSelected)
            {
                selectedVehicle = vehicle;
                selectedLauncherVehicle = "";
                vehicleCamera.transform.parent = vehicle.transform;
                vehicleCamera.transform.localPosition = new Vector3(500f, 500f, 500f);
                vehicleCamera.transform.LookAt(vehicle.transform);
                vehicleCamera.transform.parent = null;
                mouseOrbit.target = vehicle.transform;
                //mouseOrbit.UpdateState();
            }
        }
    }

    void Update()
    {
        mouseOrbit.enabled = (selectedVehicle != null);
    }

    private List<string> _launcherVehicles = new List<string>();
    private List<int> _launcherVehicleCount = new List<int>();
    void OnGUI()
    {
        int i = 0;
        Track trackToBeEngaged = null;
        if (selectedVehicle != null && selectedLauncherVehicle != "")
        {
            GUI.Label(new Rect(10f, 10f, 50f, 20f), "Targets:");
        }
        else
        {
            GUI.Label(new Rect(10f, 10f, 50f, 20f), "Units:");
        }

        viewAir = GUI.Toggle(new Rect(60f, 10f, 50f, 20f), viewAir, "Air", "button");
        viewSurf = GUI.Toggle(new Rect(120f, 10f, 50f, 20f), viewSurf, "Surf", "button");
        viewSub = GUI.Toggle(new Rect(180f, 10f, 50f, 20f), viewSub, "Sub", "button");
        viewWeapon = GUI.Toggle(new Rect(240f, 10f, 50f, 20f), viewWeapon, "Weap", "button");
        viewNonWeapon = GUI.Toggle(new Rect(300f, 10f, 50f, 20f), viewNonWeapon, "NoWeap", "button");
        viewSide0 = GUI.Toggle(new Rect(360f, 10f, 50f, 20f), viewSide0, "Side0", "button");
        viewSide1 = GUI.Toggle(new Rect(420f, 10f, 50f, 20f), viewSide1, "Side1", "button");
        viewSideNeutral = GUI.Toggle(new Rect(480f, 10f, 50f, 20f), viewSideNeutral, "Neutral", "button");
        showPlumbLine = GUI.Toggle(new Rect(540f, 10f, 50f, 20f), showPlumbLine, "PL");
        showIntentionLine = GUI.Toggle(new Rect(600f, 10f, 50f, 20f), showIntentionLine, "IL");
        showName = GUI.Toggle(new Rect(660f, 10f, 50f, 20f), showName, "Name");

        if (showName)
        {
            foreach (Vehicle vehicle in SceneManager.instance.vehicles)
            {
                float distSqr = Vector3.SqrMagnitude(vehicle.transform.position - vehicleCamera.transform.position);
                bool isFaraway = (distSqr > 25000f * 25000f);
                if (isFaraway) continue;
                bool isNotVeryClose = (distSqr > 5000f * 5000f);
                if (VehicleDatabase.sVehicleCanBeTracked[vehicle.typeName])
                {
                    Vector3 screenPoint = vehicleCamera.WorldToScreenPoint(vehicle.transform.position);
                    if (screenPoint.z > 0)
                    {
                        if (vehicle.side == 0)
                            GUI.contentColor = Color.blue;
                        else if (vehicle.side == 1)
                            GUI.contentColor = Color.red;
                        else if (vehicle.side == -1)
                            GUI.contentColor = Color.white;
                        if (isNotVeryClose)
                        {
                            float offsetX = -2f; float offsetY = -16f;
                            GUI.Label(new Rect(screenPoint.x + offsetX, Screen.height - screenPoint.y + offsetY, 200f, 30f), ".");
                        }
                        else
                        {
                            GUI.Label(new Rect(screenPoint.x, Screen.height - screenPoint.y, 200f, 30f), vehicle.typeName);
                        }
                    }
                }
            }
        } 
        GUI.contentColor = Color.white;

        GUILayout.BeginArea(new Rect(10f, 40f, 280f, Screen.height - 50f));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        foreach(Vehicle vehicle in SceneManager.instance.vehicles)
        {
            if (VehicleDatabase.sVehicleTypes[vehicle.typeName] == Vehicle.VehicleType.Air && viewAir == false) continue;
            if (VehicleDatabase.sVehicleTypes[vehicle.typeName] == Vehicle.VehicleType.Surf && viewSurf == false) continue;
            if (VehicleDatabase.sVehicleTypes[vehicle.typeName] == Vehicle.VehicleType.Sub && viewSub == false) continue;
            if (vehicle.GetComponent<WarheadModule>() != null && viewWeapon == false) continue;
            if (vehicle.GetComponent<WarheadModule>() == null && viewNonWeapon == false) continue;
            if (selectedLauncherVehicle == "")
            {
                if (vehicle.side == 0 && viewSide0 == false) continue;
                if (vehicle.side == 1 && viewSide1 == false) continue;
                if (vehicle.side == -1 && viewSideNeutral == false) continue;
            }

            if(VehicleDatabase.sVehicleCanBeTracked[vehicle.typeName])
            {
                if(selectedVehicle != null && selectedLauncherVehicle != "")
                {
                    if (vehicle.isDead) continue;
                    if (vehicle.side == -1 || vehicle.side == selectedVehicle.side) continue;
                    if (selectedVehicle.sensorCtrl == null) continue;
                    Track track = null;
                    foreach (Track trk in selectedVehicle.sensorCtrl.tracksDetected)
                    {
                        if (trk.target == vehicle)
                        {
                            track = trk;
                            break;
                        }
                    }
                    if (track == null) continue; 
                    if (selectedVehicle.launcherCtrl.CanEngageWith(track, selectedLauncherVehicle, false) == false) continue;

                    bool engage = GUILayout.Button(vehicle.typeName, GUILayout.Height(15f));
                    if(engage)
                    {
                        trackToBeEngaged = track;
                    }
                    i++;
                }
                else
                {
                    bool selected = selectedVehicle == vehicle;
                    bool nowSelected = (GUILayout.Toggle(selectedVehicle == vehicle, vehicle != null ? vehicle.typeName + (vehicle.isDead ? "(Dead)" : "") : "", "button", GUILayout.Height(15f)));
                    if(selected && nowSelected == false)
                    {
                        //vehicleCamera.transform.parent = null;
                        selectedVehicle = null;
                        mouseOrbit.target = null;
                    }
                    else if(selected == false && nowSelected)
                    {
                        selectedVehicle = vehicle;
                        vehicleCamera.transform.parent = vehicle.transform;
                        vehicleCamera.transform.localPosition = new Vector3(500f, 500f, 500f);
                        vehicleCamera.transform.LookAt(vehicle.transform);
                        vehicleCamera.transform.parent = null;
                        mouseOrbit.target = vehicle.transform;
                        //mouseOrbit.UpdateState();
                    }
                    ++i;
                }
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        if(trackToBeEngaged != null)
        {
            selectedVehicle.launcherCtrl.Launch(selectedLauncherVehicle, trackToBeEngaged);
            if (selectedVehicle.launcherCtrl.CanEngageWith(trackToBeEngaged, selectedLauncherVehicle, false, true) == false)
                selectedLauncherVehicle = "";
        }

        bool hasLauncherCtrl = (selectedVehicle != null && selectedVehicle.launcherCtrl != null);
        bool hasAvailableLauncher = false;
        if (hasLauncherCtrl)
        {
            foreach (Launcher launcher in selectedVehicle.launcherCtrl.launchers)
            {
                if (launcher.enabled)
                {
                    int vehicleIndex = launcher.vehicleNames.IndexOf(selectedLauncherVehicle);
                    if (vehicleIndex >= 0 && launcher.vehicleCounts[vehicleIndex] > 0)
                    {
                        hasAvailableLauncher = true;
                        break;
                    }
                }
            }
        }
        if (!hasAvailableLauncher)
        {
            // No launcher available for the selected weapon. (perhaps due to damage)
            selectedLauncherVehicle = "";
        }

        if(selectedVehicle != null)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 310f, 10f, 300f, 1500f));
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Unit: " + selectedVehicle.typeName + " (" + selectedVehicle.armorModule.armorPoint.ToString("F0") + "/" + selectedVehicle.armorModule.maxArmorPoint.ToString("F0") + ")" + " Side: " + selectedVehicle.side.ToString());

                    GuidanceModule guidanceModule = selectedVehicle.GetComponent<GuidanceModule>();
                    if (guidanceModule != null)
                    {
                        GUILayout.Label("Guidance: " + guidanceModule.GetType().Name);
                        if (guidanceModule.targetTrack != null)
                            GUILayout.Label(guidanceModule.targetTrack.vehicleTypeName + "(" + guidanceModule.targetTrack.age.ToString("F0") + "): " + Vector3.Distance(selectedVehicle.position, guidanceModule.targetTrack.predictedPosition).ToString("F0"));
                        else if(guidanceModule.guidanceParameter is Vector3)
                            GUILayout.Label(((Vector3)guidanceModule.guidanceParameter).ToString() + ": " + Vector3.Distance((Vector3)guidanceModule.guidanceParameter, selectedVehicle.position).ToString("F0"));
                    }

                    selectedVehicle.OnVehicleControllerGUI();
                    
                    if(selectedVehicle.launcherCtrl != null && selectedVehicle.launcherCtrl.launchers.Count > 0)
                    {
                        GUILayout.Label("Weapon: ");
                        _launcherVehicles.Clear();
                        _launcherVehicleCount.Clear();
                        foreach (Launcher launcher in selectedVehicle.launcherCtrl.launchers)
                        {
                            if (launcher.enabled == false) continue;
                            for (int j = 0; j < launcher.vehicleNames.Count; ++j)
                            {
                                if (_launcherVehicles.Contains(launcher.vehicleNames[j]))
                                {
                                    _launcherVehicleCount[_launcherVehicles.IndexOf(launcher.vehicleNames[j])] += launcher.vehicleCounts[j];
                                }
                                else
                                {
                                    _launcherVehicles.Add(launcher.vehicleNames[j]);
                                    _launcherVehicleCount.Add(launcher.vehicleCounts[j]);
                                }
                            }
                        }

                        if (_launcherVehicles.Count > 0)
                        {
                            for (int j = 0; j < _launcherVehicles.Count; ++j)
                            {
                                bool wasSelected = selectedLauncherVehicle == _launcherVehicles[j];
                                if (_launcherVehicleCount[j] == 0)
                                {
                                    if (wasSelected)
                                        selectedLauncherVehicle = "";
                                    continue;
                                }

                                if (GUILayout.Toggle(wasSelected, _launcherVehicles[j] + " x" + _launcherVehicleCount[j].ToString(), "button", GUILayout.Height(15f)))
                                {
                                    selectedLauncherVehicle = _launcherVehicles[j];
                                }
                                else
                                {
                                    if (wasSelected)
                                        selectedLauncherVehicle = "";
                                }
                            }
                        }
                        else
                        {
                            selectedLauncherVehicle = "";
                        }
                    }

                    bool hasAirStrip = selectedVehicle.airstripCtrl != null && selectedVehicle.airstripCtrl.airStrips.Count > 0;
                    bool hasAircrafts = false;
                    if(hasAirStrip)
                    {
                        foreach (int vehicleCount in selectedVehicle.airstripCtrl.vehicleCounts)
                        {
                            if (vehicleCount > 0)
                            {
                                hasAircrafts = true;
                                break;
                            }
                        }
                        if (hasAircrafts == false)
                        {
                            foreach (Airstrip airStrip in selectedVehicle.airstripCtrl.airStrips)
                            {
                                if (airStrip.vehicleAttached != null && (airStrip.vehicleIsDeploying || airStrip.vehicleIsLaunching))
                                {
                                    hasAircrafts = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (hasAircrafts)
                    {
                        GUILayout.Label("Flight Deck:");
                        for(int n = 0; n < selectedVehicle.airstripCtrl.vehicles.Count; ++n)
                        {
                            if (selectedVehicle.airstripCtrl.vehicleCounts[n] == 0) continue;
                            bool launch = GUILayout.Button(selectedVehicle.airstripCtrl.vehicles[n] + " x" + selectedVehicle.airstripCtrl.vehicleCounts[n].ToString(), "button", GUILayout.Height(15f));
                            if(launch)
                            {
                                Airstrip airStrip;
                                selectedVehicle.airstripCtrl.PrepareForTakeOff(selectedVehicle.airstripCtrl.vehicles[n], out airStrip);
                            }
                        }
                        for(int n = 0; n < selectedVehicle.airstripCtrl.airStrips.Count; ++n)
                        {
                            Airstrip a = selectedVehicle.airstripCtrl.airStrips[n];
                            if (a.vehicleAttached == null) continue;
                            string stripState = "";
                            if (a.vehicleIsDeploying && a.vehicleIsUndeploying == false)
                                stripState = "Preparing";
                            else if (a.vehicleIsDeploying && a.vehicleIsUndeploying)
                                stripState = "Canceling";
                            else if (a.vehicleIsLaunching)
                                stripState = "Launching";
                            else
                                stripState = "Landing";
                            if(a.vehicleIsDeploying && a.vehicleProgress == 100f)
                            {
                                if (VehicleDatabase.sVehicleIsDeployOnly.ContainsKey(a.vehicleAttached.typeName) && VehicleDatabase.sVehicleIsDeployOnly[a.vehicleAttached.typeName] == true)
                                {
                                    bool retract = GUILayout.Button("Retract " + a.vehicleAttached.typeName, "button", GUILayout.Height(15f));
                                    if (retract)
                                    {
                                        selectedVehicle.airstripCtrl.CancelTakeOff(a);
                                    }
                                }
                                else
                                {
                                    GUILayout.BeginHorizontal();
                                    {
                                        bool launch = GUILayout.Button(a.vehicleAttached.typeName + " (Ready to Launch)", "button", GUILayout.Height(15f));
                                        if (launch)
                                        {
                                            selectedVehicle.airstripCtrl.TakeOff(a);
                                        }
                                        bool cancelLaunch = GUILayout.Button("Cancel", "button", GUILayout.Height(15f), GUILayout.Width(70f));
                                        if (cancelLaunch)
                                        {
                                            selectedVehicle.airstripCtrl.CancelTakeOff(a);
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                            else
                                GUILayout.Label(a.vehicleAttached.typeName + " (" + stripState + "...)", "button", GUILayout.Height(15f));
                        }
                    }

                    if (selectedVehicle.takeOffFrom != null && selectedVehicle.enabled && selectedVehicle is Aircraft)
                    {
                        bool rtb = GUILayout.Toggle((selectedVehicle as Aircraft).isRTB, "Return to Base", "button", GUILayout.Height(15f));
                        if (rtb && (selectedVehicle as Aircraft).isRTB == false)
                        {
                            selectedVehicle.ReturnToBase();
                        }
                        else if (rtb == false)
                        {
                            if ((selectedVehicle as Aircraft).isRTB == true)
                            {
                                bool landSiteFound = false;
                                foreach (Vehicle v in SceneManager.instance.vehicles)
                                {
                                    if (v.airstripCtrl != null)
                                    {
                                        foreach (Airstrip a in v.airstripCtrl.airStrips)
                                        {
                                            if (a.vehicleAttached == selectedVehicle)
                                            {
                                                a.vehicleAttached = null;
                                                a.vehicleIsLanding = a.vehicleIsUndeploying = false;
                                                landSiteFound = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (landSiteFound)
                                        break;
                                }

                                (selectedVehicle as Aircraft).isRTB = false;
                                (selectedVehicle as Aircraft).isAligned = false;
                            }
                        }
                    }

                    if (selectedVehicle.sensorCtrl != null && selectedVehicle.sensorCtrl.sensors.Count > 0)
                    {
                        GUILayout.Label("Sensor:");
                        foreach (Sensor sensor in selectedVehicle.sensorCtrl.sensors)
                        {
                            bool toggle = GUILayout.Toggle(sensor.isToggled && sensor.enabled, sensor.sensorName + (sensor.enabled ? "" : " (Damaged)"), "button", GUILayout.Height(15f));
                            if (sensor.isToggleable && sensor.enabled)
                            {
                                selectedVehicle.sensorCtrl.Toggle(sensor.sensorName, toggle);
                            }
                        }
                    }

                    GUILayout.Label("Tactics:");
                    selectedVehicle.autoDealWithAirThreats = GUILayout.Toggle(selectedVehicle.autoDealWithAirThreats, "Auto Deal With Air Threats");
                    selectedVehicle.autoDealWithSubThreats = GUILayout.Toggle(selectedVehicle.autoDealWithSubThreats, "Auto Deal With Sub Threats");
                    selectedVehicle.autoEngageAirTracks = GUILayout.Toggle(selectedVehicle.autoEngageAirTracks, "Auto Engage Air Tracks");
                    selectedVehicle.autoEngageSurfTracks = GUILayout.Toggle(selectedVehicle.autoEngageSurfTracks, "Auto Engage Surf Tracks");
                    selectedVehicle.autoEngageSubTracks = GUILayout.Toggle(selectedVehicle.autoEngageSubTracks, "Auto Engage Sub Tracks");

                    if (selectedVehicle.GetVehicleInfo() != null)
                    {
                        foreach (string line in selectedVehicle.GetVehicleInfo())
                        {
                            GUILayout.Label(line);
                        }
                    }

                    if (selectedVehicle.sensorCtrl != null && selectedVehicle.sensorCtrl.sensors.Count > 0)
                    {
                        GUILayout.Label("Tracks:");
                        foreach (Track track in selectedVehicle.sensorCtrl.tracksDetected)
                        {
                            GUILayout.Label(track.vehicleTypeName + "(" + track.age.ToString("F0") + "): " + track.identification.ToString() + ", " + Vector3.Distance(track.predictedPosition, selectedVehicle.position).ToString("F0"));
                        }
                    }

                }
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(Screen.width / 2f - 100f, Screen.height - 50f, 200f, 40f));
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.RepeatButton("Zoom-", GUILayout.Height(30f)))
                    {
                        vehicleCamera.GetComponent<MouseOrbit>().distance += 20f;
                    }
                    if (GUILayout.RepeatButton("Zoom+", GUILayout.Height(30f)))
                    {
                        vehicleCamera.GetComponent<MouseOrbit>().distance -= 20f;
                    }
                    vehicleCamera.GetComponent<MouseOrbit>().useTailMode = GUILayout.Toggle(vehicleCamera.GetComponent<MouseOrbit>().useTailMode, "Tail", "button", GUILayout.Height(30f));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
