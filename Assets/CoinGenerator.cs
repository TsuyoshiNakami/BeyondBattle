using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinGenerator : MonoBehaviour
{
    [SerializeField] Vector2 generateInterval;
    [SerializeField] Vector2 generateDistance;
    [SerializeField] GameObject coinObj;
    float timer;
    float generateTime;

    int withoutItemLayer;

    void Start()
    {
        withoutItemLayer = ~( 1 << LayerMask.NameToLayer("Item"));

        Debug.Log(LayerMask.NameToLayer("Item"));
        SetGenerateTime();
    }

    void SetGenerateTime()
    {
        generateTime = Random.Range(generateInterval.x, generateInterval.y);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= generateTime)
        {
            timer -= generateTime;
            SetGenerateTime();
            GenerateCoin();
        }
    }

    void GenerateCoin()
    {
        float angle = Random.Range(0, 360);
        float distance = Random.Range(generateDistance.x, generateDistance.y);

        Vector2 vec = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;

        RaycastHit2D ray = Physics2D.Raycast(transform.position, vec, distance, withoutItemLayer);
        Debug.Log(ray.point);
        if (ray.collider != null)
        {
            Instantiate(coinObj, ray.point, Quaternion.identity);
        } else
        {
            Instantiate(coinObj, transform.position + (Vector3)vec * distance, Quaternion.identity);
        }
    }
}
