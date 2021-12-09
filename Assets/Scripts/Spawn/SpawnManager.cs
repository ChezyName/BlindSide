using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] SpawnPoints;

    public Transform getRandomSpawn()
    {
        Debug.Log(Random.Range(0, SpawnPoints.Length));
        return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
    }
}
