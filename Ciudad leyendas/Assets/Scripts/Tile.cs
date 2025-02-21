using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _offsetColor, _selectedColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    private bool _isSelected = false;
    private bool _hasBuilding = false;
    private Color _previousColor; // Nuevo: Guardar color anterior

    public void Init(bool isOffset, BuildingPlacer placer)
    {
        _previousColor = isOffset ? _offsetColor : _baseColor;
        _renderer.color = _previousColor;
    }

    void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }

    void OnMouseExit()
    {
        _highlight.SetActive(false);
    }

    void OnMouseDown()
    {
        if (!_hasBuilding)
        {
            FindObjectOfType<BuildingPlacer>().SelectTile(this);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (!_hasBuilding)
        {
            _isSelected = isSelected;
            _renderer.color = isSelected ? _selectedColor : _previousColor;
        }
    }

    public void SetBuildingColor(Color buildingColor)
    {
        _renderer.color = buildingColor;
        _hasBuilding = true;
        _previousColor = buildingColor;
    }
}
