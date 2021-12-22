using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenSettings : MonoBehaviour
{
    InputManager input;
    void Awake()
    {
        input = InputManager.current;
    }

    public void openSettings()
    {
        input.changeSettingsWindow(true);
    }
}
