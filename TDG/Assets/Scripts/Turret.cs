using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Transform turretRotationPoint; // Ponto de rotação da torre
    [SerializeField] protected LayerMask enemyMask; // Máscara para detectar inimigos
    [SerializeField] protected GameObject bulletPrefab; // Prefab do projétil que será disparado
    [SerializeField] protected Transform firingPoint; // Ponto de disparo do projétil
    [SerializeField] protected LineRenderer rangeIndicator; // LineRenderer para o indicador de alcance

    [Header("Attributes")]
    [SerializeField] public float range; // Alcance da torre
    [SerializeField] public float rotationSpeed; // Velocidade de rotação da torre
    [SerializeField] public float fireRate; // Taxa de disparo da torre
    [SerializeField] public int damage; // Dano causado por cada projétil
    [SerializeField] public int cost; // Custo da torre
    [SerializeField] public int sellValue; // Valor de venda da torre
    [SerializeField] public int upgradeCost; // Valor de upgrade da torre
    [SerializeField] private int totalCost; // Custo total da torre, incluindo upgrades
    [SerializeField] private float rangeUpgradeMultiplier; // Multiplicador para o custo do upgrade de alcance
    [SerializeField] private float speedUpgradeMultiplier; // Multiplicador para o custo do upgrade de velocidade
    public int rangeUpgradeLevel { get; private set; } = 0; // Nível atual do upgrade de alcance
    public int speedUpgradeLevel { get; private set; } = 0; // Nível atual do upgrade de velocidade

    public int RangeUpgradeLevel
    {
        get { return rangeUpgradeLevel; }
    }

    public int SpeedUpgradeLevel
    {
        get { return speedUpgradeLevel; }
    }

    private bool showGizmo = false;

    protected Transform target; // Alvo atual da torre
    protected float timeUntilFire; // Tempo restante até o próximo disparo

    // Propriedades públicas para acessar os valores atuais de upgrade
    public int CurrentRangeUpgradeCost { get; private set; }
    public int CurrentSpeedUpgradeCost { get; private set; }


    protected virtual void Start()
    {
        totalCost = cost; // O custo inicial é o custo base da torre
        rangeUpgradeMultiplier = 0.2f;
        speedUpgradeMultiplier = 0.3f;

        // Inicializa os custos de upgrade com o custo base
        CurrentRangeUpgradeCost = upgradeCost;
        CurrentSpeedUpgradeCost = upgradeCost;

        UpdateSellValue(); // Calcula o valor de venda inicial

        // Configura o LineRenderer para desenhar o círculo de alcance
        if (rangeIndicator != null)
        {
            rangeIndicator.positionCount = 51; // Número de pontos no círculo
            rangeIndicator.useWorldSpace = false; // Use coordenadas locais
            DrawRangeIndicator();
        }
    }


    protected virtual void Update()
    {
        // Se não há alvo, procura por um novo alvo
        if (target == null)
        {
            FindTarget();
            return;
        }

        // Verifica se o ponto de rotação está atribuído antes de tentar rodar a torre
        if (turretRotationPoint != null)
        {
            // Gira a torre em direção ao alvo
            RotateTowardsTarget();
        }

        // Verifica se o alvo ainda está no alcance
        if (!CheckTargetInRange())
        {
            target = null;
        }
        else
        {
            // Conta o tempo até o próximo disparo
            timeUntilFire += Time.deltaTime;

            // Dispara se o tempo de recarga terminou
            if (timeUntilFire >= 1f / fireRate)
            {
                Shoot();
                timeUntilFire = 0f;
            }
        }
    }

    // Método para desenhar o indicador de alcance
    private void DrawRangeIndicator()
    {
        float angle = 20f; // Ângulo entre cada ponto do círculo
        for (int i = 0; i <= 50; i++)
        {
            float theta = Mathf.Deg2Rad * angle * i;
            float x = Mathf.Sin(theta) * range;
            float y = Mathf.Cos(theta) * range;

            rangeIndicator.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    // Método para disparar projéteis
    protected virtual void Shoot()
    {
        if (firingPoint == null) return; // Retorna e não executa o disparo se o firingPoint não estiver atribuído

        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.SetTarget(target); // Define o alvo do projétil
        bulletScript.SetTurret(this); // Define a torre que disparou o projétil
    }


    // Método para encontrar o alvo mais próximo dentro do alcance
    protected virtual void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, range, Vector2.zero, 0f, enemyMask);

        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    // Verifica se o alvo ainda está dentro do alcance da torre
    protected virtual bool CheckTargetInRange()
    {
        return target != null && Vector2.Distance(target.position, transform.position) <= range;
    }

    // Gira a torre em direção ao alvo
    protected virtual void RotateTowardsTarget()
    {
        if (turretRotationPoint != null)
        {
            if (target == null) return;

            Vector2 direction = target.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Método para destruir a torre
    public virtual void DestroyTurret()
    {
        Destroy(gameObject);
    }

    private void UpdateSellValue()
    {
        sellValue = Mathf.RoundToInt(totalCost * 0.8f); // Define o valor de venda como 80% do custo total
    }

    public void UpgradeRange()
    {
        if (rangeUpgradeLevel >= 3)
        {
            Debug.Log("Range upgrade limit reached!");
            return;
        }

        if (GameState.Instance.SpendCoins(CurrentRangeUpgradeCost)) // Verifica e deduz as moedas
        {
            range += rangeUpgradeMultiplier; // Incremento no alcance
            totalCost += CurrentRangeUpgradeCost; // Adiciona o custo do upgrade ao custo total
            rangeUpgradeLevel++; // Incrementa o nível do upgrade
            UpdateSellValue(); // Atualiza o valor de venda

            // Atualiza o custo para o próximo upgrade
            CurrentRangeUpgradeCost *= 2;

            if (rangeIndicator != null)
            {
                DrawRangeIndicator(); // Atualiza o indicador de alcance após o upgrade
            }
        }
        else
        {
            Debug.Log("Not enough coins to upgrade range!");
        }
    }

    public void UpgradeSpeed()
    {
        if (speedUpgradeLevel >= 3)
        {
            Debug.Log("Speed upgrade limit reached!");
            return;
        }

        if (GameState.Instance.SpendCoins(CurrentSpeedUpgradeCost)) // Verifica e deduz as moedas
        {
            fireRate += speedUpgradeMultiplier; // Incremento na velocidade de disparo
            totalCost += CurrentSpeedUpgradeCost; // Adiciona o custo do upgrade ao custo total
            speedUpgradeLevel++; // Incrementa o nível do upgrade
            UpdateSellValue(); // Atualiza o valor de venda

            // Atualiza o custo para o próximo upgrade
            CurrentSpeedUpgradeCost *= 2;
        }
        else
        {
            Debug.Log("Not enough coins to upgrade speed!");
        }
    }


    public void SetGizmoVisibility(bool visibility)
    {
        showGizmo = visibility;
        if (rangeIndicator != null)
        {
            rangeIndicator.enabled = visibility; // Mostra ou esconde o indicador de alcance
        }
    }
}
