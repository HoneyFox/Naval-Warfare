using UnityEngine;
using System.Collections;

public class SeaPlane : MonoBehaviour 
{
    public float timeScale = 0.4f;
    public float waveHeight = 2f;

    private Camera focusOnCamera;

	// Use this for initialization
	void Start ()
    {
	    focusOnCamera = SceneManager.instance.GetComponent<VehicleSelector>().vehicleCamera;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (focusOnCamera != null)
            this.transform.position = new Vector3(focusOnCamera.transform.position.x, Mathf.Sin(Time.time * timeScale) * waveHeight / 2f, focusOnCamera.transform.position.z);
        else
            this.transform.position = new Vector3(this.transform.position.x, Mathf.Sin(Time.time * timeScale) * waveHeight / 2f, this.transform.position.z);
	}
}
