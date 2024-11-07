#define FUSION_ENABLE_ADDRESSABLES
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public enum STATUSCONNECTION
{
    Disconnected,
    Connecting,
    Connected,
    ConnectedPlaying,
    Offline,

}


public class GO_RunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{

    [Tooltip("Player network prefab")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int maxPlayersPublic = 3;
    [SerializeField] private int maxPlayersInvite = 3;
    [SerializeField] private int maxSecondsAFK = 600;
    
    [SerializeField] private GO_SpawnPoint spawnPoint;

    private NetworkRunner _runner;
    private string _nameSession;

    private static STATUSCONNECTION statusConnection;

    public void Start()
    {
        _ = JoinLobby();
    }


    [ContextMenu("RUNNER")]
    public async void StartGame()
    {
        string nameSession = GenerateRandomString(4);

        SetStatusConnection(STATUSCONNECTION.Connecting);
        if (!_runner)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();            
        }
        Debug.Log("Name sesion: " + _nameSession);

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = string.IsNullOrEmpty(_nameSession) ? nameSession : _nameSession,
            PlayerCount = maxPlayersPublic + maxPlayersInvite,
            CustomLobbyName = "MyCustomLobby",
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
       
        /*
        await Task.Delay(10000);

        NetworkLoadSceneParameters parameters = new NetworkLoadSceneParameters()
        {
          //  LoadSceneMode = LoadSceneMode.Single,  // Carga en modo Single (reemplaza la escena actual)
                                                   // Puedes agregar otras configuraciones aqu� si lo deseas
        };

        SceneRef sceneRef = SceneRef.FromPath("New Scene");
        NetworkSceneAsyncOp sceneLoadOperation = _runner.SceneManager.LoadScene(sceneRef, parameters);*/
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //throw new NotImplementedException();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log(runner.IsSharedModeMasterClient);

        //throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
//        throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        //throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        SetStatusConnection(STATUSCONNECTION.ConnectedPlaying);
        Debug.Log(runner.IsSharedModeMasterClient);
        spawnPlayer(runner);
    }

    public void spawnPlayer(NetworkRunner runner)
    {
        if (runner != null)
        {
            Vector3 positionPlayer;
            Quaternion rotationPlayer;

            NetworkObject _roomPlayer;

            (positionPlayer, rotationPlayer) = spawnPoint.getSpawPointPosition();

            _roomPlayer = runner.Spawn(playerPrefab, positionPlayer, rotationPlayer);
            runner.SetPlayerObject(runner.LocalPlayer, _roomPlayer);
        }
    }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        SetStatusConnection(STATUSCONNECTION.Disconnected);
        //_nameSession = null;
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    //Conecta con el lobby generico para poder acceder a la lista de usuarios
    public async Task JoinLobby()
    {

        Connect();
        SetStatusConnection(STATUSCONNECTION.Connecting);
        var result = await _runner.JoinSessionLobby(SessionLobby.Custom, "MyCustomLobby");
        if (result.Ok)
        {
            SetStatusConnection(STATUSCONNECTION.Connected);
            Debug.Log($"Conexion Exitosa " + result + " - " + _runner);
            //StartGame();
        }
        else
        {
            SetStatusConnection(STATUSCONNECTION.Disconnected);
            Debug.LogError($"Failed to Start: {result.ShutdownReason}");
        }
    }

    private void Connect()
    {
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.AddCallbacks(this);
        }
    }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"Session List Updated with {sessionList.Count} session(s)");

        foreach (var session in sessionList)
        {
            Debug.Log("Name sesion: " + sessionList.Count + " " + session.Name + " " + session.MaxPlayers + " " + session.PlayerCount);
        }

        List<SessionInfo> _sessionList = sessionList.OrderByDescending(p => p.PlayerCount).ToList();

        foreach (var session in _sessionList)
        {
            if (session.PlayerCount < maxPlayersPublic)
            {
                _nameSession = session.Name;
                Debug.Log("Name sesion Order: " + sessionList.Count + " " + _nameSession + " " + session.MaxPlayers + " " + session.PlayerCount + " " + maxPlayersPublic);
                break;
            }
        }
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }
    // Update is called once per frame


    #region Utils
    public void SetStatusConnection(STATUSCONNECTION _status)
    {
        statusConnection = _status;
    }




    public static string GenerateRandomString(int length)
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        var stringChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }





    #endregion

}