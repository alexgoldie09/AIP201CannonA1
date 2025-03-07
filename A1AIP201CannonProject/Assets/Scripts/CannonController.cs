using UnityEngine;

public class CannonController : MonoBehaviour
{
    /*
     * Handles cannon movement and power adjustments.
     * - Allows W/S keys to adjust angle.
     * - Allows A/D keys to adjust power.
     * - Debug logs display **angle and power changes**.
     */

    public Transform cannonPivot; // The rotating cannon pivot
    public float angleSpeed = 30f; // Speed of rotation
    public float power = 10f; // Initial firing power
    public float maxPower = 20f;
    public float minPower = 2f;

    private float angle = 45f; // Default angle, set pivot point to this angle

    // New flag to control manual rotation.
    public bool manualRotationEnabled = true;

    private void Update()
    {
        if (manualRotationEnabled)
        {
            // Adjust cannon angle
            if (Input.GetKey(KeyCode.W)) { angle += angleSpeed * Time.deltaTime; }
            if (Input.GetKey(KeyCode.S)) { angle -= angleSpeed * Time.deltaTime; }
            angle = Mathf.Clamp(angle, 0, 65f); // Set the range of the cannon's angle
            cannonPivot.rotation = Quaternion.Euler(0, 0, angle);

            // Adjust cannon power
            if (Input.GetKey(KeyCode.D)) { power += Time.deltaTime * 5f; }
            if (Input.GetKey(KeyCode.A)) { power -= Time.deltaTime * 5f; }
            power = Mathf.Clamp(power, minPower, maxPower);

            // Debugging: Print Angle & Power to Console
            // Debug.Log($"Cannon Angle: {angle}°, Power: {power}");
        }
    }

    // Getters for firing logic
    public float GetLaunchAngle() => angle;
    public float GetPower() => power;
}

