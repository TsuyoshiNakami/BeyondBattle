using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

enum WallType
{
    Ground,
    Wall,
    Ceiling
}
public class Player : MonoBehaviour
{
    enum States
    {
        Walk,
        ReadyToJump,
        Jump,
    }

    [SerializeField] float walkSpeed;
    Rigidbody2D rigidbody;
    float defaultGrabvityScale;
    States state;

    Vector2 StickDirection
    {
        get { return inputProvider.StickDirection; }
    }

    // ジャンプ
    bool isJumping;
    public Vector2 jumpDirection { get; private set; }
    Vector2 jumpGuideDirection;
    float jumpTime;
    bool isJumpArmer;

    [SerializeField] PlayerJumpGuide jumpGuide;
    [SerializeField] float jumpPower;
    [SerializeField] float jumpingGravityScale;
    [SerializeField] float guideMoveSpeed;
    [SerializeField] float jumpStopMagnitude = 8;

    // 攻撃判定
    Vector2 oldVelocity;
    CameraMover cameraMover;


    // 接触判定
    [SerializeField] PlayerCollider playerCollider;

    // 入力
    [SerializeField] PlayerInputProvider inputProvider;


    Collider2D BodyCollider
    {
        get { return playerCollider.gameObject.GetComponent<Collider2D>(); }
    }

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        defaultGrabvityScale = rigidbody.gravityScale;
        inputProvider = GetComponent<PlayerInputProvider>();
        cameraMover = GameObject.Find("Main Camera").GetComponent<CameraMover>();
    }

    private void Start()
    {
        playerCollider.OnHitOtherPlayer.Subscribe(other =>
        {
            OnHitOtherPlayer(other);
        });
    }

    [SerializeField] float shakeDur = 0.5f;
    [SerializeField] float shakeStrength = 0.5f;

    void OnHitOtherPlayer(Player other)
    {

        if (state == States.Jump)
        {
            Vector2 otherVelocity = other.GetComponent<Rigidbody2D>().velocity;

            Player enemy = other.GetComponent<Player>();

            bool beat = WasHitStraighter(this, enemy);

            if (rigidbody.velocity.sqrMagnitude < otherVelocity.sqrMagnitude)
            {
                //    Destroy(gameObject);
            }
            else
            {
                Player a = this;
                Player b = enemy;
                Vector2 hitVec = b.transform.position - a.transform.position;
                enemy.NockBack(hitVec * oldVelocity.magnitude);
                //    Destroy(collision.gameObject);
                rigidbody.velocity = oldVelocity;
                cameraMover.Shake(shakeStrength, shakeDur);
                IgnorePlayerTemporarily(enemy);
            }
        }
    }

    void NockBack(Vector2 vec)
    {
        switch (state)
        {
            case States.Walk:
                ReleaseWall();
                break;
            case States.ReadyToJump:
                break;
            case States.Jump:
        rigidbody.AddForce(vec);
                break;
            default:
                break;
        }
    }

    void ReleaseWall()
    {
        Collider2D[] results = new Collider2D[3];
        Vector2 releaseDir = Vector2.zero;

        Physics2D.OverlapPointNonAlloc((Vector2)transform.position + Vector2.left * 1, results);

        foreach(Collider2D col in results)
        {
            if(col ==null)
            {
                continue;
            }

            if(col.gameObject.CompareTag("Wall"))
            {
        Debug.Log("Detect left");
                releaseDir += Vector2.right * 1;
            }
        }
        results[0] = null;
        results[1] = null;
        results[2] = null;

        Physics2D.OverlapPointNonAlloc((Vector2)transform.position + Vector2.right * 1, results);

        foreach(Collider2D col in results)
        {
            if(col ==null)
            {
                continue;
            }

            if(col.gameObject.CompareTag("Wall"))
            {     
        Debug.Log("Detect right");
                releaseDir += Vector2.left * 1;
            }
        }
        results[0] = null;
        results[1] = null;
        results[2] = null;
        Physics2D.OverlapPointNonAlloc((Vector2)transform.position + Vector2.up * 1, results);

        foreach(Collider2D col in results)
        {
            if(col ==null)
            {
                continue;
            }

            if(col.gameObject.CompareTag("Wall"))
            {
        Debug.Log("Detect up");
                releaseDir += Vector2.down * 1;
            }
        }
        results[0] = null;
        results[1] = null;
        results[2] = null;
        Physics2D.OverlapPointNonAlloc((Vector2)transform.position + Vector2.down * 1, results);

        foreach(Collider2D col in results)
        {
            if(col ==null)
            {
                continue;
            }

            if(col.gameObject.CompareTag("Wall"))
            {
        Debug.Log("Detect down");
                releaseDir += Vector2.up * 1;
            }
        }
        results[0] = null;
        results[1] = null;
        results[2] = null;
        Debug.Log("Release Dir : " + releaseDir);

        rigidbody.AddForce(releaseDir * 500);
    }

    void IgnorePlayerTemporarily(Player other)
    {
        StartCoroutine(IIgnorePlayerTemporarily(other));
    }

    IEnumerator IIgnorePlayerTemporarily(Player other)
    {
        Physics2D.IgnoreCollision(this.BodyCollider, other.BodyCollider, true);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(this.BodyCollider, other.BodyCollider, false);

    }

    bool WasHitStraighter(Player a, Player b)
    {
        Vector2 hitVec = b.transform.position - a.transform.position;
        //Debug.Log(Vector2.Angle(Vector2.right, hitVec));

        float aAngle = Vector2.Angle(hitVec, a.jumpDirection);
        float bAngle = Vector2.Angle(hitVec, b.jumpDirection);

        //Debug.Log(aAngle + ", " + bAngle);

        return aAngle >= bAngle;
    }

    void Update()
    {

        if (state == States.ReadyToJump && StickDirection != Vector2.zero)
        {
            DrawGuideLine();
        }
        else
        {
            HideGuideLine();
        }

        Move();
        ManageJump();
        ManageGravity();



        oldVelocity = rigidbody.velocity;
    }



    void HideGuideLine()
    {
        jumpGuide.HideLine();
    }

    void DrawGuideLine()
    {
        if (!jumpGuide.DrawEnabled)
        {
            jumpGuide.DrawLine(transform.position, StickDirection * 100);
            jumpGuideDirection = StickDirection;
        }

        float startAngle = Vector2.Angle(Vector2.right, jumpGuideDirection);
        if (jumpGuideDirection.y < 0)
        {
            startAngle *= -1;
        }

        float endAngle = Vector2.Angle(Vector2.right, StickDirection);
        if (StickDirection.y < 0)
        {
            endAngle *= -1;
        }
        float jumpGuideAngle = Mathf.LerpAngle(startAngle, endAngle, Time.deltaTime * guideMoveSpeed);
        jumpGuideDirection = jumpGuideAngle.DegToVector();
        jumpGuide.DrawLine(transform.position, jumpGuideDirection * 100);
    }

    void ManageJump()
    {

        if (jumpTime > 0.2f && (playerCollider.IsHoldingAnything || rigidbody.velocity.magnitude < jumpStopMagnitude))
        {
            jumpTime = 0;
            isJumping = false;
            state = States.Walk;
        }

        if (!isJumping && playerCollider.IsHoldingAnything)
        {
            if (inputProvider.GetButton(PlayerButton.Jump) == ButtonState.Down)
            {
                state = States.ReadyToJump;
            }
            if (inputProvider.GetButton(PlayerButton.Jump) == ButtonState.Up && state == States.ReadyToJump)
            {
                state = States.Jump;
                //Debug.Log("Jump");
                isJumping = true;
                isJumpArmer = true;
                jumpDirection = jumpGuideDirection.normalized;
                rigidbody.AddForce(jumpDirection * jumpPower);
                oldVelocity = rigidbody.velocity;
                //ResetHoldingStates();
            }
        }

        if (isJumping)
        {
            if (isJumpArmer)
            {
            }
            jumpTime += Time.deltaTime;
            //Debug.Log(rigidbody.velocity.magnitude);

        }
    }


    void ManageGravity()
    {
        //Debug.Log("holding wall : " + IsHoldingWall );
        if (playerCollider.IsHoldingWall)
        {
            rigidbody.gravityScale = 0;
        }
        else if (isJumping)
        {
            rigidbody.gravityScale = jumpingGravityScale;
        }
        else
        {
            rigidbody.gravityScale = defaultGrabvityScale;
        }
    }

    void Move()
    {
        Vector2 vec = Vector2.zero;

        //Debug.Log(state);
        if (state == States.Walk)
        {
            Vector2 input = inputProvider.GetAxis();
            float horizontal = input.x;
            float vertical = input.y;

            float moveHorizontal = !playerCollider.isHoldingWall ? horizontal : 0;
            float moveVertical = playerCollider.isHoldingWall ? vertical : 0;

            vec = new Vector2(moveHorizontal, moveVertical);
            vec.x = moveHorizontal * walkSpeed * Time.deltaTime;
            vec.y = moveVertical * walkSpeed * Time.deltaTime;
        }
        else
        {
        }

        if (vec != Vector2.zero)
        {
            float vecX = vec.x != 0 ? vec.x : rigidbody.velocity.x;
            float vecY = vec.y != 0 ? vec.y : rigidbody.velocity.y;

            rigidbody.velocity = new Vector2(vecX, vecY);
        }
        else
        {
            if (playerCollider.IsHoldingWall || state == States.ReadyToJump)

            {
                rigidbody.velocity *= 0.8f;
            }
            else if (state == States.Walk)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x * 0.8f, rigidbody.velocity.y);
            }
        }
    }





}
