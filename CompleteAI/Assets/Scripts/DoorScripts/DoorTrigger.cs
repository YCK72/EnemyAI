using UnityEngine;
using UnityEngine.AI;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField]
    private Door Door;
    private int AgentsInRange = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
        {
            AgentsInRange++;

            if (AgentsInRange == 1)
            {
                OpenDoorWithDelay();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent)) 
        {
            AgentsInRange--;// Code for closing the door once agent is out of range.

            if (AgentsInRange == 0)
            {
                Door.Close();
            }
        }
    }

    public void OpenDoorWithDelay()
    {
        Door.Open(AgentsInRange > 0 ? Door.transform.position : Vector3.zero);
        AgentsInRange = 0;
    }
}
