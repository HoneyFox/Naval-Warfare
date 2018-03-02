using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ResourceManager
{
    public static Material LoadMaterial(string path)
    {
        return (Material)Material.Instantiate(Resources.Load<Material>(path));
    }

    public static GameObject LoadPrefab(string path)
    {
        return (GameObject)GameObject.Instantiate(Resources.Load<GameObject>(path));
    }
}

