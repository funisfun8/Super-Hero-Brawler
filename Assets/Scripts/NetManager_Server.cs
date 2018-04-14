using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System.Text;
using funisfun.NetUtilities;

public class NetManager_Server : MonoBehaviour {

	public int hostId;
	public int myReliableChannelId;
	public int myUnreliableChannelId;
	public int connectionId;

	private float updateInterval;

	Dictionary<int, PlayerData> playerDict = new Dictionary<int, PlayerData>();
	Dictionary<int, PlayerController_Remote> playerObjDict = new Dictionary<int, PlayerController_Remote>();

	public GameObject playerRemote;

	public bool serverStarted = false;

	void Start()
	{
		Debug.LogError("LOG OPEN.");
	}

	// Update is called once per frame
	void Update()
	{
		if (!serverStarted)
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
				Debug.Log("Client, " + recConnectionId + " has initiated a connection. Time: " + Time.time);

				// Check if player ID exists
				if (playerDict.ContainsKey(recConnectionId))
				{
					// Remove player ID
					Debug.Log("PLAYER ID EXISTS! Deleting...");
					playerDict.Remove(recConnectionId);
				}

				// Add player to dictionary
				playerDict.Add(recConnectionId, new PlayerData(Vector3.zero, Vector3.zero));

				// Spawn remote player, add it to the object dictionary, and set it's player ID
				playerObjDict.Add(recConnectionId, GameObject.Instantiate(playerRemote, Vector3.zero, Quaternion.Euler(Vector3.zero)).GetComponent<PlayerController_Remote>());
				playerObjDict[recConnectionId].playerId = recConnectionId;
				Debug.Log("Gameobject exists? " + playerObjDict[recConnectionId].gameObject.transform);

				// Tell the client to setup its player object.
				NetUtils.SendCmd(NetUtils.PlayerSetup(recConnectionId, Vector3.zero, Vector3.zero), hostId, recConnectionId, myReliableChannelId);

				// Send client current player positions.
				NetUtils.SendCmd(NetUtils.SendPlayerDict(playerDict), hostId, recConnectionId, myReliableChannelId);
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
				playerDisconnect(recConnectionId);
				break;
		}

		updateInterval += Time.deltaTime;
		if (updateInterval > 0.11f) // 9 times per second
		{
			updateInterval = 0;
			// Position Update
			if (playerDict.Count != 0)
			{
				foreach (var kvp in playerDict)
				{
					NetUtils.SendCmd(NetUtils.SendPlayerDict(playerDict), hostId, kvp.Key, myUnreliableChannelId);
				}
			}
		}
	}

	// Custom Methods

	public void StartServer()
	{
		// Initialize the Transport Layer
		NetworkTransport.Init();

		// Create connection config
		ConnectionConfig conn_config = new ConnectionConfig();
		myReliableChannelId = conn_config.AddChannel(QosType.Reliable);
		myUnreliableChannelId = conn_config.AddChannel(QosType.UnreliableSequenced);

		// Define network topology
		HostTopology topology = new HostTopology(conn_config, 6);

		// Create host
		hostId = NetworkTransport.AddHost(topology, 8192);
		Debug.Log("Started Server Host: " + hostId);

		// Mark the server as started
		serverStarted = true;
	}

	public void ShutdownServer()
	{
		NetworkTransport.RemoveHost(hostId);
		NetworkTransport.Shutdown();

		serverStarted = false;
		Debug.Log("Shutdown complete.");
	}

	void RunCmd(NetCommand runningCmd)
	{
		switch (runningCmd.commandName)
		{
			case "PosUpdate":
				PosUpdate(runningCmd.cmdParams);
				break;
			case "PlayerDisconnect":
				Debug.LogWarning("PLAYER DISCONNECT MESSAGE!");
				playerDisconnect(runningCmd.cmdParams);
				break;
			default:
				Debug.Log("Unrecognized command! Aborting...");
				break;
		}
	}

	////--COMMAND FUNCTIONS--////

	// Update player dictionary with new position
	void PosUpdate(string[] _paramArr)
	{
		int _playerId;
		Vector3 _pos;
		Vector3 _rot;
		PlayerController_Remote _remotePlayer;

		int.TryParse(_paramArr[0], out _playerId);

		float.TryParse(_paramArr[1], out _pos.x);
		float.TryParse(_paramArr[2], out _pos.y);
		float.TryParse(_paramArr[3], out _pos.z);

		float.TryParse(_paramArr[4], out _rot.x);
		float.TryParse(_paramArr[5], out _rot.y);
		float.TryParse(_paramArr[6], out _rot.z);

		playerDict[_playerId] = new PlayerData(_pos, _rot);
		if(playerObjDict.TryGetValue(_playerId, out _remotePlayer))
		{
			_remotePlayer.gameObject.transform.position = _pos;
			_remotePlayer.gameObject.transform.rotation = Quaternion.Euler(_rot);
		}
	}

	// Remove player on disconnect
	void playerDisconnect(int _discPlayer)
	{
		if (playerDict.ContainsKey(_discPlayer))
		{
			Debug.Log("Client, " + _discPlayer + " has disconnected. Time: " + Time.time);
			playerDict.Remove(_discPlayer);
			GameObject.Destroy(playerObjDict[_discPlayer].gameObject);
			playerObjDict.Remove(_discPlayer);
			foreach (var kvp in playerDict)
			{
				NetUtils.SendCmd(NetUtils.RemovePlayer(_discPlayer), hostId, kvp.Key, myReliableChannelId);
			}
		}
	}
	void playerDisconnect(string[] _paramArr)
	{
		int _discPlayer;
		int.TryParse(_paramArr[0], out _discPlayer);
		Debug.Log("Client, " + _discPlayer + " has disconnected. Time: " + Time.time);
		playerDict.Remove(_discPlayer);
		GameObject.Destroy(playerObjDict[_discPlayer].gameObject);
		playerObjDict.Remove(_discPlayer);
		NetUtils.SendCmd(NetUtils.ConfirmDisconnect(), hostId, _discPlayer, myReliableChannelId);
		foreach (var kvp in playerDict)
		{
			NetUtils.SendCmd(NetUtils.RemovePlayer(_discPlayer), hostId, kvp.Key, myReliableChannelId);
		}
	}
}
