using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Frozen_Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask enemyMask;

    [Header("Atributes")]
    [SerializeField] private float range = 1.5f;
    [SerializeField] private float aps = 0.25f;
    [SerializeField] private float freezeTime = 1f;


    private float timeUntilFire;

    private void Update()
    {
        timeUntilFire += Time.deltaTime;

        if (timeUntilFire >= 1f / aps)
        {
            Debug.Log(timeUntilFire);
            Freeze();
            timeUntilFire = 0f;
        }
    }

    private void Freeze()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, range, (Vector2)
        transform.position, 0f, enemyMask);

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];

                EnemyMovement em = hit.transform.GetComponent<EnemyMovement>();
                em.UpdateSpeed(0.5f);

                StartCoroutine(RestEnemySpeed(em));
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
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, transform.forward, range);
    }

}

