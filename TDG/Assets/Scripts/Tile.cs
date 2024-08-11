using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isOcupied;
    private Color startColor;

    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;

    // Adicionando refer�ncia � torre
    public Turret tower;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
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

    private void Update()
    {
        if (isOcupied)
        {
            // Mantendo a cor da transpar�ncia como estava antes, mas vis�vel
            Color tempColor = startColor;
            tempColor.a = 0.5f; // Mantenha a cor semi-transparente para indicar ocupa��o, ajuste conforme necess�rio
            sr.color = tempColor;
        }
    }
}
