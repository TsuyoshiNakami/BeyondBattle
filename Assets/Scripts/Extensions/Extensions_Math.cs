using System;
using UnityEngine;

public static class Extensions_Math
{

    // ...

    public static float VectorToRad(this Vector2 thisVec)
    {
        return Mathf.Atan2(thisVec.y, thisVec.x);
    }

    public static Vector2 RadToVector(this float thisFloat)
    {
        return new Vector2(Mathf.Cos(thisFloat), Mathf.Sin(thisFloat));
    }

    public static float VectorToDeg(this Vector2 thisVec)
    {
        return Mathf.Atan2(thisVec.y, thisVec.x) * Mathf.Rad2Deg;
    }

    public static Vector2 DegToVector(this float thisFloat)
    {
        return new Vector2(Mathf.Cos(thisFloat * Mathf.Deg2Rad), Mathf.Sin(thisFloat * Mathf.Deg2Rad));
    }

    // ...
}