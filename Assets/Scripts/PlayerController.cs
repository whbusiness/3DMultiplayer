using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

public enum FaceType
{
    ORIGINAL,
    HURT,
    ANGRY
}
public enum Colours
{
    BLACK,
    BLUE,
    YELLOW,
    GREEN,
    MAGENTA
}

public class PlayerController : NetworkBehaviour
{
    private Vector3 movementDirection, move, look;
    private float LookingX, LookingY;
    NewControls controls;
    private Rigidbody _rb;
    public float speed, SensX, SensY, jumpForce;
    private float RotateX, RotateY;
    [SerializeField] private float xClamp = 85f;
    private bool isJumping, isAttacking, isInteracting;
    public static int damageDealt;
    public GameObject AIenemy;
    public float interactingDist;
    public float closeEnoughToAttack, radius, maxDist, smoothTime;
    private GameObject[] players;
    private int woodenLayerMask;
    public Vector3 offsetForWeapon;
    private bool canPlayerPickUp;
    public GameObject PlayerWoodenPlank;
    private GameObject woodenPlankSpawner;
    NetworkManager nManager;
    private int amountOfRocks;

    //Rock Object
    [SerializeField]
    private GameObject Hand;
    [SerializeField]
    private GameObject rocks;
    [SerializeField]
    private float forceAmount;

    //Face
    [SerializeField]
    private GameObject face;
    [SerializeField]
    private Texture2D[] facesTex;

