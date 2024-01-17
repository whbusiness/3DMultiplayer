using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public enum AIFaceType
{
    ORIGINAL,
    HURT,
    ANGRY
}

public class AIHandler : NetworkBehaviour
{
    public float aiMovementSpeed;
    public GameObject[] Players;
    public List<Vector3> playerLocations;
    private int randomValue;
    private int layerMask;
    private GameObject AIenemy;
    public static int health;
    public Vector3 randomDestinationAwayFromPlayer;
    public float radius;
    private Collider[] listOfPlayersInArea;
    public int maxHealth;
    [SerializeField]
    private GameObject face;
    [SerializeField]
    private Texture2D[] facesTex;
    private NetworkManager nManager;
    public static bool searchForPlayer = false;
    public static bool holdingWeapon = false, lookingForWeapon = true;
    private GameObject[] woodenPlanksInScene;
    private GameObject spawner, targetWoodenPlankGO;
    public GameObject targetPlayer;
    private float dist = Mathf.Infinity;
    private Vector3 targetWoodenPlankPos = new Vector3(100,100,100);
    [SerializeField]
    private GameObject woodenPlankChildGO;
    private bool hasWeapon = false;
    private AudioSource source;
    [SerializeField]
    private AudioClip screech, slowHeartBeat, fastHeartBeat;
    public float time;
    [SerializeField]
    private float screechTimer, runTimer;
    public float originalSpeed = 4;
    [SerializeField]
    private float runAwaySpeed = 6;
    private float timer;
    [SerializeField]
    private GameObject TriggerArea;
    public float timeBeforeHunt;
    [SerializeField]
    private float timeWhenAICanHunt = 15;
    public float closestDistOfPlayer = Mathf.Infinity;
    public bool chasingPlayer, foundPlayer;
    [SerializeField]
    private float distanceToPlayerBeingChased = Mathf.Infinity;
    public float meleeDist;
    private bool aiCanAttack = false;
    [SerializeField]
    private float knockbackForce;
    [SerializeField]
    private int damage;
    private float lookingForWeaponTimer;
    [SerializeField]
    private Vector3 targetPos;
    [SerializeField]
    private float rotSpeed;
    public bool canRun = true;
    [SerializeField]
    private float maxDistance;
    [SerializeField]
    private bool stopMovingToTarget = false;
    private GameObject playerQuit;
    //private NavMeshAgent[] agents;
    void Start()
    {
        nManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
        face.GetComponent<Renderer>().material.SetTexture("_MainTex", facesTex[(int)AIFaceType.ORIGINAL]);
        source = GetComponent<AudioSource>();
        health = maxHealth;
        layerMask = 1 << 7;
        spawner = GameObject.Find("Spawner");
        Players = GameObject.FindGameObjectsWithTag("Player");
        playerQuit = GameObject.Find("SoundHandler");
        for (int i = 0; i < Players.Length; i++)
        {
            playerLocations.Add(Players[i].transform.position);
        }
        //AIenemy = GameObject.FindGameObjectWithTag("AI");
        originalSpeed = aiMovementSpeed;
    }
    // Update is called once per frame
    void Update()
    {
        Rotations();

        if(aiMovementSpeed > originalSpeed && !AICheckArea.playerFound)
        {
            timer += Time.deltaTime;
            if(timer > 5)
            {
                aiMovementSpeed = originalSpeed;
            }
        }

        if (clientDisconnect.clientLeft)
        {
            print("Client Has Disconnected");
            playerQuit.GetComponent<AllowRespawn>().OnPlayerQuit();
            UpdateTargetInfoServerRpc();
            UpdateAITargetsServerRpc();
            clientDisconnect.clientLeft = false;
        }

        //If there is no one around the AI and no ones done enough damage to it
        if(PlayerController.damageDealt < 100 && !AICheckArea.playerFound && !lookingForWeapon && !chasingPlayer) 
        {
            if (Vector3.Distance(transform.position, targetPos) <= 2 && canRun)
            {
                randomDestinationAwayFromPlayer = new Vector3(Random.Range(1.5f, 298f), 1, Random.Range(1.4f, 298.2f));
                targetPos = randomDestinationAwayFromPlayer;
                PlayerController.damageDealt = 0;
                canRun = false;
            }
            PickANewSpot();
        }

        if(!hasWeapon && !lookingForWeapon)
        {
            lookingForWeaponTimer += Time.deltaTime;
            if(lookingForWeaponTimer > 5)
            {
                lookingForWeaponTimer = 0;
                lookingForWeapon = true;
            }
        }

        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxDistance))
        {
            if (hit.collider.CompareTag("ObjectInScene"))
            {
                stopMovingToTarget = true;
            }
            else
            {
                stopMovingToTarget = false;
            }
        }
        else
        {
            stopMovingToTarget = false;
        }

       if(stopMovingToTarget)
        {
            GetComponent<Rigidbody>().AddForce(aiMovementSpeed * Time.deltaTime * transform.right, ForceMode.VelocityChange);
        }


        if (hasWeapon)
        {
            if (timeBeforeHunt < timeWhenAICanHunt)
            {
                timeBeforeHunt += Time.deltaTime;
                if(!TriggerArea.activeInHierarchy)
                {
                    TurnOffTriggerServerRpc(); //should turn it back on
                }
                if(distanceToPlayerBeingChased < Mathf.Infinity)
                {
                    print("Making closestdistplayer far once");
                    distanceToPlayerBeingChased = Mathf.Infinity;
                }
            }
            if(timeBeforeHunt > timeWhenAICanHunt)
            {
                if (!chasingPlayer)
                {
                    chasingPlayer = true;
                    foundPlayer = false;
                }
                PlayHeartBeatOnChase();
                FindClosestPlayerLocationServerRpc();
                if(TriggerArea.activeInHierarchy)
                {
                    DeactiveTriggerAreaWhenChasingServerRpc();
                }
            }
        }

        if(!hasWeapon && timeBeforeHunt > 0)
        {
            if (!TriggerArea.activeInHierarchy)
            {
                TurnOffTriggerServerRpc(); //should turn it back on
            }
            print("Correct Timer");
            timeBeforeHunt = 0;
        }

        if(hasWeapon && chasingPlayer && distanceToPlayerBeingChased <= meleeDist)
        {
            if(aiCanAttack && aiMovementSpeed > 0)
            {
                targetPlayer.GetComponent<PlayerController>().playerHealth -= damage;
                targetPlayer.GetComponent<PlayerController>().ApplyForceOnAttackServerRpc(transform.forward, knockbackForce, targetPlayer.GetComponent<PlayerController>().playerHealth);
                StartCoroutine(nameof(DelayNextAttack), 1f);
            }
        }


        // min x = 1.5, min z = 1.4, max z = 298.2, max x = 298
        if (PlayerController.damageDealt >= 100)//if player(s) damage AI enough make it run away quickly enough that it is impossible to chase. Then after a while pick a new target to chase
        {
            TurnOffTriggerServerRpc();
            AICheckArea.playerFound = false;
            AICheckArea.playerFoundCloseBy = null;
            targetPlayer = null;
            aiMovementSpeed = runAwaySpeed;
            randomDestinationAwayFromPlayer = new Vector3(Random.Range(1.5f, 298f), 1, Random.Range(1.4f, 298.2f));
            listOfPlayersInArea = Physics.OverlapSphere(randomDestinationAwayFromPlayer, radius, layerMask);
            if(listOfPlayersInArea.Length >0)
            {
                randomDestinationAwayFromPlayer = new Vector3(Random.Range(1.5f, 298f), 1, Random.Range(1.4f, 298.2f));
            }
            else
            {
                targetPos = randomDestinationAwayFromPlayer;
                SetAgentDestinationServerRpc(targetPos);
                timeBeforeHunt = 0;
                chasingPlayer = false;
                closestDistOfPlayer = Mathf.Infinity;
                PlayerController.damageDealt = 0;
            }
        }
        if(PlayerController.damageDealt < 100 && searchForPlayer)
        {
            //FindConnectedPlayerLocationsServerRpc();           
            if (lookingForWeapon) //if AI doesnt have a weapon
            {
                foreach(GameObject go in spawner.GetComponent<SpawnWoodenPlanks>().WoodenPlanksInScene)
                {
                    float getDist = (transform.position - go.transform.position).sqrMagnitude;
                    if (getDist < dist)
                    {
                        targetWoodenPlankPos = go.transform.position;
                        targetWoodenPlankGO = go;
                        dist = getDist;
                    }
                    if (PlayerController.playerPickUpWoodenPlank)
                    {
                        dist = Mathf.Infinity;
                        PlayerPickedUpWoodenPlankFalseServerRpc();
                    }
                }
                MoveToTargetWoodenPlankServerRpc(targetWoodenPlankPos);
                if(Vector3.Distance(transform.position, targetWoodenPlankPos) <= 1)
                {
                    print("Stop");
                    var netObj = targetWoodenPlankGO.GetComponent<NetworkObject>().NetworkObjectId;
                    PickUpWoodenPlankServerRpc(netObj);
                    hasWeapon = true;
                    lookingForWeapon = false;
                    aiCanAttack = true;
                }
            }
            if (AICheckArea.playerFound) //If player has entered an area around the AI
            {
                if(hasWeapon)//If AI has a weapon in hand. Intend to chase and attack player
                {
                    if (time < screechTimer)
                    {
                        time += Time.deltaTime;
                        ChangeFaceAttackServerRpc();
                        aiMovementSpeed = 0;
                        if (!source.isPlaying && IsHost)
                        {
                            source.Stop();
                            source.clip = screech;
                            source.time = 0f;
                            source.Play();
                        }
                    }
                    else if (time >= screechTimer)
                    {
                        aiMovementSpeed = originalSpeed;
                        if (!chasingPlayer)
                        {
                            chasingPlayer = true;
                            targetPlayer = AICheckArea.playerFoundCloseBy;
                        }
                        MoveToTargetPlayerServerRpc(AICheckArea.playerFoundCloseBy.transform.position);
                        PlayHeartBeatOnChase();
                        if (TriggerArea.activeInHierarchy)
                        {
                            DeactiveTriggerAreaWhenChasingServerRpc();
                        }
                        if (PlayerController.damageDealt >= 100)
                        {
                            chasingPlayer = false;
                            time = 0;
                        }
                    }
                }
                else//If AI doesn't have a weapon currently. Intend to scare player by screeching (Play sound) And Run fast in opposite direction
                {
                    //Timer. Stop movement. Play Sound. When Sound ends run.
                    lookingForWeapon = false;
                    time += Time.deltaTime;
                    if(time < screechTimer)
                    {
                        ChangeFaceAttackServerRpc();
                        aiMovementSpeed = 0;
                        if(!source.isPlaying && IsHost)
                        {
                            source.Stop();
                            source.clip = screech;
                            source.time = 0f;
                            source.Play();
                        }
                    }
                    else if(time > screechTimer)
                    {
                        aiMovementSpeed = runAwaySpeed;
                        randomDestinationAwayFromPlayer = new Vector3(Random.Range(1.5f, 298f), 1, Random.Range(1.4f, 298.2f));
                        listOfPlayersInArea = Physics.OverlapSphere(randomDestinationAwayFromPlayer, radius, layerMask);
                        if (listOfPlayersInArea.Length > 0)
                        {
                            randomDestinationAwayFromPlayer = new Vector3(Random.Range(1.5f, 298f), 1, Random.Range(1.4f, 298.2f));
                        }
                        else
                        {
                            targetPos = randomDestinationAwayFromPlayer;
                            SetAgentDestinationServerRpc(targetPos);
                        }
                        time = 0;
                        AICheckArea.playerFound = false;
                        AICheckArea.playerFoundCloseBy = null;
                        TurnOffTriggerServerRpc();
                    }
                }
            }
        }
        Debug.DrawLine(transform.position, transform.position + transform.forward * maxDistance, Color.red);

    }

    void Rotations()
    {
        var targetPoint = new Vector3(targetPos.x, transform.position.y, targetPos.z) - transform.position;
        var targetRot = Quaternion.LookRotation(targetPoint, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
    }

    void PickANewSpot()
    {
        SetAgentDestinationServerRpc(targetPos);
    }


    IEnumerator DelayNextAttack(float sec)
    {
        aiCanAttack = false;
        yield return new WaitForSeconds(sec);
        aiCanAttack = true;
    }

    void PlayHeartBeatOnChase()
    {
        if(distanceToPlayerBeingChased > 1000)
        {
            if(!source.isPlaying)
            {
                source.clip = slowHeartBeat;
                source.Play();
            }
        }
        else if(distanceToPlayerBeingChased < 1000)
        { 
            if(!source.isPlaying)
            {
                source.clip = fastHeartBeat;
                source.Play();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void FindClosestPlayerLocationServerRpc()
    {
        if(!foundPlayer)
        {
            Players = GameObject.FindGameObjectsWithTag("Player");
            print("Run Once When Found Closest Player");
            foreach (GameObject go in Players)
            {
                print("Foreach GO");
                float getDist = (transform.position - go.transform.position).sqrMagnitude;
                if (getDist < closestDistOfPlayer)
                {
                    print("Assign Value to TargetPlayer");
                    targetPlayer = go;
                    closestDistOfPlayer = getDist;
                }
            }
            foundPlayer = true;
        }
        if(targetPlayer != null)
        {
            distanceToPlayerBeingChased = (transform.position - targetPlayer.transform.position).sqrMagnitude;
            MoveToTargetPlayerServerRpc(targetPlayer.transform.position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DeactiveTriggerAreaWhenChasingServerRpc()
    {
        TriggerArea.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayerPickedUpWoodenPlankFalseServerRpc()
    {
        PlayerController.playerPickUpWoodenPlank = false;
        PlayerPickedUpWoodenPlankFalseClientRpc();
    }
    [ClientRpc]
    void PlayerPickedUpWoodenPlankFalseClientRpc()
    {
        PlayerController.playerPickUpWoodenPlank = false;
    }

    [ServerRpc(RequireOwnership = false)]
    void TurnOffTriggerServerRpc()
    {
        StartCoroutine(nameof(TurnOffTrigger), 4f);
    }

    IEnumerator TurnOffTrigger(float sec)
    {
        aiMovementSpeed = runAwaySpeed;
        TriggerArea.SetActive(false);
        yield return new WaitForSeconds(sec);
        aiMovementSpeed = originalSpeed;
        TriggerArea.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    void MoveToTargetPlayerServerRpc(Vector3 pos)
    {
        targetPos = pos;
        distanceToPlayerBeingChased = (transform.position - pos).sqrMagnitude;
        if(!stopMovingToTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(pos.x, transform.position.y, pos.z), aiMovementSpeed * Time.deltaTime);
        }
        //_agent.SetDestination(targetPos);
    }

    [ServerRpc(RequireOwnership = false)]
    void MoveToTargetWoodenPlankServerRpc(Vector3 target)
    {
        targetPos = target;
        if(!stopMovingToTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.x, transform.position.y, target.z), aiMovementSpeed * Time.deltaTime);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PickUpWoodenPlankServerRpc(ulong netObj)
    {
        nManager.SpawnManager.SpawnedObjects.TryGetValue(netObj, out var woodPlank);
        woodPlank.Despawn();
        woodenPlankChildGO.SetActive(true);
        DisplayWoodenPlankChildGOActiveClientRpc();
    }

    [ClientRpc]
    void DisplayWoodenPlankChildGOActiveClientRpc()
    {
        woodenPlankChildGO.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(randomDestinationAwayFromPlayer, radius);
    }


    [ServerRpc(RequireOwnership = false)]
    public void ChangeFaceAttackServerRpc()
    {
        ChangeFaceAttackClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeFaceHurtServerRpc()
    {
        ChangeFaceHurtClientRpc();
    }
    [ClientRpc]
    void ChangeFaceHurtClientRpc()
    {
        StopCoroutine(nameof(HurtFace));
        StopCoroutine(nameof(ChangeFaceAttack));
        StartCoroutine(nameof(HurtFace), 2f);
    }
    [ClientRpc]
    void ChangeFaceAttackClientRpc()
    {
        StopCoroutine(nameof(ChangeFaceAttack));
        StopCoroutine(nameof(HurtFace));
        StartCoroutine(nameof(ChangeFaceAttack), 2f);
    }

    IEnumerator ChangeFaceAttack(float sec)
    {
        face.GetComponent<Renderer>().material.SetTexture("_MainTex", facesTex[(int)AIFaceType.ANGRY]);
        yield return new WaitForSeconds(sec);
        face.GetComponent<Renderer>().material.SetTexture("_MainTex", facesTex[(int)AIFaceType.ORIGINAL]);
    }
    IEnumerator HurtFace(float sec)
    {
        face.GetComponent<Renderer>().material.SetTexture("_MainTex", facesTex[(int)AIFaceType.HURT]);
        yield return new WaitForSeconds(sec);
        face.GetComponent<Renderer>().material.SetTexture("_MainTex", facesTex[(int)AIFaceType.ORIGINAL]);
    }

    [ServerRpc(RequireOwnership = false)]
    void SetAgentDestinationServerRpc(Vector3 dest)
    {
        if(!stopMovingToTarget)
        {
            if (Vector3.Distance(transform.position, dest) <= 2 && !canRun)
            {
                canRun = true;
            }
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(dest.x, transform.position.y, dest.z), aiMovementSpeed * Time.deltaTime);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateTargetInfoServerRpc()
    {
        foundPlayer = false;
        chasingPlayer = false;
        time = 0;
        timeBeforeHunt = 0;
        closestDistOfPlayer = Mathf.Infinity;
        AICheckArea.playerFound = false;
        AICheckArea.playerFoundCloseBy = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateAITargetsServerRpc()
    {
        StartCoroutine(nameof(ResetParameters), 0.2f);
    }

    IEnumerator ResetParameters(float dur)
    {
        searchForPlayer = false;
        yield return new WaitForSeconds(dur);
        Players = GameObject.FindGameObjectsWithTag("Player");
        playerLocations.Clear();
        for (int i = 0; i < Players.Length; i++)
        {
            print(i);
            playerLocations.Add(Players[i].transform.position);
        }
        searchForPlayer = true;
    }

}
