using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelSpawnManager : MonoBehaviour
{
    public Transform spawn1;
    public Transform spawn2;

    public Transform getSpawnOne()
    {
        return spawn1;
    }

    public Transform getSpawnTwo()
    {
        return spawn2;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(spawn1.position, .5f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(spawn1.position, spawn1.position + spawn1.forward * .5f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(spawn2.position, .5f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(spawn2.position, spawn2.position + spawn2.forward * .5f);
    }
}
