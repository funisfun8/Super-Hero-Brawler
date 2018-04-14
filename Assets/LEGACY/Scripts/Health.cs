using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {

	public RectTransform healthBar;
	public const int maxHealth = 100;
	public bool destroyOnDeath;

	public bool tpFlag = false;

	[SyncVar(hook = "OnChangeHealth")]
	public int currentHealth = maxHealth;

	private NetworkStartPosition[] spawnPoints;

	void Start()
	{
		if (isLocalPlayer)
		{
			spawnPoints = FindObjectsOfType<NetworkStartPosition>();
		}
	}

	public void TakeDamage(int amount)
	{
		if (!isServer)
		{
			return;
		}

		currentHealth -= amount;
		if (currentHealth <= 0)
		{
			if (destroyOnDeath)
			{
				Destroy(gameObject);
			}
			else
			{
				currentHealth = maxHealth;

				// Client Rpc Call called on server but run on the clients
				RpcRespawn();
			}
		}
	}

	void OnChangeHealth (int currentHealth)
	{
		healthBar.sizeDelta = new Vector2(currentHealth, healthBar.sizeDelta.y);
	}

	[ClientRpc]
	void RpcRespawn()
	{
		if (isLocalPlayer)
		{
			// Set the spawn point to origin as a default value
			Vector3 spawnPoint = Vector3.zero;

			findSpawn:
			// If there is a spawn point array and the array is not empty, pick a spawn point at random
			if (spawnPoints != null && spawnPoints.Length > 0)
			{
				spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
				if (spawnPoint == transform.position)
				{
					print("At spawnpoint! Retrying!");
					goto findSpawn;
				}
			}

			transform.position = spawnPoint;
		}
		// Set tpFlag true;
		tpFlag = true;
		GetComponent<Renderer>().material.color = Color.white;
	}
}
