using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class Door : MonoBehaviour
{
    private NavMeshObstacle Obstacle;

    public bool IsOpen = false;
    [SerializeField]
    private float Speed = 1f;
    [SerializeField]
    private float RotationAmount = 90f;
    [SerializeField]
    private float ForwardDirection = 0;

    private Vector3 StartRotation;
    private Vector3 Forward;

    private Coroutine AnimationCoroutine;

    private void Awake()
    {
        Obstacle = GetComponent<NavMeshObstacle>();
        Obstacle.carveOnlyStationary = false;
        Obstacle.carving = IsOpen;
        Obstacle.enabled = IsOpen;

        StartRotation = transform.rotation.eulerAngles;
        // Since "Forward" is actually pointing into the door frame, choose a direction to think about as "Forward"
        Forward = transform.right;
    }

    public void Open(Vector3 UserPosition)
    {
        if (!IsOpen)
        {
            if (AnimationCoroutine != null)
            {
                StopCoroutine(AnimationCoroutine);
            }

            float dot = Vector3.Dot(Forward, (UserPosition - transform.position).normalized);
            AnimationCoroutine = StartCoroutine(DoRotationOpen(dot));
        }
    }

    private IEnumerator DoRotationOpen(float ForwardAmount)
    {
        float targetRotation = ForwardAmount >= ForwardDirection ? StartRotation.y - RotationAmount : StartRotation.y + RotationAmount;

        IsOpen = true;

        yield return DoRotationOpenOrClose(targetRotation);
    }

    public void Close()
    {
        if (IsOpen)
        {
            if (AnimationCoroutine != null)
            {
                StopCoroutine(AnimationCoroutine);
            }

            AnimationCoroutine = StartCoroutine(DoRotationClose(StartRotation.y));
        }
    }

    private IEnumerator DoRotationClose(float targetRotation)
    {
        IsOpen = false;

        yield return DoRotationOpenOrClose(targetRotation);
    }

    private IEnumerator DoRotationOpenOrClose(float targetRotation)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(new Vector3(StartRotation.x, targetRotation, StartRotation.z));

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * Speed;
        }

        Obstacle.enabled = !IsOpen;
        Obstacle.carving = IsOpen;
    }
}
