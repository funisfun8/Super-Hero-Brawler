using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System.Text;
using funisfun.NetUtilities;

public class NetManager_Client : MonoBehaviour {

	public int hostId;
	public int myReliableChannelId;
	public int myUnreliableChannelId;
	public int connectionId;

	Dictionary<int, GameObject> playerDict = new Dictionary<int, GameObject>();

	public GameObject playerLocal;
	public GameObject playerRemote;

	GameObject playerObj;
	PlayerController_Local playerObjController;

	float disconnectTimer;
	public bool disconnectTimerActive = false;

	public bool clientStarted = false;

	void Start()
	{
		Debug.LogError("LOG OPEN.");
	}

	// Update is called once per frame
	void Update()
	{
		if (!clientStarted)
		{
			return;
		}

		int recHostId;
		int recConnectionId;
		int channelId;
		byte[] recBuffer = new byte[1024];
		int dataSize;
		byte error;

		NetworkEventType recData = NetworkTransport.Receive(out recHostId, out recConnectionId, out channelId, recBuffer, recBuffer.Length, out dataSize, out error);

		switch (recData)
		{
			case NetworkEventType.Nothing:
				break;
			case NetworkEventType.ConnectEvent:
				break;
			case NetworkEventType.DataEvent:
				Debug.Log("Data recieved! Message Length: " + recBuffer.Length);
				NetCommand[] cmdBuffer = NetUtils.DecodeBuffer(recBuffer);
				Debug.Log("Running commands...");
				foreach (NetCommand currCmd in cmdBuffer)
				{
					RunCmd(currCmd);
				}
				break;
			case NetworkEventType.DisconnectEvent:
				Debug.Log("Server disconnected. Shutting down;");
				clientStarted = false;
				Destroy(playerObj);
				NetworkTransport.RemoveHost(hostId);
				NetworkTransport.Shutdown();
				Debug.Log("Shutdown complete");
				break;
		}

		if (disconnectTimerActive)
		{
			if (disconnectTimer > 3)
			{
				Debug.LogWarning("TIMEOUT. FORCING SHUTDOWN!");
				Disconnect(true);
			}
			else
			{
				disconnectTimer += Time.deltaTime;
			}
		}
	}

	public void Connect()
	{
		// Initialize the Transport Layer
		NetworkTransport.Init();

		// --What is this for? Do I need it? Something to do with multiple clients maybe?--
		//NetworkServer.Reset();

		// Create connection config
		ConnectionConfig conn_config = new ConnectionConfig();
		myReliableChannelId = conn_config.AddChannel(QosType.Reliable);
		myUnreliableChannelId = conn_config.AddChannel(QosType.UnreliableSequenced);

		// Define network topology
		HostTopology topology = new HostTopology(conn_config, 6);

		// Create host
		hostId = NetworkTransport.AddHost(topology, 0);

		Debug.Log("Started Client Host: " + hostId);

		// Connect to localhost
		byte error;
		connectionId = NetworkTransport.Connect(hostId, "127.0.0.1", 8192, 0, out error);

		Debug.Log(connectionId);
		// Mark the client as started
		clientStarted = true;
	}

	public void Disconnect()
	{
		NetUtils.SendCmd(NetUtils.PlayerDisconnect(playerObjController.playerId), hostId, connectionId, myReliableChannelId);
		disconnectTimer = 0;
		disconnectTimerActive = true;
	}

	public void Disconnect(bool forceFlag)
	{
		if (forceFlag)
		{
			Debug.LogWarning("Initiating shutdown...");
			Destroy(playerObj);
			playerDict.Clear();
			NetworkTransport.RemoveHost(hostId);
			NetworkTransport.Shutdown();

			clientStarted = false;
			Debug.LogError("Shutdown complete.");
		}
		else
		{
			Disconnect();
		}
	}

