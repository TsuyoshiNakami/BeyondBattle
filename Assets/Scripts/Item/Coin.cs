using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour, IGettable
{
    public bool canJumpGet => true;

    public bool canWalkGet => true;

    public void OnGet()
    {
        Destroy(gameObject);
    }


}
