using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 5f;
    private Turret turret; // Referência para o script da Torre

    public Transform target;

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    public void SetTurret(Turret _turret)
    {
        turret = _turret;
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject); // Destrói a bala se o alvo for perdido
            return;
        }

        // Calcula a direção para o alvo
        Vector2 direction = (target.position - transform.position).normalized;

        // Move a bala em direção ao alvo
        float distanceThisFrame = bulletSpeed * Time.deltaTime;
        if (Vector2.Distance(transform.position, target.position) <= distanceThisFrame)
        {
            HitTarget();
        }
        else
        {
            rb.velocity = direction * bulletSpeed;
        }
    }

    private void HitTarget()
    {
        // Lida com o dano ao atingir o alvo
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(turret.damage);
        }

        // Destrói a bala
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject); // Destrói a bala se ela sair da tela
    }
}
