using UnityEngine;
using UnityEngine.AI;

public class WolfAI : MonoBehaviour
{
    public WolfStates state;
    public bool ALLOW_MOVEMENT = false;

    [SerializeField] NavMeshAgent agent;
    [SerializeField] float roamSpeed;
    [SerializeField] float chaseSpeed;
    [SerializeField] float cameraThreshold = 10f;
    [SerializeField] float roamDistance = 100f;
    [SerializeField] float roamSensitivity = 1f;
    [SerializeField] float attackRange = 1f;
    [SerializeField] Transform wolfEyeTransform;
    [SerializeField] Transform playerTransform;
    [SerializeField] Camera wolfVisionCamera;

    Vector3 roamDestination;
    MeshRenderer playerVisionRenderer;
    Vector3 lastSeenPlayerPosition;

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
                if(ALLOW_MOVEMENT)
                {
                    agent.speed = roamSpeed;
                    agent.destination = roamDestination;
                }
                
                if (Vector3.Distance(transform.position, roamDestination) < roamSensitivity)
                {
                    roamDestination = GetRandomRoamPoint();
                    agent.destination = roamDestination;
                }
                if(Vector3.Distance(transform.position,playerTransform.position) <= cameraThreshold)
                {
                    wolfVisionCamera.enabled = true;
                    if(PlayerInFrustrum())
                    {
                        var rayDirection = playerTransform.position - transform.position;

                        if (Physics.Raycast(transform.position, rayDirection, out RaycastHit eyeHit, 100f))
                        {
                            if (eyeHit.collider.CompareTag("Player"))
                            {
                                state = WolfStates.Chasing;
                            }
                        }
                    }
                }
                else 
                { 
                    wolfVisionCamera.enabled = false;
                }
                break;

            case WolfStates.Chasing:
                wolfVisionCamera.enabled = false;
                if (ALLOW_MOVEMENT)
                {
                    agent.speed = chaseSpeed;
                    agent.destination = playerTransform.position;
                }
                if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
                {
                    state = WolfStates.Attacking;
                }
                break;

            case WolfStates.Attacking:
                agent.destination = transform.position;
                state = WolfStates.Chasing;
                break;

            case WolfStates.GoToLastSeen:
                agent.destination = lastSeenPlayerPosition;
                if(Vector3.Distance(transform.position, lastSeenPlayerPosition) < roamSensitivity)
                {
                    state = WolfStates.Roaming;
                }
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
