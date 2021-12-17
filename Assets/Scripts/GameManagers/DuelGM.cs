using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DuelGM : GameManager
{
    private NetworkConnection player1 = null;
    private PlayerData p1;
    private DuelPlayer p1p;
    private int p1Wins = 0;

    private NetworkConnection player2 = null;
    private PlayerData p2;
    private DuelPlayer p2p;
    private int p2Wins = 0;

    private int Round;

    private Gun duelGun;

    private float CantMoveTimer;

    public  int[] Maps;
    private string currentMap;
    private DuelSpawnManager sm;

    private static string NameFromIndex(int BuildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }

    [Server]
    protected override void serverStart()
    {
        base.serverStart();
        currentMap = NameFromIndex(Maps[Random.Range(0,Maps.Length)]);
        duelGun = getRandomGun();
        //Debug.Log(currentMap + "!");
        DontDestroyOnLoad(this);
        Round = 0;
        CantMoveTimer = 5;
    }

    [Server]
    private Gun getRandomGun()
    {
        Gun[] objects = Resources.LoadAll<Gun>("Weapons/WeaponSO");
        return objects[Random.Range(0,objects.Length)];
    }

    protected override void clientConnect(NetworkConnection conn)
    {
        conn.Send(new PlayerData { name = "User " + UnityEngine.Random.Range(0, 255) });
    }

    protected override void onGameObjectData(GameObjectData g)
    {
        g.obj.SetActive(g.active);
    }

    [Server]
    protected override void OnCreatePlayer(NetworkConnection conn, PlayerData createPlayerMessage)
    {
        NetworkServer.SendToAll(new SceneLoader { sceneName = currentMap, loading = true, connection = conn.ToString() });
        if(player1 == null)
        {
            player1 = conn;
            p1 = createPlayerMessage;
            p1.ID = 1;
            //Debug.Log("Player 1 Connected");
        }
        else if(player2 == null)
        {
            player2 = conn;
            p2 = createPlayerMessage;
            p2.ID = 2;

            //Debug.Log("Player 2 Connected");

            if (player1.isReady && player2.isReady)
            {
                //Debug.Log("Loading " + currentMap);
                ServerChangeScene(currentMap);
            }
        }
        else if(player1 != null && player2 != null)
        {
            //kick
            Debug.Log(createPlayerMessage.name + " Getting Kicked! To Many Players");

            NetworkServer.SendToAll(new DiscconnectMessage { Header = "Disconnected From Server",Body = "Games Already Started.",NetworkConn = conn.ToString()});
            conn.Disconnect();
        }
    }

    [Server]
    void LoadAll(bool l)
    {
        NetworkServer.SendToAll(new SceneLoader { sceneName = currentMap, loading = l, connection = ""});
    }

    [Server]
    private void resetMatch()
    {
        // Set Map + Random Guns
        p1p = null;
        p2p = null;

        LoadAll(true);

        currentMap = NameFromIndex(Maps[Random.Range(0, Maps.Length)]);
        duelGun = getRandomGun();
        ServerChangeScene(currentMap);
    }

    [Server]
    public void onDeath(string n)
    {
        if (p1.name.Equals(n))
        {
            //Debug.Log("P1 Died, P2 Wins!");
            p1Wins++;
            //NetworkServer.SendToAll(new GameObjectData { obj = p1p.gameObject, active = false });
        }
        else
        {
            //Debug.Log("P2 Died, P1 Wins!");
            p2Wins++;
            //NetworkServer.SendToAll(new GameObjectData { obj = p2p.gameObject, active = false});
        }

        if(p1Wins == 5)
        {
            Debug.Log("Player 1 Has Won Whole Game!");
            kickClient(player1, "Kicked For Winning The Game!");
        }
        else if(p2Wins == 5)
        {
            Debug.Log("Player 2 Has Won Whole Game!");
            kickClient(player1, "Kicked For Loosing The Game!");
        }
        else
        {
            /*
            if(p1p != null)
            {
                p1p.changeCanMove(false);
            }

            if (p2p != null)
            {
                p2p.changeCanMove(false);
            }
            */

            resetMatch();
        }
    }

    [Server]
    public int getMyWins(string n)
    {
        if (p2.name.Equals(n))
        {
            //Debug.Log("WINS: " + p2Wins);
            return p2Wins;
        }
        else
        {
            //Debug.Log("WINS: " + p1Wins);
            return p1Wins;
        }
    }

    [Server]
    public int getOtherWins(string n)
    {
        if (p1.name.Equals(n))
        {
            return p2Wins;
        }
        else
        {
            return p1Wins;
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        sm = GameObject.FindObjectOfType<DuelSpawnManager>();

        //WaitWhile(player1.isReady && player2.isReady);

        p1p = spawnPlayers(player1, p1);
        p2p = spawnPlayers(player2, p2);
        //5 second timer that players cant move

        CantMoveTimer = 5;
        gameStarted = false;

        LoadAll(false);
    }

    [Server]
    DuelPlayer spawnPlayers(NetworkConnection connection, PlayerData createPlayerMessage)
    {
        // create a gameobject using the name supplied by client
        GameObject playergo = Instantiate(playerPrefab);
        Transform spawn;

        if(createPlayerMessage.ID == 1)
        {
            spawn = sm.getSpawnOne();
        }
        else
        {
            spawn = sm.getSpawnTwo();
        }

        playergo.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

        //playergo.GetComponent<Player>().name = createPlayerMessage.name;

        // set it as the player
        PlayerData data = createPlayerMessage;
        DuelPlayer dp = playergo.GetComponent<DuelPlayer>();
        NetworkServer.AddPlayerForConnection(connection, playergo);
        dp.setName(createPlayerMessage.name);
        dp.changeCanMove(false);
        dp.EquipWeapon(duelGun.id, connection.ToString());
        //Debug.Log("Player " + data.name + " Added!");
        return dp;
    }

    [Server]
    public int getRoundTimer()
    {
        if(CantMoveTimer > 0)
        {
            return Mathf.RoundToInt(CantMoveTimer);
        }
        else
        {
            return numPlayers;
        }
    }


    bool gameStarted = false;
    [ServerCallback]
    private void Update()
    {
        //Debug.Log(CantMoveTimer);
        //Debug.Log("P1: " + p1Wins + " P2: " + p2Wins);

        if(p1p != null && p2p != null && gameStarted == false)
        {
            CantMoveTimer -= Time.deltaTime;

            p1p.setTimer(Mathf.RoundToInt(CantMoveTimer + 0.5f));
            p2p.setTimer(Mathf.RoundToInt(CantMoveTimer + 0.5f));

            p1p.changeCanMove((CantMoveTimer <= 0));
            p2p.changeCanMove((CantMoveTimer <= 0));

            if (CantMoveTimer <= 0)
            {
                gameStarted = true;
            }
        }
    }

    private DuelPlayer getOtherPlayer(DuelPlayer p)
    {
        if(p == p1p)
        {
            return p2p;
        }
        else
        {
            return p1p;
        }
    }
}