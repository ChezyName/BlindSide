using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tab : MonoBehaviour
{
    public GameObject[] Tabs;

    void CloseAll()
    {
        foreach (GameObject t in Tabs)
        {
            t.SetActive(false);
        }
    }

    private void Start()
    {
        CloseAll();
    }

    public void OpenTab(GameObject ct)
    {
        CloseAll();
        ct.SetActive(true);
    }
}
