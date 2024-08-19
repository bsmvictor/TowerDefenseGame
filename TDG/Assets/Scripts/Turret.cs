using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Transform turretRotationPoint; // Ponto de rota��o da torre
    [SerializeField] protected LayerMask enemyMask; // M�scara para detectar inimigos
    [SerializeField] protected GameObject bulletPrefab; // Prefab do proj�til que ser� disparado
    [SerializeField] protected Transform firingPoint; // Ponto de disparo do proj�til
    [SerializeField] protected LineRenderer rangeIndicator; // LineRenderer para o indicador de alcance

    [Header("Attributes")]
    [SerializeField] public float range; // Alcance da torre
    [SerializeField] public float rotationSpeed; // Velocidade de rota��o da torre
    [SerializeField] public float fireRate; // Taxa de disparo da torre
    [SerializeField] public int damage; // Dano causado por cada proj�til
    [SerializeField] public int cost; // Custo da torre
    [SerializeField] public int sellValue; // Valor de venda da torre
    [SerializeField] public int upgradeCost; // Valor de upgrade da torre
    [SerializeField] private int totalCost; // Custo total da torre, incluindo upgrades
    [SerializeField] private float rangeUpgradeMultiplier; // Multiplicador para o custo do upgrade de alcance
    [SerializeField] private float speedUpgradeMultiplier; // Multiplicador para o custo do upgrade de velocidade
    public int rangeUpgradeLevel { get; private set; } = 0; // N�vel atual do upgrade de alcance
    public int speedUpgradeLevel { get; private set; } = 0; // N�vel atual do upgrade de velocidade

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
    protected float timeUntilFire; // Tempo restante at� o pr�ximo disparo

    // Propriedades p�blicas para acessar os valores atuais de upgrade
    public int CurrentRangeUpgradeCost { get; private set; }
    public int CurrentSpeedUpgradeCost { get; private set; }


    protected virtual void Start()
    {
        totalCost = cost; // O custo inicial � o custo base da torre
        rangeUpgradeMultiplier = 0.2f;
        speedUpgradeMultiplier = 0.3f;

        // Inicializa os custos de upgrade com o custo base
        CurrentRangeUpgradeCost = upgradeCost;
        CurrentSpeedUpgradeCost = upgradeCost;

        UpdateSellValue(); // Calcula o valor de venda inicial

        // Configura o LineRenderer para desenhar o c�rculo de alcance
        if (rangeIndicator != null)
        {
            rangeIndicator.positionCount = 51; // N�mero de pontos no c�rculo
            rangeIndicator.useWorldSpace = false; // Use coordenadas locais
            DrawRangeIndicator();
        }
    }


    protected virtual void Update()
    {
        // Se n�o h� alvo, procura por um novo alvo
        if (target == null)
        {
            FindTarget();
            return;
        }

        // Verifica se o ponto de rota��o est� atribu�do antes de tentar rodar a torre
        if (turretRotationPoint != null)
        {
            // Gira a torre em dire��o ao alvo
            RotateTowardsTarget();
        }

        // Verifica se o alvo ainda est� no alcance
        if (!CheckTargetInRange())
        {
            target = null;
        }
        else
        {
            // Conta o tempo at� o pr�ximo disparo
            timeUntilFire += Time.deltaTime;

            // Dispara se o tempo de recarga terminou
            if (timeUntilFire >= 1f / fireRate)
            {
                Shoot();
                timeUntilFire = 0f;
            }
        }
    }

    // M�todo para desenhar o indicador de alcance
    private void DrawRangeIndicator()
    {
        float angle = 20f; // �ngulo entre cada ponto do c�rculo
        for (int i = 0; i <= 50; i++)
        {
            float theta = Mathf.Deg2Rad * angle * i;
            float x = Mathf.Sin(theta) * range;
            float y = Mathf.Cos(theta) * range;

            rangeIndicator.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    // M�todo para disparar proj�teis
    protected virtual void Shoot()
    {
        if (firingPoint == null) return; // Retorna e n�o executa o disparo se o firingPoint n�o estiver atribu�do

        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.SetTarget(target); // Define o alvo do proj�til
        bulletScript.SetTurret(this); // Define a torre que disparou o proj�til
    }


    // M�todo para encontrar o alvo mais pr�ximo dentro do alcance
    protected virtual void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, range, Vector2.zero, 0f, enemyMask);

        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    // Verifica se o alvo ainda est� dentro do alcance da torre
    protected virtual bool CheckTargetInRange()
    {
        return target != null && Vector2.Distance(target.position, transform.position) <= range;
    }

    // Gira a torre em dire��o ao alvo
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

    // M�todo para destruir a torre
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
            rangeUpgradeLevel++; // Incrementa o n�vel do upgrade
            UpdateSellValue(); // Atualiza o valor de venda

            // Atualiza o custo para o pr�ximo upgrade
            CurrentRangeUpgradeCost *= 2;

            if (rangeIndicator != null)
            {
                DrawRangeIndicator(); // Atualiza o indicador de alcance ap�s o upgrade
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
            speedUpgradeLevel++; // Incrementa o n�vel do upgrade
            UpdateSellValue(); // Atualiza o valor de venda

            // Atualiza o custo para o pr�ximo upgrade
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
