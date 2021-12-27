using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DoorOpenClose : OnInteration
{
    public DoorManager doc;

    [Server]
    public override void onInteract(Interactable interactable)
    {
        //base.onInteract(interactable);
        doc.OpenClose(.75f);
    }

    public override string getName()
    {
        if(doc.getOpened() == true)
        {
            return "SLOW CLOSE";
        }
        else
        {
            return "SLOW OPEN";
        }
    }
}
