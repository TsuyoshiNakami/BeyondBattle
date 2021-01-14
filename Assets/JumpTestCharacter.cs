using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class JumpTestCharacter : MonoBehaviour
{
    PlayerInputProvider inputProvider;
    Vector2 StickDirection
    {
        get { return inputProvider.StickDirection; }
    }
    Vector2 oldStickDirection;
    Vector2 DefaultStickDirection { get { return transform.up; } }


    public Vector2 jumpDirection { get; private set; }
    Vector2 jumpGuideDirection;
    [SerializeField] float jumpPower;
    Rigidbody2D rigid;


    PlayerStatusScriptable param;

    [SerializeField] PlayerJumpGuide jumpGuide;
    float stickMaxInputTime;

    // Start is called before the first frame update
    void Start()
    {
        param = PlayerStatusScriptable.Entity;
        rigid = GetComponent<Rigidbody2D>();

        inputProvider = GetComponent<PlayerInputProvider>();
        Observable.
            EveryUpdate()
            //.Where(_ => inputProvider.GetButton(PlayerButton.Jump) == ButtonState.Down)
            .Subscribe(_ =>
            {
                float inputLevel = Mathf.Max(Mathf.Abs(oldStickDirection.x), Mathf.Abs(oldStickDirection.y));
                if (inputLevel >= param.jumpStartInputMagnitude)
                {
                    stickMaxInputTime = 0;
                }

                if (StickDirection == Vector2.zero)
                {
                    if (oldStickDirection != Vector2.zero &&
                        param.jumpEnableDuration >= stickMaxInputTime)
                    {
                        jumpDirection = jumpGuideDirection.normalized;
                        rigid.velocity = jumpDirection * jumpPower;
                    }
                }
            });

        Observable
            .EveryUpdate()
            .Select(_ => inputProvider.GetAxis())
            .Subscribe(vec =>
            {

                float startAngle = Vector2.Angle(Vector2.right, jumpGuideDirection);
                if (jumpGuideDirection.y < 0)
                {
                    startAngle *= -1;
                }

                Vector2 dir = Vector2.zero;
                float endAngle = 0;

                if (StickDirection != Vector2.zero)
                {
                    dir = -StickDirection;

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
            });


        Observable.EveryLateUpdate()
            .Subscribe(_ =>
            {
                oldStickDirection = StickDirection;

                stickMaxInputTime += Time.deltaTime;
            });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
