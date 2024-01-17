using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnWoodenPlanks : MonoBehaviour
{
    [SerializeField]
    private GameObject woodPlanks, rocks;
    public List<GameObject> WoodenPlanksInScene = new List<GameObject>();
    [SerializeField]
    private int amountOfWoodenPlanksToSpawn, amountOfRocksToSpawn;

    public void SpawnWoodPlank()
    {
        for(int i =0; i< amountOfWoodenPlanksToSpawn; i++)
        {
            GameObject go = Instantiate(woodPlanks, RandomPos(), Quaternion.Euler(90,0,0));
            go.name = "PlankOfWood";
            WoodenPlanksInScene.Add(go);
            go.GetComponent<NetworkObject>().Spawn(true);
        }
    }

    public void SpawnRock()
    {
        for(int i =0; i< amountOfRocksToSpawn; i++)
        {
            GameObject go = Instantiate(rocks, RandomPos(), Quaternion.Euler(90, 0, 0));
            go.name = "Rock";
            go.GetComponent<NetworkObject>().Spawn(true);
        }
    }

    public void RespawnWoodenPlankAfterDropped(Vector3 t, Vector3 fwd)
    {
        GameObject go = Instantiate(woodPlanks, t, Quaternion.identity);
        go.name = "PlankOfWood";
        go.GetComponent<Rigidbody>().isKinematic = false;
        go.GetComponent<Rigidbody>().AddForce(fwd * 1, ForceMode.Impulse);
        go.GetComponent<NetworkObject>().Spawn();

    }

    Vector3 RandomPos()
    {
        var randomPos = new Vector3(Random.Range(1.5f, 298f), 0.1f, Random.Range(1.4f, 298.2f));
        return randomPos;
    }

}
