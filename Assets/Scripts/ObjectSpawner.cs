using System.Collections;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject fallingObjectPrefab;  // Prefab for the falling object
    public float spawnRate = 1.0f;          // Rate at which objects spawn
    public Vector2 spawnAreaSize = new Vector2(10, 10); // Width and depth of the area

    private void Start()
    {
        StartCoroutine(SpawnObjects());
    }

    private IEnumerator SpawnObjects()
    {
        while (true)
        {
            SpawnFallingObject();
            yield return new WaitForSeconds(spawnRate);
        }
    }

    private void SpawnFallingObject()
    {
        Vector3 spawnPosition = new Vector3(
            transform.position.x + Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            transform.position.y,
            transform.position.z + Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2)
        );

        GameObject fallingObject = Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity);
        Rigidbody rb = fallingObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true; // Ensure it falls due to gravity
        }
    }
}
