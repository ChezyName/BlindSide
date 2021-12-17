using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSys : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
