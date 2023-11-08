using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] public float speed = 6f;

    [SerializeField] public float turnSmoothTime = 0.1f;

    [SerializeField] public float jumpForce = 7f;

    [SerializeField] public float fallMultiplier = 2.5f;

    [SerializeField] Vector3 currentForceVelocity;

    [Header("Sprint")]
    [SerializeField] public float sprintSpeed = 8f;

    [Header("Dash")]
    [SerializeField] private bool canDash = true;

    [SerializeField] private bool isDashing;

    [SerializeField] private float dashingPower = 24f;

    [SerializeField] private float dashingTime = 0.2f;

    [SerializeField] private float dashingCooldown = 1f;

    [Header("Stamina Parameter")]
    [SerializeField] public float playerCurrentStamina;

    [SerializeField] public float playerMaxStamina = 100f;

    [SerializeField] public float playerStaminaRegenRate = 0.01f;

    [SerializeField] public float jumpCost = 20f;

    [SerializeField] public float sprintCost = 0.01f;

    [SerializeField] public float dashCost = 30f;

    [Header("References")]
    public Transform groundCheck;

    public LayerMask groundLayer;

    public CharacterController controller;

    public Transform cam;

    public TrailRenderer tr;

    public PlayerStamina stamina;

    float turnSmoothVelocity;
    Vector3 moveDir;

    private void Awake()
    {
        playerCurrentStamina = playerMaxStamina;
    }

    private void Update()
    {
        if (isDashing)
        {
            return;
        }
        PlayerMovement();
    }

    void PlayerMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDir.normalized * speed * Time.deltaTime);

            HearingManager.Instance.OnSoundEmited(gameObject, transform.position, EHeardSoundCategory.Footstep, 0.1f);
        }

        //Sprint
        if(IsGrounded())
        {
            if(Input.GetKey(KeyCode.R) && playerCurrentStamina >= sprintCost)
            {
                controller.Move(moveDir.normalized * sprintSpeed * Time.deltaTime);
                stamina.UseStamina(sprintCost);
            }
        }

        //Jump
        if(IsGrounded())
        {
            currentForceVelocity.y = jumpForce;
        }

        if (currentForceVelocity.y > 0f && Input.GetKeyDown(KeyCode.Space) && playerCurrentStamina >= jumpCost)
        {
            currentForceVelocity.y += Physics.gravity.y * Time.deltaTime;

            stamina.UseStamina(jumpCost);
        }
        else if (!IsGrounded())
        {
            currentForceVelocity.y += Physics.gravity.y * fallMultiplier * Time.deltaTime;
        }
        else
        {
            currentForceVelocity.y = 0f;
        }

        controller.Move(currentForceVelocity * Time.deltaTime);

        //Dash
        if(Input.GetKeyDown(KeyCode.LeftShift) && canDash && IsGrounded() && playerCurrentStamina >= dashCost)
        {
            StartCoroutine(Dash(moveDir));
            stamina.UseStamina(dashCost);
        }
    }

    public bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, 0.01f, groundLayer);
    }

    IEnumerator Dash(Vector3 dir)
    {
        canDash = false;
        isDashing = true;

        float startTime = Time.time;

        while (Time.time < startTime + dashingTime)
        {
            controller.Move(dir * dashingPower * Time.deltaTime);

            tr.emitting = true;

            yield return null;
        }


        yield return new WaitForSeconds(dashingTime);

        tr.emitting = false;
        isDashing = false;

        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
