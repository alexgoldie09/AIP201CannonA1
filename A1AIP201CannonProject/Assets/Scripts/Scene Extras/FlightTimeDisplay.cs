using System.Collections;
using TMPro;
using UnityEngine;

/*
* This class is used for displaying the calculated flight time of a projectile.
* - It is used only in Scene02 for demonstrating the time for the user.
* - Inherits from monobehaviour to use Unity's Update().
*/

public class FlightTimeDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI flightTimeText; // UI Text element.
    [SerializeField] private Transform spawnPoint; // The spawn point of your projectile.
    [SerializeField] private float speed = 10f; // The speed at which the projectile moves in kinematic mode.
    [SerializeField] private float displayDuration = 10f; // How long to display the flight time (in seconds).
    private Coroutine displayCoroutine; // Coroutine to be used for display for a certain time.

    /*
    * Update() is called once per frame.
    * - On click input, convert mouse position to target vector.
    * - Calculates distance and computes flight time as distance / speed.
    * - Updates text UI.
    * - Hide text after duration.
    *   + It is cancelled, if there are concurrent coroutines being run.
    */
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 targetPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPoint.z = 0f;

            float distance = Vector3.Distance(spawnPoint.position, targetPoint);
            float flightTime = distance / speed;

            // Format text to two decimal places.
            flightTimeText.text = "Flight Time: " + flightTime.ToString("F2") + "s";

            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }
            displayCoroutine = StartCoroutine(HideTextAfter(displayDuration));
        }
    }

    /*
     * HideTextAfter() uses a delay to reset the text to empty.
     * - Uses a Coroutine so that it can be initiated separate from the Update() cycle.
     */
    private IEnumerator HideTextAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        flightTimeText.text = "";
    }
}
