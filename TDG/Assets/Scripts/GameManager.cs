using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager main;

    private Turret TowerToPlace;
    public GameObject grid;

    public CustomCursor customCursor;

    public Tile[] tiles;

    public Transform startPoint;
    public Transform[] path;

    public int currency;
    public int life;

    // Lista para armazenar as torres colocadas
    public List<Turret> placedTowers = new List<Turret>();

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        currency = 300;
        life = 10;
    }

    private void Update()
    {
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
                placedTowers.Add(newTower); // Adiciona a nova torre à lista de torres colocadas
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
                    SellTower(selectedTile, selectedTile.tower); // Passa o Tile e a torre associados para a função
                }
            }
        }
    }

    public void BuyTower(Turret tower)
    {
        if (currency >= tower.cost)
        {
            customCursor.gameObject.SetActive(true);
            customCursor.GetComponent<SpriteRenderer>().sprite = tower.GetComponent<SpriteRenderer>().sprite;
            Cursor.visible = false;

            currency -= tower.cost;
            TowerToPlace = tower;
            grid.SetActive(true);
        }
    }

    public void SellTower(Tile tile, Turret tower)
    {
        if (tower != null && tile != null) // Verifique se a torre e o Tile não são null
        {
            currency += tower.sellValue;
            tile.isOcupied = false;
            tile.tower = null; // Remove a referência da torre do Tile
            placedTowers.Remove(tower); // Remove a torre da lista de torres colocadas
            Destroy(tower.gameObject); // Destrua a torre
        }
    }

    public void IncreaseCurrency(int amount)
    {
        currency += amount;
    }

    public bool SpendCurrency(int amount)
    {
        if (currency >= amount)
        {
            currency -= amount;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DecreaseLife(int amount)
    {
        life -= amount;
        if (life <= 0)
        {
            // Game Over
            Debug.Log("Game Over");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
