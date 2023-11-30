using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainSpawnerScript : MonoBehaviour
{
    [SerializeField] private GameObject Raindrop;
    [SerializeField] private Vector2 spawnRadius = Vector2.one * 10;
    [SerializeField] public int raindropSpawnAmount;


    private void Start() {
        spawnRaindrop();
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.white;
        Vector3 radius = new Vector3(spawnRadius.x*2, 0.1f, spawnRadius.y*2);
        Gizmos.DrawWireCube(transform.position, radius);
    }


    Vector3 getRandomPosition() {
        float xVal = Random.Range(-spawnRadius.x, spawnRadius.x);
        float zVal = Random.Range(-spawnRadius.y, spawnRadius.y);

        Vector3 position = transform.position;
        position.x += xVal;
        position.z += zVal;
        return position;
    }

    private void spawnRaindrop() {
        for (int i = 0; i < raindropSpawnAmount; i++)
        {
            GameObject raindropGameObject = Instantiate(Raindrop, getRandomPosition(), Quaternion.identity);
        }
    }
}
