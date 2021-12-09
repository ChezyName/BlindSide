using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameSettings : MonoBehaviour
{
    private PlayerSettings ps;
    public static GameSettings current;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        current = this;

        LoadSettings();
    }

    public event Action<PlayerSettings> onSettingsChanged;

    void LoadSettings()
    {
        string data = SaveLoad.LoadJson("GameUserSettings.config");
        SaveLoad.SaveJson("data.txt", data);
        if (data == null)
        {
            ps = new PlayerSettings();
            save();
            SetScreen();
        }
        else
        {
            ps = JsonUtility.FromJson<PlayerSettings>(data);
            ChangeSettings(ps);
            SetScreen();
        }
    }

    public void Start()
    {
        LoadSettings();
    }

    private void save()
    {
        SaveLoad.SaveJson("GameUserSettings.config", JsonUtility.ToJson(ps,true));
        //Debug.Log(JsonUtility.ToJson(ps,true));
    }

    public PlayerSettings GetSettings()
    {
        return ps;
    }

    public void ChangeSettings(PlayerSettings pst)
    {
        ps = pst;
        save();

        if(onSettingsChanged != null)
        {
            onSettingsChanged(ps);
        }
    }

    public static PlayerSettings GetSettingsFromFile()
    {
        string data = SaveLoad.LoadJson(Application.persistentDataPath + "/config/GameUserSettings.config");
        if(data != null)
        {
            return JsonUtility.FromJson<PlayerSettings>(data);
        }
        else
        {
            return null;
        }
    }

    public void SetScreen()
    {
        string DD = ps.Resolution;

        if (DD == "2560x1440")
        {
            Screen.SetResolution(2560, 1440, ps.Fullscreen, ps.RefreshRate);
        }
        else if (DD == "1920x1080")
        {
            Screen.SetResolution(1920, 1080, ps.Fullscreen, ps.RefreshRate);
        }
        else if (DD == "1600x900")
        {
            Screen.SetResolution(1600, 900, ps.Fullscreen, ps.RefreshRate);
        }
        else if (DD == "1366x768")
        {
            Screen.SetResolution(1366, 768, ps.Fullscreen, ps.RefreshRate);
        }
        else
        {
            Screen.SetResolution(1280, 720, ps.Fullscreen, ps.RefreshRate);
        }
    }

    public class PlayerSettings
    {
        //Game Settings
        [Header("Video Settings")]
        public string Resolution = "1920x1080";
        public bool Fullscreen = true;
        public int RefreshRate = 60;
        [Header("Game Settings")]
        public double Sensitivty = 6;
        public double AimSensitivty = 6;
        //Keybinds
        [Header("Keybinds")]
        public KeyCode Forward = KeyCode.W;
        public KeyCode Backward = KeyCode.S;
        public KeyCode Left = KeyCode.A;
        public KeyCode Right = KeyCode.D;
    }

    // Changing Settings / Public Functions
    /*
    public void ChangeSens(float sens)
    {
        ps.Sensitivty = sens;
        save();
    }
    */
}
