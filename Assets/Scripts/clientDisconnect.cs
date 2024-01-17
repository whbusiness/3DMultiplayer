using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class clientDisconnect : MonoBehaviour
{
    public static bool clientLeft = false;
    public static int amountOfPlayersConnected;
    // Start is called before the first frame update
    void Start()
    {
        clientLeft = true;
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnect;
    }
    private void ClientDisconnect(ulong id)
    {
        if (!NetworkManager.Singleton.ShutdownInProgress)
        {
            PlayerSpawn.amountOfPlayersConnected--;
            clientLeft = true;
        }
    }

}
