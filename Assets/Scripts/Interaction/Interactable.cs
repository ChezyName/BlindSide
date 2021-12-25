using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Interactable : NetworkBehaviour
{
    // SERVER + CLIENT
    public OnInteration[] Interactions;

    // Client Only
    int InteractionIndex = 0;


    private void Start()
    {
        if (netIdentity.isClient)
        {
            //Client
            InteractionIndex = 0;
        }
    }

    public void Next()
    {
        int n = InteractionIndex + 1;
        if(n >= Interactions.Length - 1)
        {
            n = Interactions.Length - 1;
        }
        InteractionIndex = n;
    }

    public void Last()
    {
        int n = InteractionIndex - 1;
        if (n <= 0)
        {
            n = 0;
        }
        InteractionIndex = n;
    }

    [Command(requiresAuthority = false)]
    public void Interact()
    {
        Interactions[InteractionIndex].onInteract();
    }
}
