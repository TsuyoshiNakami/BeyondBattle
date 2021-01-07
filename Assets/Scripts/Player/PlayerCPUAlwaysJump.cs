using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerCPUAlwaysJump : MonoBehaviour, PlayerInputProvider
{
    [SerializeField] PlayerCollider playerCollider;

    public Vector2 StickDirection { get {
            if (state == States.ReadyToJump)
            {
                if(nearest != null) { 
                jumpDir = nearest.transform.position - transform.position;
                    }else
                {
                    jumpDir = transform.up;
                }
                return jumpDir;
            }
            else
            {
                return transform.up;
            }

        } }

    [SerializeField] Vector2 jumpInterval = new Vector2(1, 2);
    float timeToJump = 1;
    float jumpTimer = 0;

        GameObject nearest = null;
    Vector2 jumpDir;
    Vector2 moveVec;
    public enum States
    {
        Decide,
        ReadyToJump,
        StartJumping,
        Jump
    }

    States state;


    public Vector2 GetAxis()
    {

            return moveVec;
    }

    public ButtonState GetButton(PlayerButton button)
    {
        string buttonName = button.ToString();

                    Debug.Log("cpu state : " + state);
        if(buttonName == "Jump")
        {
            switch (state)
            {
                case States.Decide:
                    break;
                case States.ReadyToJump:
                    if (jumpTimer == 0)
                    {
                        return ButtonState.Down;
                    }
                    return ButtonState.Hold;

                case States.StartJumping:
                return ButtonState.Up;

                case States.Jump:
                    break;
                default:
                    break;
            }
        }

        return ButtonState.None;
    }

    private void Update()
    {

        switch (state)
        {
            case States.Decide:
                DecideAction();

                break;
            case States.ReadyToJump:
                jumpTimer += Time.deltaTime;
                if(jumpTimer >= timeToJump)
                {
                    state = States.StartJumping;
                }
                break;
            case States.StartJumping:
                jumpTimer = 0;
                state = States.Jump;
                break;
            case States.Jump:
                JudgeHitWall();
                break;
            default:
                break;
        }
    }

    void DecideAction()
    {
        timeToJump = Random.Range(jumpInterval.x, jumpInterval.y);
        state = States.ReadyToJump;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float distance = 999;
        foreach(GameObject player in players)
        {
            if(player == gameObject) { continue; }
            float d = Vector2.Distance(transform.position, player.transform.position);
            if(d < distance)
            {
                nearest = player;
                distance = d;
            }
        }

    }

    void JudgeHitWall()
    {
        if( playerCollider.holdingCount >= 2)
        {
            state = States.Decide;
        }
        else if(!playerCollider.IsHoldingAnything && state == States.StartJumping)
        {
            state = States.Jump;
        }

        if( playerCollider.IsHoldingAnything && state == States.Jump)
        {
            state = States.Decide;
        }


    }
}
