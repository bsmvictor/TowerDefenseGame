using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager main; // Inst�ncia singleton do GameManager

    [Header("Game Objects")]
    [SerializeField] private GameObject grid; // Grade onde as torres podem ser colocadas
    [SerializeField] private GameObject pauseMenu; // Menu de pausa a ser ativado/desativado

    [Header("Buttons and Colors")]
    [SerializeField] private Button sellTowerButton;
    [SerializeField] private TMP_Text sellTowerButtonText; // Texto para exibir o valor de venda
    [SerializeField] private Color sellTowerButtonInactiveColor; // Cor quando nenhuma torre est� selecionada
    [SerializeField] private Color sellTowerButtonActiveColor;  // Cor quando uma torre est� selecionada
    [Space(10)]

    [SerializeField] private Button upgradeRangeButton; // Bot�o de upgrade de alcance
    [SerializeField] private TMP_Text upgradeRangeButtonText; // Texto para exibir o custo do upgrade de alcance
    [SerializeField] private Color upgradeRangeButtonInactiveColor; // Cor quando nenhuma torre est� selecionada
    [SerializeField] private Color upgradeRangeButtonActiveColor;  // Cor quando uma torre est� selecionada
    [Space(10)]

    [SerializeField] private Button upgradeSpeedButton; // Bot�o de upgrade de velocidade
    [SerializeField] private TMP_Text upgradeSpeedButtonText; // Texto para exibir o custo do upgrade de velocidade
    [SerializeField] private Color upgradeSpeedButtonInactiveColor; // Cor quando nenhuma torre est� selecionada
    [SerializeField] private Color upgradeSpeedButtonActiveColor;  // Cor quando uma torre est� selecionada

    [Header("Cursor and Towers")]
    [SerializeField] private CustomCursor customCursor; // Cursor customizado usado para mostrar onde a torre ser� colocada
    [FormerlySerializedAs("TowerToPlace")] [SerializeField] private Turret towerToPlace; // Refer�ncia � torre que est� prestes a ser colocada

    [Header("Tiles and Path")]
    private Tile selectedTile; // Adicione isso fora de qualquer m�todo
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
        // Reinicializa o estado do jogo com moedas, vida e ondas
        GameState.Instance.ResetGameState();  // Garante que o estado esteja resetado ao começar

        sellTowerButton.onClick.AddListener(() => SellSelectedTower());
        upgradeRangeButton.onClick.AddListener(() => UpgradeSelectedTowerRange());
        upgradeSpeedButton.onClick.AddListener(() => UpgradeSelectedTowerSpeed());
    }



    private void Update()
    {
        HandlePauseInput();

        if (isPaused) return; // Se o jogo est� pausado, n�o executa o restante do Update

        HandleTowerPlacement();
        HandleTowerSelling();
        HandleTowerCancellation(); // Verifica se o jogador quer cancelar a compra da torre
        UpdateButtonColors(); // Atualiza a cor dos bot�es
        UpdateButtonValues(); // Atualiza os textos dos bot�es
    }

    // Atualiza os textos dos bot�es com os valores da torre selecionada
    // Atualiza os textos dos bot�es com os valores da torre selecionada
    private void UpdateButtonValues()
    {
        if (selectedTile != null && selectedTile.isOcupied)
        {
            Turret selectedTurret = selectedTile.tower;
            sellTowerButtonText.text = $"Sell: ${selectedTurret.sellValue}";

            if (selectedTurret.rangeUpgradeLevel < 3)
            {
                upgradeRangeButtonText.text = $"Upgrade Range: ${selectedTurret.CurrentRangeUpgradeCost}";
                upgradeRangeButton.interactable = true; // Habilita o bot�o se ainda for poss�vel fazer upgrade
            }
            else
            {
                upgradeRangeButtonText.text = "Max Range Upgrade";
                upgradeRangeButton.interactable = false; // Desabilita o bot�o se o limite de upgrade foi atingido
            }

            if (selectedTurret.speedUpgradeLevel < 3)
            {
                upgradeSpeedButtonText.text = $"Upgrade Speed: ${selectedTurret.CurrentSpeedUpgradeCost}";
                upgradeSpeedButton.interactable = true; // Habilita o bot�o se ainda for poss�vel fazer upgrade
            }
            else
            {
                upgradeSpeedButtonText.text = "Max Speed Upgrade";
                upgradeSpeedButton.interactable = false; // Desabilita o bot�o se o limite de upgrade foi atingido
            }
        }
        else
        {
            sellTowerButtonText.text = "Sell";
            upgradeRangeButtonText.text = "Upgrade Range";
            upgradeSpeedButtonText.text = "Upgrade Speed";
            upgradeRangeButton.interactable = false; // Desabilita o bot�o quando nenhuma torre est� selecionada
            upgradeSpeedButton.interactable = false; // Desabilita o bot�o quando nenhuma torre est� selecionada
        }
    }


    // Lida com a entrada do usu�rio para pausar/despausar o jogo.
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

    // Lida com a coloca��o de uma torre no tile mais pr�ximo ao clique do jogador.
    private void HandleTowerPlacement()
    {
        if (Input.GetMouseButtonDown(0) && towerToPlace != null)
        {
            Tile nearestTile = FindNearestTile();

            if (nearestTile != null && !nearestTile.isOcupied)
            {
                PlaceTower(nearestTile);
            }
        }
        else if (towerToPlace != null)
        {
            towerToPlace.SetGizmoVisibility(true);
        }
    }


    private void HandleTowerCancellation()
    {
        if (Input.GetMouseButtonDown(1) && towerToPlace != null)
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
        towerToPlace = null; // Reseta a torre a ser colocada
        customCursor.gameObject.SetActive(false); // Esconde o cursor customizado
        Cursor.visible = true; // Torna o cursor padr�o vis�vel novamente
        Debug.Log("Tower placement canceled.");
    }


    // Encontra o tile mais pr�ximo da posi��o do mouse.
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
        if (GameState.Instance.SpendCoins(towerToPlace.cost)) // Verifica e deduz as moedas
        {
            Turret newTower = Instantiate(towerToPlace, tile.transform.position, Quaternion.identity);
            tile.tower = newTower; // Vincula a torre ao Tile
            tile.isOcupied = true; // Marca o tile como ocupado
            placedTowers.Add(newTower); // Adiciona a nova torre � lista de torres colocadas

            // Desativa o gizmo da torre ap�s coloc�-la
            newTower.SetGizmoVisibility(false);

            towerToPlace = null; // Reseta a torre a ser colocada
            customCursor.gameObject.SetActive(false); // Esconde o cursor customizado
            Cursor.visible = true; // Torna o cursor padr�o vis�vel novamente
        }
        else
        {
            Debug.Log("Not enough coins to place the tower!");
            // Reseta o cursor se n�o houver moedas suficientes
            customCursor.gameObject.SetActive(false);
            Cursor.visible = true;
            towerToPlace = null;
        }
    }


    private void SellSelectedTower()
    {
        if (selectedTile != null && selectedTile.isOcupied)
        {
            SellTower(selectedTile, selectedTile.tower);
            selectedTile = null; // Reseta o tile selecionado ap�s a venda
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
            tile.tower = null; // Remove a refer�ncia da torre do Tile
            placedTowers.Remove(tower); // Remove a torre da lista de torres colocadas
            Destroy(tower.gameObject); // Destr�i a torre
        }
    }

    // Realiza a compra de uma torre, verificando se o jogador tem moedas suficientes.
    public void BuyTower(Turret tower)
    {
        customCursor.gameObject.SetActive(true);
        customCursor.GetComponent<SpriteRenderer>().sprite = tower.GetComponent<SpriteRenderer>().sprite;
        Cursor.visible = false;
        towerToPlace = tower;
        grid.SetActive(true); // Mostra a grade de coloca��o
    }



    private void UpgradeSelectedTowerRange()
    {
        if (selectedTile != null && selectedTile.isOcupied)
        {
            selectedTile.tower.UpgradeRange(); // Chame um m�todo na classe Turret para aumentar o alcance
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
            selectedTile.tower.UpgradeSpeed(); // Chame um m�todo na classe Turret para aumentar a velocidade
            Debug.Log("Tower speed upgraded.");
        }
        else
        {
            Debug.Log("No tower selected to upgrade.");
        }
    }

    private void UpdateButtonColors()
    {
// Verificação de nulidade para o botão de venda da torre
        if (sellTowerButton != null && sellTowerButton.image != null)
        {
            if (selectedTile != null && selectedTile.isOcupied)
            {
                sellTowerButton.image.color = sellTowerButtonActiveColor;
            }
            else
            {
                sellTowerButton.image.color = sellTowerButtonInactiveColor;
            }
        }


// Verificação de nulidade para o botão de upgrade de alcance
        if (upgradeRangeButton != null && upgradeRangeButton.image != null)
        {
            if (selectedTile != null && selectedTile.isOcupied)
            {
                upgradeRangeButton.image.color = upgradeRangeButtonActiveColor;
            }
            else
            {
                upgradeRangeButton.image.color = upgradeRangeButtonInactiveColor;
            }
        }


// Verificação de nulidade para o botão de upgrade de velocidade
        if (upgradeSpeedButton != null && upgradeSpeedButton.image != null)
        {
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

}