	void RunCmd(NetCommand runningCmd)
	{
		switch (runningCmd.commandName)
		{
			case "PlayerSetup":
				SetupPlayer(runningCmd.cmdParams);
				break;
			case "SendPlayerDict":
				if (runningCmd.cmdParams.Length == 0)
				{
					break;
				}
				ReceivePlayerDict(runningCmd.cmdParams);
				break;
			case "RemovePlayer":
				RemovePlayer(runningCmd.cmdParams);
				break;
			case "ConfirmDisconnect":
				disconnectTimerActive = false;
				disconnectTimer = 0;
				Disconnect(true);
				break;
			default:
				Debug.Log("Unrecognized command! Aborting...");
				break;
		}
	}

	////--COMMAND FUNCTIONS--////

	// Player start and setup
	void SetupPlayer(string[] _paramArr)
	{
		int _playerID;
		Vector3 _spawnPos;
		Vector3 _spawnRot;

		int.TryParse(_paramArr[0], out _playerID);

		float.TryParse(_paramArr[1], out _spawnPos.x);
		float.TryParse(_paramArr[2], out _spawnPos.y);
		float.TryParse(_paramArr[3], out _spawnPos.z);

		float.TryParse(_paramArr[4], out _spawnRot.x);
		float.TryParse(_paramArr[5], out _spawnRot.y);
		float.TryParse(_paramArr[6], out _spawnRot.z);

		//Spawn local player
		Debug.Log("Spawning local player with ID: " + _playerID);
		playerObj = GameObject.Instantiate(playerLocal, _spawnPos, Quaternion.Euler(_spawnRot));
		playerObjController = playerObj.GetComponent<PlayerController_Local>();
		playerObjController.playerId = _playerID;
		playerDict.Add(_playerID, playerObj);
	}

	// Update player positions
	void ReceivePlayerDict(string[] _paramArr)
	{
		Dictionary<int, PlayerData> _recDict = new Dictionary<int, PlayerData>();

		for (int i = 0; i < _paramArr.Length / 7; i++)
		{
			int _playerID;
			Vector3 _pos;
			Vector3 _rot;

			int.TryParse(_paramArr[i * 7 + 0], out _playerID);

			float.TryParse(_paramArr[i * 7 + 1], out _pos.x);
			float.TryParse(_paramArr[i * 7 + 2], out _pos.y);
			float.TryParse(_paramArr[i * 7 + 3], out _pos.z);

			float.TryParse(_paramArr[i * 7 + 4], out _rot.x);
			float.TryParse(_paramArr[i * 7 + 5], out _rot.y);
			float.TryParse(_paramArr[i * 7 + 6], out _rot.z);

			_recDict.Add(_playerID, new PlayerData(_pos, _rot));
		}

		foreach (var kvp in _recDict)
		{
			if (kvp.Key != playerObjController.playerId)
			{
				Debug.Log("Updating player: " + kvp.Key);
				GameObject _remotePlayer;
				PlayerController_Remote _remotePlayerController;
				if (!playerDict.ContainsKey(kvp.Key))
				{
					Debug.Log("Player doesn't exist. Spawning...");
					_remotePlayer = GameObject.Instantiate(playerRemote, kvp.Value.pos, Quaternion.Euler(kvp.Value.rot));
					_remotePlayerController = _remotePlayer.GetComponent<PlayerController_Remote>();
					_remotePlayerController.playerId = kvp.Key;
					playerDict.Add(kvp.Key, _remotePlayer);
				}
				else
				{
					Debug.Log("Player exists. Updating postion.");
					playerDict.TryGetValue(kvp.Key, out _remotePlayer);
					if (_remotePlayer != null)
					{
						_remotePlayerController = _remotePlayer.GetComponent<PlayerController_Remote>();
						_remotePlayer.transform.position = kvp.Value.pos;
						_remotePlayer.transform.rotation = Quaternion.Euler(kvp.Value.rot);
					}
				}
			}
		}
	}

	// Remove the player object and remove them from the dictionary
	void RemovePlayer(string[] _paramArr)
	{
		GameObject _remPlayer;
		int _remPlayerID;
		int.TryParse(_paramArr[0], out _remPlayerID);
		playerDict.TryGetValue(_remPlayerID, out _remPlayer);

		Destroy(_remPlayer);
		playerDict.Remove(_remPlayerID);
	}
}
