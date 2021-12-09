using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkManager
{
    public static GameManager self;
    protected PlayerData[] players;

    public override void OnStartServer()
    {
        base.OnStartServer();
        self = this;
        NetworkServer.RegisterHandler<PlayerData>(OnCreatePlayer);
        serverStart();
    }

    [Client]
    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterHandler<SceneLoader>(clientLoader);
        NetworkClient.RegisterHandler<DiscconnectMessage>(clientKicked);
    }

    protected virtual void serverStart()
    {

    }

    [Client]
    protected virtual void clientLoader(SceneLoader s)
    {
        Debug.Log(s.sceneName);
        if(s.Equals(null))
        {
            return;
        }
        if (s.connection.ToString().Equals(NetworkClient.connection.ToString()))
        {
            Loading l = Loading.current;

            if (s.loading == true)
            {
                Debug.Log("Loading : " + s.sceneName);
                SceneData map = SceneData.getSceneByName(s.sceneName);
                if(map == null)
                {
                    map = new SceneData { MapName = s.sceneName };
                }
                l.showLoading(map);
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Debug.Log("DeLoading");
                l.stopLoading();
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        else if (s.connection.Equals(""))
        {
            Loading l = Loading.current;

            if (s.loading == true)
            {
                Debug.Log("Loading : " + s.sceneName);
                SceneData map = SceneData.getSceneByName(s.sceneName);
                if (map == null)
                {
                    map = new SceneData { MapName = s.sceneName };
                }
                l.showLoading(map);
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Debug.Log("DeLoading");
                l.stopLoading();
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        ModalWindow m = ModalWindow.self;
        if (!m.checkifShown())
        {
            m.showModalWindow("Error", "Lost Connection To Server.");
            Cursor.lockState = CursorLockMode.None;
        }
    }

    protected virtual void clientKicked(DiscconnectMessage msg)
    {
        if (msg.NetworkConn.ToString().Equals(NetworkClient.connection.ToString()))
        {
            ModalWindow m = ModalWindow.self;
            m.showModalWindow(msg.Header, msg.Body);
            Cursor.lockState = CursorLockMode.None;
        }
    }

    [Server]
    protected void kickClient(NetworkConnection conn,string reason)
    {
        string r = "Kicked For Unknown Reasons";

        if(!reason.Equals("") || reason != null)
        {
            r = reason;
        }
        conn.Send(new DiscconnectMessage { Header = "Kicked From Server", Body = r });
        conn.Disconnect();
    }

    [Server]
    protected virtual void OnCreatePlayer(NetworkConnection connection, PlayerData createPlayerMessage)
    {
        // create a gameobject using the name supplied by client
        GameObject playergo = Instantiate(playerPrefab);
        //playergo.GetComponent<Player>().name = createPlayerMessage.name;

        // set it as the player
        PlayerData data = createPlayerMessage;
        NetworkServer.AddPlayerForConnection(connection, playergo);
        Debug.Log("Player " + data.name + " Added!");
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        clientConnect(conn);
    }

    protected virtual void clientConnect(NetworkConnection conn)
    {
        conn.Send(new PlayerData { name = "User " + UnityEngine.Random.Range(0, 255) });
    }

    [Server]
    public void onKilled(string killer,string enemy)
    {
        Debug.Log("Player " + killer + " Has Killed " + enemy);

        PlayerData k = getPlayeryByName(killer);
        PlayerData e = getPlayeryByName(enemy);

        k.kills++;
        e.deaths++;

        k.setKDA();
        e.setKDA();
    }

    public PlayerData getPlayeryByName(string name)
    {
        foreach (PlayerData ps in players)
        {
            if (ps.name.Equals(name))
            {
                return ps;
            }
        }

        return new PlayerData();
    }

    public PlayerData[] getLeaderboard()
    {
        PlayerData[] data = players;
        Array.Sort(data, new PlayerDataCompairer());
        return data;
    }

    public struct PlayerData : NetworkMessage
    {
        public string name;
        public int ID;

        public int kills;
        public int deaths;
        public float kda;

        public GameObject playerbody { get; set; }
        public Player player { get; set; }

        public void setKDA()
        {
            this.kda = (float)(kills / deaths);
        }
    }

    protected struct SceneLoader : NetworkMessage
    {
        public string sceneName;
        public bool loading;
        public string connection;
    }

    protected class PlayerDataCompairer : IComparer
    {
        public int Compare(object x, object y)
        {
            return (new CaseInsensitiveComparer()).Compare(((PlayerData)x).kda, ((PlayerData)y).kda);
        }
    }

    protected struct DiscconnectMessage : NetworkMessage
    {
        public string Header;
        public string Body;
        public string NetworkConn;
    }
}
