using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReturningToMainMenu : MonoBehaviour
{
    private float time;
    private GameObject soundHandler;
    // Start is called before the first frame update
    void Awake()
    {
        soundHandler = GameObject.Find("SoundHandler");
        time = soundHandler.GetComponent<AllowRespawn>().waitingForReturnDuration;
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        GetComponent<TextMeshProUGUI>().text = "Returning To Main Menu in: " + time;
    }
}
