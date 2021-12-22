using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // file location of the object
    const string filelocation = "InputSettings.save";

    // basic information
    public static InputManager current;
    public PlayerInput pis;

    public GameObject Window;
    public GameObject SettingsMenu;

    [Header("User Interface")]
    //basic movement
    public Text Forward;
    public Text Backward;
    public Text Left;
    public Text Right;
    public Text Jump;
    public Text Sprint;
    public Text Crouch;
    //firing reloading & other gun mechanics
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

        changeWindow(false);
        changeSettingsWindow(false);
    }

    private void saveSettings()
    {
        InputAction Movement = pis.currentActionMap.FindAction("Movement");
        InputAction J = pis.currentActionMap.FindAction("Jump");
        InputAction spr = pis.currentActionMap.FindAction("Sprint");
        InputAction crch = pis.currentActionMap.FindAction("Crouch");

        if (movement == null) return;

        // Save To JSON as Class
        InputSaved m = new InputSaved
        {
            // directional movement
            Forward = Movement.bindings[1].effectivePath,
            Backward = Movement.bindings[2].effectivePath,
            Left = Movement.bindings[3].effectivePath,
            Right = Movement.bindings[4].effectivePath,

            // othermovement based buttons
            Jump = J.bindings[0].effectivePath,
            Sprint = spr.bindings[0].effectivePath,
            Crouch = crch.bindings[0].effectivePath
        };

        //Debug.Log(m.ToString());

        string savedata = JsonUtility.ToJson(m,true);
        //Debug.Log(savedata);
        SaveLoad.SaveJson(filelocation, savedata);
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
            // Load From Class
            string d = SaveLoad.LoadJson(filelocation);

            if (!string.IsNullOrEmpty(d))
            {
                InputSaved inputs = JsonUtility.FromJson<InputSaved>(d);

                updateBinding("Movement", inputs.Forward, 1);
                updateBinding("Movement", inputs.Backward, 2);
                updateBinding("Movement", inputs.Left, 3);
                updateBinding("Movement", inputs.Right, 4);

                updateBinding("Jump", inputs.Jump, 0);
                updateBinding("Crouch", inputs.Crouch, 0);
                updateBinding("Sprint", inputs.Sprint, 0);
            }
        }
    }

    private void updateBinding(string Action,string Binding,int Index)
    {
        InputAction action = pis.currentActionMap.FindAction(Action);

        if(action != null)
        {
            action.ApplyBindingOverride(Index, new InputBinding { overridePath = Binding });
            UpdateMovementText();
        }
    }

    private void changeWindow(bool oc)
    {
        Window.SetActive(oc);
    }
    public void changeSettingsWindow(bool oc)
    {
        SettingsMenu.SetActive(oc);
    }

    private InputActionRebindingExtensions.RebindingOperation operation;
    public void changeControl(string Action)
    {
        bool usingMouse = false;
        int index = 0;

        // Looking for the Input of the same name as Action
        InputAction finalAction = pis.currentActionMap.FindAction(Action);

        if (finalAction != null)
        {
            // Disable Pressing Buttons
            pis.SwitchCurrentActionMap("None");

            if (usingMouse)
            {
                // keeping mouse input
                operation = finalAction.PerformInteractiveRebinding(index)
                   .WithCancelingThrough("<Keyboard>/escape")
                   .OnComplete(operation => onBindingCompleted(finalAction))
                   .OnCancel(operation => onBindingCompleted(finalAction))
                   .OnMatchWaitForAnother(0.1f)
                   .Start();
            }
            else
            {
                // removes mouse input
                 operation = finalAction.PerformInteractiveRebinding(index)
                    .WithCancelingThrough("<Keyboard>/escape")
                    .OnComplete(operation => onBindingCompleted(finalAction))
                    .OnCancel(operation => onBindingCompleted(finalAction))
                    .WithControlsExcluding("Mouse")
                    .OnMatchWaitForAnother(0.1f)
                    .Start();
            }
            // modal window telling to pressbutton to bind <BUTTON NAME> or press ESCAPE to cancel
            changeWindow(true);

        }
    }

    public void changeMovement(string direction)
    {
        // Looking for the Input of the same name as Action
        InputAction finalAction = pis.currentActionMap.FindAction("Movement");

        // -1 = CANCEL / CANT DO
        // 1 = FORWARD
        // 2 = BACKWARD
        // 3 = LEFT
        // 4 = RIGHT

        int index = -1;

        // if direction is equil to string above
        switch (direction)
        {
            case "FORWARD":
                index = 1;
                break;
            case "BACKWARD":
                index = 2;
                break;
            case "LEFT":
                index = 3;
                break;
            case "RIGHT":
                index = 4;
                break;
            default:
                index = -1;
                break;
        }

        //Debug.Log("INDEX: " + index);

        if (index != -1 && finalAction != null && index < finalAction.bindings.Count && finalAction.bindings[0].isComposite)
        {
            // Disable Pressing Buttons
            pis.SwitchCurrentActionMap("None");

            //Debug.Log("REBINDING " + direction + " ON INDEX " + index);
            //Debug.Log("1 : CURRENT BUTTON IS " + InputControlPath.ToHumanReadableString(finalAction.bindings[0].effectivePath));
            //Debug.Log("2 : CURRENT BUTTON IS " + finalAction.bindings[1].isPartOfComposite);

            // modal window telling to pressbutton to bind <BUTTON NAME> or press ESCAPE to cancel
            changeWindow(true);


            // removes mouse input
            operation = finalAction.PerformInteractiveRebinding(index)
               .WithCancelingThrough("<Keyboard>/escape")
               .OnComplete(operation => onBindingCompleted(finalAction))
               .OnCancel(operation => onBindingCompleted(finalAction))
               .WithControlsExcluding("Mouse")
               .OnMatchWaitForAnother(0.1f)
               .Start();
        }
    }

    public void UpdateMovementText()
    {
        InputAction Movement = pis.currentActionMap.FindAction("Movement");
        InputAction J = pis.currentActionMap.FindAction("Jump");
        InputAction spr = pis.currentActionMap.FindAction("Sprint");
        InputAction crch = pis.currentActionMap.FindAction("Crouch");

        if(Movement != null)
        {
            // removeing <Keyboard> button name part of the text
            InputControlPath.HumanReadableStringOptions type = InputControlPath.HumanReadableStringOptions.OmitDevice;

            // directional movement
            string f = InputControlPath.ToHumanReadableString(Movement.bindings[1].effectivePath, type).ToUpper();
            string b = InputControlPath.ToHumanReadableString(Movement.bindings[2].effectivePath, type).ToUpper();
            string l = InputControlPath.ToHumanReadableString(Movement.bindings[3].effectivePath, type).ToUpper();
            string r = InputControlPath.ToHumanReadableString(Movement.bindings[4].effectivePath, type).ToUpper();

            // othermovement based buttons
            string jmp = InputControlPath.ToHumanReadableString(J.bindings[0].effectivePath, type).ToUpper();
            string s = InputControlPath.ToHumanReadableString(spr.bindings[0].effectivePath, type).ToUpper();
            string c = InputControlPath.ToHumanReadableString(crch.bindings[0].effectivePath, type).ToUpper();

            Forward.text = f;
            Backward.text = b;
            Left.text = l;
            Right.text = r;
            Jump.text = jmp;
            Sprint.text = s;
            Crouch.text = c;
        }
    }

    public void resetToDefault()
    {
        foreach(InputAction action in pis.currentActionMap.actions)
        {
            if (!action.bindings[0].isComposite)
            {
                action.RemoveAllBindingOverrides();
            }
            else
            {
                // remove all bindings of MOVEMENT COMPOSITE
                // It's a composite. Remove overrides from part bindings.
                for (var i = 0 + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                    action.RemoveBindingOverride(i);
            }

            //Debug.Log("RESET CONTROLS : " + action.GetBindingDisplayString());
        }

        UpdateMovementText();
        saveSettings();
    }

    public void onBindingCompleted(InputAction action)
    {
        // close UI
        changeWindow(false);
        // Dispose of 'operation'
        operation.Dispose();
        pis.SwitchCurrentActionMap("Player");
        //Debug.Log("CHANGED CONTROL OF " + action.GetBindingDisplayString());
        saveSettings();
        UpdateMovementText();
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

        if (SettingsMenu.activeSelf)
        {
            changeSettingsWindow(false);
        }
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

    public class InputSaved
    {
        public string Forward;
        public string Backward;
        public string Left;
        public string Right;

        public string Sprint;
        public string Jump;
        public string Crouch;

        public override string ToString()
        {
            return Forward + "/" + Backward + "/" + Left + "/" + Right;
        }
    }
}
