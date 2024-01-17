using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AISpawn : MonoBehaviour
{
    [SerializeField]
    private GameObject aiCharacter;

    public void SpawnAI()
    {
        GameObject go = Instantiate(aiCharacter, RandomPos(), Quaternion.identity);
        go.name = "AI";
        go.GetComponent<NetworkObject>().Spawn();
    }

    Vector3 RandomPos()
    {
        var randomPos = new Vector3(Random.Range(1.5f, 298f), 1, Random.Range(1.4f, 298.2f));
        return randomPos;
    }


}
