using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit")]
public class MouseOrbit : MonoBehaviour
{
    public Transform target;
    private float _distance = 10.0f;
    public float distance { get { return _distance; } set { _distance = Mathf.Clamp(value, zMinLimit, zMaxLimit); } }

    float xSpeed = 250.0f;
    float ySpeed = 120.0f;
    float zSpeed = 500.0f;

    public float yMinLimit = -20;
    public float yMaxLimit = 80;

    public float zMinLimit = 500;
    public float zMaxLimit = 10000;

    public bool useTailMode = false;

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Make the rigid body not change rotation  
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }

        distance = 200;
    }

    private void Update()
    {
        //var angles = transform.eulerAngles;
        //x = angles.x;
        //y = angles.y;
    }

    void LateUpdate()
    {
        if (target)
        {
            if (useTailMode == false)
            {
                if (Input.GetMouseButton(0))
                {
                    x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                    y = ClampAngle(y, yMinLimit, yMaxLimit);
                }
                else if (Input.GetMouseButton(1))
                {
                    distance -= Input.GetAxis("Mouse Y") * zSpeed * 0.02f;
                    distance = Mathf.Clamp(distance, zMinLimit, zMaxLimit);
                }

                var rotation = Quaternion.Euler(y, x, 0);
                var position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;

                transform.rotation = rotation;
                transform.position = position;
            }
            else
            {
                if (Input.GetMouseButton(1))
                {
                    distance -= Input.GetAxis("Mouse Y") * zSpeed * 0.02f;
                    distance = Mathf.Clamp(distance, zMinLimit, zMaxLimit);
                }
                x = target.rotation.eulerAngles.x;
                y = target.rotation.eulerAngles.y;

                var rotation = Quaternion.Euler(x, y, 0);
                var position = rotation * new Vector3(0.0f, distance * 0.1f, -distance) + target.position;

                transform.rotation = rotation;
                transform.position = position;
            }
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
        return Mathf.Clamp(angle, min, max);
    }
}