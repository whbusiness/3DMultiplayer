using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWater : MonoBehaviour
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
        targetPos = new Vector3(originalPos.x, -0.6f, originalPos.z);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        lerpTimer += Time.deltaTime /4;
        if (transform.position == originalPos && !moveToTargetPos)
        {
            lerpTimer = 0;
            moveFromTargetPos = false;
            moveToTargetPos = true;
        }
        if (transform.position == targetPos && !moveFromTargetPos)
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
