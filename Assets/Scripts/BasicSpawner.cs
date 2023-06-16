using Fusion.Sockets;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//URL : https://doc.photonengine.com/fusion/current/tutorials/host-mode-basics/2-setting-up-a-scene

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{

    [SerializeField] private NetworkPrefabRef _ball;


    private NetworkRunner _runner;

    [SerializeField] private NetworkPrefabRef _playerPrefab;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    private bool _mouseButton0;
    private bool _mouseButton1;
    private void Update()
    {
        _mouseButton0 = _mouseButton0 | Input.GetMouseButton(0);
        _mouseButton1 = _mouseButton1 | Input.GetMouseButton(1);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {

        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);

            // Keep track of the player avatars so we can remove it when they disconnect
            _spawnedCharacters.Add(player, networkPlayerObject);
            Debug.Log("You have joined the game.");
        }

    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {

        // Find and remove the players avatar
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
            Debug.Log("You have left the game.");
        }

    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        if (Input.GetKey(KeyCode.W))
            data.direction += Vector3.forward;

        if (Input.GetKey(KeyCode.S))
            data.direction += Vector3.back;

        if (Input.GetKey(KeyCode.A))
            data.direction += Vector3.left;

        if (Input.GetKey(KeyCode.D))
            data.direction += Vector3.right;

        if (_mouseButton0)
            data.buttons |= NetworkInputData.MOUSEBUTTON1;
        _mouseButton0 = false;

        if (_mouseButton1)
            data.buttons |= NetworkInputData.MOUSEBUTTON2;
        _mouseButton1 = false;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { Debug.Log("On Input Missing."); }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { Debug.Log("On Shut Down."); }
    public void OnConnectedToServer(NetworkRunner runner) { Debug.Log("On Connected to Server."); }
    public void OnDisconnectedFromServer(NetworkRunner runner) { Debug.Log("On Disconnected from Server."); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { Debug.Log("On Connect Request."); }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.Log("On Conenct Failed."); }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { Debug.Log("On User Simulation Message."); }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { Debug.Log("On Session List Updated."); }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { Debug.Log("On Custom Authentication Response."); }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { Debug.Log("On Host Migration."); }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { Debug.Log("On Reliable Data Received."); }
    public void OnSceneLoadDone(NetworkRunner runner) { Debug.Log("On Scene Load Done."); }
    public void OnSceneLoadStart(NetworkRunner runner) {
        Debug.Log("On Scene Load Start.");

        StartCoroutine("spawnBalls");

    }


    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

    }


    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 400, 80), "Host"))
            {
                StartGame(GameMode.Host);
            }

            if (GUI.Button(new Rect(0, 80, 400, 80), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }

    IEnumerator spawnBalls()
    {
        yield return new WaitForSeconds(5);
        _runner.Spawn(_ball);
        Debug.Log("Ball spawned.");
    }
}
