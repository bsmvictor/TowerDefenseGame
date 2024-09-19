using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [FormerlySerializedAs("MoveSpeed")]
    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int enemyID;  // ID do inimigo

    private Transform target;
    private int pathIndex = 0;

    private float baseSpeed;

    private void Start()
    {
        baseSpeed = moveSpeed;
        target = GameManager.main.path[pathIndex]; // Começa a seguir o caminho a partir do índice atual
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
        rb.velocity = direction * moveSpeed;
    }

    public void UpdateSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetSpeed()
    {
        moveSpeed = baseSpeed;
    }

    private void OnDestroy()
    {
        // Certifica que o evento seja chamado apenas uma vez quando o inimigo for destruído
        if (EnemySpawner.onEnemyDestroy != null)
        {
            EnemySpawner.onEnemyDestroy.Invoke();
        }
        
        // Verifica o ID do inimigo quando ele é destruído
        if (enemyID == 1)
        {
            // Instancia um inimigo do tipo 0 e define o caminho a partir do ponto atual
            GameObject newEnemy = Instantiate(EnemySpawner.Instance.enemyPrefabs[0], transform.position, Quaternion.identity);
            newEnemy.GetComponent<EnemyMovement>().SetPathIndex(pathIndex); // Passa o índice do caminho atual para o novo inimigo
        }
    }

    // Método para definir o ID do inimigo
    public void SetEnemyID(int id)
    {
        enemyID = id;
    }

    // Método para definir o índice do caminho
    public void SetPathIndex(int index)
    {
        pathIndex = index;
        target = GameManager.main.path[pathIndex]; // Atualiza o alvo do novo inimigo
    }
}
