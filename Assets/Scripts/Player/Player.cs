using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

enum WallType
{
    None,
    Normal,
    Slip
}
public class Player : MonoBehaviour, Damageable
{
    enum States
    {
        Walk,
        ReadyToJump,
        Jump,
    }

    Rigidbody2D rigidbody;
    float defaultGrabvityScale;
    States state;

    Vector2 StickDirection
    {
        get { return inputProvider.StickDirection; }
    }

    Vector2 DefaultStickDirection { get { return transform.up; } }

    // ジャンプ
    bool isJumping;
    public Vector2 jumpDirection { get; private set; }
    Vector2 jumpGuideDirection;
    float jumpTime;
    bool isJumpArmer;


    [SerializeField] PlayerJumpGuide jumpGuide;
    //[SerializeField] float jumpPower;
    //[SerializeField] float jumpingGravityScale;
    //[SerializeField] float guideMoveSpeed;
    //[SerializeField] float jumpStopMagnitude = 8;
    //[SerializeField] float maxJumpDuration = 0.5f;
    //[SerializeField] float gravityMultiplier = 1.5f;

    // 攻撃
    Vector2 oldVelocity;
    CameraMover cameraMover;
    Vector2[] checkReleaseWallArray;

    // カメラ
    [SerializeField] float shakeDur = 0.5f;
    [SerializeField] float shakeStrength = 0.5f;
    [SerializeField] int shakeVibrato = 10;

    // 接触判定
    [SerializeField] PlayerCollider playerCollider;
    WallType currentHoldingWallType;
    float wallSlipValue = 1;

    // 入力
    [SerializeField] PlayerInputProvider inputProvider;

    // 移動
    Vector2 currentMoveVec;
    //[SerializeField] float walkSpeed;
    //[SerializeField] float walkSpeedWhileJumping;

    //[SerializeField] float jumpFraction = 0.95f;

    PlayerStatusScriptable status;

    Collider2D BodyCollider
    {
        get { return playerCollider.gameObject.GetComponent<Collider2D>(); }
    }

    void Awake()
    {
        status = PlayerStatusScriptable.Entity;
        rigidbody = GetComponent<Rigidbody2D>();
        defaultGrabvityScale =status.gravityMultiplier * rigidbody.gravityScale;
        inputProvider = GetComponent<PlayerInputProvider>();
        cameraMover = ObjectFinder.GetCameraMover();

        checkReleaseWallArray = new Vector2[]{
            Vector2.left,
            Vector2.right,
            Vector2.up,
            Vector2.down
        };
    }

    private void Start()
    {
        playerCollider.OnHitOtherPlayer.Subscribe(other =>
        {
            OnHitOtherPlayer(other);
        });

        playerCollider.OnStayWall.Subscribe(col =>
        {
            OnStayWall(col);
        });

        playerCollider.OnStartHoldingWall.Subscribe(col =>
        {
            OnStartHoldingWall(col);
        });

        playerCollider.OnHitDamageable.Subscribe(damageable =>
        {
            OnHitDamageable(damageable);
        });

        playerCollider.OnTouchItem.Subscribe(item =>
        {
            OnTouchItem(item);
        });

        playerCollider.OnUpdatePositionToWall.Subscribe(hit =>
        {
            transform.position = transform.position + (-transform.up * hit.distance * Time.fixedDeltaTime);
        });
    }


