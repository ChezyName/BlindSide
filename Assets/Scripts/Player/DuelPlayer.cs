using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class DuelPlayer : Player
{
    DuelGM dgm;
    public Text roundTimer;
    public Image[] PlrWins;
    public Image[] EnemyWins;

    [SyncVar]
    private int Timer = 0;

    [SyncVar]
    private int myWins = 0;
    [SyncVar]
    private int enemyWins = 0;

    [Server]
    public void setTimer(int T)
    {
        Timer = T;
    }

    void setWins(Image[] w, int win)
    {
        for(int i = 0; i < w.Length; i++)
        {
            if(i < win)
            {
                Color wc = w[i].color;
                wc.a = 1;
                w[i].color = wc; // solid color
            }
            else
            {
                Color lc = w[i].color;
                lc.a = .25f;
                w[i].color = lc; // solid color
            }
        }
    }

    [Server]
    private void getWins()
    {
        if (dgm == null) return;
        myWins = dgm.getMyWins(playerName);
        enemyWins = dgm.getOtherWins(playerName);
    }

    [Client]
    void updateTimer()
    {
        if(Timer > 0)
        {
            roundTimer.text = Timer.ToString();
            roundTimer.gameObject.SetActive(true);
        }
        else
        {
            roundTimer.gameObject.SetActive(false);
        }
    }

    protected override void onClientUpdate()
    {
        base.onClientUpdate();
        updateTimer();

        setWins(EnemyWins, myWins);
        setWins(PlrWins, enemyWins);
    }

    protected override void onServerUpdate()
    {
        base.onServerUpdate();
        getWins();
        //Debug.Log("MyWins: " + myWins + "Enemy Wins: " + enemyWins);

        p_Mag = 100;
    }

    protected override void clientStart()
    {
        dgm = GameObject.FindObjectOfType<DuelGM>();
    }

    protected override void serverStart()
    {
        base.serverStart();
        dgm = GameObject.FindObjectOfType<DuelGM>();
    }

    protected override void onDowned()
    {
        died();
    }

    protected override void OnPrimaryPressed()
    {
        //base.OnPrimaryPressed();
    }

    [Command]
    void died()
    {
        dgm.onDeath(playerName);
    }

    [Server]
    public void EquipWeapon(int ID,string conn)
    {
        Gun g = GetGun(ID);
        CurrentGun = g;
        Debug.Log("EQUIPING " + CurrentGun.name + " On Player : " + playerName);
        rpcEquipW(ID,conn);
    }

    [ClientRpc]
    private void rpcEquipW(int gID,string Connection)
    {
        if (netIdentity.isClient)
        {
            CurrentGun = GetGun(gID);

            if (Viewmodel != null)
            {
                Destroy(Viewmodel);
            }

            Viewmodel = ShowViewmodel(CurrentGun, Camera, Cam);
            Debug.Log("Equiping This : " + CurrentGun.name + " On " + playerName);
        }
    }
}
