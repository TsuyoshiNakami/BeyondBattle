using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerCPUAI
{

}





public class PlayerCPUInput : MonoBehaviour, PlayerInputProvider
{
    [SerializeField] PlayerCollider playerCollider;

    public Vector2 StickDirection { get { return moveVec; } }

    Vector2 moveVec = new Vector2();
    public enum States
    {
        Decide,
        Walk
    }

    States state;


    public Vector2 GetAxis()
    {
        return moveVec;
    }

    public ButtonState GetButton(PlayerButton button)
    {
        return ButtonState.None;
    }

    private void Update()
    {

        switch (state)
        {
            case States.Decide:
                DecideAction();

                break;
            case States.Walk:
                JudgeHitWall();
                break;
            default:
                break;
        }
    }

    void DecideAction()
    {
        moveVec = Vector2.zero;
        if (playerCollider.isHoldingWall)
        {
            moveVec.y = Random.Range(0, 2) < 1 ? -1 : 1;

        }
        else
        {
            moveVec.x = Random.Range(0, 2) < 1 ? -1 : 1;
        }

        state = States.Walk;
    }

    void JudgeHitWall()
    {
        if( playerCollider.holdingCount >= 2)
        {
            state = States.Decide;
        }


    }
}
