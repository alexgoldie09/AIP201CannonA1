using UnityEngine;

public class CannonFire : MonoBehaviour
{
    /*
     * 🚀 Handles the firing action and trajectory preview.
     * - Updates **trajectory preview** in real-time.
     * - Shows **landing point** before firing.
     */

    public GameObject projectilePrefab;
    public Transform spawnPoint;
    public CannonController cannon;

    private void Update()
    {
        // 🚀 Get the projectile's initial velocity
        Vector2 velocity = GetLaunchVelocity();

        // 🚀 Fire projectile when Space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }


    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        CustomPhysicsBody physicsBody = projectile.GetComponent<CustomPhysicsBody>();

        if (physicsBody != null)
        {
            physicsBody.SetInitialVelocity(GetLaunchVelocity());
        }
        else
        {
            Debug.LogError("Projectile is missing CustomPhysicsBody component!");
        }
    }

    Vector2 GetLaunchVelocity()
    {
        float angle = cannon.GetLaunchAngle() * Mathf.Deg2Rad;
        float power = cannon.GetPower();

        float xVelocity = power * Mathf.Cos(angle);
        float yVelocity = power * Mathf.Sin(angle);

        return new Vector2(xVelocity, yVelocity);
    }
}
