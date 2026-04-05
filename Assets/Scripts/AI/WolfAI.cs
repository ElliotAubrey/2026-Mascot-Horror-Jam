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
    [SerializeField] float attackRage = 1f;
    [SerializeField] float chaseAfterNotSeeingPlayerTime = 3f;
    [SerializeField] Transform playerTransform;
    [SerializeField] Camera wolfVisionCamera;

    Vector3 roamDestination;
    MeshRenderer playerVisionRenderer;
    float timeInVision = 0f;
    float cantSeePlayerChaseTimer = 0f;
    Vector3 lastSeenPlayerPosition;

    private void Awake()
    {
        roamDestination = GetRandomRoamPoint();
        playerVisionRenderer = playerTransform.GetChild(0).GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        Debug.Log(agent.destination);
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
                    
                }
                else 
                { 
                    wolfVisionCamera.enabled = false;
                }
                break;

            case WolfStates.Chasing:
                agent.speed = chaseSpeed;
                agent.destination = playerTransform.position;
                if (Vector3.Distance(transform.position, playerTransform.position) <= attackRage)
                {
                    state = WolfStates.Attacking;
                }
                if(Physics.Raycast(transform.position, playerTransform.position, out RaycastHit hit))
                {
                    if (hit.collider.transform.position != playerTransform.position)
                    {
                        if(Time.time - cantSeePlayerChaseTimer > chaseAfterNotSeeingPlayerTime)
                        {
                            state = WolfStates.GoToLastSeen;
                        }
                    }
                    else
                    {
                        cantSeePlayerChaseTimer = Time.time;
                        lastSeenPlayerPosition = playerTransform.position;
                    }
                }
                    break;

            case WolfStates.Attacking:
                agent.destination = transform.position;
                Debug.Log("Attacking");
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