    [SerializeField]
    public int playerHealth;
    private int faceLayerMask, woodenPlankChildGOandStumpLayerMask;
    public static ulong ClientID;
    [SerializeField]
    private List<int> previousRandNums = new List<int>();
    [SerializeField]
    private Material[] matColours;
    public static bool hasStarted;
    public static List<GameObject> listOfPlayers = new List<GameObject>();
    [SerializeField]
    private Animator anim;
    private bool canAttack = false;
    [SerializeField]
    private AudioSource windAudioSource, rainAudioSource, ambienceAudioSource, waterAudioSource;
    public static bool playerPickUpWoodenPlank = false;
    [SerializeField]
    private List<GameObject> PlayersAlive;
    private int randInt;
    private GameObject spectatingCanvas, RespawnBtn, RespawnCheckGO;
    public static int amountOfNotesCollected;
    [SerializeField]
    private TextMeshProUGUI notesTxt, rocksTxt;
    public TextMeshProUGUI healthTxt;
    private UpdateNoteCount updateNotes;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("AI"))
        {
            ChangeFaceHurtServerRpc();
            playerHealth -= 1;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        updateNotes = FindObjectOfType<UpdateNoteCount>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        controls = new NewControls();
        _rb = GetComponent<Rigidbody>();
        controls.PlayerMap.Movement.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.PlayerMap.Movement.canceled += ctx => move = Vector2.zero;
        controls.PlayerMap.Look.performed += ctx => look = ctx.ReadValue<Vector2>();
        controls.PlayerMap.Look.canceled += ctx => look = Vector2.zero;
    }
    private void Start()
    {
        spectatingCanvas = EnabledRespawnBtn.sCanvas;
        RespawnBtn = EnabledRespawnBtn.btn.gameObject;
        playerHealth = 100;
        healthTxt.text = "Health: " + playerHealth;
        listOfPlayers.Add(gameObject);
        faceLayerMask = 1 << 3;
        woodenPlankChildGOandStumpLayerMask = 1 << 8;
        amountOfNotesCollected = 0;
        playerHealth = 100;
        if(IsHost)
        {
            ambienceAudioSource = GameObject.Find("AmbienceSounds").GetComponent<AudioSource>();
            windAudioSource = GameObject.Find("SoundHandler").GetComponent<AudioSource>();
            rainAudioSource = GameObject.Find("RainAudio").GetComponent<AudioSource>();
            waterAudioSource = GameObject.Find("DistanceToWaterCheck").GetComponent<AudioSource>();
            ambienceAudioSource.Play();
            windAudioSource.Play();
            rainAudioSource.Play();
        }
        RespawnCheckGO = GameObject.Find("SoundHandler");
        face.GetComponent<Renderer>().material.SetTexture("_MainTex", facesTex[(int)FaceType.ORIGINAL]);
        amountOfRocks = 0;
        nManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
        woodenPlankSpawner = GameObject.Find("Spawner");
        woodenLayerMask = 1 << 6;
        print(gameObject.transform.GetChild(1).name);
        if (!IsOwner)
        {
            gameObject.GetComponentInChildren<Camera>().enabled = false;
            gameObject.transform.GetChild(1).gameObject.SetActive(false);//playerCanvas
        }
    }


    [ServerRpc(RequireOwnership = false)]
    void ClientidServerrpc()
    {
        ClientID = NetworkManager.Singleton.LocalClientId;
        print(ClientID);
    }

    private void OnEnable()
    {
        controls.PlayerMap.Enable();
    }
    private void OnDisable()
    {
        controls.PlayerMap.Disable();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsLocalPlayer) return;

        ApplyMovement();
        //Jump
        if (isJumping)
        {
            Jump();
        }

        //Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, look.x, 0) * Time.fixedDeltaTime * SensX);
        //_rb.MoveRotation(_rb.rotation * deltaRotation);
        //print(look.x);
        _rb.AddRelativeTorque(Vector3.up * LookingX * smoothTime, ForceMode.Acceleration);

    }
    private void Update()
    {
        /*if (PlayerSpawn.amountOfPlayersConnected == 4 && !hasStarted)
        {
            UpdateColourServerRpc();
            hasStarted = true;
        }*/


        if (!IsLocalPlayer) return;

        if (AIenemy == null)
        {
            FindAIServerRpc();
        }

        healthTxt.text = "Health: " + playerHealth;

        if(IsHost)
        {
            if(!WaterCheck.awayFromWater)
            {
                if(!waterAudioSource.isPlaying)
                {
                    waterAudioSource.Play();
                }
            }
            else
            {
                waterAudioSource.Stop();
            }
        }

        isJumping = Keyboard.current.spaceKey.isPressed;

        isAttacking = Input.GetMouseButtonDown(0);
        if (isAttacking)
        {
            Attack();
        }
        ApplyCameraRotation();
        if (IsOwner)
        {
            movementDirection = new(move.x, 0, move.y);
            movementDirection = gameObject.GetComponentInChildren<Camera>().transform.TransformDirection(movementDirection);
            movementDirection.y = 0;
        }

        /*if (gameObject.gameObject.transform.childCount > 1)
        {
            print("Has Wooden Plank");
            var woodnPlank = transform.Find("PlankOfWood");
            woodnPlank.SetPositionAndRotation(gameObject.GetComponentInChildren<Camera>().transform.position + offsetForWeapon, gameObject.GetComponentInChildren<Camera>().transform.rotation);
        }*/

        Debug.DrawRay(gameObject.GetComponentInChildren<Camera>().transform.position, gameObject.GetComponentInChildren<Camera>().transform.forward * maxDist, Color.red);
        Debug.DrawRay(gameObject.GetComponentInChildren<Camera>().transform.position, gameObject.GetComponentInChildren<Camera>().transform.forward * interactingDist, Color.blue);
        //if (Physics.SphereCast(transform.position, radius, transform.forward, out RaycastHit hit, maxDist)) print(hit.collider.name);
    }

    [ServerRpc(RequireOwnership = false)]
    void FindAIServerRpc()
    {
        AIenemy = GameObject.FindGameObjectWithTag("AI");
        FindAIClientRpc();
    }
    [ClientRpc]
    void FindAIClientRpc()
    {
        AIenemy = GameObject.FindGameObjectWithTag("AI");
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateColourServerRpc()
    {
        var randNum = Random.Range(0, 4);
        ChangeColourClientRpc(randNum);
    }
    [ClientRpc]
    void ChangeColourClientRpc(int randNum)
    {
        switch (randNum)
        {
            case 0:
                gameObject.GetComponent<MeshRenderer>().material = matColours[(int)Colours.BLACK];
                break;
            case 1:
                gameObject.GetComponent<MeshRenderer>().material = matColours[(int)Colours.BLUE];
                break;
            case 2:
                gameObject.GetComponent<MeshRenderer>().material = matColours[(int)Colours.YELLOW];
                break;
            case 3:
                gameObject.GetComponent<MeshRenderer>().material = matColours[(int)Colours.GREEN];
                break;
            case 4:
                gameObject.GetComponent<MeshRenderer>().material = matColours[(int)Colours.MAGENTA];
                break;
        }
    }

    void ApplyCameraRotation()
    {
        LookingX = look.x * SensX;
        LookingY = look.y * SensY;
        RotateY += LookingX;
        RotateX -= LookingY;

        //_rb.MoveRotation(Quaternion.LookRotation(Vector3.up * LookingX));
        //_rb.MoveRotation(Vector3.up, LookingX);
        //transform.Rotate(Vector3.up, LookingX);
        RotateX = Mathf.Clamp(RotateX, -xClamp, xClamp);
        Vector3 YRotation = transform.eulerAngles;
        YRotation.x = RotateX;
        gameObject.GetComponentInChildren<Camera>().transform.eulerAngles = YRotation;
    }
    void Jump()
    {
        print("Jump");
        if (Mathf.Abs(_rb.velocity.y) < 0.001f)
        {
            _rb.AddForce(new Vector2(0, jumpForce), ForceMode.Impulse);
        }
    }
    public void OnInteract()
    {
        if (IsLocalPlayer)
        {
            print("Interact");
            if (Physics.Raycast(GetComponentInChildren<Camera>().transform.position, GetComponentInChildren<Camera>().transform.forward, out RaycastHit hit, interactingDist, ~woodenPlankChildGOandStumpLayerMask))
            {
                var hitObj = hit.collider.gameObject;
                print(hitObj.name);
                var hitObjParent = hit.collider.gameObject.transform.parent;
                if (!PlayerWoodenPlank.activeInHierarchy)
                {
                    if (hitObj.CompareTag("WoodenPlank"))
                    {
                        var netObj = hitObjParent.GetComponent<NetworkObject>().NetworkObjectId;
                        WoodenPlankServerRpc(netObj);
                        PlayerWoodenPlank.SetActive(true);
                        playerPickUpWoodenPlank = true;
                        UpdatePlanksFoundInSceneServerRpc();
                        canAttack = true;
                        //hitObjParent.GetComponent<NetworkObject>().Despawn();
                    }
                }
                if (hitObj.CompareTag("Rock"))
                {
                    amountOfRocks++;
                    rocksTxt.text = "Rocks: " + amountOfRocks;
                    var netObj = hitObjParent.GetComponent<NetworkObject>().NetworkObjectId;
                    RockServerRpc(netObj);
                }
                if (hitObj.CompareTag("Note"))
                {
                    amountOfNotesCollected++;
                    var netObj = hitObj.GetComponent<NetworkObject>().NetworkObjectId;
                    NoteServerRpc(netObj, amountOfNotesCollected);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void NoteServerRpc(ulong netObj, int amount)
    {
        amountOfNotesCollected = amount;
        nManager.SpawnManager.SpawnedObjects.TryGetValue(netObj, out var note);
        note.Despawn();
        IncreaseAmountOfNotesCollectedClientRpc(amountOfNotesCollected);
    }

    [ClientRpc]
    void IncreaseAmountOfNotesCollectedClientRpc(int amount)
    {
        print("Notes Increment on all clients");
        amountOfNotesCollected = amount;
        var noteTxt = GameObject.FindGameObjectWithTag("NoteCountTxt");
        noteTxt.GetComponent<TextMeshProUGUI>().text = "Notes: " + amountOfNotesCollected;
        RespawnCheckGO.GetComponent<AllowRespawn>().Check();
    }



    [ServerRpc(RequireOwnership = false)]
    void UpdatePlanksFoundInSceneServerRpc()
    {
        playerPickUpWoodenPlank = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void WoodenPlankServerRpc(ulong netObj)
    {
        DisplayWoodenPlankClientRpc();
        PlayerWoodenPlank.SetActive(true);
        nManager.SpawnManager.SpawnedObjects.TryGetValue(netObj, out var woodPlank);
        woodenPlankSpawner.GetComponent<SpawnWoodenPlanks>().WoodenPlanksInScene.Remove(woodPlank.gameObject);
        woodPlank.Despawn();
    }

    [ClientRpc]
    void DisplayWoodenPlankClientRpc()
    {
        print("Player Picked Up Client Wooden Plank " + Time.time);
        PlayerWoodenPlank.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    void RockServerRpc(ulong netObj)
    {
        nManager.SpawnManager.SpawnedObjects.TryGetValue(netObj, out var rockObj);
        rockObj.Despawn();
    }

    public void OnDrop()
    {
        //if (IsLocalPlayer)
        //{
        if (PlayerWoodenPlank.activeInHierarchy) //if the players camera has a child (Weapon is child of camera) Camera is always child of which is why it has to be more than 1 and not 0
        {
            canAttack = false;
            PlayerWoodenPlank.SetActive(false);
            RespawnWoodenPlankServerRPC();
        }
        //}
    }

    public void OnThrow()
    {
        if (amountOfRocks > 0)
        {
            amountOfRocks--;
            rocksTxt.text = "Rocks: " + amountOfRocks;
            ThrowRockServerRpc(Hand.transform.position, Hand.transform.rotation);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ThrowRockServerRpc(Vector3 pos, Quaternion rot)
    {
        GameObject ThrowingRock = Instantiate(rocks, pos, rot);
        ThrowingRock.name = "RockThrown";
        ThrowingRock.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    void RespawnWoodenPlankServerRPC()
    {
        print("Dropped Wooden Plank " + Time.time);
        DontDisplayWoodenPlankClientRpc();
        PlayerWoodenPlank.SetActive(false);
        woodenPlankSpawner.GetComponent<SpawnWoodenPlanks>().RespawnWoodenPlankAfterDropped(new Vector3(transform.position.x, 1f, transform.position.z), transform.forward);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyForceOnAttackServerRpc(Vector3 direction, float force, int health)
    {
        playerHealth = health;
        ApplyForceOnAttackClientRpc(direction, force, health);
        if(playerHealth <= 0)
        {
            print("Destroy");
            AIenemy.GetComponent<AIHandler>().UpdateTargetInfoServerRpc();
            //OnPlayerDeath();
            //gameObject.SetActive(false);
        }
    }


    public void OnPlayerDeath()
    {
        print("OnDeath");
        PlayersAlive = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        PlayersAlive.Remove(gameObject);
        if (PlayersAlive.Count > 0)//if there is another player alive in the scene
        {
            randInt = UnityEngine.Random.Range(0, PlayersAlive.Count);
            GetComponentInChildren<Camera>().enabled = false;
            //PlayersAlive[randInt].GetComponentInChildren<Camera>().enabled = true;
            spectatingCanvas.SetActive(true);
        }
        else
        {
            print("Everyone is dead");
            RespawnBtn.SetActive(true);
        }
        //Pick A random player
        //Spectate that player (See through their camera but have no effect on them
    }


    [ClientRpc]
    void ApplyForceOnAttackClientRpc(Vector3 dir, float force, int health)
    {
        playerHealth = health;
        AIenemy.GetComponent<AIHandler>().ChangeFaceAttackServerRpc();
        _rb.AddForce(dir * force, ForceMode.Impulse);
        if (playerHealth <= 0)
        {
            if (IsLocalPlayer)
            {
                OnPlayerDeath();
            }
            gameObject.transform.position = new Vector3(-1000, -1000, -1000);
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.GetComponent<CheckPlayerDeath>().OnDeathClientRpc();
            RespawnCheckGO.GetComponent<AllowRespawn>().Check();
            //gameObject.SetActive(false);
            //gameObject.SetActive(false);
            //gameObject.SetActive(false);
            print("Die");
        }
    }

    [ClientRpc]
    void DontDisplayWoodenPlankClientRpc()
    {
        PlayerWoodenPlank.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void AllowPickupServerRpc(bool go)
    {
        go = canPlayerPickUp;
        AllowPickupClientRpc(canPlayerPickUp);
    }
    [ClientRpc]
    void AllowPickupClientRpc(bool go)
    {
        go = canPlayerPickUp;
    }

    void Attack()
    {
        if (canAttack)
        {
            print("Attack");
            anim.SetTrigger("Attack");
            if (Physics.Raycast(face.transform.position, gameObject.GetComponentInChildren<Camera>().transform.forward, out RaycastHit hit, maxDist, ~faceLayerMask, QueryTriggerInteraction.Ignore))
            {
                print(hit.collider.gameObject.name);
                if (hit.collider.gameObject.CompareTag("AI"))
                {
                    //getDamageDealt();
                    //print("Damage");
                    AIHandler.health -= 10;
                    damageDealt += 10;
                    AIenemy.GetComponent<AIHandler>().ChangeFaceHurtServerRpc();
                    ChangeFaceAttackServerRpc();
                    SetDamageAndHealthOfAIServerRpc(damageDealt, AIHandler.health);
                    print(damageDealt);
                }
            }
            StartCoroutine(nameof(DelayNextAttack), 1f);
        }
    }

    IEnumerator DelayNextAttack(float sec)
    {
        canAttack = false;
        yield return new WaitForSeconds(sec);
        canAttack = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeFaceAttackServerRpc()
    {
        StartCoroutine(nameof(ChangeFaceAttack), 2f);
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeFaceHurtServerRpc()
    {
        StartCoroutine(nameof(HurtFace), 2f);
    }

    IEnumerator ChangeFaceAttack(float sec)
    {
        face.GetComponent<Renderer>().material.SetTexture("_MainTex", facesTex[(int)FaceType.ANGRY]);
        yield return new WaitForSeconds(sec);
        face.GetComponent<Renderer>().material.SetTexture("_MainTex", facesTex[(int)FaceType.ORIGINAL]);
    }
    IEnumerator HurtFace(float sec)
    {
        face.GetComponent<Renderer>().material.SetTexture("_MainTex", facesTex[(int)FaceType.HURT]);
        yield return new WaitForSeconds(sec);
        face.GetComponent<Renderer>().material.SetTexture("_MainTex", facesTex[(int)FaceType.ORIGINAL]);
    }
    void ApplyMovement()
    {
        //controller.Move(speed * Time.fixedDeltaTime * movementDirection.normalized);
        //_rb.velocity = speed * Time.fixedDeltaTime * movementDirection.normalized;
        _rb.AddForce(speed * movementDirection.normalized, ForceMode.VelocityChange);


    }
    [ServerRpc(RequireOwnership = false)] //Updates damage dealt and AI health for host
    public void SetDamageAndHealthOfAIServerRpc(int damage, int health)
    {
        print("SetDamageAndHealthServerRpc");
        damageDealt = damage;
        AIHandler.health = health;
        SetDamageAndHealthOfAIClientRpc(damage, health);
    }

    [ClientRpc] //Updates damage dealt and AI health for clients
    public void SetDamageAndHealthOfAIClientRpc(int damage, int health)
    {
        print("SetDamageAndHealthClientRPC");
        damageDealt = damage;
        AIHandler.health = health;
    }
    /*void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // Vector3 fwd = transform.TransformDirection(Vector3.forward);
        Gizmos.DrawWireSphere(gameObject.GetComponentInChildren<Camera>().transform.position * interactingDist, interactingRadius);
    }*/
}
