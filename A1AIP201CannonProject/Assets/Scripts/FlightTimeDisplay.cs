using System.Collections;
using TMPro;
using UnityEngine;

public class FlightTimeDisplay : MonoBehaviour
{
    // Assign your UI Text element in the Inspector.
    [SerializeField] private TextMeshProUGUI flightTimeText;
    // The spawn point of your projectile.
    [SerializeField] private Transform spawnPoint;
    // The constant speed at which the projectile moves in kinematic mode.
    [SerializeField] private float kinematicSpeed = 10f;
    // How long to display the flight time (in seconds).
    [SerializeField] private float displayDuration = 10f;
    private Coroutine displayCoroutine;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Get click position in world space (assume z=0 for 2D)
            Vector3 targetPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPoint.z = 0f;

            // Calculate the distance from the spawn point to the click position.
            float distance = Vector3.Distance(spawnPoint.position, targetPoint);
            // Compute flight time = distance / speed.
            float flightTime = distance / kinematicSpeed;

            // Update the UI text.
            flightTimeText.text = "Flight Time: " + flightTime.ToString("F2") + "s";

            // Optionally, clear the text after displayDuration seconds.
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }
            displayCoroutine = StartCoroutine(HideTextAfter(displayDuration));
        }
    }

    private IEnumerator HideTextAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        flightTimeText.text = "";
    }
}
