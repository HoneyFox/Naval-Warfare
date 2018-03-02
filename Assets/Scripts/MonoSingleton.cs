using UnityEngine;
using System.Collections;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T sInstance;

    private static bool sIsQuitting = false;

    private static object sLock = new object();

    public static T instance
    {
        get
        {
            if (sIsQuitting)
            {
                Debug.LogWarning("application is quitting");
                return null;
            }

            lock (sLock)
            {
                if (sInstance == null)
                {
                    sInstance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError("Found more than one object!");
                        return sInstance;
                    }

                    if (sInstance == null)
                    {
                        GameObject singleton = new GameObject();
                        sInstance = singleton.AddComponent<T>();

                        singleton.name = "(singleton) " + typeof(T).ToString();
                        DontDestroyOnLoad(singleton);
#if DEBUGLOG
						Debug.Log("create an instance of " +  typeof(T).ToString());
#endif
                    }
                    else
                    {
#if DEBUGLOG
						Debug.Log("Find the instance of " + typeof(T).ToString() + " : " + sInstance.gameObject.name);
#endif
                    }
                }

                return sInstance;
            }
        }
    }

    public void OnDestroy()
    {
        sIsQuitting = true;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}


