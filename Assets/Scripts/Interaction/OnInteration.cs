using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OnInteration : NetworkBehaviour
{
    protected string InteractionName;

    public virtual void onInteract()
    {

    }

    public virtual string getName()
    {
        return InteractionName;
    }
}
