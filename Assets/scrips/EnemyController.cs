using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Patrol, Alert, Chase, Search, Return, Attack }

    [Header("State (read-only, for debugging)")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    public float patrolSpeed = 3.5f;
    public float waitTimeAtPoint = 2f;
    private bool isWaitingAtPoint = false;


    [Header("Chase")]
    public float chaseSpeed = 6f;
    public float rotationSpeed = 8f;
    public float attackRange = 2f;
    public float attackDamage = 15f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime = -999f;
    private PlayerHealth playerHealth;

    [Header("Vision Detection")]
    public Transform player;
    public float viewRadius = 30f;
    public float crouchViewRadius = 12f;
    [Range(0, 360)] public float viewAngle = 110f;
    public LayerMask playerMask;
    public LayerMask obstacleMask;
    public Transform eyes; // empty child object placed at guard's head height

    [Header("Alert / Search Timers")]
    public float alertDuration = 1.5f;   // time before Alert confirms into Chase
    public float searchDuration = 5f;    // time spent searching last known position

    [Header("Facing")]
    // public float rotationSpeed = 8f;
    private NavMeshAgent agent;
    private Vector3 lastKnownPlayerPos;
    private float stateTimer;
    private PlayerCrouch playerCrouch;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        GoToNextPatrolPoint();
        playerCrouch = player.GetComponent<PlayerCrouch>();
        playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        bool canSeePlayer = CanSeePlayer();

        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrol(canSeePlayer);
                break;
            case EnemyState.Alert:
                HandleAlert(canSeePlayer);
                break;
            case EnemyState.Chase:
                HandleChase(canSeePlayer);
                break;
            case EnemyState.Search:
                HandleSearch(canSeePlayer);
                break;
            case EnemyState.Return:
                HandleReturn(canSeePlayer);
                break;
            case EnemyState.Attack:
                HandleAttack(canSeePlayer);
                break;
        }

        if (currentState == EnemyState.Alert || currentState == EnemyState.Chase || currentState == EnemyState.Attack)
        {
            FacePlayer();
        }
    }

    // ---------------- STATE HANDLERS ----------------

    void HandlePatrol(bool canSeePlayer)
    {
        if (canSeePlayer)
        {
            ChangeState(EnemyState.Alert);
            return;
        }

        if (!isWaitingAtPoint && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitThenNextPoint());
        }
    }

    void HandleAlert(bool canSeePlayer)
    {
        agent.isStopped = true; // guard pauses, "huh?" moment

        if (canSeePlayer)
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= alertDuration)
            {
                ChangeState(EnemyState.Chase);
            }
        }
        else
        {
            // lost sight before confirming - false alarm
            ChangeState(EnemyState.Patrol);
        }
    }

    void HandleChase(bool canSeePlayer)
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;

        if (canSeePlayer)
        {
            lastKnownPlayerPos = player.position;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                ChangeState(EnemyState.Attack);
                return;
            }

            agent.SetDestination(player.position);
        }
        else
        {
            ChangeState(EnemyState.Search);
        }
    }

    void HandleAttack(bool canSeePlayer)
    {
        agent.isStopped = true;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    void HandleSearch(bool canSeePlayer)
    {
        if (canSeePlayer)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= searchDuration)
            {
                ChangeState(EnemyState.Return);
            }
        }
    }

    void HandleReturn(bool canSeePlayer)
    {
        if (canSeePlayer)
        {
            ChangeState(EnemyState.Alert);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            agent.speed = patrolSpeed;
            ChangeState(EnemyState.Patrol);
        }
    }

    // ---------------- STATE TRANSITION ----------------

    void ChangeState(EnemyState newState)
    {
        currentState = newState;
        stateTimer = 0f;

        switch (newState)
        {
            case EnemyState.Alert:
                agent.isStopped = true;
                agent.updateRotation = false;
                break;
            case EnemyState.Chase:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                agent.updateRotation = false;
                // FIX: stop walking BEFORE the attack distance, not exactly at it.
                // Before this was 2f, the same as attackRange, so the guard could
                // park just outside attack range and never attack.
                agent.stoppingDistance = Mathf.Max(0.5f, attackRange - 0.5f);
                break;
            case EnemyState.Search:
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.stoppingDistance = 0.5f;
                agent.speed = patrolSpeed;
                agent.SetDestination(lastKnownPlayerPos);
                break;
            case EnemyState.Return:
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.stoppingDistance = 0.5f;
                agent.speed = patrolSpeed;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                break;
            case EnemyState.Patrol:
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.stoppingDistance = 0.5f;
                agent.speed = patrolSpeed;
                GoToNextPatrolPoint();
                break;
            case EnemyState.Attack:
                agent.isStopped = true;
                agent.updateRotation = false;
                break;
        }
    }

    // ---------------- PATROL HELPERS ----------------

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    IEnumerator WaitThenNextPoint()
    {
        isWaitingAtPoint = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(waitTimeAtPoint);
        agent.isStopped = false;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        GoToNextPatrolPoint();
        isWaitingAtPoint = false;
    }

    // ---------------- VISION DETECTION ----------------

    void OnDrawGizmos()
    {
        if (eyes == null) return;

        // Draw the view radius as a wire circle
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyes.position, viewRadius);

        // Draw the two edges of the view angle cone
        Gizmos.color = Color.red;
        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2f);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2f);
        Gizmos.DrawLine(eyes.position, eyes.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(eyes.position, eyes.position + rightBoundary * viewRadius);

        // Draw the guard's actual forward direction (the "center" of his vision)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(eyes.position, eyes.position + eyes.forward * viewRadius);
    }

    Vector3 DirFromAngle(float angleInDegrees)
    {
        angleInDegrees += eyes.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    bool CanSeePlayer()
    {
        float currentRadius = viewRadius;
        if (playerCrouch != null && playerCrouch.IsCrouching)
        {
            currentRadius = crouchViewRadius;
        }
        Collider[] hits = Physics.OverlapSphere(eyes.position, viewRadius, playerMask);
        if (hits.Length == 0) return false;

        Vector3 playerTarget = player.position + Vector3.up * 1.4f; // aim at chest/head height, not feet
        Vector3 dirToPlayer = (playerTarget - eyes.position).normalized;

        float angle = Vector3.Angle(eyes.forward, dirToPlayer);

        if (angle > viewAngle / 2f) return false;

        float distance = Vector3.Distance(eyes.position, playerTarget);
        if (Physics.Raycast(eyes.position, dirToPlayer, out RaycastHit hit, distance, obstacleMask | playerMask))
        {
            if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
            {
                return true; // clear line of sight, nothing blocking
            }
        }
    
        return false;
    }

    void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}