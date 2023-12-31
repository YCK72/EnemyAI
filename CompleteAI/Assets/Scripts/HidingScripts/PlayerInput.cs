﻿using UnityEngine;
using UnityEngine.AI;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    private Camera Camera;
    [SerializeField]
    private LayerMask FloorLayers;
    [SerializeField]
    private NavMeshAgent Agent;

    private void Update()
{
    if (Input.GetKeyUp(KeyCode.Mouse1))
    {
        if (Agent != null && Agent.enabled) // Add a null check and ensure the agent is enabled
        {
            if (Physics.Raycast(Camera.ScreenPointToRay(Input.mousePosition), out RaycastHit Hit, float.MaxValue, FloorLayers))
            {
                Agent.SetDestination(Hit.point);
            }
        }
    }
}
}

