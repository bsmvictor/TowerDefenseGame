using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager main;

    private Turret TowerToPlace;
    public GameObject grid;
    public GameObject pauseMenu;

    public CustomCursor customCursor;

    public Tile[] tiles;

    public Transform startPoint;
    public Transform[] path;

    // Lista para armazenar as torres colocadas
    public List<Turret> placedTowers = new List<Turret>();

    private bool isPaused = false; // Vari�vel para armazenar o estado de pausa

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        GameState.Instance.ResetCoins(300); // Inicializa as moedas com 300
        GameState.Instance.ResetHealth(10); // Inicializa a vida com 10
    }

    private void Update()
    {
        // Verifica se o jogador apertou a tecla de pausa (por exemplo, Escape)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (isPaused) return; // Se o jogo est� pausado, n�o executa o restante do Update

        if (Input.GetMouseButtonDown(0) && TowerToPlace != null)
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
            if (nearestTile.isOcupied == false)
            {
                Turret newTower = Instantiate(TowerToPlace, nearestTile.transform.position, Quaternion.identity);
                nearestTile.tower = newTower; // Vincula a torre ao Tile
                placedTowers.Add(newTower); // Adiciona a nova torre � lista de torres colocadas
                TowerToPlace = null;
                nearestTile.isOcupied = true;
                customCursor.gameObject.SetActive(false);
                Cursor.visible = true;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Tile selectedTile = hit.collider.GetComponent<Tile>();
                if (selectedTile != null && selectedTile.isOcupied && selectedTile.tower != null)
                {
                    SellTower(selectedTile, selectedTile.tower); // Passa o Tile e a torre associados para a fun��o
                }
            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f; // Se pausado, para o tempo do jogo, caso contr�rio, continua

        if(isPaused)
            pauseMenu.gameObject.SetActive(true); // Ativa o menu de pausa
        else
            pauseMenu.gameObject.SetActive(false); // Desativa o menu de pausa
    }

    public void BuyTower(Turret tower)
    {
        if (GameState.Instance.SpendCoins(tower.cost))
        {
            customCursor.gameObject.SetActive(true);
            customCursor.GetComponent<SpriteRenderer>().sprite = tower.GetComponent<SpriteRenderer>().sprite;
            Cursor.visible = false;

            TowerToPlace = tower;
            grid.SetActive(true);
        }
        else
        {
            Debug.Log("Not enough coins to buy the tower!");
        }
    }

    public void SellTower(Tile tile, Turret tower)
    {
        if (tower != null && tile != null) // Verifique se a torre e o Tile n�o s�o null
        {
            GameState.Instance.AddCoins(tower.sellValue); // Adiciona as moedas da venda
            tile.isOcupied = false;
            tile.tower = null; // Remove a refer�ncia da torre do Tile
            placedTowers.Remove(tower); // Remove a torre da lista de torres colocadas
            Destroy(tower.gameObject); // Destrua a torre
        }
    }

    public void DecreaseLife(int amount)
    {
        GameState.Instance.DecreaseHealth(amount); // Diminui a vida no GameState
        if (GameState.Instance.PlayerHealth <= 0)
        {
            // Game Over
            Debug.Log("Game Over");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
