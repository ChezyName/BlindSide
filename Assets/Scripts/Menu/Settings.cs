using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    private GameSettings.PlayerSettings ps;
    private InputManager manager;

    [Header("Sensitivity")]
    public InputField Sensif;
    public Slider SensSlider;

    [Header("Video Screen")]
    public Dropdown Resolution;
    public Dropdown RefreshRate;
    public Toggle Fullscreen;

    public void Start()
    {
        ps = GameSettings.current.GetSettings();
        Sensif.text = ps.Sensitivty.ToString();
        SensSlider.value = (float)ps.Sensitivty;
        manager = InputManager.current;
        //UpdateVideoScreen();
    }

    public void openSettingsMenu()
    {
        manager.changeSettingsWindow(true);
    }

    public void onSlideSens()
    {
        float Sens = SensSlider.value;
        ps = GameSettings.current.GetSettings();
        ps.Sensitivty = (double)Sens;
        Sensif.text = Sens.ToString();
        GameSettings.current.ChangeSettings(ps);
    }

    public void onInputSens()
    {
        float Sens = float.Parse(Sensif.text);
        ps = GameSettings.current.GetSettings();
        ps.Sensitivty = Sens;
        SensSlider.value = Sens;
        GameSettings.current.ChangeSettings(ps);
    }

    void UpdateVideoScreen()
    {
        Fullscreen.isOn = ps.Fullscreen;

        int rezo = 1;
        string res = ps.Resolution;

        if (res == "2560x1440")
        {
            rezo = 0;
        }
        else if (res == "1920x1080")
        {
            rezo = 2;
        }
        else if (res == "1600x900")
        {
            rezo = 3;
        }
        else if (res == "1366x768")
        {
            rezo = 4;
        }
        else if (res == "1280x720")
        {
            rezo = 5;
        }

        Resolution.value = rezo;

        int rate = ps.RefreshRate;
        int r = 0;

        if (rate == 240)
        {
            r = 0;
        }
        else if (rate == 144)
        {
            r = 1;
        }
        else if (rate == 75)
        {
            r = 2;
        }
        else
        {
            r = 3;
        }

        RefreshRate.value = r;
    }

    public void onDropDownScreen(int rate)
    {
        string rez = "1920x1080";

        if (rate == 0)
        {
            rez = "2560x1440";
        }
        else if (rate == 1)
        {
            rez = "1920x1080";
        }
        else if (rate == 2)
        {
            rez = "1600x900";
        }
        else if(rate == 3)
        {
            rez = "1366x768";
        }
        else
        {
            rez = "1280x720";
        }

        ps = GameSettings.current.GetSettings();
        ps.Resolution = rez;
        GameSettings.current.ChangeSettings(ps);
        GameSettings.current.SetScreen();
        //SaveLoad.SaveJson("otherdata.txt",rez + " / " + System.DateTime.Today + " / " + rate);
    }

    public void onToggleFS(bool toggle)
    {
        ps = GameSettings.current.GetSettings();
        ps.Fullscreen = toggle;
        GameSettings.current.ChangeSettings(ps);
        GameSettings.current.SetScreen();
    }

    public void onRefresh(int rate)
    {
        int r = 60;

        if(rate == 0)
        {
            r = 240;
        }
        else if(rate == 1)
        {
            r = 144;
        }
        else if(rate == 2)
        {
            r = 75;
        }
        else
        {
            r = 60;
        }

        ps = GameSettings.current.GetSettings();
        ps.RefreshRate = r;
        GameSettings.current.ChangeSettings(ps);
        GameSettings.current.SetScreen();
    }
}
