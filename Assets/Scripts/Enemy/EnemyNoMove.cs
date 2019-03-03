using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNoMove : MonoBehaviour, Damageable
{
    CameraMover cameraMover;
    public void Damage(int damage)
    {
        //cameraMover.Shake(0.05f, 0.5f);
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraMover = ObjectFinder.GetCameraMover();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
