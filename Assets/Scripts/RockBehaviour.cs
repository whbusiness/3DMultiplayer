using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class RockBehaviour : NetworkBehaviour
{
    [SerializeField]
    private float speed;
    private GameObject AIEnemy;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GetComponent<Rigidbody>().AddForce(transform.forward * speed, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GetComponent<Rigidbody>().velocity.magnitude < 0.001) return;
        if (collision.gameObject.CompareTag("AI"))
        {
            StunAIServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = true)]
    void StunAIServerRpc()
    {
        StartCoroutine(nameof(StunAIForDuration), 2f);
    }

    IEnumerator StunAIForDuration(float duration)
    {
        AIEnemy = GameObject.FindGameObjectWithTag("AI");
        AIEnemy.GetComponent<AIHandler>().aiMovementSpeed = 0;
        yield return new WaitForSeconds(duration);
        AIEnemy.GetComponent<AIHandler>().aiMovementSpeed = AIEnemy.GetComponent<AIHandler>().originalSpeed;
    }

}
