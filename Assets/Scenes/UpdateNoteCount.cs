using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UpdateNoteCount : MonoBehaviour
{
    [SerializeField]
    private GameObject[] noteTxts;

    
    public void OnNoteUpdate(int noteCount)
    {
        noteTxts = GameObject.FindGameObjectsWithTag("NoteCountTxt");
        for (int i = 0; i < noteTxts.Length; i++)
        {
            print("Note FOund: " + i);
            noteTxts[i].GetComponent<TextMeshProUGUI>().text = "Notes: " + noteCount;
        }
        UpdateNotesForAllClientRpc(noteCount);
    }
    [ClientRpc]
    void UpdateNotesForAllClientRpc(int noteCount)
    {
        print("Updating Note Count");
        for(int i = 0; i < noteTxts.Length; i++)
        {
            noteTxts[i].GetComponent<TextMeshProUGUI>().text = "Notes: " + noteCount;
        }
    }
}
