using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GlobalRangeTurret : Turret
{
    [SerializeField] private float fireDelay = 2f; // Tempo de espera entre os tiros
    [SerializeField] private float fireTimer = 0f; // Temporizador para controlar o tempo entre os tiros

    protected override void Update()
    {
        base.Update();

        if (target != null)
        {
            // Verifica se o temporizador atingiu o tempo de espera
            if (fireTimer >= fireDelay)
            {
                Shoot(); // Dispara o projétil
                fireTimer = 0f; // Reinicia o temporizador
            }
            else
            {
                fireTimer += Time.deltaTime; // Incrementa o temporizador com o tempo decorrido desde o último frame
            }
        }
    }

    protected override void FindTarget()
    {
        // Encontra todos os inimigos na cena
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length > 0)
        {
            // Encontra o inimigo mais próximo da torre (opcional, se quiser atirar no inimigo mais próximo)
            float shortestDistance = Mathf.Infinity;
            GameObject nearestEnemy = null;

            foreach (GameObject enemy in enemies)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy;
                }
            }

            if (nearestEnemy != null)
            {
                target = nearestEnemy.transform;
            }
        }
    }

    protected override bool CheckTargetInRange()
    {
        // Como o range é global, o alvo está sempre no alcance
        return target != null;
    }
}
