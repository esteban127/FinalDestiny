using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour 
{

    public static GameMaster GM;
    public Transform PlayerPrefab;
    public float SpawnDelay = 1.5f;

    private Transform SpawnPoint;
    private float DeltaTime = 0.0f;
    private GameObject Player;

	void Start () {
        if (GM == null)
        {
            GM = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
        }
	}

    public IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(SpawnDelay);
        Instantiate(PlayerPrefab, transform);
        Player = GameObject.FindGameObjectWithTag("Player");
    }
}
