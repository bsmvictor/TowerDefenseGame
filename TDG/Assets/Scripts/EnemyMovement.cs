using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Atributes")]
    [SerializeField] private float MoveSpeed = 2f;

    private Transform target;
    private int pathIndex = 0;

    private float baseSpeed;

    private void Start()
    {
        baseSpeed = MoveSpeed;
        target = GameManager.main.path[pathIndex];
    }

    private void Update()
    {
        if (Vector2.Distance(target.position, transform.position) <= 0.1f)
        {
            pathIndex++;

            if (pathIndex >= GameManager.main.path.Length)
            {
                GameState.Instance.DecreaseHealth(1);
                EnemySpawner.onEnemyDestroy.Invoke();
                Destroy(gameObject);
                return;
            }
            else
            {
                target = GameManager.main.path[pathIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * MoveSpeed;
    }

    public void UpdateSpeed(float newSpeed)
    {
        MoveSpeed = newSpeed;
    }

    public void ResetSpeed()
    {
        MoveSpeed = baseSpeed;
    }
}
