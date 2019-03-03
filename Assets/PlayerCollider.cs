using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class PlayerCollider : MonoBehaviour
{

    // イベント
    Subject<Player> hitOtherPlayerSubject = new Subject<Player>();
    public IObservable<Player> OnHitOtherPlayer
    {
        get { return hitOtherPlayerSubject; }
    }

    Subject<Damageable> hitDamageableSubject = new Subject<Damageable>();
    public IObservable<Damageable> OnHitDamageable
    {
        get { return hitDamageableSubject; }
    }

    Subject<Collision2D> stayWallSubject = new Subject<Collision2D>();
    public IObservable<Collision2D> OnStayWall
    {
        get { return stayWallSubject; }
    }

    //  壁つかまっているか判定

    List<GameObject> holdObjs = new List<GameObject>();
    public int holdingCount { get; private set; }
    public bool IsHoldingAnything
    {
        get { return holdingCount > 0; }
    }

    public bool IsHoldingWall
    {
        get { return holdingCount > 0;
            }
    }

    public bool isHoldingWall { get; private set; }
    public bool isHoldingCeiling { get; private set; }
    WallType JudgeWhereHoldingOnTo(Vector2 normal)
    {
        if (Mathf.Abs(normal.x) < Mathf.Abs(normal.y))
        {
            //　上下方向にぶつかっている
            if (normal.y < 0)
            {
                //天井
                return WallType.Ceiling;
            }
            else
            {
                //地面
                return WallType.Ground;
            }
        }
        else
        {
            return WallType.Wall;
        }
    }
    void ResetHoldingStates()
    {
        isHoldingWall = false;
        isHoldingCeiling = false;
    }

    bool JudgeHolding()
    {
        if (holdingCount <= 0)
        {
            return false;
        }

        return IsHoldingWall;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isHoldingCeiling = false;
            isHoldingWall = false;
            holdingCount++;

            WallType wallType = JudgeWhereHoldingOnTo(collision.GetContact(0).normal);
            switch (wallType)
            {
                case WallType.Ground:
                    break;
                case WallType.Wall:
                    isHoldingWall = true;
                    break;
                case WallType.Ceiling:
                    isHoldingCeiling = true;
                    break;
                default:
                    break;
            }

            holdObjs.Add(collision.gameObject);
            //Debug.Log(collision.GetContact(0).normal);
            //Debug.Log(wallType);
        }

        Damageable d = collision.gameObject.GetComponent<Damageable>();
        if(d != null)
        {
            hitDamageableSubject.OnNext(d);
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            hitOtherPlayerSubject.OnNext(collision.gameObject.GetComponent<Player>());
        }

    }

    private void OnCollisionStay2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Wall"))
        {
            stayWallSubject.OnNext(collision);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Wall"))
        {
            holdingCount--;
            holdObjs.Remove(collision.gameObject);
        }
    }

    private void Update()
    {
        if (!JudgeHolding())
        {
            ResetHoldingStates();
        }
    }
}
