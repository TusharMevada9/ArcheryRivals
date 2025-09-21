using System.Collections;
using UnityEngine;

public class ArrowShooter : MonoBehaviour
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;         // તીરનો prefab
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

    public GameObject BowClickImage;
    public GameObject BowNoClickImage;

    [Header("Shooting Cooldown")]
    public float shootCooldown = 1f; // 1.5 seconds cooldown between shots
    private bool canShoot = true; // Flag to check if player can shoot

    [Header("Hold to Shoot")]
    public float holdTimeRequired = 1f; // 2 seconds hold required
    private float holdTimer = 0f; // Timer for holding space
    private bool isHoldingSpace = false; // Flag to track if space is being held
    private bool canReleaseToShoot = false; // Flag to check if can shoot on release

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
        // Space key hold logic
        if (Input.GetKey(KeyCode.Space) && canShoot)
        {
            if (!isHoldingSpace)
            {
                isHoldingSpace = true;
                holdTimer = 0f;
                canReleaseToShoot = false;
                Debug.Log("[ArrowShooter] Started holding Space - Hold for 2 seconds to shoot!");
            }

            holdTimer += Time.deltaTime;

            // Check if held long enough to shoot
            if (holdTimer >= holdTimeRequired && !canReleaseToShoot)
            {
                canReleaseToShoot = true;
                BowClickImage.SetActive(true);
                BowNoClickImage.SetActive(false);
                Debug.Log("[ArrowShooter] ✅ Hold time completed! Ready to shoot on release!");
            }
        }
        else
        {
            // Space not held or can't shoot
            if (isHoldingSpace)
            {
                isHoldingSpace = false;

                // Check if held long enough to shoot
                if (canReleaseToShoot && canShoot)
                {
                    Debug.Log("[ArrowShooter] 🏹 Shooting arrow after successful 2-second hold!");

                    // Shoot arrow and start cooldown
                    StartCoroutine(ShootArrow());
                    StartCoroutine(ShootingCooldown());
                }
                else if (holdTimer < holdTimeRequired)
                {
                    Debug.Log($"[ArrowShooter] ❌ Hold time too short! ({holdTimer:F2}s / {holdTimeRequired}s) - Need to hold longer!");
                }

                holdTimer = 0f;
                canReleaseToShoot = false;
            }

            // Show BowNoClickImage when not holding
            if (BowClickImage != null)
            {
                BowClickImage.SetActive(false);
            }
            if (BowNoClickImage != null)
            {
                BowNoClickImage.SetActive(true);
            }
        }

        spawnPosition = this.gameObject.transform.position;
    }

    // તીર શૂટ કરવાનું ફંક્શન
    public IEnumerator ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("Arrow Prefab is not assigned!");
            yield break;
        }

        GameObject newArrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);

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

        yield return new WaitForSeconds(3f);

        if (newArrow != null)
        {
            Destroy(newArrow, 3f);
        }

        Debug.Log("Arrow Shot! Force: " + shootForce);
    }

    // Shooting cooldown coroutine
    private IEnumerator ShootingCooldown()
    {
        canShoot = false; // Disable shooting
        Debug.Log($"[ArrowShooter] Shooting cooldown started - {shootCooldown} seconds");

        yield return new WaitForSeconds(shootCooldown);

        canShoot = true; // Re-enable shooting
        Debug.Log("[ArrowShooter] Shooting cooldown finished - Ready to shoot!");
    }

}
