using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FrozenTurret : Turret
{
    [Header("Frozen Turret Attributes")]
    [SerializeField] private float aps = 0.25f; // Attacks per second (equivalente ao fireRate)
    [SerializeField] private float freezeTime = 1f;

    protected override void Update()
    {
        timeUntilFire += Time.deltaTime;

        if (timeUntilFire >= 1f / aps)
        {
            Freeze();
            timeUntilFire = 0f;
        }
    }

    private void Freeze()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, range, Vector2.zero, 0f, enemyMask);

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];

                EnemyMovement em = hit.transform.GetComponent<EnemyMovement>();
                if (em != null)
                {
                    em.UpdateSpeed(0.5f); // Diminui a velocidade do inimigo

                    StartCoroutine(RestEnemySpeed(em)); // Restaura a velocidade do inimigo após o tempo de congelamento
                }
            }
        }
    }

    private IEnumerator RestEnemySpeed(EnemyMovement em)
    {
        yield return new WaitForSeconds(freezeTime);
        em.ResetSpeed();
    }

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.blue; // Alterado para azul para indicar a área de efeito da Frozen Turret
        Handles.DrawWireDisc(transform.position, transform.forward, range);
    }
}
