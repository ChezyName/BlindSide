using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCrosshair : MonoBehaviour
{
    private RectTransform Retical;
    private float currentSize;
    private float b;

    private void Start()
    {
        Retical = gameObject.GetComponent<RectTransform>();
        currentSize = Retical.sizeDelta.x;
    }

    public void setSize(float Bloom)
    {
        b = Bloom;
    }

    private void Update()
    {
        Debug.Log("SIZE: " + currentSize);
        currentSize = Mathf.Lerp(currentSize, 2 * b, Time.deltaTime * 25);
        Retical.sizeDelta = new Vector2(currentSize, currentSize);
    }

    public void setActive(bool value)
    {
        Retical.gameObject.SetActive(value);
    }
}
