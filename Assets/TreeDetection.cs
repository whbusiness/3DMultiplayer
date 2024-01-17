using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TreeDetection : NetworkBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        if (IsLocalPlayer)
        {
            if (other.gameObject.CompareTag("Tree"))
            {
                other.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(IsLocalPlayer)
        {
            if (other.gameObject.CompareTag("Tree"))
            {
                other.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}
