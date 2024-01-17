using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCheck : MonoBehaviour
{
    public static bool awayFromWater = true;

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            awayFromWater = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            awayFromWater = true;
        }
    }
}
