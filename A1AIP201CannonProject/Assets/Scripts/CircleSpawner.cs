using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSpawner : MonoBehaviour
{
    /*
     * This class is for testing purposes only.
     * - Allows the user to spawn circle objects by clicking the mouse.
     * - Each spawned circle gets a random initial velocity along the x-axis.
     * - Uses the `CustomPhysicsBody` component to handle movement.
     */

    public GameObject circlePrefab; // Reference to the circle object to be spawned.

    /*
     * Update() is called once per frame.
     * - Detects mouse clicks and spawns a circle at the click position.
     * - Applies a random initial velocity to the spawned circle.
     */
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            /*
             * Convert mouse position to world coordinates
             * - 'ScreenToWorldPoint' converts the 2D screen position into a 3D world position.
             * - Only the x and y coordinates are used since this is a 2D game.
             */
            Vector3 mouseCoords = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 spawnCoords = new Vector3(mouseCoords.x, mouseCoords.y, 0f);

            // Instantiate a new circle at the calculated spawn position
            GameObject circle = GameObject.Instantiate(circlePrefab);
            circle.transform.position = spawnCoords;

            // Apply an initial velocity to the circle (randomized in the x direction)
            CustomPhysicsBody pb = circle.GetComponent<CustomPhysicsBody>();
            pb.SetInitialVelocity(new Vector2(Random.Range(-10f, 10f), 0));
        }
    }
}
