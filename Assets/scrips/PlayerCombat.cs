using UnityEngine;
using StarterAssets;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat State")]
    public bool IsInCombat = false;

    [Header("Sword")]
    [SerializeField] private GameObject swordObject;

    [Header("Attack Damage Window")]
    public bool canDamage = false;
    public Transform attackPoint;
    public float attackRadius = 1f;
    public float attackDamage = 20f;
    public LayerMask enemyLayers;
    private HashSet<EnemyHealth> alreadyHitThisSwing = new HashSet<EnemyHealth>();

    [Header("Combat Movement")]
    public float combatMoveSpeed = 3f; // single speed used for both walk and "sprint" while in combat

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;

    private ThirdPersonController thirdPersonController;
    private PlayerCrouch playerCrouch;

    private float cachedMoveSpeed;
    private float cachedSprintSpeed;

    void Start()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        playerCrouch = GetComponent<PlayerCrouch>();

        if (swordObject != null)
        {
            swordObject.SetActive(IsInCombat);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleCombat();
        }

        if (IsInCombat)
        {
            if (Input.GetMouseButtonDown(0))
            {
                animator.SetTrigger("Attack");
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                animator.SetTrigger("Dodge");
            }
        }

        if (canDamage)
        {
            CheckForHit();
        }

        void CheckForHit()
        {
            Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, enemyLayers);

            foreach (Collider hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();

        if (enemyHealth != null && !alreadyHitThisSwing.Contains(enemyHealth))
        {
            enemyHealth.TakeDamage(attackDamage, attackPoint.position);
            alreadyHitThisSwing.Add(enemyHealth);
        }
    }
}

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        bool shouldUseRootMotion = state.IsName("Attack");
        animator.applyRootMotion = shouldUseRootMotion;
    }

    void ToggleCombat()
    {
        IsInCombat = !IsInCombat;

        if (swordObject != null)
        {
            swordObject.SetActive(IsInCombat);
        }

        animator.SetBool("IsInCombat", IsInCombat);

        if (IsInCombat)
        {
            // Entering combat: force standing up if crouched
            if (playerCrouch != null)
            {
                playerCrouch.ForceStand();
            }

            // Remember whatever speed values were active, then lock both to one combat speed
            cachedMoveSpeed = thirdPersonController.MoveSpeed;
            cachedSprintSpeed = thirdPersonController.SprintSpeed;

            thirdPersonController.MoveSpeed = combatMoveSpeed;
            thirdPersonController.SprintSpeed = combatMoveSpeed;
        }
        else
        {
            // Leaving combat: restore whatever speeds were active before combat started
            thirdPersonController.MoveSpeed = cachedMoveSpeed;
            thirdPersonController.SprintSpeed = cachedSprintSpeed;
        }

        Debug.Log("IsInCombat: " + IsInCombat);
    }

    void OnAnimatorMove()
    {
        
        if (!animator.applyRootMotion) return;

        Vector3 rootMotion = animator.deltaPosition;
        characterController.Move(rootMotion);
    }

    public void EnableDamage()
    {
        canDamage = true;
        alreadyHitThisSwing.Clear();
        if (SfxManager.Instance != null) SfxManager.Instance.PlaySwing(transform.position);
    }

    public void DisableDamage()
    {
        canDamage = false;
    }
}