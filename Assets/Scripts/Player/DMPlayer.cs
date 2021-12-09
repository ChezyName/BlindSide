using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DMPlayer : Player
{
    private SpawnManager respawner;

    //called on start
    [Server]
    protected override void serverStart()
    {
        respawner = GameObject.FindObjectOfType<SpawnManager>();

        p_MaxAmmo = Primary.MaxAmmo;
        p_Mag = Primary.MagSize;

        CurrentGun = Primary;
        RpcOnEquip(Primary.id);
        Debug.Log("'Spawining Player'");
    }

    protected override void clientStart()
    {
        respawner = GameObject.FindObjectOfType<SpawnManager>();
        CmdRespawnPlayer();
    }

    protected override void onDowned()
    {
        CmdRespawnPlayer();
    }


    [Command]
    private void CmdRespawnPlayer()
    {
        Debug.Log("Server Respawning " + cc.gameObject.name);

        isDowned = false;
        Health = 100;
        Shield = 50;

        p_MaxAmmo = Primary.MaxAmmo;
        p_Mag = Primary.MagSize;

        CurrentGun = Primary;
        RpcOnEquip(Primary.id);

        RpcRespawnPlayer();
        isDowned = false;
    }

    [TargetRpc]
    private void RpcRespawnPlayer()
    {
        Debug.Log("Client Respawning " + cc.gameObject.name);

        Transform spawn = respawner.getRandomSpawn();
        //cc.enabled = false;
        cc.transform.position = spawn.position;
        cc.transform.rotation = spawn.rotation;
        //cc.enabled = true;
        //Debug.Log("Moving Player To: " + spawn.position);
    }

    public override void TakeDamage(string name, int dmg)
    {
        base.TakeDamage(name, dmg);
        if(Health <= 0 || isDowned)
        {
            gm.onKilled(name, cc.gameObject.name);
        }
    }
}
