using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DoorManager : NetworkBehaviour
{
    public GameObject door;
    public Interactable DoorInteractable;
    private Quaternion Rotation;
    float speed;

    [SyncVar]
    bool opened = false;

    [Server]
    private void Start()
    {
        Rotation = door.transform.rotation;
    }

    [Server]
    public void OpenClose(float s)
    {
        if (opened)
        {
            Rotation = door.transform.rotation * Quaternion.Euler(0, 90, 0);
        }
        else
        {
            Rotation = door.transform.rotation * Quaternion.Euler(0, -90, 0);
        }

        opened = !opened;
        speed = s;
    }

    public bool getOpened()
    {
        return opened;
    }

    [Server]
    private void FixedUpdate()
    {
        door.transform.rotation = Quaternion.Lerp(door.transform.rotation,Rotation,speed * Time.fixedDeltaTime);
        DoorInteractable.CanUse = (door.transform.rotation == Rotation);
    }
}
