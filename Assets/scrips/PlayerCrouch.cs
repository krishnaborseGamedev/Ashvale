using UnityEngine;
using StarterAssets;

public class PlayerCrouch : MonoBehaviour
{
    [Header("Crouch Settings")]
    public float standingHeight = 1.8f;
    public float crouchHeight = 1.0f;
    public float standingSpeed = 2f;
    public float crouchSpeed = 1f;

    public bool IsCrouching { get; private set; } = false;

    private CharacterController controller;
    private ThirdPersonController thirdPersonController;
    private PlayerCombat playerCombat;
    private Vector3 standingCenter;
    private Vector3 crouchCenter;
    private Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        playerCombat = GetComponent<PlayerCombat>();
        animator = GetComponent<Animator>();

        standingCenter = controller.center;
        crouchCenter = new Vector3(standingCenter.x, standingCenter.y * (crouchHeight / standingHeight), standingCenter.z);
    }

    void Update()
    {
        // Ignore crouch input entirely while in combat mode
        if (playerCombat != null && playerCombat.IsInCombat)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            IsCrouching = !IsCrouching;
            ApplyCrouchState();
        }
    }

    void ApplyCrouchState()
    {
        if (IsCrouching)
        {
            controller.height = crouchHeight;
            controller.center = crouchCenter;
            thirdPersonController.MoveSpeed = crouchSpeed;
        }
        else
        {
            controller.height = standingHeight;
            controller.center = standingCenter;
            thirdPersonController.MoveSpeed = standingSpeed;
        }

        animator.SetBool("IsCrouching", IsCrouching);
    }

    // Called by PlayerCombat if the player enters combat while crouched
    public void ForceStand()
    {
        if (IsCrouching)
        {
            IsCrouching = false;
            ApplyCrouchState();
        }
    }
}