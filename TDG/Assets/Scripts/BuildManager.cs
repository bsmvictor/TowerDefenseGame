using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager main;

    [Header("References")]
    [SerializeField] private Tower[] towers;

    private int selectedTower = 0;

    private void Awake()
    {
        if (main == null)
        {
            main = this;
        }
    }

    public Tower GetSelectedTower()
    {
        return towers[selectedTower];
    }

    public void SetSelectTower(int _selectedTower)
    {
        selectedTower = _selectedTower;
    }

}
