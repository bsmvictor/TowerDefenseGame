using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Plot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;

    public GameObject towerObj;
    public Turret turret;
    private Color startColor;

    private void Start()
    {
        startColor = sr.color;
    }

    private void OnMouseEnter()
    {
        sr.color = hoverColor;
    }

    private void OnMouseExit()
    {
        sr.color = startColor;
    }

    private void OnMouseDown()
    {
        // Detecta o bot�o esquerdo do mouse para construir a torre
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Bot�o esquerdo pressionado");
            ConstructTower();
        }
    }

    private void Update()
    {
        // Detecta o bot�o direito do mouse para remover a torre
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Bot�o direito pressionado");
            RemoveTower();
        }
    }

    private void ConstructTower()
    {
        // Verifica se j� existe uma torre no plot. Se existir, retorna imediatamente sem fazer nada.
        if (towerObj != null) return;

        // Obt�m a torre selecionada a partir do BuildManager.
        Tower towerToBuild = BuildManager.main.GetSelectedTower();

        if (towerToBuild.cost > LevelManager.main.currency)
        {
            Debug.Log("Not enough currency to build that!");
            return;
        }

        // Deduz o custo da torre do total de moedas no LevelManager.
        LevelManager.main.SpendCurrency(towerToBuild.cost);

        // Instancia a torre no local do plot, definindo sua posi��o para a posi��o do plot.
        towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);
        turret = towerObj.GetComponent<Turret>();
    }

    private void RemoveTower()
    {
        if (towerObj == null) return;

        Destroy(towerObj);
        turret = null;
        towerObj = null;

        Debug.Log("Torre removida!");
    }

}
