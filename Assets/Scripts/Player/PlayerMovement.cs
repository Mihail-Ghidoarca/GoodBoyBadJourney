using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;

    public float runSpeed = 40f;
    
    float horizontalMove = 0f;
    bool jump = false;


    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Mathf.Abs(horizontalMove) > 0f)
        {
            animator.SetInteger("AnimState", 2);
        }
        else
        {
            animator.SetInteger("AnimState", 0);
        }
        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("Jump");
            jump = true;
        }
    } 
    public void OnLanding()
    {
        animator.ResetTrigger("Jump");
        jump = false;
        //animator.SetBool("Grounded", true);
    }

    void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump);
        jump = false;
        
    }
}
