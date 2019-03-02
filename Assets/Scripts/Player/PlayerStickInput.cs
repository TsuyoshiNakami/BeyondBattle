using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStickInput : MonoBehaviour, PlayerInputProvider
{
    public Vector2 StickDirection
    {
        get { return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); }
    }

    public Vector2 GetAxis()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public ButtonState GetButton(PlayerButton button)
    {
        string buttonName = button.ToString();
        if (Input.GetButtonDown(buttonName))
        {
            return ButtonState.Down;
        }
        if (Input.GetButtonUp(buttonName))
        {
            return ButtonState.Up;
        }

        if (Input.GetButton(buttonName))
        {
            return ButtonState.Hold;
        }

        return ButtonState.None;
    }
}
