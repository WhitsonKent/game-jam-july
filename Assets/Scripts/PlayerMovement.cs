using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    const string idleUp = "IdleAnimUp";
    const string idleDown = "IdleAnimDown";
    const string idleRight = "IdleAnimRight";
    const string idleLeft = "IdleAnimLeft";

    const string walkUp = "WalkUp";
    const string walkDown = "WalkDown";
    const string walkRight = "WalkSideRight";
    const string walkLeft = "WalkSideLeft";
    public GameObject laserStartPoint;

    public float laserLength = 5f;

    public float moveSpeed;
    public Rigidbody2D rb2d;
    private Vector2 moveInput;
    public LayerMask materialLayer;
    public float raycastDistance = 5f;
    public float raycastLineWidthstart = 0.2f;
    public float raycastLineWidthend = 0.1f;

    public int damageAmount = 1;
    public float attackInterval = 0.5f;
   // public Color rayColor = Color.red;
    public Animator animator;

    private bool isAttacking = false;
    private float attackTimer = 0f;
    public LineRenderer lineRenderer;
    private Vector2 lastMoveDirection;
    private Vector3 lastPosition;
    private int coins = 0;
    private int direction = 0;
    private Dictionary<string, int> materials = new Dictionary<string, int>();

    void Start()
    {
        // Find or create a LineRenderer component
       

        //lineRenderer.startWidth = raycastLineWidthstart;
        //lineRenderer.endWidth = raycastLineWidthend;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
       // lineRenderer.material.color = rayColor;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        rb2d.velocity = moveInput * moveSpeed;

        // Update animator parameters

        if (moveInput != Vector2.zero)
        {
            lastPosition = transform.position;

        }
       
        handleAnimation();
        // Calculate the angle based on moveInput
        float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;

        // Set the corresponding animator parameter based on the angle
        if (angle >= -22.5f && angle < 22.5f)
        {
            // Right
            direction = 0;
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            // Up Right
            direction = 1;
        }
        else if (angle >= 67.5f && angle < 112.5f)
        {
            // Up
            direction = 2;
        }
        else if (angle >= 112.5f && angle < 157.5f)
        {
            // Up Left
            direction = 3;
        }
        else if (angle >= 157.5f || angle < -157.5f)
        {
            // Left
            direction = 4;
        }
        else if (angle >= -157.5f && angle < -112.5f)
        {
            // Down Left
            direction = 5;
        }
        else if (angle >= -112.5f && angle < -67.5f)
        {
            // Down
            direction = 6;
        }
        else if (angle >= -67.5f && angle < -22.5f)
        {
            // Down Right
            direction = 7;
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }

        // Aim towards the mouse position
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 directionToMouse = (mousePosition - transform.position).normalized;
        Vector2 aimDirection = directionToMouse;

        // Attack control
        if (Input.GetMouseButton(0))
        {
            isAttacking = true;
            lineRenderer.enabled = true;

            Vector3 laserStart = laserStartPoint != null ? laserStartPoint.transform.position : transform.position;
            Vector3 laserEnd = laserStart + (new Vector3(aimDirection.x, aimDirection.y, 0) * laserLength);

            lineRenderer.SetPosition(0, laserStart);
            lineRenderer.SetPosition(1, laserEnd);
        }
        else
        {
            isAttacking = false;
            lineRenderer.enabled = false;
        }


        // Attacking and damaging the material
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDirection, raycastDistance, materialLayer);
                if (hit.collider != null)
                {
                    MaterialController materialController = hit.collider.GetComponent<MaterialController>();
                    if (materialController != null)
                    {
                        materialController.TakeDamage(damageAmount);
                    }
                }
            }
        }
    }
    void handleAnimation()
    {
        // Only play walk animation if character is moving
        if (moveInput.magnitude > 0)
        {
            switch (direction)
            {
                case 0:
                    animator.Play(walkRight);
                    lastMoveDirection = Vector2.right;
                    break;
                case 1:
                case 2:
                case 3:
                    animator.Play(walkUp);
                    lastMoveDirection = Vector2.up;
                    break;
                case 4:
                    animator.Play(walkLeft);
                    lastMoveDirection = Vector2.left;
                    break;
                case 5:
                case 6:
                case 7:
                    animator.Play(walkDown);
                    lastMoveDirection = Vector2.down;
                    break;
            }
        }
        else
        {
            // Play idle animation based on last move direction
            if (lastMoveDirection == Vector2.right)
            {
                animator.Play(idleRight);
            }
            else if (lastMoveDirection == Vector2.up)
            {
                animator.Play(idleUp);
            }
            else if (lastMoveDirection == Vector2.left)
            {
                animator.Play(idleLeft);
            }
            else if (lastMoveDirection == Vector2.down)
            {
                animator.Play(idleDown);
            }
        }
    }
    public void AddCoins(int amount)
    {
        coins += amount;
    }

    public void AddMaterial(string type, int amount)
    {
        if (!materials.ContainsKey(type))
        {
            materials[type] = 0;
        }
        materials[type] += amount;
    }

    public bool TryUpgrade(string property, int cost, float upgradeAmount)
    {
        // Check if player has enough coins
        if (coins >= cost)
        {
            // Subtract cost
            coins -= cost;

            // Upgrade the desired property
            switch (property)
            {
                case "laserLength":
                    laserLength += upgradeAmount;
                    break;
                case "damageAmount":
                    damageAmount += (int)upgradeAmount;  // assuming that damageAmount should remain an integer
                    break;
                case "moveSpeed":
                    moveSpeed += upgradeAmount;
                    break;
                default:
                    Debug.LogWarning("Unknown property: " + property);
                    return false;
            }

            return true;
        }
        else
        {
            Debug.Log("Not enough coins for upgrade");
            Debug.Log("Upgrade failed");

            return false;
        }
    }
    public void upgradeDamage()
    {
        if (TryUpgrade("damageAmount", 100, 2))
        {
            Debug.Log("Upgrade succeeded");
            Debug.Log("Damage Amount = " + damageAmount);
        }
    }

    public void upgradeLaserLength()
    {
        if (TryUpgrade("laserLength", 100, 1))
        {
            Debug.Log("Upgrade succeeded");
            Debug.Log("Laser Length = " + laserLength);
        }
    }

    public void upgradeMoveSpeed()
    {
        if (TryUpgrade("moveSpeed", 100, 1))
        {
            Debug.Log("Upgrade succeeded");
            Debug.Log("moveSpeed = " + moveSpeed);
        }
    }
}
