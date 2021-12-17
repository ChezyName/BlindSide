using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCrosshair : MonoBehaviour
{
    private RectTransform Retical;
    private float currentSize;
    private Rect Basesize;
    [Range(0,500)]
    public float b;

    private void Start()
    {
        Retical = gameObject.GetComponent<RectTransform>();
        Basesize = Retical.rect;
    }

    public void setSize(float Bloom)
    {
        b = Bloom * 2.5f;
    }

    private void Update()
    {
        //Debug.Log("SIZE: " + currentSize);
        currentSize = Mathf.Lerp(currentSize,b, 5);
        Retical.sizeDelta = new Vector2(50 + b,50 + b);
    }

    public void setActive(bool value)
    {
        Retical.gameObject.SetActive(value);
    }
}
