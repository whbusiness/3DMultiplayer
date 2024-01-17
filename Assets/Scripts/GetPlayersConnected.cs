using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class GetPlayersConnected : MonoBehaviour
{
    private GameObject spawner;
    private float radius = 4;
    private int layerMask;
    private bool hasStarted = false, firstPlayerJoined;
    public static bool gamePlayingState = false;
    private GameObject[] players;
    public Material[] matColours;
    private NetworkManager nManager;
    // Start is called before the first frame update
    void Start()
    {
        hasStarted = false;
        layerMask = 1 << 7;
        PlayerSpawn.amountOfPlayersConnected = 0;
        spawner = GameObject.Find("Spawner");
        nManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }

    

    // Update is called once per frame
    void Update()
    {
        if(PlayerSpawn.amountOfPlayersConnected ==1 && !firstPlayerJoined)
        {
            SpawnTreeServerRpc();
            firstPlayerJoined = true;
        }
        if(PlayerSpawn.amountOfPlayersConnected == 2 && !hasStarted)
        {
            if(!hasStarted)
            {                
                SpawnObjectsServerRpc();
                gamePlayingState = true;
                hasStarted = true;
            }
        }
    }

    [ServerRpc(RequireOwnership = true)]
    void SpawnTreeServerRpc()
    {
        spawner.GetComponent<TreeSpawner>().SpawnTree();
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnObjectsServerRpc()
    {
        spawner.GetComponent<SpawnWoodenPlanks>().SpawnWoodPlank();
        spawner.GetComponent<SpawnWoodenPlanks>().SpawnRock();
        spawner.GetComponent<AISpawn>().SpawnAI();
        spawner.GetComponent<StumpSpawner>().SpawnStump();
    }

    Vector3 RandomPos()
    {
        var randomPos = new Vector3(Random.Range(1.5f, 298f), 1, Random.Range(1.4f, 298.2f));
        /*var listOfPlayersInArea = Physics.OverlapSphere(randomPos, radius, layerMask);
        if (listOfPlayersInArea.Length > 0)
        {
            randomPos = new Vector3(Random.Range(1.5f, 298f), 1, Random.Range(1.4f, 298.2f));
        }*/
        return randomPos;
    }
}
