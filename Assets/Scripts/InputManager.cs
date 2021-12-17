using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // file location of the object
    string filelocation = "InputSettings.settings";

    // basic information
    public static InputManager current;
    public PlayerInput pis;
    //private static InputActionAsset inputsystem;

    // Values To Give Out To Others
    Vector2 movement;
    Vector2 mouse;
    int jump;
    bool crouch;
    bool sprint;
    int esc;
    bool fire_h;
    bool fire_p;

    private void Awake()
    {
        current = this;

        loadSettings();

        //inputsystem = getIAAbyName("PlayerInput");

        DontDestroyOnLoad(this);
        //Debug.Log(inputsystem + " | " + inputsystem.name);
    }

    private void saveSettings()
    {
        //Debug.Log(pis.actions.ToJson());
        SaveLoad.SaveJson(filelocation, pis.actions.ToJson());
    }

    private void loadSettings()
    {
        string data = SaveLoad.LoadJson(filelocation);

        if (string.IsNullOrEmpty(data))
        {
            saveSettings();
        }
        else
        {
            if (string.IsNullOrEmpty(SaveLoad.LoadJson(data)))
            {
                saveSettings();
            }
            else
            {
                pis.actions.LoadFromJson(data);
            }
        }
    }

    // Getting The Inputs For Each
    public event Action<bool> onEscape;
    public event Action<bool> onJump;
    public void setJump(InputAction.CallbackContext ctx)
    {
        onJump(ctx.started);
    }

    public void setCrouch(InputAction.CallbackContext ctx)
    {
        crouch = ctx.performed;
    }

    public void setSprint(InputAction.CallbackContext ctx)
    {
        sprint = ctx.performed;
    }

    public void setMovement(InputAction.CallbackContext ctx)
    {
        movement = ctx.ReadValue<Vector2>();
    }

    public void setCamera(InputAction.CallbackContext ctx)
    {
        mouse = ctx.ReadValue<Vector2>();
    }

    public void setEscape(InputAction.CallbackContext ctx)
    {
        onEscape(ctx.started);
    }

    public void setFire(InputAction.CallbackContext ctx)
    {
        fire_p = ctx.started;
        fire_h = ctx.performed;
    }

    // Getting Button Inputs
    #region Getting Inputs

    public Vector2 getMovement()
    {
        return movement;
    }

    public bool getCrouch()
    {
        return crouch;
    }

    public bool getSprint()
    {
        return sprint;
    }

    public Vector2 getMouse()
    {
        return mouse;
    }

    public bool getFirePress()
    {
        return fire_p;
    }

    public bool getFireHold()
    {
        return fire_h;
    }

    #endregion
}
