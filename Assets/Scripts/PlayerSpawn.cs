using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawn : NetworkBehaviour
{
    [SerializeField]
    private Vector3 randomPos;
    [SerializeField]
    private float radius;
    private int layerMask;
    public static int amountOfPlayersConnected;
    public MeshRenderer thisObj;
    [SerializeField]
    private Material[] matColours;
    private List<int> previousRandNums = new List<int>();
    [SerializeField]
    private Color[] colours;
    public GameObject aiEnemy;
    public List<GameObject> listOfPlayers = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        //CheckPlayerCountServerRpc();
        layerMask = 1 << 7; //Player layer
    }

    public override void OnNetworkSpawn()
    {               
        if (IsOwner)
        {
            print("Run");
            listOfPlayers.Add(gameObject);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            IncreasePlayerCountServerRpc();
        }


        //works for spawning player in random position
        transform.position = RandomPos();


        base.OnNetworkSpawn();
        var randInt = Random.Range(0, colours.Length);
        gameObject.GetComponent<MeshRenderer>().material.color = colours[randInt];
        if(IsServer)
        {
            print("Updating AITargets On Spawn");
            UpdateAITargetsServerRpc();
        }
        //IsClient changes every players colour when someone connects)
        // ChangeColourServerRpc();
    }

    private void Update()
    {
        if (aiEnemy == null)
        {
            aiEnemy = GameObject.FindGameObjectWithTag("AI");
        }
    }


    [ServerRpc(RequireOwnership = false)]
    void IncreasePlayerCountServerRpc()
    {
        listOfPlayers.Add(gameObject);
        transform.position = new Vector3(5 + Random.Range(2, 4), 1.2f, 5 + Random.Range(2, 4));
        amountOfPlayersConnected++;
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateAITargetsServerRpc()
    {
        AIHandler.searchForPlayer = false;
        StartCoroutine(nameof(ChangeAITargets), 1f);
    }

    IEnumerator ChangeAITargets(float sec)
    {
        //aiEnemy.GetComponent<AIHandler>().playerLocations.Clear();
        yield return new WaitForSeconds(sec);
        if (aiEnemy != null)
        {
            aiEnemy.GetComponent<AIHandler>().Players = GameObject.FindGameObjectsWithTag("Player");
            aiEnemy.GetComponent<AIHandler>().playerLocations.Clear();
            for (int i = 0; i < aiEnemy.GetComponent<AIHandler>().Players.Length; i++)
            {
                print(i);
                aiEnemy.GetComponent<AIHandler>().playerLocations.Add(aiEnemy.GetComponent<AIHandler>().Players[i].transform.position);
            }
        }
        AIHandler.searchForPlayer = true;
    }

    Vector3 RandomPos()
    {
        var randomPos = new Vector3(Random.Range(1.5f, 298f), 1, Random.Range(1.4f, 298.2f));
        return randomPos;
    }


}
