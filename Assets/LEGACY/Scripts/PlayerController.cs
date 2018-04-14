using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
	[SyncVar]
	Vector3 realPosition = Vector3.zero;
	[SyncVar]
	Quaternion realRotation;

	private float updateInterval;

	Health healthScr;

	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	//private float teleportLimit = 10f;

	void Start()
	{
		healthScr = GetComponent<Health>();
	}

	void Update()
	{
		if (!isLocalPlayer)
		{
			//Interpolate position & roatation
			if (healthScr.tpFlag != true) {
				transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
				transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
			}
			return;
		}

		var x = Input.GetAxis("Horizontal") * Time.deltaTime * 200.0f;
		var z = Input.GetAxis("Vertical") * Time.deltaTime * 9.0f;

		transform.Rotate(0, x, 0);
		transform.Translate(0, 0, z);

		updateInterval += Time.deltaTime;
		if (updateInterval > 0.11f) // 9 times per second
		{
			updateInterval = 0;
			CmdSync(transform.position, transform.rotation);
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			CmdFire();
		}
	}

	void realPosUpdate(Vector3 _realPos)
	{
		realPosition = _realPos;
	
		if (!isLocalPlayer && healthScr.tpFlag == true)
		{
			transform.position = realPosition;
			transform.rotation = realRotation;
			healthScr.tpFlag = false;
		}
	}

	public override void OnStartLocalPlayer()
	{
		GetComponent<Renderer>().material.color = Color.cyan;
	}

	[Command]
	void CmdFire()
	{
		// Create the bullet from the bullet prefab
		var bullet = (GameObject)Instantiate(
			bulletPrefab,
			bulletSpawn.position,
			bulletSpawn.rotation
			);

		// Add velocity
		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 20;

		// Spaw nthe bullet on the clients
		NetworkServer.Spawn(bullet);

		// Destroy the bullet after 2 seconds
		Destroy(bullet, 2.0f);
	}

	[Command]
	void CmdSync(Vector3 position, Quaternion rotation)
	{
		realPosition = position;
		realRotation = rotation;
	}
}