using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailFader : MonoBehaviour
{
    //private Color c;
    private TrailRenderer tr;
    public float Speed;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Fade());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(1.5f);
        int trans = Mathf.RoundToInt(tr.startColor.a / 15);

        for(int i = Mathf.RoundToInt(tr.startColor.a); i >= 0; i+= trans)
        {
            tr.startColor = new Color(tr.startColor.r, tr.startColor.g, tr.startColor.b, i);
            tr.endColor = new Color(tr.endColor.r, tr.endColor.g, tr.endColor.b, i);

            yield return new WaitForSeconds(.15f);
        }
    }
}
