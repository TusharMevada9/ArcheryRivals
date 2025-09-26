using Fusion;
using UnityEngine;
using System.Collections;
using static Unity.Collections.Unicode;

public class ArrowShooterMultiPlayer : NetworkBehaviour
{
    [Header("Arrow Settings")]
    public NetworkObject arrowPrefab;         // તીરનો prefab
    public Transform shootPoint;          // તીર ક્યાંથી શૂટ થશે
    public float shootForce = 10f;        // તીર પર લગાવવાનું બળ
    public bool isLeft = true;            // ડાબી બાજુથી શૂટ કરવું છે કે જમણી બાજુથી

    [Header("Shooting Controls")]
    // Space key ઉપર તીર શૂટ થાય છે

    [Header("Arrow Physics")]
    public bool useGravity = false;       // gravity લાગવી છે કે નહીં (false = સીધું જાય)
    public float arrowLifetime = 5f;
    public bool resetVelocityOnSpawn = true; // spawn પર velocity reset કરવી છે કે નહીં
    public bool useDirectVelocity = true; // velocity directly set કરવી છે કે force લગાવવું છે

    public SpriteRenderer BowClickImage;
    public Sprite Check;
    public GameObject BowNoClickImage;
    public GameObject BowNoArrowClickImage;

    [Header("Shooting Cooldown")]
    public float shootCooldown = 1f; // 1 second cooldown between shots
    private bool canShoot = true; // Flag to check if player can shoot

    [Header("Hold to Shoot")]
    public float holdTimeRequired = 0.5f; // 0.5 seconds hold required
    private float holdTimer = 0f; // Timer for holding space
    private bool isHoldingSpace = false; // Flag to track if space is being held
    private bool canReleaseToShoot = false; // Flag to check if can shoot on release

    public bool isArrowGo;

    public Animator animator;

    void Start()
    {
        if (shootPoint == null)
        {
            shootPoint = transform;
        }
    }
    public Vector3 spawnPosition;

    void Update()
    {
        if (UIManager.Instance.isGameStart == true)
        {
            if (Object.HasStateAuthority)
            {
                // Check if space OR mouse button is held
                bool isInputHeld = (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && canShoot;

                if (isInputHeld)
                {
                    if (!isHoldingSpace)
                    {
                        isHoldingSpace = true;
                        holdTimer = 0f;
                        canReleaseToShoot = false;
                        Debug.Log("[Multiplayer] Started holding input (Space/Mouse) - Hold for 0.5 seconds to shoot!");
                        RPCFalse();
                        // Play bow pull sound when hold starts (local only)
                        if (SoundManager.Instance != null)
                        {
                            SoundManager.Instance.PlayRandomBowPull();
                        }
                    }

                    holdTimer += Time.deltaTime;

                    if (holdTimer >= holdTimeRequired && !canReleaseToShoot)
                    {
                        canReleaseToShoot = true;
                        
                        Debug.Log("[Multiplayer] ✅ Hold time completed! Ready to shoot on release!");
                    }
                }
                else
                {
                    if (isHoldingSpace)
                    {
                        // Check if space OR mouse button was released
                        bool spaceReleased = Input.GetKeyUp(KeyCode.Space);
                        bool mouseReleased = Input.GetMouseButtonUp(0);

                        if (spaceReleased || mouseReleased)
                        {
                            isHoldingSpace = false;

                            if (canReleaseToShoot && canShoot)
                            {
                                Debug.Log("[Multiplayer] 🏹 Shooting arrow after successful 0.5-second hold!");

                                // Play bow release sound on successful shot (local only)
                                if (SoundManager.Instance != null)
                                {
                                    SoundManager.Instance.PlayRandomBowRelease();
                                }

                                StartCoroutine(ShootArrow());
                                StartCoroutine(ShootingCooldown());
                            }
                            else if (holdTimer < holdTimeRequired)
                            {
                                Debug.Log($"[Multiplayer] ❌ Hold time too short! ({holdTimer:F2}s / {holdTimeRequired}s) - Need to hold longer!");
                            }

                            holdTimer = 0f;
                        }
                        canReleaseToShoot = false;
                    }
                    if(isArrowGo == false)
                    {
                        RPCTrue();
                    }
                }

                spawnPosition = this.gameObject.transform.position;
            }
        }
    }

    public IEnumerator ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("Arrow Prefab is not assigned!");
            yield break;
        }

        NetworkObject newArrow = FusionConnector.instance.NetworkRunner.Spawn(arrowPrefab, spawnPosition, Quaternion.identity);
        isArrowGo = true;
        RPCNoArrowTrue();
        
        Rigidbody2D arrowRb = newArrow.GetComponent<Rigidbody2D>();
        if (arrowRb != null)
        {
            if (resetVelocityOnSpawn)
            {
                arrowRb.linearVelocity = Vector2.zero;
                arrowRb.angularVelocity = 0f;
            }

            Vector2 forceDirection;
            if (isLeft)
            {
                forceDirection = new Vector2(1f, 0f); // જમણી બાજુ તરફ
            }
            else
            {
                forceDirection = new Vector2(-1f, 0f); // ડાબી બાજુ તરફ
            }

            Debug.Log("Arrow shot " + (isLeft ? "left to right" : "right to left") + " with force: " + shootForce);

            if (useDirectVelocity)
            {
                arrowRb.linearVelocity = forceDirection * shootForce;
                Debug.Log("Direct velocity set: " + arrowRb.linearVelocity);
            }
            else
            {
                arrowRb.AddForce(forceDirection * shootForce, ForceMode2D.Impulse);
                Debug.Log("Force applied: " + (forceDirection * shootForce));
            }

            Debug.Log("Arrow spawned at: " + spawnPosition + " with force: " + shootForce);
        }
        else
        {
            Debug.LogWarning("Arrow prefab doesn't have Rigidbody2D component!");
        }

        yield return new WaitForSeconds(1f);

        isArrowGo = false;
        RPCNoArrowfalse();
        

        yield return new WaitForSeconds(2f);

        if (newArrow != null)
        {
            yield return new WaitForSeconds(2f);
            FusionConnector.instance.NetworkRunner.Despawn(newArrow);
        }

        Debug.Log("Arrow Shot! Force: " + shootForce);
    }

    // Shooting cooldown coroutine
    private IEnumerator ShootingCooldown()
    {
        canShoot = false; // Disable shooting
        Debug.Log($"[Multiplayer] Shooting cooldown started - {shootCooldown} seconds");

        yield return new WaitForSeconds(shootCooldown);

        canShoot = true; // Re-enable shooting
        Debug.Log("[Multiplayer] Shooting cooldown finished - Ready to shoot!");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCTrue()
    {
        animator.SetBool("isClick", false);
        BowNoArrowClickImage.SetActive(false);
        BowNoClickImage.SetActive(true);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCFalse()
    {
        animator.SetBool("isClick", true);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCNoArrowTrue()
    {
        BowClickImage.sprite = Check;
        BowNoClickImage.SetActive(false);
        BowNoArrowClickImage.SetActive(true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCNoArrowfalse()
    {
        animator.SetBool("isClick", false);
        BowNoArrowClickImage.SetActive(false);
        BowNoClickImage.SetActive(true);
    }

   
}
