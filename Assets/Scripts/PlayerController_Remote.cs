using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_Remote : MonoBehaviour {

	//public float MoveSpeed;
	//public float TurnSpeed;

	public int playerId;

	GameObject netManagerObj;
	NetManager_Client netManagerClient;
	NetManager_Server netManagerServer;

	// Use this for initialization
	void Start () {
		gameObject.GetComponent<Renderer>().material.color = Color.red;
		netManagerObj = GameObject.Find("NetManager-Obj");
		netManagerClient = netManagerObj.GetComponent<NetManager_Client>();
		netManagerServer = netManagerObj.GetComponent<NetManager_Server>();
	}
	
	// Update is called once per frame
	void Update () {
		if(!netManagerClient.clientStarted && !netManagerServer.serverStarted)
		{
			Destroy(this.gameObject);
		}
		//var z = Input.GetAxis("Vertical") * Time.deltaTime * MoveSpeed;
		//var x = Input.GetAxis("Horizontal") * Time.deltaTime * TurnSpeed;
		//
		//transform.Translate(0, 0, z);
		//transform.Rotate(0, x, 0);
	}
}
