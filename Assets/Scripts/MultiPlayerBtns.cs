using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.VisualScripting;
public class MultiPlayerBtns : MonoBehaviour
{
    [SerializeField]
    private GameObject joiningPanel;
    public List<GameObject> listOfPlayers = new();
    public void OnHost()
    {
        NetworkManager.Singleton.StartHost();
        //SpawnPlayerServerRpc();
        joiningPanel.SetActive(false);
    }
    public void OnServer()
    {
        NetworkManager.Singleton.StartServer();
        foreach (Transform g in GetComponentsInChildren<Transform>())
        {
            g.gameObject.SetActive(false);
        }
    }
    public void OnClient()
    {
        NetworkManager.Singleton.StartClient();
        //SpawnPlayerServerRpc();
        joiningPanel.SetActive(false);
    }

}
