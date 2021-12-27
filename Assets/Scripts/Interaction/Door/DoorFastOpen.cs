using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DoorFastOpen : OnInteration
{
    public DoorManager doc;

    [Server]
    public override void onInteract(Interactable interactable)
    {
        //base.onInteract(interactable);
        doc.OpenClose(12f);
        Debug.Log("FAST OPEN/CLOSE");
    }

    public override string getName()
    {
        if (doc.getOpened() == true)
        {
            return "FAST CLOSE";
        }
        else
        {
            return "FAST OPEN";
        }
    }
}
