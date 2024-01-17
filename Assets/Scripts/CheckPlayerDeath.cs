using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CheckPlayerDeath : NetworkBehaviour
{
    [SerializeField]
    private List<GameObject> PlayersAlive = new();
    [SerializeField]
    private GameObject spectatingCanvas, RespawnBtn, deadCanvas;
    [SerializeField]
    private Button nextSpectateBtn;
    private int nextClick;
    // Start is called before the first frame update
    void Start()
    {
        nextClick = 0;
        spectatingCanvas = EnabledRespawnBtn.sCanvas;
        RespawnBtn = EnabledRespawnBtn.btn.gameObject;
        deadCanvas = EnabledRespawnBtn.dCan;
    }

    [ClientRpc]
    public void OnDeathClientRpc()
    {
        if(!IsLocalPlayer) return;
        print("OnDeath");
        PlayersAlive = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        PlayersAlive.Remove(gameObject);
        if (PlayersAlive.Count > 0)
        {
            GetComponentInChildren<Camera>().enabled = false;
            gameObject.transform.GetChild(1).gameObject.SetActive(false);
            //PlayersAlive[randInt].GetComponentInChildren<Camera>().enabled = true;
            for (int i = 0; i < PlayersAlive.Count; i++)
            {
                print("Finding Which Player To Display Camera On Death");
                if (PlayersAlive[i].transform.position.y > -100)
                {
                    PlayersAlive[i].GetComponentInChildren<Camera>().enabled = true;
                    break;
                }
            }
            spectatingCanvas.SetActive(true);
        }
        else
        {
            RespawnBtn.SetActive(true);
        }        
        //Pick A random player
        //Spectate that player (See through their camera but have no effect on them
    }

    private void Update()
    {
        if(gameObject.GetComponent<PlayerController>().playerHealth <= 0)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                print("Left Clicked");
                NextSpectate();
            }
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                print("Right Clicked");
                PrevSpectate();
            }
            if (PlayersAlive.Count > 0 && PlayersAlive != null && PlayersAlive[nextClick].transform.position.y < -100 && PlayersAlive[nextClick].GetComponentInChildren<Camera>().enabled)
            {
                print("Run When The Person U R Spectating Dies");
                if(nextClick+1 < PlayersAlive.Count) { NextSpectate(); }
                else
                {
                    PrevSpectate();
                }
            }
        }
        
    }

    void NextSpectate()
    {
        print("TRANSFORMERS");
        if (nextClick+1 < PlayersAlive.Count)
        {
            if (PlayersAlive[nextClick+1].transform.position.y < -100)
            {
                print("Next is kinematic player");
                PlayersAlive[nextClick].GetComponentInChildren<Camera>().enabled = false;
                PlayersAlive[nextClick].transform.GetChild(1).gameObject.SetActive(false);
                nextClick++;
                deadCanvas.SetActive(true);
            }
            else
            {
                print("Next is not kinematic player");
                deadCanvas.SetActive(false);
                PlayersAlive[nextClick].GetComponentInChildren<Camera>().enabled = false;
                PlayersAlive[nextClick].transform.GetChild(1).gameObject.SetActive(false);
                nextClick++;
                PlayersAlive[nextClick].GetComponentInChildren<Camera>().enabled = true;
            }
        }
    }

    void PrevSpectate()
    {
        print("prev player");
        if (nextClick>0)
        {
            if (PlayersAlive[nextClick -1].transform.position.y < -100)
            {
                print("prev is kinematic player");
                PlayersAlive[nextClick].GetComponentInChildren<Camera>().enabled = false;
                PlayersAlive[nextClick].transform.GetChild(1).gameObject.SetActive(false);
                nextClick--;
                deadCanvas.SetActive(true);
            }
            else
            {
                print("prev is not kinematic player");
                deadCanvas.SetActive(false);
                PlayersAlive[nextClick].GetComponentInChildren<Camera>().enabled = false;
                PlayersAlive[nextClick].transform.GetChild(1).gameObject.SetActive(false);
                nextClick--;
                PlayersAlive[nextClick].GetComponentInChildren<Camera>().enabled = true;
            }
        }
    }

}
