using UnityEngine;

/*
* This class is used for handling manual cannon rotation and power adjustments.
* - It uses keyboard input to move the cannon.
*  + Movement is dependant on what scene it is.
* - Provides methods for getting launch angle and power.
* - Inherits from monobehaviour to use Unity's in-built Update().
*/

public class CannonController : MonoBehaviour
{
    [SerializeField] private Transform cannonPivot; // The pivot that rotates the cannon.
    [SerializeField] private float angleSpeed = 30f; // Speed at which the cannon rotates.
    [SerializeField] private float power = 10f; // Initial firing power.
    [SerializeField] private float maxPower = 35f; // Maximum allowed power.
    [SerializeField] private float minPower = 8f; // Mimimum allowed power.
    [SerializeField] private float maxAngle = 65f; // Maximum allowed power.
    [SerializeField] private float minAngle = 0f; // Mimimum allowed power.

    private float angle = 45f; // Current cannon angle, set in Inspector.

    // Flag to control if manual rotation is enabled
    [SerializeField] private bool manualRotationEnabled = true;

    /*
     * Update() is called once per frame.
     * - Manually adjusts the cannon based on keyboard input.
     *  + Only done so if the check flag for manual rotation is enabled.
     *    Otherwise, allows external control via other scripts.
     */
    private void Update()
    {
        if (manualRotationEnabled)
        {
            // Adjust cannon angle using W/S keys
            if (Input.GetKey(KeyCode.W)) { angle += angleSpeed * Time.deltaTime; }
            if (Input.GetKey(KeyCode.S)) { angle -= angleSpeed * Time.deltaTime; }
            // Set the range of the cannon's angle (0 - 65 degrees was determined to be comfortable)
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            cannonPivot.rotation = Quaternion.Euler(0, 0, angle);

            // Adjust cannon power using A/D keys
            if (Input.GetKey(KeyCode.D)) { power += Time.deltaTime * 5f; }
            if (Input.GetKey(KeyCode.A)) { power -= Time.deltaTime * 5f; }
            // Set the range of the cannon's power (8 - 35 power was determined to be comfortable)
            power = Mathf.Clamp(power, minPower, maxPower);
        }
    }

    #region **Setters & Getters**
    // GetLaunchAngle() returns the cannon's current angle which directly modifies the cannon fire.
    public float GetLaunchAngle() => angle;
    // GetPower() returns the cannon's current power which directly modifies the cannon fire.
    public float GetPower() => power;
    // GetCannonPivot() returns the cannon's pivot transform for rotating via external script.
    public Transform GetCannonPivot() => cannonPivot;
    #endregion
}

