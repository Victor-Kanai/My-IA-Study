using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class FollowMutipleTest : MonoBehaviour
{
    NavMeshAgent Enemy;
    public GameObject target;

    void Start()
    {
        Enemy = GetComponent<NavMeshAgent>();
        //FindTarget();
    }
    void Update()
    {
        if (target == null)
        {
            FindTarget();
        }

        Enemy.SetDestination(target.transform.position);
    }

    void FindTarget()
    {
        target = GameObject.FindWithTag("Player");
        if (target == null)
        {
            target = Enemy.gameObject;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            print("A");
            Destroy(target);
        }
    }
}
