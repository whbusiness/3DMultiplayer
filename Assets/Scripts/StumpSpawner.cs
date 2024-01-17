using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class StumpSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject stumpPrefab, notePrefab;
    [SerializeField]
    private List<Vector3> spawnPoints = new();
    private float closestDist = Mathf.Infinity;
    [SerializeField]
    private int amountOfStumpsToSpawn;
    private Vector3 randomPosition;
    // Start is called before the first frame update

    public void SpawnStump()
    {
        for(int i = 0; i < amountOfStumpsToSpawn; i++)
        {
            do
            {
                randomPosition = RandomPos();
            } while (randomPosition == new Vector3(0, 0, 0));

            GameObject stump = Instantiate(stumpPrefab, randomPosition, Quaternion.identity);
            GameObject note = Instantiate(notePrefab, randomPosition + new Vector3(0f, 0.26f, 0), Quaternion.identity);
            stump.name = "Stump";
            note.name = "Note";
            note.GetComponent<NetworkObject>().Spawn(true);
            stump.GetComponent<NetworkObject>().Spawn(true);
        }
    }


    Vector3 RandomPos()
    {
        var randomPos = new Vector3(Random.Range(1.5f, 298f), 0.01f, Random.Range(1.4f, 298.2f));
        closestDist = Mathf.Infinity;
        if (spawnPoints.Count > 0)
        {
            foreach (var point in spawnPoints)
            {
                var dist = (point - randomPos).sqrMagnitude;
                print("This is dist: " + dist);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    print("This is close: " + closestDist);
                }
            }
            if(closestDist > 500)
            {
                print("Closest Distance Spawned");
                spawnPoints.Add(randomPos);
                return randomPos;
            }
            else if(closestDist <= 500)
            {
                print("OOPPS");
                return new Vector3(0, 0, 0);
            }
        }
        print("Only should run Once");
        spawnPoints.Add(randomPos);
        return randomPos;

    }
}
