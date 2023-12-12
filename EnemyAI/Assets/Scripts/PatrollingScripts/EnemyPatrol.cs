using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    public Transform[] PatrolPoints; // Array of patrol points

    private int currentPatrolIndex = 0; // Index to keep track of the current patrol point
    private bool patrolling = false; // Flag to indicate if the enemy is patrolling

    public Transform Player;
    public LayerMask HidableLayers;
    public EnemyLineOfSightChecker LineOfSightChecker;
    public NavMeshAgent Agent;
    [Range(-1, 1)]
    [Tooltip("Lower is a better hiding spot")]
    public float HideSensitivity = 0;
    [Range(1, 10)]
    public float MinPlayerDistance = 5f;
    [Range(0, 5f)]
    public float MinObstacleHeight = 1.25f;
    [Range(0.01f, 1f)]
    public float UpdateFrequency = 0.25f;
    [Range(1f, 300f)]
    public float MaxHideTime = 10f; // Maximum hiding time before chasing
    [Range(1f, 300f)]
    public float MaxChaseTime = 10f; // Maximum chasing time before going back to cover

    private Coroutine MovementCoroutine;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        LineOfSightChecker.OnGainSight += HandleGainSight;
        LineOfSightChecker.OnLoseSight += HandleLoseSight;

        // Start patrolling initially
        StartPatrol();
    }

    private void StartPatrol()
    {
        if (PatrolPoints.Length > 0)
        {
            patrolling = true;
            Agent.SetDestination(PatrolPoints[currentPatrolIndex].position);
        }
    }

    private void Update()
    {
        // Check if the player is near the enemy
        if (Player != null && Vector3.Distance(transform.position, Player.position) < LineOfSightChecker.Collider.radius)
        {
            // Stop patrolling and start chasing the player
            StopPatrol();
            StartChase();
        }

        // Check if the enemy is close to the current patrol point
        if (patrolling && Agent.remainingDistance < 0.5f)
        {
            // Move to the next patrol point
            currentPatrolIndex = (currentPatrolIndex + 1) % PatrolPoints.Length;
            Agent.SetDestination(PatrolPoints[currentPatrolIndex].position);
        }
    }

    private void HandleGainSight(Transform Target)
    {
        if (MovementCoroutine != null)
        {
            StopCoroutine(MovementCoroutine);
        }
        Player = Target;

        // If patrolling, stop patrolling and start chasing the player
        if (patrolling)
        {
            StopPatrol();
            StartChase();
        }
    }

    private void HandleLoseSight(Transform Target)
    {
        if (MovementCoroutine != null)
        {
            StopCoroutine(MovementCoroutine);
        }
        Player = null;

        // If chasing, stop chasing and resume patrolling
        if (!patrolling)
        {
            StopChase();
            StartPatrol();
        }
    }

    private void StartChase()
    {
        // Your existing chasing logic here...

        // Sample chasing logic:
        MovementCoroutine = StartCoroutine(HideAndChase(Player));
    }

    private void StopChase()
    {
        // Stop any chasing logic if needed
        if (MovementCoroutine != null)
        {
            StopCoroutine(MovementCoroutine);
        }
    }

    private void StopPatrol()
    {
        // Stop patrolling
        patrolling = false;
        Agent.ResetPath();
    }

    private IEnumerator HideAndChase(Transform Target)
    {
        WaitForSeconds Wait = new WaitForSeconds(UpdateFrequency);

        float hideTimer = 0f;
        float chaseTimer = 0f;

        while (hideTimer < MaxHideTime)
        {
            // Hide logic
            HideFromPlayer(Target);
            hideTimer += UpdateFrequency;
            yield return Wait;
        }

        // Chase logic
        while (Player != null && chaseTimer < MaxChaseTime)
        {
            Agent.SetDestination(Player.position);
            chaseTimer += UpdateFrequency;
            yield return Wait;
        }

        // Go back to cover after chasing
        hideTimer = 0f;
        while (hideTimer < MaxHideTime)
        {
            // Hide logic
            HideFromPlayer(Target);
            hideTimer += UpdateFrequency;
            yield return Wait;
        }

        // Resume patrolling after going back to cover
        StartPatrol();
    }

    private void HideFromPlayer(Transform Target)
    {
        // Your existing hiding logic here

        // Sample hiding logic:
        Collider[] colliders = new Collider[10];
        int hits = Physics.OverlapSphereNonAlloc(Agent.transform.position, LineOfSightChecker.Collider.radius, colliders, HidableLayers);

        for (int i = 0; i < hits; i++)
        {
            if (Vector3.Distance(colliders[i].transform.position, Target.position) < MinPlayerDistance || colliders[i].bounds.size.y < MinObstacleHeight)
            {
                colliders[i] = null;
            }
        }

        System.Array.Sort(colliders, ColliderArraySortComparer);

        for (int i = 0; i < hits; i++)
        {
            if (NavMesh.SamplePosition(colliders[i].transform.position, out NavMeshHit hit, 2f, Agent.areaMask))
            {
                if (!NavMesh.FindClosestEdge(hit.position, out hit, Agent.areaMask))
                {
                    Debug.LogError($"Unable to find edge close to {hit.position}");
                }

                if (Vector3.Dot(hit.normal, (Target.position - hit.position).normalized) < HideSensitivity)
                {
                    Agent.SetDestination(hit.position);
                    return;
                }
                else
                {
                    // Since the previous spot wasn't facing "away" enough from the target, we'll try on the other side of the object
                    if (NavMesh.SamplePosition(colliders[i].transform.position - (Target.position - hit.position).normalized * 2, out NavMeshHit hit2, 2f, Agent.areaMask))
                    {
                        if (!NavMesh.FindClosestEdge(hit2.position, out hit2, Agent.areaMask))
                        {
                            Debug.LogError($"Unable to find edge close to {hit2.position} (second attempt)");
                        }

                        if (Vector3.Dot(hit2.normal, (Target.position - hit2.position).normalized) < HideSensitivity)
                        {
                            Agent.SetDestination(hit2.position);
                            return;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"Unable to find NavMesh near object {colliders[i].name} at {colliders[i].transform.position}");
            }
        }
    }

    public int ColliderArraySortComparer(Collider A, Collider B)
    {
        if (A == null && B != null)
        {
            return 1;
        }
        else if (A != null && B == null)
        {
            return -1;
        }
        else if (A == null && B == null)
        {
            return 0;
        }
        else
        {
            return Vector3.Distance(Agent.transform.position, A.transform.position).CompareTo(Vector3.Distance(Agent.transform.position, B.transform.position));
        }
    }
}
