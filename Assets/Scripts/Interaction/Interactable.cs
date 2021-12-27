using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Interactable : NetworkBehaviour
{
    // SERVER + CLIENT
    public OnInteration[] Interactions;
    //public Transform InteractionPoint;
    [SyncVar]
    public bool CanUse = true;

    private void Start()
    {
        if (netIdentity.isClient)
        {
            //Client
        }
    }

    [Command(requiresAuthority = false)]
    public void Interact(int Inx)
    {
        if (Inx >= Interactions.Length || Interactions[Inx] == null || CanUse == false) return;
        Interactions[Inx].onInteract(this);
    }
}
