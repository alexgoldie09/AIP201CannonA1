using UnityEngine;

public class KinematicProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    // Set this to your desired constant speed (units per second)
    [SerializeField] private float kinematicSpeed = 10f;
    [SerializeField] private float gravityDelayOnTransition = 0.2f; // delay before gravity takes effect

    private CustomPhysicsBody physicsBody;

    private void Awake()
    {
        physicsBody = GetComponent<CustomPhysicsBody>();
    }

    private void Update()
    {
        if (physicsBody != null && physicsBody.IsKinematic())
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * kinematicSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                // Snap to target.
                transform.position = targetPosition;
                // Set the projectile's velocity to continue its trajectory.
                Vector3 currentVelocity = direction * kinematicSpeed;
                physicsBody.SetVelocity(currentVelocity);

                // Set a small gravity delay so that gravity doesn't kick in immediately.
                physicsBody.SetGravityDelay(gravityDelayOnTransition);
                // Turn off kinematic mode so the custom physics resumes.
                physicsBody.SetKinematic(false);
            }
        }
    }

    public void SetTargetPosition(Vector3 newTargetPosition) => targetPosition = newTargetPosition;
}
