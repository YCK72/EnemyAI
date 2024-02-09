using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Camera Camera = null;
    [SerializeField]
    private Vector3 CameraFollowOffset = new Vector3(0, 10, -2);
    [SerializeField]
    private LayerMask LayerMask;
    private NavMeshAgent Agent;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Debug.Log("PlayerMovement Awake");
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask))
            {
                Debug.Log("Mouse Clicked");
                
                if (Agent != null)
                {
                    Debug.Log("NavMeshAgent Exists");
                    Debug.Log("Agent enabled: " + Agent.enabled);

                    if (Agent.enabled)
                    {
                        Debug.Log("Setting destination");
                        Agent.SetDestination(hit.point);
                    }
                    else
                    {
                        Debug.LogWarning("NavMeshAgent is not enabled!");
                    }
                }
                else
                {
                    Debug.LogError("NavMeshAgent is null!");
                }
            }
        }
    }

    private void LateUpdate()
    {
        Camera.transform.position = Agent.transform.position + CameraFollowOffset;
    }
}
