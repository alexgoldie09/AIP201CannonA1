using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSpawner : MonoBehaviour
{
    /*
     * This script is for testing purposes only 
     */
    public GameObject circlePrefab;

    // Update is called once per frame
    void Update()
    {
        /*
         * Spawn projectile at the mouse click position and set velocity to a random range on the x-axis
         */
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseCoords = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 spawnCoords = new Vector3(mouseCoords.x, mouseCoords.y, 0f);

            GameObject circle = GameObject.Instantiate(circlePrefab);
            circle.transform.position = spawnCoords;

            CustomPhysicsBody pb = circle.GetComponent<CustomPhysicsBody>();
            pb.SetInitialVelocity(new Vector2(Random.Range(-10f, 10f), 0));
        }
    }
}
