using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FrozenTurret : Turret
{
    protected override void Update()
    {
        base.Update(); // Chama o Update da classe base Turret

        timeUntilFire += Time.deltaTime;

        if (timeUntilFire >= 1f / fireRate)
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
            foreach (RaycastHit2D hit in hits)
            {
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
        yield return new WaitForSeconds(fireRate);
        em.ResetSpeed();
    }
}
