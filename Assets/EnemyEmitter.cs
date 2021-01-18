using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Random = UnityEngine.Random;

public class EnemyEmitter : MonoBehaviour
{
    [SerializeField] float interval;
    [SerializeField] GameObject enemy;
    // Start is called before the first frame update
    void Start()
    {
        Observable.Interval(TimeSpan.FromSeconds(interval))
            .Subscribe(_ =>
            {
                Vector3 offset = new Vector3(Random.Range(-3, 3),Random.Range(-3, 3));
                Instantiate(enemy,transform.position + offset, Quaternion.identity);
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
