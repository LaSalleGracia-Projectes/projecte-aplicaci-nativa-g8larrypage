using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    private Tile _selectedTile;
    [SerializeField] private Color buildingColor;

    public void SelectTile(Tile tile)
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
        }

        _selectedTile = tile;
        _selectedTile.SetSelected(true);
    }

    public void PlaceBuilding()
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetBuildingColor(buildingColor);
            _selectedTile = null;
        }
    }
}
