using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSpawner : MonoBehaviour
{
    // Reference to the circle prefab.
    public GameObject circlePrefab;

    // Time interval (in seconds) between spawns.
    public float spawnInterval = 1f;
    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnCircle();
            timer = 0f;
        }
    }

    void SpawnCircle()
    {
        // Generate a random position within the bounds.
        float randomX = Random.Range(-8f, 8f);
        Vector3 spawnCoords = new Vector3(randomX, 0f, 0f);

        // Instantiate the circle prefab at the random position.
        GameObject circle = Instantiate(circlePrefab, spawnCoords, Quaternion.identity);

        // Destroy the spawned object after 10 seconds.
        Destroy(circle, 10f);

        // Set the initial velocity:
        // Random x velocity between -3 and 3, fixed y velocity of 0f.
        CustomPhysicsBody pb = circle.GetComponent<CustomPhysicsBody>();
        if (pb != null)
        {
            pb.SetInitialVelocity(new Vector2(Random.Range(-3f, 3f), 0f));
        }
    }
}
