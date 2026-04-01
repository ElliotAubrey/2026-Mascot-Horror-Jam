using UnityEngine;
using UnityEngine.AI;

public class WolfAI : MonoBehaviour
{
    public WolfStates state;

    [SerializeField] NavMeshAgent agent;
    [SerializeField] float roamSpeed;
    [SerializeField] float chaseSpeed;
    [SerializeField] float roamDistance = 100f;
    [SerializeField] float roamSensitivity = 0.5f;
    [SerializeField] Transform playerTransform;

    Vector3 roamDestination;
    bool roamset = true;

    private void Awake()
    {
        roamDestination = GetRandomRoamPoint();
    }

    private void Update()
    {
        switch (state)
        {
            case WolfStates.Roaming:
                agent.speed = roamSpeed;
                agent.destination = roamDestination;
                if(Vector3.Distance(transform.position, roamDestination) < roamSensitivity)
                {
                    roamDestination = GetRandomRoamPoint();
                }
                break;
            case WolfStates.Chasing:
                agent.speed = chaseSpeed;
                agent.destination = playerTransform.position;
                break;
            case WolfStates.Attacking:
                break;
            case WolfStates.PathToCandle:
                break;
            case WolfStates.ExtinguishCandle:
                break;
        }
    }

    Vector3 GetRandomRoamPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamDistance;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, roamDistance, 1);
        Debug.Log(hit.position);
        return hit.position + Vector3.up;
    }
}

public enum WolfStates { Roaming, Chasing, Attacking, PathToCandle, ExtinguishCandle}
