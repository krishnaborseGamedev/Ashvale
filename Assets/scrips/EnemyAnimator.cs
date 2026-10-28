using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private EnemyController enemyController;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        enemyController = GetComponent<EnemyController>();
    }

    void Update()
    {
        // Feed movement speed into the Blend Trees
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        // Feed current FSM state into the Animator's State parameter
        animator.SetInteger("State", (int)enemyController.currentState);
    }
}