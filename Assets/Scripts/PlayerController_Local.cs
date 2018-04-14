using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using funisfun.NetUtilities;

public class PlayerController_Local : MonoBehaviour {

	public float MoveSpeed;
	public float TurnSpeed;

	public int playerId;

	private float updateInterval;

	GameObject netManagerObj;
	NetManager_Client netManager;

	// Use this for initialization
	void Start () {
		netManagerObj = GameObject.Find("NetManager-Obj");
		netManager = netManagerObj.GetComponent<NetManager_Client>();
	}
	
	// Update is called once per frame
	void Update () {
		var z = Input.GetAxis("Vertical") * Time.deltaTime * MoveSpeed;
		var x = Input.GetAxis("Horizontal") * Time.deltaTime * TurnSpeed;

		transform.Translate(0, 0, z);
		transform.Rotate(0, x, 0);

		updateInterval += Time.deltaTime;
		if (updateInterval > 0.11f) // 9 times per second
		{
			updateInterval = 0;
			// Position Update
			if (!netManager.disconnectTimerActive)
			{
				NetUtils.SendCmd(new NetCommand("PosUpdate", new string[] { playerId.ToString(), transform.position.x.ToString(), transform.position.y.ToString(), transform.position.z.ToString(), transform.rotation.eulerAngles.x.ToString(), transform.rotation.eulerAngles.y.ToString(), transform.rotation.eulerAngles.z.ToString() }), netManager.hostId, netManager.connectionId, netManager.myUnreliableChannelId);
			}
		}
	}
}
