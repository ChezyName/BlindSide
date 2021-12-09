using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadMapClick : MonoBehaviour
{
    private Loading loader = Loading.current;

    public void LoadScene(int scene)
    {
        loader.LoadScene(scene);
    }
}
