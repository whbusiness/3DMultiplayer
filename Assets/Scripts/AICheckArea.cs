using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICheckArea : MonoBehaviour
{
    public static GameObject playerFoundCloseBy;
    public static bool playerFound;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            print("Found Player");
            playerFound = true;
            playerFoundCloseBy = collision.gameObject;
        }
    }
}
