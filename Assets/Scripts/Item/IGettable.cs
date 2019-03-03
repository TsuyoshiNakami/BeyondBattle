using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGettable 
{
    bool canJumpGet { get; }
    bool canWalkGet { get; }
    void OnGet();
}
