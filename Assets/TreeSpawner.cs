using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject treePrefab;
    [SerializeField]
    private List<Vector3> spawnPoints = new();
    private float closestDist = Mathf.Infinity;
    [SerializeField]
    private int amountOfTreesToSpawn;
    private Vector3 randomPosition;
    // Start is called before the first frame update

    public void SpawnTree()
    {
        for (int i = 0; i < amountOfTreesToSpawn; i++)
        {
            do
            {
                randomPosition = RandomPos();
            } while (randomPosition == new Vector3(0, 0, 0));

            GameObject tree = Instantiate(treePrefab, randomPosition, Quaternion.identity);
            tree.GetComponentInChildren<NetworkObject>().Spawn(true);
            StaticBatchingUtility.Combine(tree);
        }
    }


    Vector3 RandomPos()
    {
        var randomPos = new Vector3(Random.Range(1.5f, 298f), 0.3f, Random.Range(1.4f, 298.2f));
        closestDist = Mathf.Infinity;
        if (spawnPoints.Count > 0)
        {
            foreach (var point in spawnPoints)
            {
                var dist = (point - randomPos).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                }
            }
            if (closestDist > 100)
            {
                spawnPoints.Add(randomPos);
                return randomPos;
            }
            else if (closestDist <= 100)
            {
                return new Vector3(0, 0, 0);
            }
        }
        spawnPoints.Add(randomPos);
        return randomPos;

    }
}
