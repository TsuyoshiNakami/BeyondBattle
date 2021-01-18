using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;

public class CameraManager : SingletonMonobehavior<CameraManager>
{
    public ProCamera2DShake shake;

    // Start is called before the first frame update
    void Start()
    {
        shake = GetComponent<ProCamera2DShake>();
    }
}
