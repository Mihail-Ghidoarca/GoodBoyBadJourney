using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    public static List<GameObject> dontDestroyObjs = new List<GameObject>();
    void Awake()
    {
        //dontDestroyObjs.Add(gameObject);
        //foreach (var obj in dontDestroyObjs)
        //{
        //    if (gameObject && gameObject.name == obj.name && gameObject != obj)
        //    {
        //        return;
        //    }
        //}
        DontDestroyOnLoad(gameObject);  
        DontDestroyOnLoad(gameObject.transform);
        
    }
}