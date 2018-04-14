using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace funisfun.NetUtilities
{
	public class NetUtils : MonoBehaviour
	{
		public static NetCommand[] DecodeBuffer(byte[] buff)
		{
			buff = cleanGarbage(buff);

			string cmdStr = Encoding.ASCII.GetString(buff);
			cmdStr.Trim();
			

			Debug.Log("READING COMMANDS: " + cmdStr);
			string[] cmds = cmdStr.Split('|');
			NetCommand[] netCmds = new NetCommand[cmds.Length];

			for (int i = 0; i < cmds.Length; i++)
			{
				Debug.Log("=====");
				string[] cmdParams = new string[0];
				cmdParams = cmds[i].Split('#');
				string _commandName = cmdParams[0];
				cmdParams = cmdParams.Skip(1).ToArray();
				Debug.Log("Command: " + cmds[i]);
				Debug.Log(cmdParams.Length.ToString() + " parameters.");
				Debug.Log("Command Name: " + _commandName);
				foreach (string par in cmdParams)
				{
					Debug.Log("Parameter: " + par);
				}
				Debug.Log("=====");
				netCmds[i] = new NetCommand(_commandName, cmdParams);
			}

			return netCmds;
		}

		private static byte[] cleanGarbage(byte[] buff)
		{
			int i = 0;

			while (i < buff.Length)
			{
				if (buff[i] == 0)
				{
					break;
				}
				else
				{
					i++;
				}
			}
			if (i == buff.Length)
			{
				i--;
			}
			Array.Clear(buff, i, (buff.Length - 1) - i);
			return buff;
		}

		public static byte SendCmd(string _cmd, int hostId, int targetID, int channelId)
		{
			byte[] msgBytes = new byte[0];
			byte error;

			msgBytes = Encoding.ASCII.GetBytes(_cmd);
			Debug.Log("Message size: " + msgBytes.Length + " bytes.");
			NetworkTransport.Send(hostId, targetID, channelId, msgBytes, 1024, out error);
			return error;
		}

		public static byte SendCmd(NetCommand _cmdNetCmd, int hostId, int targetID, int channelId)
		{
			string _cmd = string.Concat(_cmdNetCmd.commandName + "#", string.Join("#", _cmdNetCmd.cmdParams));
			Debug.Log("Sending command: " + _cmd);
			byte[] msgBytes = new byte[0];
			byte error;

			msgBytes = Encoding.ASCII.GetBytes(_cmd);
			Debug.Log("Message size: " + msgBytes.Length + " bytes.");
			NetworkTransport.Send(hostId, targetID, channelId, msgBytes, 1024, out error);
			return error;
		}

		// Setup a new player
		public static NetCommand PlayerSetup(int _playerId, Vector3 _position, Vector3 _rotation)
		{
			string[] _cmdParams = new string[7] { _playerId.ToString(), _position.x.ToString(), _position.y.ToString(), _position.z.ToString(), _rotation.x.ToString(), _rotation.y.ToString(), _rotation.z.ToString() };
			return new NetCommand("PlayerSetup", _cmdParams);
		}

		// Position updates and new players
		public static NetCommand SendPlayerDict(Dictionary<int, PlayerData> _sendPlayerDict)
		{
			List<string> _cmdParams = new List<string>();
			foreach (var kvp in _sendPlayerDict)
			{
				PlayerData _playerData = kvp.Value;
				_cmdParams.Add(kvp.Key.ToString());
				_cmdParams.Add(_playerData.pos.x.ToString());
				_cmdParams.Add(_playerData.pos.y.ToString());
				_cmdParams.Add(_playerData.pos.z.ToString());
				_cmdParams.Add(_playerData.rot.x.ToString());
				_cmdParams.Add(_playerData.rot.y.ToString());
				_cmdParams.Add(_playerData.rot.z.ToString());
			}

			return new NetCommand("SendPlayerDict", _cmdParams.ToArray());
		}

		public static NetCommand RemovePlayer(int _connectionID)
		{
			string[] _cmdParams = new string[1] { _connectionID.ToString() };
			return new NetCommand("RemovePlayer", _cmdParams);
		}

		public static NetCommand PlayerDisconnect(int _connectionID)
		{
			string[] _cmdParams = new string[1] { _connectionID.ToString() };
			return new NetCommand("PlayerDisconnect", _cmdParams);
		}

		public static NetCommand ConfirmDisconnect()
		{
			string[] _cmdParams = new string[0];
			return new NetCommand("ConfirmDisconnect", _cmdParams);
		}
	}
}
