using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectFinder
{
    static CameraMover cameraMover;

    public static CameraMover GetCameraMover()
    {
        if(cameraMover == null)
        {
            cameraMover = GameObject.Find("CameraParent").GetComponent<CameraMover>();
        }

        return cameraMover;
    }
}
