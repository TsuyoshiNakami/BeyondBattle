using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ButtonState
{
    Up,
    Hold,
    Down,
    None,
}

public enum PlayerButton
{
    Jump,
}
interface PlayerInputProvider
{
    ButtonState GetButton(PlayerButton button);
    Vector2 GetAxis();
    Vector2 StickDirection {
        get;
    }
}
