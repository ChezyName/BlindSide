using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class Interactable : NetworkBehaviour
{
    public float MaxInteractDist = 5f;

    public virtual void onInteract()
    {

    }
}
