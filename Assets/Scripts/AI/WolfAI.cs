using UnityEngine;
using UnityEngine.AI;

public class WolfAI : MonoBehaviour
{
    public WolfStates state;

    [SerializeField] NavMeshAgent agent;
    [SerializeField] float roamSpeed;
    [SerializeField] float chaseSpeed;
    [SerializeField] float chaseThreshold = 10f;
    [SerializeField] float roamDistance = 100f;
    [SerializeField] float roamSensitivity = 1f;
    [SerializeField] Transform playerTransform;
    [SerializeField] Camera wolfVisionCamera;

    Vector3 roamDestination;
    MeshRenderer playerVisionRenderer;
    float timeInVision = 0f;

    private void Awake()
    {
        roamDestination = GetRandomRoamPoint();
        playerVisionRenderer = playerTransform.GetChild(0).GetComponent<MeshRenderer>();
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
                if(Vector3.Distance(transform.position,playerTransform.position) <= chaseThreshold)
                {
                    wolfVisionCamera.enabled = true;
                    if(PlayerInFrustrum())
                    {
                        state = WolfStates.Chasing;
                    }
                    else
                    {

                    }
                    
                }
                else 
                { 
                    wolfVisionCamera.enabled = false;
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
        return hit.position + Vector3.up;
    }

    bool PlayerInFrustrum()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(wolfVisionCamera);
        if(playerVisionRenderer == null)
        {
            playerVisionRenderer = playerTransform.GetComponent<MeshRenderer>();
        }
        return GeometryUtility.TestPlanesAABB(planes, playerVisionRenderer.bounds);
    }
}

public enum WolfStates { Roaming, Chasing, GoToLastSeen, Attacking, PathToCandle, ExtinguishCandle}
