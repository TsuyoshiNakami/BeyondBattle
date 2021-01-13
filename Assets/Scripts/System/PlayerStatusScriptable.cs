using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Data/PlayerStatusScriptable")]
public class PlayerStatusScriptable : MyScriptable<PlayerStatusScriptable>
{
    [SerializeField] public float jumpPower = 1000;
    [SerializeField] public float jumpingGravityScale = 0;
    [SerializeField] public float guideMoveSpeed = 3;
    [SerializeField] public float jumpStopMagnitude = 0.001f;
    [SerializeField] public float maxJumpDuration = 1.5f;
    [SerializeField] public float gravityMultiplier = 1.8f;

    [SerializeField] public float walkSpeed = 2000;
    [SerializeField] public float walkSpeedWhileJumping = 100;

    [SerializeField] public float jumpFraction = 0.99f;
    public float moveInAir = 0.01f;
}
