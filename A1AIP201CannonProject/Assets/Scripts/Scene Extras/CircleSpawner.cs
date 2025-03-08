using UnityEngine;

/*
* This class is used for spawning circles for shooting at.
* - It is used only in Scene01 for demonstrating physics.
* - Inherits from monobehaviour to use Unity's Update().
*/

public class CircleSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject circlePrefab; // Circle prefab with a CustomPhysicsBody component.
    [SerializeField] private float spawnInterval = 1f; // Time interval between spawns.
    private float timer = 0f; // Timer to check against for resetting spawn.

    /*
    * Update() is called once per frame.
    * - Spawn circles at every moment the timer runs down.
    *   + Spawn interval acts as a check against timer.
    */
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnCircle();
            timer = 0f;
        }
    }

    private void SpawnCircle()
    {
        // Generate a random position on the x-axis (-8f to +8f fits the area)
        float randomX = Random.Range(-8f, 8f);
        Vector3 spawnCoords = new Vector3(randomX, 0f, 0f);

        // Instantiate the circle prefab at the random position
        GameObject circle = Instantiate(circlePrefab, spawnCoords, Quaternion.identity);

        // Destroy the spawned object after 10 seconds (Safety check)
        Destroy(circle, 10f);

        // Set the initial velocity on the x (-3f to +3f ensure the ball don't go too far)
        CustomPhysicsBody pb = circle.GetComponent<CustomPhysicsBody>();
        if (pb != null)
        {
            pb.SetInitialVelocity(new Vector2(Random.Range(-3f, 3f), 0f));
        }
    }
}
