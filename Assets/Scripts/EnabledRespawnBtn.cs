using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnabledRespawnBtn : MonoBehaviour
{
    [SerializeField]
    private Button respawnBtn;
    public static Button btn;
    [SerializeField]
    private GameObject spectatingCanvas;
    public static GameObject sCanvas;
    [SerializeField]
    private GameObject deadCanvas;
    public static GameObject dCan;
    private void Start()
    {
        btn = respawnBtn;
        sCanvas = spectatingCanvas;
        dCan = deadCanvas;
    }

}
