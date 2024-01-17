using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WaterMovement : MonoBehaviour
{
    private Vector3 originalPos;
    [SerializeField]
    private Vector3 targetPos;
    [SerializeField]
    private bool moveToTargetPos, moveFromTargetPos;
    [SerializeField]
    private float lerpTimer;
    // Start is called before the first frame update
    void Awake()
    {
        originalPos = transform.position;
        targetPos = new Vector3(originalPos.x, -1.25f, originalPos.z);
    }

    // Update is called once per frame
    void Update()
    {
        lerpTimer += Time.deltaTime;
        if(transform.position == originalPos)
        {
            lerpTimer = 0;
            moveFromTargetPos = false;
            moveToTargetPos = true;
        }
        if (transform.position == targetPos)
        {
            lerpTimer = 0;
            moveToTargetPos = false;
            moveFromTargetPos = true;
        }
        if (moveToTargetPos)
        {
            print("Move To Target Position");
            transform.position = Vector3.Lerp(originalPos, targetPos, lerpTimer);
        }
        else if (moveFromTargetPos)
        {
            print("Move TO Original Position");
            transform.position = Vector3.Lerp(targetPos, originalPos, lerpTimer);
        }
    }
}
