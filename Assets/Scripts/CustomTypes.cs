using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
	public Vector3 pos;
	public Vector3 rot;

	public PlayerData(Vector3 _pos, Vector3 _rot)
	{
		pos = _pos;
		rot = _rot;
	}
}

public class PosUpdate
{
	public int playerId;
	public Vector3 pos;
	public Vector3 rot;
	
	public PosUpdate(Vector3 _pos, Vector3 _rot, int _playId)
	{
		playerId = _playId;
		pos = _pos;
		rot = _rot;
	}
}

public class NetCommand
{
	public string commandName;
	public string[] cmdParams;

	public NetCommand(string _commandName, string[] _cmdParams)
	{
		commandName = _commandName;
		cmdParams = _cmdParams;
	}
}

////---OLD DATA TYPES---////

// [Serializable]
// public class NetCommand
// {
// 
// 	public string commandName;
// 
// }
// 
// [Serializable]
// public class TestMsg : NetCommand
// {
// 	public string strMsg;
// 	public TestMsg()
// 	{
// 		commandName = "TestMsg";
// 		strMsg = "Woohoo!";
// 	}
// }
// 
// [Serializable]
// public class PlayerSetup : NetCommand
// {
// 	public int playId;
// 	public float[] spawnPos;
// 	public float[] spawnRot;
// 	public PlayerSetup(int _connId, Vector3 _spawnPos, Vector3 _spawnRot)
// 	{
// 		playId = _connId;
// 
// 		spawnPos = new float[3] { _spawnPos.x, _spawnPos.y, _spawnPos.z };
// 
// 		spawnRot = new float[3] { _spawnRot.x, _spawnRot.y, _spawnRot.z };
// 
// 		commandName = "PlayerSetup";
// 	}
// }
// 
// [Serializable]
// public class SendPlayerDict : NetCommand
// {
// 	public Dictionary<int, PlayerData> sendDict;
// 
// 	public SendPlayerDict(Dictionary<int, PlayerData> _sendDict)
// 	{
// 		commandName = "SendPlayerDict";
// 		sendDict = _sendDict;
// 	}
// }
// 
// [Serializable]
// public class PosUpdate : NetCommand {
// 
// 	public float[] position;
// 	public float[] rotation;
// 	public int playerId;
// 
// 	public PosUpdate(Vector3 _postition, Vector3 _rotation, int _playerId)
// 	{
// 		commandName = "PosUpdate";
// 
// 		position = new float[3];
// 		position[0] = _postition.x;
// 		position[1] = _postition.y;
// 		position[2] = _postition.z;
// 
// 		rotation = new float[3];
// 		rotation[0] = _rotation.x;
// 		rotation[1] = _rotation.y;
// 		rotation[2] = _rotation.z;
// 
// 		playerId = _playerId;
// 	}
// 
// }