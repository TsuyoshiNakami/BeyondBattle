using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;

public class JumpTestCharacter : MonoBehaviour
{
    PlayerInputProvider inputProvider;
    Vector2 StickDirection
    {
        get { return inputProvider.StickDirection; }
    }
    Vector2 oldStickDirection;
    Vector2 DefaultStickDirection { get { return transform.up; } }

    enum States
    {
        Walk,
        ReadyToJump,
        Jump,
        Air,
    }

    private WallType currentHoldingWallType;
    States state;
    private Vector3 lastWallPos;

    public Vector2 jumpDirection { get; private set; }
    Vector2 jumpGuideDirection;
    float jumpPower;
    Rigidbody2D rigid;

    [SerializeField] PlayerCollider playerCollider;

    float defaultGravityScale;
    PlayerStatusScriptable param;

    [SerializeField] PlayerJumpGuide jumpGuide;
    float afterStickMaxInputTime;
    private float jumpChargeTime;

    // Start is called before the first frame update
    void Start()
    {
        GetInstances();

        defaultGravityScale = param.gravityMultiplier * rigid.gravityScale;


        SubscribeEvents();

        Observable.
            EveryUpdate()
            //.Where(_ => inputProvider.GetButton(PlayerButton.Jump) == ButtonState.Down)
            .Subscribe(_ =>
            {
                ManageJump();
                ManageGravity();
                ManageAxisInput();

                ShowDebugUI();
                //bodyTransform.rotation = Quaternion.FromToRotation(Vector3.up, rigid.velocity);

                if (state != States.ReadyToJump)
                {
                    jumpGuide.HideLine();
                }
            });

        Observable.EveryLateUpdate()
            .Subscribe(_ =>
            {
                oldStickDirection = StickDirection;

                afterStickMaxInputTime += Time.deltaTime;
            });
    }

    private void ShowDebugUI()
    {
        GameObject.Find("DebugText").GetComponent<Text>().text = "stickMaxInputTime : " + afterStickMaxInputTime
                                                                //+ "\ninputLevel : " + inputLevel
                                                                //+ "\nisJumping : " + isJumping
                                                                + "\ngravity : " + rigid.gravityScale
                                                                + "\ncharge : " + jumpChargeTime
                                                                + "\nstate : " + state;
    }

    void GetInstances()
    {
        param = PlayerStatusScriptable.Entity;
        rigid = GetComponent<Rigidbody2D>();
        inputProvider = GetComponent<PlayerInputProvider>();

    }

    void SubscribeEvents()
    {
        playerCollider.OnStartHoldingWall.Subscribe(col =>
        {
            OnStartHoldingWall(col);
        });
        playerCollider.OnStayWall.Subscribe(col =>
        {
            OnStayWall(col);
        });
    }

    void ManageGravity()
    {
        switch (state)
        {
            case States.Walk:
                rigid.gravityScale = 0;
                rigid.velocity = Vector2.zero;
                break;
            case States.ReadyToJump:
                rigid.gravityScale = defaultGravityScale;
                break;
            case States.Jump:
                rigid.gravityScale = param.jumpingGravityScale;
                break;
            case States.Air:
                rigid.gravityScale = param.airGravityScale;
                break;
            default:
                //rigidbody.gravityScale = defaultGrabvityScale;
                break;
        }
    }

    void OnStartHoldingWall(Collision2D c)
    {

        currentHoldingWallType = WallType.Normal;
        state = States.Walk;
        lastWallPos = transform.position;
    }

    void OnStayWall(Collision2D col)
    {
        //Vector2 v = col.GetContact(0).normal;
        //currentMoveVec = Quaternion.Euler(0, 0, -90) * v;


        //Quaternion q = Quaternion.FromToRotation(transform.up, col.GetContact(0).normal);
        //transform.rotation *= q;
        //state = States.Walk;
        if (jumpingTime >= 0.5f)
        {
            state = States.Walk;
        }
    }


    void ManageAxisInput()
    {

        if (justJumped)
        {
            return;
        }
        float startAngle = Vector2.Angle(Vector2.right, jumpGuideDirection);
        if (jumpGuideDirection.y < 0)
        {
            startAngle *= -1;
        }

        Vector2 dir;// = Vector2.zero;
        float endAngle;// = 0;

        if (StickDirection != Vector2.zero)
        {
            dir = -StickDirection;
            state = States.ReadyToJump;

        }
        else
        {
            dir = DefaultStickDirection;

        }
        endAngle = Vector2.Angle(Vector2.right, dir);
        if (dir.y < 0)
        {
            endAngle *= -1;
        }
        float jumpGuideAngle = Mathf.LerpAngle(startAngle, endAngle, Time.deltaTime * 13);
        jumpGuideDirection = jumpGuideAngle.DegToVector();
        jumpGuide.DrawLine(transform.position, (Vector2)transform.position + jumpGuideDirection * 10);
    }

    void ManageJump()
    {

        float inputLevel = Mathf.Max(Mathf.Abs(oldStickDirection.x), Mathf.Abs(oldStickDirection.y));
        if (inputLevel >= param.jumpStartInputMagnitude)
        {
            afterStickMaxInputTime = 0;
        }

        if (StickDirection != Vector2.zero)
        {

            jumpChargeTime += Time.deltaTime;
        }

        if (state == States.ReadyToJump)
        {
            if (jumpChargeTime >= param.jumpChargeTimeMax)
            {
                GetComponentInChildren<SpriteRenderer>().color = Color.red;
            }
            else
            {
                var c = (new Color(1, 1, 0) * (1 - jumpChargeTime / param.jumpChargeTimeMax));
                c.a = 1;
                GetComponentInChildren<SpriteRenderer>().color = c;
            }
        }

        if (JumpRequired())
        {
            Jump();
        }
        if (state == States.Jump)
        {
            rigid.velocity = new Vector2(rigid.velocity.x * param.jumpFraction / 10000, rigid.velocity.y);

            jumpingTime += Time.deltaTime;
        }
    }

    bool justJumped;
    private float wallAndPlayerAngle;

    void Jump()
    {
        if (justJumped)
        {
            Debug.LogError("Multiple Jump required");
        }
        justJumped = true;
        Observable.Timer(TimeSpan.FromSeconds(0.3f))
            .Subscribe(_ =>
            {
                justJumped = false;
            });

        state = States.Jump;
        jumpDirection = jumpGuideDirection.normalized;
        if (jumpChargeTime >= param.jumpChargeTimeMax)
        {
            rigid.velocity = jumpDirection * param.maxJumpPower;

            GetComponentInChildren<SpriteRenderer>().color = Color.magenta;
        }
        else
        {
            rigid.velocity = jumpDirection * param.minJumpPower;
            GetComponentInChildren<SpriteRenderer>().color = Color.yellow;

        }

        jumpingTime = 0;
        jumpChargeTime = 0;

    }
    bool JumpRequired()
    {
        if (state != States.ReadyToJump)
        {
            return false;
        }
        if (StickDirection == Vector2.zero)
        {
            if (oldStickDirection != Vector2.zero)
            {
                if (param.jumpEnableDuration >= afterStickMaxInputTime)
                {
                    return true;
                }
                else
                {
                    jumpChargeTime = 0;
                }
            }
        }
        return false;

    }
}
