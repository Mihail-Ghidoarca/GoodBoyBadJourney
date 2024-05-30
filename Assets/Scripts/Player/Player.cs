using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7.5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpTime = 0.5f;

    [Header("Turn Check")]
    [HideInInspector] private GameObject leftLeg;
    [HideInInspector] private GameObject rightLeg;
    [HideInInspector] public bool isFacingRight;

    [Header("Ground Check")]
    [SerializeField] private float extraHeight = 0.25f;
    [SerializeField] private LayerMask whatIsGround;

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 12f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    [SerializeField] private TrailRenderer trailRenderer;

    private Rigidbody2D rb;
    private Collider2D coll;
    private Animator animator;
    private float moveInput;

    private bool isJumping;
    private bool isFalling;
    private float jumpTimeCounter;

    private RaycastHit2D groundHit;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
       
        StartDirectionCheck();
    }

    private void Update()
    {
        if (isDashing)
        {
            return;
        }
        Move();
        Jump();
        if(UserInput.instance.controls.Dashing.Dash.WasPressedThisFrame() && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    #region Movement Functions

    private void Move()
    {
        moveInput = UserInput.instance.moveInput.x;

        if(moveInput > 0 || moveInput < 0) 
        {
            animator.SetBool("IsRunning", true);
            TurnCheck();
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }
        rb.velocity = new Vector2 (moveInput * moveSpeed, rb.velocity.y);
    }

    private void Jump()
    {
        if (UserInput.instance.controls.Jumping.Jump.WasPressedThisFrame() && IsGrounded())
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if (UserInput.instance.controls.Jumping.Jump.IsPressed())
        {
            if (jumpTimeCounter > 0 && isJumping)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }

            else
            {
                isJumping = false;
            }
        }

        if(UserInput.instance.controls.Jumping.Jump.WasReleasedThisFrame())
        {
            isJumping = false;
        }
    }



    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        if(isFacingRight)
            rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        else
            rb.velocity = new Vector2(-transform.localScale.x * dashingPower, 0f);
        trailRenderer.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        trailRenderer.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    #endregion

    #region Ground Check

    private bool IsGrounded()
    {
        groundHit = Physics2D.BoxCast(transform.position, coll.bounds.size, 0f, Vector2.down, extraHeight, whatIsGround);
        if (groundHit.collider != null)
        {
            return true;
        }

        else
        {
            return false;
        }

    }
    #endregion

    #region Turn Checks
    private void StartDirectionCheck()
    {
        if (rightLeg.transform.position.x < leftLeg.transform.position.x)
        {
            isFacingRight = false;
        }
        else
        {
            isFacingRight = true;
        }
    }

    private void TurnCheck()
    {
        if(UserInput.instance.moveInput.x > 0 && !isFacingRight)
        {
            Turn();
        }

        else if (UserInput.instance.moveInput.x < 0 && isFacingRight)
        {
            Turn();
        }
    }

    private void Turn()
    {
        if (isFacingRight)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;
        }
        else
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;
        }
    }

    #endregion
}
