using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject fallingObjectPrefab;       // Prefab for the falling object
    public float spawnRate = 1.0f;               // Rate at which objects spawn
    public Vector2 spawnAreaSize = new Vector2(10, 10); // Width and depth of the area
    public GameObject warningIndicatorPrefab;    // Prefab for the warning indicator
    public float warningDuration = 2.0f;         // Time the warning is displayed before object drops

    public Canvas uiCanvas;                      // Canvas for the UI indicators
    public RectTransform indicatorPrefab;        // UI indicator prefab (arrow + exclamation mark)
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(SpawnObjects());
    }

    private IEnumerator SpawnObjects()
    {
        while (true)
        {
            StartCoroutine(SpawnWithWarning());
            yield return new WaitForSeconds(spawnRate);
        }
    }

    private IEnumerator SpawnWithWarning()
    {
        // Calculate random spawn position
        Vector3 spawnPosition = new Vector3(
            transform.position.x + Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            transform.position.y,
            transform.position.z + Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2)
        );

        // Show warning indicator in the world
        if (warningIndicatorPrefab != null)
        {
            GameObject warning = Instantiate(warningIndicatorPrefab, spawnPosition, Quaternion.identity);
            Destroy(warning, warningDuration); // Destroy warning after its duration
        }

        // Create a UI indicator pointing to the warning
        RectTransform uiIndicator = Instantiate(indicatorPrefab, uiCanvas.transform);
        StartCoroutine(UpdateIndicator(uiIndicator, spawnPosition));

        // Wait for warning duration
        yield return new WaitForSeconds(warningDuration);

        // Spawn the falling object
        GameObject fallingObject = Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity);
        Rigidbody rb = fallingObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true; // Ensure it falls due to gravity
        }

        // Remove UI indicator
        Destroy(uiIndicator.gameObject);
    }

    private IEnumerator UpdateIndicator(RectTransform indicator, Vector3 targetPosition)
    {
        while (true)
        {
            // Convert world position to screen position
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);

            // Check if the target is behind the camera
            if (screenPosition.z < 0)
            {
                screenPosition *= -1; // Flip to keep indicator on screen
            }

            // Clamp the position to the screen edges
            screenPosition.x = Mathf.Clamp(screenPosition.x, 0, Screen.width);
            screenPosition.y = Mathf.Clamp(screenPosition.y, 0, Screen.height);

            // Update the indicator's position
            indicator.position = screenPosition;

            // Rotate the arrow to point toward the target
            Vector3 direction = (targetPosition - mainCamera.transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            indicator.rotation = Quaternion.Euler(0, 0, angle);

            yield return null; // Wait for the next frame
        }
    }
}
