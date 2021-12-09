using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalWindow : MonoBehaviour
{
    public static ModalWindow self;

    public GameObject Window;
    public Text Header;
    public Text Content;

    private void Awake()
    {
        self = this;
        Window.SetActive(false);
        DontDestroyOnLoad(this);
    }

    public void showModalWindow(string header,string content)
    {
        Header.text = header;
        Content.text = content;

        Window.SetActive(true);
    }

    public void closeWindow()
    {
        Window.SetActive(false);
    }

    public bool checkifShown()
    {
        return Window.active;
    }
}
