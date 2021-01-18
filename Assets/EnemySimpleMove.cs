using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Random = UnityEngine.Random;

public class EnemySimpleMove : MonoBehaviour, Damageable
{
    Rigidbody2D rigid;
    [SerializeField] float speed;
    public void Damage(int damage)
    {
        //cameraMover.Shake(0.05f, 0.5f);
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        rigid.velocity = new Vector2(Random.Range(-1f, 1), Random.Range(-1f, 1)) * speed;


        Observable
            .Interval(TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                rigid.velocity = new Vector2(Random.Range(-1f, 1), Random.Range(-1f, 1)) * speed;
            }).AddTo(this);
    }
}