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

    private void Update()
    {
        // Se o alvo for destruído ou não existir, destrói a bala
        if (!target)
        {
            Destroy(gameObject);
            return;
        }

        // Move a bala diretamente em direção ao alvo, sem usar física
        Vector2 direction = (target.position - transform.position).normalized;
        float distanceThisFrame = bulletSpeed * Time.deltaTime;

        // Verifica se a bala atingiu o alvo
        if (Vector2.Distance(transform.position, target.position) <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        // Move a bala
        transform.Translate(direction * distanceThisFrame, Space.World);
    }

    private void HitTarget()
    {
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(turret.damage); // Usa o dano da torre
        }
        Destroy(gameObject); // Destrói a bala após atingir o alvo
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject); // Destrói a bala se ela sair da tela
    }
}
