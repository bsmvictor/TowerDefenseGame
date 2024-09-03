using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager main; // Instância singleton do GameManager

    [Header("Game Objects")]
    [SerializeField] private GameObject grid; // Grade onde as torres podem ser colocadas
    [SerializeField] private GameObject pauseMenu; // Menu de pausa a ser ativado/desativado

    [Header("Buttons and Colors")]
    [SerializeField] private Button sellTowerButton;
    [SerializeField] private TMP_Text sellTowerButtonText; // Texto para exibir o valor de venda
    [SerializeField] private Color sellTowerButtonInactiveColor; // Cor quando nenhuma torre está selecionada
    [SerializeField] private Color sellTowerButtonActiveColor;  // Cor quando uma torre está selecionada
    [Space(10)]

    [SerializeField] private Button upgradeRangeButton; // Botão de upgrade de alcance
    [SerializeField] private TMP_Text upgradeRangeButtonText; // Texto para exibir o custo do upgrade de alcance
    [SerializeField] private Color upgradeRangeButtonInactiveColor; // Cor quando nenhuma torre está selecionada
    [SerializeField] private Color upgradeRangeButtonActiveColor;  // Cor quando uma torre está selecionada
    [Space(10)]

    [SerializeField] private Button upgradeSpeedButton; // Botão de upgrade de velocidade
    [SerializeField] private TMP_Text upgradeSpeedButtonText; // Texto para exibir o custo do upgrade de velocidade
    [SerializeField] private Color upgradeSpeedButtonInactiveColor; // Cor quando nenhuma torre está selecionada
    [SerializeField] private Color upgradeSpeedButtonActiveColor;  // Cor quando uma torre está selecionada

    [Header("Cursor and Towers")]
    [SerializeField] private CustomCursor customCursor; // Cursor customizado usado para mostrar onde a torre será colocada
    [SerializeField] private Turret TowerToPlace; // Referência à torre que está prestes a ser colocada

    [Header("Tiles and Path")]
    private Tile selectedTile; // Adicione isso fora de qualquer método
    public Tile[] tiles; // Array de tiles onde as torres podem ser colocadas
    public Transform startPoint; // Ponto inicial para os inimigos
    public Transform[] path; // Caminho que os inimigos seguem

    // Lista para armazenar as torres colocadas no mapa
    private List<Turret> placedTowers = new List<Turret>();

    private bool isPaused = false; // Estado de pausa do jogo

    private void Awake()
    {
        // Define o singleton
        main = this;

    }

    private void Start()
    {
        // Inicializa o estado do jogo com moedas e vida
        GameState.Instance.ResetCoins(300);
        GameState.Instance.ResetHealth(10);
        GameState.Instance.GameSpeed = 1f; // Define a velocidade do jogo para 1x

        sellTowerButton.onClick.AddListener(() => SellSelectedTower());
        upgradeRangeButton.onClick.AddListener(() => UpgradeSelectedTowerRange());
        upgradeSpeedButton.onClick.AddListener(() => UpgradeSelectedTowerSpeed());
    }


    private void Update()
    {
        HandlePauseInput();

        if (isPaused) return; // Se o jogo está pausado, não executa o restante do Update

        HandleTowerPlacement();
        HandleTowerSelling();
        HandleTowerCancellation(); // Verifica se o jogador quer cancelar a compra da torre
        UpdateButtonColors(); // Atualiza a cor dos botões
        UpdateButtonValues(); // Atualiza os textos dos botões
    }

    // Atualiza os textos dos botões com os valores da torre selecionada
    // Atualiza os textos dos botões com os valores da torre selecionada
    private void UpdateButtonValues()
    {
        if (selectedTile != null && selectedTile.isOcupied)
        {
            Turret selectedTurret = selectedTile.tower;
            sellTowerButtonText.text = $"Sell: ${selectedTurret.sellValue}";

            if (selectedTurret.rangeUpgradeLevel < 3)
            {
                upgradeRangeButtonText.text = $"Upgrade Range: ${selectedTurret.CurrentRangeUpgradeCost}";
                upgradeRangeButton.interactable = true; // Habilita o botão se ainda for possível fazer upgrade
            }
            else
            {
                upgradeRangeButtonText.text = "Max Range Upgrade";
                upgradeRangeButton.interactable = false; // Desabilita o botão se o limite de upgrade foi atingido
            }

            if (selectedTurret.speedUpgradeLevel < 3)
            {
                upgradeSpeedButtonText.text = $"Upgrade Speed: ${selectedTurret.CurrentSpeedUpgradeCost}";
                upgradeSpeedButton.interactable = true; // Habilita o botão se ainda for possível fazer upgrade
            }
            else
            {
                upgradeSpeedButtonText.text = "Max Speed Upgrade";
                upgradeSpeedButton.interactable = false; // Desabilita o botão se o limite de upgrade foi atingido
            }
        }
        else
        {
            sellTowerButtonText.text = "Sell";
            upgradeRangeButtonText.text = "Upgrade Range";
            upgradeSpeedButtonText.text = "Upgrade Speed";
            upgradeRangeButton.interactable = false; // Desabilita o botão quando nenhuma torre está selecionada
            upgradeSpeedButton.interactable = false; // Desabilita o botão quando nenhuma torre está selecionada
        }
    }


    // Lida com a entrada do usuário para pausar/despausar o jogo.
    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // Alterna o estado de pausa do jogo e mostra/esconde o menu de pausa.
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        // Mostra ou esconde o menu de pausa dependendo do estado de pausa
        pauseMenu.SetActive(isPaused);
    }

    // Lida com a colocação de uma torre no tile mais próximo ao clique do jogador.
    private void HandleTowerPlacement()
    {
        if (Input.GetMouseButtonDown(0) && TowerToPlace != null)
        {
            Tile nearestTile = FindNearestTile();

            if (nearestTile != null && !nearestTile.isOcupied)
            {
                PlaceTower(nearestTile);
            }
        }
        else if (TowerToPlace != null)
        {
            TowerToPlace.SetGizmoVisibility(true);
        }
    }


    private void HandleTowerCancellation()
    {
        if (Input.GetMouseButtonDown(1) && TowerToPlace != null)
        {
            CancelTowerPlacement();
        }
    }

    private void HandleTowerSelling()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                // Desativa o gizmo da torre previamente selecionada
                if (selectedTile != null && selectedTile.tower != null)
                {
                    selectedTile.tower.SetGizmoVisibility(false);
                }

                // Atualiza a torre selecionada
                selectedTile = hit.collider.GetComponent<Tile>();

                // Ativa o gizmo da nova torre selecionada
                if (selectedTile != null && selectedTile.tower != null)
                {
                    selectedTile.tower.SetGizmoVisibility(true);
                }
            }
            else
            {
                // Desativa o gizmo se nenhuma torre for selecionada
                if (selectedTile != null && selectedTile.tower != null)
                {
                    selectedTile.tower.SetGizmoVisibility(false);
                }
                selectedTile = null; // Limpa a torre selecionada
            }
        }
    }


    private void CancelTowerPlacement()
    {
        TowerToPlace = null; // Reseta a torre a ser colocada
        customCursor.gameObject.SetActive(false); // Esconde o cursor customizado
        Cursor.visible = true; // Torna o cursor padrão visível novamente
        Debug.Log("Tower placement canceled.");
    }


    // Encontra o tile mais próximo da posição do mouse.
    private Tile FindNearestTile()
    {
        Tile nearestTile = null;
        float shortestDistance = float.MaxValue;

        foreach (Tile tile in tiles)
        {
            float distance = Vector2.Distance(tile.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTile = tile;
            }
        }

        return nearestTile;
    }

    // Coloca uma torre no tile especificado.
    private void PlaceTower(Tile tile)
    {
        if (GameState.Instance.SpendCoins(TowerToPlace.cost)) // Verifica e deduz as moedas
        {
            Turret newTower = Instantiate(TowerToPlace, tile.transform.position, Quaternion.identity);
            tile.tower = newTower; // Vincula a torre ao Tile
            tile.isOcupied = true; // Marca o tile como ocupado
            placedTowers.Add(newTower); // Adiciona a nova torre à lista de torres colocadas

            // Desativa o gizmo da torre após colocá-la
            newTower.SetGizmoVisibility(false);

            TowerToPlace = null; // Reseta a torre a ser colocada
            customCursor.gameObject.SetActive(false); // Esconde o cursor customizado
            Cursor.visible = true; // Torna o cursor padrão visível novamente
        }
        else
        {
            Debug.Log("Not enough coins to place the tower!");
            // Reseta o cursor se não houver moedas suficientes
            customCursor.gameObject.SetActive(false);
            Cursor.visible = true;
            TowerToPlace = null;
        }
    }


    private void SellSelectedTower()
    {
        if (selectedTile != null && selectedTile.isOcupied)
        {
            SellTower(selectedTile, selectedTile.tower);
            selectedTile = null; // Reseta o tile selecionado após a venda
        }
        else
        {
            Debug.Log("No tower selected to sell.");
        }
    }

    // Vende a torre especificada, removendo-a do tile e adicionando moedas ao jogador.
    public void SellTower(Tile tile, Turret tower)
    {
        if (tower != null && tile != null)
        {
            GameState.Instance.AddCoins(tower.sellValue); // Adiciona as moedas da venda
            tile.isOcupied = false;
            tile.tower = null; // Remove a referência da torre do Tile
            placedTowers.Remove(tower); // Remove a torre da lista de torres colocadas
            Destroy(tower.gameObject); // Destrói a torre
        }
    }

    // Realiza a compra de uma torre, verificando se o jogador tem moedas suficientes.
    public void BuyTower(Turret tower)
    {
        customCursor.gameObject.SetActive(true);
        customCursor.GetComponent<SpriteRenderer>().sprite = tower.GetComponent<SpriteRenderer>().sprite;
        Cursor.visible = false;
        TowerToPlace = tower;
        grid.SetActive(true); // Mostra a grade de colocação
    }



    private void UpgradeSelectedTowerRange()
    {
        if (selectedTile != null && selectedTile.isOcupied)
        {
            selectedTile.tower.UpgradeRange(); // Chame um método na classe Turret para aumentar o alcance
            Debug.Log("Tower range upgraded.");
        }
        else
        {
            Debug.Log("No tower selected to upgrade.");
        }
    }

    private void UpgradeSelectedTowerSpeed()
    {
        if (selectedTile != null && selectedTile.isOcupied)
        {
            selectedTile.tower.UpgradeSpeed(); // Chame um método na classe Turret para aumentar a velocidade
            Debug.Log("Tower speed upgraded.");
        }
        else
        {
            Debug.Log("No tower selected to upgrade.");
        }
    }

    private void UpdateButtonColors()
    {
        if (selectedTile != null && selectedTile.isOcupied)
        {
            sellTowerButton.image.color = sellTowerButtonActiveColor;
        }
        else
        {
            sellTowerButton.image.color = sellTowerButtonInactiveColor;
        }

        if (selectedTile != null && selectedTile.isOcupied)
        {
            upgradeRangeButton.image.color = upgradeRangeButtonActiveColor;
        }
        else
        {
            upgradeRangeButton.image.color = upgradeRangeButtonInactiveColor;
        }

        if (selectedTile != null && selectedTile.isOcupied)
        {
            upgradeSpeedButton.image.color = upgradeSpeedButtonActiveColor;
        }
        else
        {
            upgradeSpeedButton.image.color = upgradeSpeedButtonInactiveColor;
        }
    }

}
