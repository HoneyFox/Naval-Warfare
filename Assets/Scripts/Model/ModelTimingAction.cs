using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelTimingAction : MonoBehaviour 
{
    public float delay = 0f;
    public GameObject target = null;

    public bool destroy = true;
    public bool show = false;
    public bool hide = false;
    public Vector3 translate = Vector3.zero;
    public Vector3 rotate = Vector3.zero;
    
    private float timeOfStart;

    void Start () 
    {
        timeOfStart = Time.time;
	}

    void Update () 
    {
	    if(Time.time - timeOfStart > delay)
        {
            if(target == null)
            {
                target = this.gameObject;
            }

            if (show)
                target.SetActive(true);
            if (hide)
                target.SetActive(false);
            if (destroy)
                GameObject.Destroy(target);

            target.transform.localPosition += translate * Time.deltaTime;
            target.transform.Rotate(rotate * Time.deltaTime, Space.Self);
        }
	}
}
