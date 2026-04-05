using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimTest : MonoBehaviour
{
    [SerializeField] private GameObject neckBone;
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 1.0f;

    void Update()
    {
        Vector3 targetDirection = target.position - neckBone.transform.position;
        float singleStep = speed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(neckBone.transform.forward, targetDirection, singleStep, 0.0f);

        Debug.DrawRay(neckBone.transform.position, newDirection, Color.red);

        neckBone.transform.rotation = Quaternion.LookRotation(newDirection);
    }
}
