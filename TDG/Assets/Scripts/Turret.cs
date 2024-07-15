using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;

    [Header("Atributes")]
    [SerializeField] private float range = 2f;
    [SerializeField] private float rotationSpeed = 200f;

    private Transform target;

    private void Update()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();

        if (CheckTargetInRange())
        {
            target = null;
        }
    }

    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, range, (Vector2)
        transform.position, 0f, enemyMask);

        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    private bool CheckTargetInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= range;
    }

    private void RotateTowardsTarget()
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y,
        target.position.x - transform.position.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation,
        rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, transform.forward, range);
    }
}
