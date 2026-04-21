using UnityEngine;

public class SimplePrefabSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private float spawnInterval = 4f;

    private float nextSpawnTime;
    private GameObject currentSpawnedObject;

    private void Update()
    {
        if (prefabToSpawn == null)
        {
            return;
        }

        if (Time.time < nextSpawnTime)
        {
            return;
        }

        if (currentSpawnedObject != null)
        {
            return;
        }

        currentSpawnedObject = Instantiate(prefabToSpawn, transform.position, transform.rotation);
        nextSpawnTime = Time.time + spawnInterval;
    }
}