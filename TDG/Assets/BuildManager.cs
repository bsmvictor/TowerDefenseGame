using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager main;

    [Header("References")]
    [SerializeField] private GameObject[] towerPrefab;

    private int selectedTower = 0;

    private void Awake()
    {
        if (main == null)
        {
            main = this;
        }
    }

    public GameObject GetSelectedTower()
    {
        return towerPrefab[selectedTower];
    }
}
