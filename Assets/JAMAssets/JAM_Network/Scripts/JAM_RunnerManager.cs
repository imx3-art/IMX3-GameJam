#define FUSION_ENABLE_ADDRESSABLES
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum STATUSCONNECTION
{
    Disconnected,
    Connecting,
    Connected,
    Offline,

}


public class JAM_RunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{

    [Tooltip("Player network prefab")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int maxPlayers = 10;
    [SerializeField] private int maxSecondsAFK = 600;
    [SerializeField] private NetworkRunner _runner;
    [SerializeField] private JAM_SpawnPoint spawnPoint;

    private string nameSession = "GameRoom";

    private static STATUSCONNECTION statusConnection;

    [ContextMenu("RUNNER")]
    public async void Start()
    {
        SetStatusConnection(STATUSCONNECTION.Connecting);
        if(_runner)
        {
            Destroy(_runner);
        }
        _runner = gameObject.AddComponent<NetworkRunner>();

        if (_runner == null)
            _runner = gameObject.AddComponent<NetworkRunner>();

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = string.IsNullOrEmpty(nameSession) ? _runner.SessionInfo.Name : nameSession,
            PlayerCount = 10,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
       
        /*
        await Task.Delay(10000);

        NetworkLoadSceneParameters parameters = new NetworkLoadSceneParameters()
        {
          //  LoadSceneMode = LoadSceneMode.Single,  // Carga en modo Single (reemplaza la escena actual)
                                                   // Puedes agregar otras configuraciones aquí si lo deseas
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
        throw new NotImplementedException();
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
        SetStatusConnection(STATUSCONNECTION.Connected);
        Debug.Log(runner.IsSharedModeMasterClient);
        spawnPlayer(runner);
        //throw new NotImplementedException();
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

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
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

    public void SetStatusConnection(STATUSCONNECTION _status)
    {
        statusConnection = _status;
    }

}
