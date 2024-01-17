using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AllowRespawn : MonoBehaviour
{
    public GameObject[] players;
    public List<GameObject> deadPlayers = new();
    public GameObject deadCanvas, RespawnBtn, lostCanvas, wonCanvas;
    public GameObject AIEnemy;
    public float waitingForReturnDuration;
    private AudioSource[] listOfAudioSources;
    private AudioSource lostAudio, wonAudio;
    [SerializeField]
    private int amountOfNotesNeedToWin;

    private void Start()
    {
        lostAudio = GameObject.Find("LoseSound").GetComponent<AudioSource>();
        wonAudio = GameObject.Find("WonSound").GetComponent<AudioSource>();
    }

    public void Check()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        if(players.Length > 0)
        {
            foreach(GameObject p in players)
            {
                if(p.transform.position.y < -100 && !deadPlayers.Contains(p))
                {
                    deadPlayers.Add(p);
                }
            }
        }
        if(deadPlayers.Count == players.Length)
        {
            LostServerRpc();
            //UpdateDeathsForAllClientRpc();
        }
        if(PlayerController.amountOfNotesCollected >= amountOfNotesNeedToWin)
        {
            WonServerRpc();
        }
    }

    public void OnPlayerQuit()
    {
        deadPlayers.Clear();
    }

    [ServerRpc(RequireOwnership = true)]
    void LostServerRpc()
    {
        listOfAudioSources = FindObjectsOfType<AudioSource>();
        foreach(var source in listOfAudioSources)
        {
            source.Stop();
        }
        lostAudio.Play();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        NetworkManager.Singleton.Shutdown();
        DisplayLostCanvasClientRpc();
        StartCoroutine(nameof(ReloadScene), waitingForReturnDuration);
    }

    [ClientRpc]
    void DisplayLostCanvasClientRpc()
    {
        lostCanvas.SetActive(true);
    }

    [ServerRpc(RequireOwnership = true)]
    void WonServerRpc()
    {
        listOfAudioSources = FindObjectsOfType<AudioSource>();
        foreach (var source in listOfAudioSources)
        {
            source.Stop();
        }
        wonAudio.Play();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        NetworkManager.Singleton.Shutdown();
        DisplayWonCanvasClientRpc();
        StartCoroutine(nameof(ReloadScene), waitingForReturnDuration);
    }

    [ClientRpc]
    void DisplayWonCanvasClientRpc()
    {
        wonCanvas.SetActive(true);
    }

    IEnumerator ReloadScene(float dur)
    {
        yield return new WaitForSeconds(dur);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [ClientRpc]
    void UpdateDeathsForAllClientRpc()
    {
        deadCanvas.SetActive(true);
        RespawnBtn.SetActive(true);
    }

}
