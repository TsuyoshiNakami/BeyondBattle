using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonobehavior<T> : MonoBehaviour where T : SingletonMonobehavior<T>
{
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = GameObject.FindObjectOfType<T>();
            if (instance == null)
            {
                instance = new GameObject(typeof(T).Name).AddComponent<T>();
            }
            return instance;
        }
    }

    public bool dontDestoryOnLoad;

    private void Awake()

    {
        if (instance == null)
        {
            instance = GetComponent<T>();
        }
        if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
