using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Data/PlayerStatusScriptable")]
public class PlayerStatusScriptable : MyScriptable<PlayerStatusScriptable>
{
    [SerializeField] public float maxJumpPower = 8000;
    [SerializeField] public float minJumpPower = 2000;
    [SerializeField] public float airGravityScale = 0;
    [SerializeField] public float guideMoveSpeed = 3;
    [SerializeField] public float gravityMultiplier = 1.8f;

    [SerializeField] public float walkSpeed = 2000;


    [Header("ジャンプの減速")]
    [SerializeField] public float jumpingGravityScale = 0;
    [SerializeField] public float jumpFraction = 0.99f;

    [Header("ジャンプ中かどうかの判定")]
    [SerializeField] public float maxJumpDuration = 1.5f;
    [SerializeField] public float jumpStopMagnitude = 0.001f;
    [Header("Input")]
    public float jumpStartInputMagnitude;
    public float jumpEnableDuration;
    public float jumpChargeTimeMax = 0.5f;


    public float moveInAir = 0.01f;
}
