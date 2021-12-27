using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OnInteration : NetworkBehaviour
{
    protected string InteractionName;

    [Server]
    public virtual void onInteract(Interactable interactable)
    {

    }

    public virtual string getName()
    {
        return InteractionName;
    }
}
