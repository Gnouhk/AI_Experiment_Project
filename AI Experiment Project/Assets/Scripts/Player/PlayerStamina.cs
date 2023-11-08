using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    public ThirdPersonMovement player;

    [SerializeField]
    public Slider staminaWheel;

    [SerializeField]
    public Slider usageWheel;

    [SerializeField]
    public CanvasGroup sliderCanvasGroup = null;
    public bool isDrainingStamina = false;
    public bool isRegeneratingStamina = false;

    private void Start()
    {
        player = GetComponent<ThirdPersonMovement>();
    }

    private void Update()
    {
        StaminaRegen();
        UpdateUI();
    }
    
    public void StaminaRegen()
    {
        if (player.IsGrounded())
        {
            player.playerCurrentStamina = Mathf.Clamp(player.playerCurrentStamina + player.playerStaminaRegenRate * Time.deltaTime, 0f, player.playerMaxStamina);
            isRegeneratingStamina = true;
        }
    }

    public void UseStamina(float amount)
    {
        player.playerCurrentStamina = Mathf.Clamp(player.playerCurrentStamina - amount, 0f, player.playerMaxStamina);
        isDrainingStamina = true;
    }

    public void UpdateUI()
    {
        if (player.playerCurrentStamina < player.playerMaxStamina)
        {
            sliderCanvasGroup.alpha += Time.deltaTime;
            if (isDrainingStamina)
            {
                usageWheel.value = player.playerCurrentStamina / player.playerMaxStamina + 0.05f;
            }
            else if (isRegeneratingStamina)
            {
                usageWheel.value = player.playerCurrentStamina / player.playerMaxStamina;
            }

            staminaWheel.value = player.playerCurrentStamina / player.playerMaxStamina;
        } 
        else
        {
            sliderCanvasGroup.alpha -= Time.deltaTime;
        }
    }
}
