using UnityEngine;
using System.Collections;

public class WarheadModule : MonoBehaviour 
{
    public Vehicle self
    {
        get { return this.gameObject.GetComponent<Vehicle>(); }
    }

    public float safetyTimer = 5f;
    public float timeOfLaunch = 0f;

    public float damageAmount;
    public float damageRadius;
    public float proximityRange;
    public Track target;
    public Vector3 targetPosition;
    public AnimationCurve damageCurve;

    public bool isTrackBased = true;

    public GameObject explosionFX;
    public float explosionFXVisibleRange = 1000f;

    public void SetupTarget(Track target)
    {
        this.target = target;
        this.targetPosition = Vector3.zero;
        //isTrackBased = true;
    }

    public void SetupTarget(Vector3 targetPosition)
    {
        this.target = null;
        this.targetPosition = targetPosition;
        //isTrackBased = false;
    }

	// Use this for initialization
	void Start ()
    {
        timeOfLaunch = Time.time;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (isTrackBased)
        {
            if (target != null)
            {
                if (target.target != null)
                {
                    if (Vector3.Distance(target.target.position, self.position) - target.target.size < proximityRange)
                    {
                        // Kaboom!
                        Ignite();
                    }
                }
                else if (Vector3.Distance(target.predictedPosition, self.position) < proximityRange)
                {
                    // Kaboom!
                    Ignite();
                }
            }
        }
        else
        {
            if (Vector3.Distance(targetPosition, self.position) < proximityRange)
            {
                // Kaboom!
                Ignite();
            }
        }
	}

    public void Ignite()
    {
        if (Time.time > timeOfLaunch + safetyTimer)
            Explode();
        else
            Shutdown();
    }

    public virtual void Explode()
    {
        if(explosionFX != null)
        {
            if (Vector3.Distance(SceneManager.instance.GetComponent<VehicleSelector>().vehicleCamera.transform.position, self.transform.position) < explosionFXVisibleRange)
            {
                GameObject explosionFXObj = (GameObject)GameObject.Instantiate(explosionFX);
                explosionFXObj.transform.parent = self.transform;
                explosionFXObj.transform.localPosition = Vector3.zero;
                explosionFXObj.transform.parent = SceneManager.instance.transform;
                explosionFXObj.GetComponent<ParticleSystem>().Play(true);
            }
        }

        if(target != null)
        {
            if(target.target != null)
            {
                float actualDistance = Vector3.Distance(target.target.position, this.transform.position);
                float damage = damageAmount * damageCurve.Evaluate(Mathf.Max(0f, actualDistance - target.target.size) / damageRadius);
                ArmorModule armor = target.target.GetComponent<ArmorModule>();

                if(self.isDead == false)
                    Debug.Log(self.typeName + " does " + Mathf.RoundToInt(damage).ToString() + " damage to " + target.vehicleTypeName);

                armor.DoDamage(damage);
            }
        }
        else
        {
            foreach (Vehicle v in SceneManager.instance.vehicles)
            {
                if (v != self && Vector3.Distance(v.position, this.transform.position) < proximityRange)
                {
                    float actualDistance = Vector3.Distance(v.position, this.transform.position);
                    float damage = damageAmount * damageCurve.Evaluate(Mathf.Max(0f, actualDistance - v.size) / damageRadius);
                    ArmorModule armor = v.GetComponent<ArmorModule>();

                    if (self.isDead == false)
                        Debug.Log(self.typeName + " does " + Mathf.RoundToInt(damage).ToString() + " damage to " + v.typeName);

                    armor.DoDamage(damage);
                }
            }
        }

        Shutdown();
    }

    public void Shutdown()
    {
        SceneManager.instance.GetComponent<VehicleSelector>().OnVehicleDead(self);
        SceneManager.instance.vehicles.Remove(self);
        GameObject.Destroy(self.gameObject);
    }
    
}
