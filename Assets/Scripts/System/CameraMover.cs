using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraMover : MonoBehaviour
{
    [SerializeField] GameObject followObj;
    [SerializeField] Vector2 cameraOffset;
    [SerializeField] float lerpSpeed = 10;
    [SerializeField] GameObject cameraObj;
    Vector2 oldPosition;
    // Start is called before the first frame update
    void Start()
    {
        oldPosition = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        Vector2 pos = Vector2.Lerp(oldPosition, followObj.transform.position, Time.deltaTime * lerpSpeed);
        oldPosition = pos;
        transform.position = new Vector3(pos.x, pos.y, -10);
        transform.position += (Vector3)cameraOffset;
    }

    public void Shake(float strength, float duration, int vibrato = 10)
    {
        cameraObj.transform.DOShakePosition(duration, strength, vibrato);
    }
}