    void Update()
    {

        GameObject.Find("DebugText").GetComponent<Text>().text = "" + rigidbody.gravityScale;
        if (Input.GetButtonDown("Home"))
        {
            Scene loadScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(loadScene.name);
        }
        if (playerCollider.IsHoldingAnything)
        {
        }

        if (state == States.ReadyToJump)
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

    void OnStartHoldingWall(Collision2D c)
    {
        SlipWall slipWall = c.gameObject.GetComponent<SlipWall>();
        if (slipWall != null)
        {
            wallSlipValue = slipWall.gravityMultiplier;
            currentHoldingWallType = WallType.Slip;
            return;
        }

        currentHoldingWallType = WallType.Normal;
    }

    void OnTouchItem(IGettable item)
    {
        switch (state)
        {
            case States.Walk:
                if (item.canWalkGet)
                {
                    item.OnGet();
                }
                break;
            case States.ReadyToJump:
                break;
            case States.Jump:
                if (item.canJumpGet)
                {
                    item.OnGet();
                }
                break;
            default:
                break;
        }
    }

    void OnStayWall(Collision2D col)
    {
        Vector2 v = col.GetContact(0).normal;
        currentMoveVec = Quaternion.Euler(0, 0, -90) * v;


        Quaternion q = Quaternion.FromToRotation(transform.up, col.GetContact(0).normal);
        transform.rotation *= q;

    }

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
                NockBack(-hitVec * oldVelocity.magnitude);
                enemy.NockBack(hitVec * oldVelocity.magnitude);
                //    Destroy(collision.gameObject);
                rigidbody.velocity = oldVelocity;
                IgnorePlayerTemporarily(enemy);
            }
        }
    }

    void OnHitDamageable(Damageable damageable)
    {
        if (isJumping)
        {
            cameraMover.Shake(shakeStrength, shakeDur, shakeVibrato);
            damageable.Damage(1);
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
                state = States.Walk;
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

        for (int i = 0; i < 4; i++)
        {
            Physics2D.OverlapPointNonAlloc((Vector2)transform.position + checkReleaseWallArray[i] * 1, results);

            foreach (Collider2D col in results)
            {
                if (col == null)
                {
                    continue;
                }

                if (col.gameObject.CompareTag("Wall"))
                {
                    releaseDir += checkReleaseWallArray[i] * -1;
                    Debug.Log(releaseDir);
                }
            }
            results[0] = null;
            results[1] = null;
            results[2] = null;
        }

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

    void HideGuideLine()
    {
        jumpGuide.HideLine();
    }

    void DrawGuideLine()
    {
        Debug.Log("Draw line");
        if (!jumpGuide.DrawEnabled)
        {
            jumpGuide.DrawLine(transform.position, StickDirection * 100);
            jumpGuideDirection = StickDirection != Vector2.zero ? StickDirection : (Vector2)transform.up;
        }

        float startAngle = Vector2.Angle(Vector2.right, jumpGuideDirection);
        if (jumpGuideDirection.y < 0)
        {
            startAngle *= -1;
        }

        Vector2 dir = Vector2.zero;
        float endAngle = 0;

        if (StickDirection != Vector2.zero)
        {
            dir = StickDirection;

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

        float jumpGuideAngle = Mathf.LerpAngle(startAngle, endAngle, Time.deltaTime * status. guideMoveSpeed);
        jumpGuideDirection = jumpGuideAngle.DegToVector();
        jumpGuide.DrawLine(transform.position, (Vector2)transform.position + jumpGuideDirection * 10);
    }

    void ManageJump()
    {

        Debug.Log("magnitude : " + rigidbody.velocity.magnitude);
        if (jumpTime > 0.2f)
        {
            if (playerCollider.IsHoldingAnything)
            {
                CancelJump();
            }
            if (rigidbody.velocity.magnitude < status.jumpStopMagnitude)
            {
                CancelJump();
            }
        }

        if (status.maxJumpDuration <= jumpTime)
        {
            CancelJump();
            //rigidbody.velocity /= 2;
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
                Debug.Log("Jump" + jumpGuideDirection);
                isJumping = true;
                isJumpArmer = true;
                jumpDirection = jumpGuideDirection.normalized;
                rigidbody.AddForce(jumpDirection * status.jumpPower);
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
            rigidbody.velocity *= status.jumpFraction;
            //Debug.Log(rigidbody.velocity.magnitude);

        }

    }


    void CancelJump()
    {
        jumpTime = 0;
        isJumping = false;
        //rigidbody.velocity = Vector2.zero;
        state = States.Walk;
    }

    void ManageGravity()
    {
        //Debug.Log("holding wall : " + IsHoldingWall );
        if (playerCollider.IsHoldingWall)
        {
            switch (currentHoldingWallType)
            {
                case WallType.None:
                    break;
                case WallType.Normal:
                    rigidbody.gravityScale = 0;
                    break;
                case WallType.Slip:
                    rigidbody.gravityScale = defaultGrabvityScale * wallSlipValue;
                    break;
                default:
                    break;
            }
        }
        else if (isJumping)
        {
            rigidbody.gravityScale = status.jumpingGravityScale;
        }
        else
        {
            rigidbody.gravityScale = defaultGrabvityScale;
        }
    }

    public static float FindDegree(Vector2 vec)
    {
        float value = (float)((Mathf.Atan2(vec.x, vec.y) / Mathf.PI) * 180f);
        if (value < 0) value += 360f;

        return value;
    }

    Vector2 CalcMoveVec(Vector2 input)
    {

        if (playerCollider.IsHoldingAnything)
        {

            Vector2 vec = Vector2.zero;
            //currentMoveVec.x = Mathf.Abs(currentMoveVec.x);
            //currentMoveVec.y = Mathf.Abs(currentMoveVec.y);

            float wallAngle = Vector2.Angle(Vector2.up, currentMoveVec);
            float dot = Vector2.Dot(Vector2.up, currentMoveVec);

            //Debug.Log("dot : " + FindDegree( currentMoveVec));

            float moveHorizontal = 45 <= wallAngle && wallAngle <= 135 ? input.x : 0;
            float moveVertical = wallAngle < 45 || 135 < wallAngle ? input.y : 0;

            //Debug.Log(moveHorizontal + ", " + moveVertical);
            //if (FindDegree(currentMoveVec) >= 180)
            //{
            //    moveHorizontal *= -1;
            //    moveVertical *= -1;
            //}
            if (currentMoveVec.x < 0)
            {
                moveHorizontal *= -1;
            }

            if (currentMoveVec.y < 0)
            {
                moveVertical *= -1;
            }
            //Debug.Log("movevec = " + currentMoveVec + ", " + wallAngle);

            if (moveHorizontal != 0)
            {
                vec = currentMoveVec * moveHorizontal * status.walkSpeed * Time.deltaTime;
            }

            if (moveVertical != 0)
            {
                vec = currentMoveVec * moveVertical * status.walkSpeed * Time.deltaTime;
                //Debug.Log(currentMoveVec + "," + vertical);
            }

            //GameObject.Find("DebugText").GetComponent<Text>().text = "" + vec;
            return vec;
        }

        return input * Vector2.right * status.walkSpeed * Time.deltaTime;

    }

    void Move()
    {
        Vector2 vec = Vector2.zero;
        Vector2 input = inputProvider.GetAxis();
        //Debug.Log(state);
        if (state == States.Walk)
        {
            vec = CalcMoveVec(input);
            switch (currentHoldingWallType)
            {
                case WallType.None:
                    break;
                case WallType.Normal:
                    break;
                case WallType.Slip:
                    vec = Vector2.zero;
                    break;
                default:
                    break;
            }
        }

        if (state == States.Jump)
        {
            float angleOffset = Vector2.Angle(currentMoveVec, Vector2.up);

            Vector3 offsetInput = Quaternion.Euler(0, 0, angleOffset) * input;
            Vector2 offsetNewInput = new Vector2(offsetInput.x * Mathf.Cos(0), offsetInput.y * Mathf.Sign(0));


            //vec = input.normalized;
            //vec = new Vector2(Mathf.Cos(input.x), Mathf.Sin(input.y)).normalized;
            //vec = Quaternion.Euler(0, 0, -angleOffset) * offsetNewInput;
            //vec *= Time.deltaTime * walkSpeedWhileJumping;
            //vec += rigidbody.velocity;
        }

        if (vec != Vector2.zero)
        {
            float vecX = vec.x != 0 ? vec.x : rigidbody.velocity.x;
            float vecY = vec.y != 0 ? vec.y : rigidbody.velocity.y;

            switch (state)
            {
                case States.Walk:
            rigidbody.velocity += new Vector2(vecX, vecY) * status.moveInAir;
                    break;
                case States.ReadyToJump:
                    break;
                case States.Jump:
            rigidbody.velocity += new Vector2(vecX, 0) * status.moveInAir;
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (playerCollider.IsHoldingAnything || state == States.ReadyToJump)
            {
                Debug.Log("AAA");
                rigidbody.velocity *= 0.99f;
            }
            else if (state == States.Walk)
            {
                //rigidbody.velocity *= 0.8f;
                //rigidbody.velocity = new Vector2(rigidbody.velocity.x * 0.8f, rigidbody.velocity.y);
            }
        }
    }

    public void Damage(int damage)
    {
    }
}
