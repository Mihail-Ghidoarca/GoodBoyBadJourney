using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    public static List<GameObject> dontDestroyObjs = new List<GameObject>();
    void Awake()
    {
        foreach (var obj in dontDestroyObjs)
        {
            if (gameObject.name == obj.name && gameObject != obj)
            {
                Destroy(gameObject);
                return;
            }
        }
        dontDestroyObjs.Add(gameObject);
        DontDestroyOnLoad(gameObject);
    }
}