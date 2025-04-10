using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Services;
using Models;
using System;

public class GridManager : MonoBehaviour
{
    public int rows = 10;
    public int cols = 10;
    public float cellSize = 1.0f;
    public GameObject cellPrefab;
    public Structure[] availableStructures;
    public Button[] structureButtons;

    private Vector2 gridOrigin;
    private Cell[,] gridArray;
    private Dictionary<GameObject, Structure> placedStructures = new Dictionary<GameObject, Structure>();
    private Structure selectedStructure = null;

    void Start()
    {
        CalculateGridSize();
        GenerateGrid();

        for (int i = 0; i < structureButtons.Length; i++)
        {
            int index = i;
            structureButtons[i].onClick.AddListener(() => SelectStructure(index));
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && selectedStructure != null)
        {
            DetectCellClick();
        }
    }

    void CalculateGridSize()
    {
        Camera cam = Camera.main;
        float screenHeight = cam.orthographicSize * 2;
        float screenWidth = screenHeight * cam.aspect;
        float gridWidth = cols * cellSize;
        float gridHeight = rows * cellSize;
        float startX = -gridWidth / 2;
        float startY = -gridHeight / 2;
        gridOrigin = new Vector2(startX, startY);
    }

    void GenerateGrid()
    {
        gridArray = new Cell[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector2 cellPosition = new Vector2(
                    gridOrigin.x + col * cellSize + (cellSize / 2),
                    gridOrigin.y + row * cellSize + (cellSize / 2)
                );

                GameObject cellObject = CreateCell(cellPosition);
                Cell cell = cellObject.GetComponent<Cell>();
                cell.cellID = row * cols + col;
                gridArray[row, col] = cell;
            }
        }
    }

    GameObject CreateCell(Vector2 position)
    {
        GameObject cell = cellPrefab != null ?
            Instantiate(cellPrefab, position, Quaternion.identity, transform) :
            new GameObject("Cell");

        if (cellPrefab == null)
        {
            cell.transform.position = position;
            cell.transform.parent = transform;
            SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
            sr.color = new Color(1, 1, 1, 0.2f);
        }

        cell.AddComponent<BoxCollider2D>();
        cell.AddComponent<Cell>();
        return cell;
    }

    void DetectCellClick()
    {
        Vector2 touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

        if (hit.collider != null)
        {
            GameObject clickedCell = hit.collider.gameObject;

            if (selectedStructure != null)
            {
                PlaceStructureInCell(clickedCell);
            }
        }
    }

    void PlaceStructureInCell(GameObject cellObject)
    {
        Cell cell = cellObject.GetComponent<Cell>();
        if (cell != null && cell.placedStructure == null)
        {
            GameObject newStructureObj = new GameObject(selectedStructure.structureName);
            newStructureObj.transform.position = cell.transform.position + new Vector3(0, 0, -1);

            SpriteRenderer sr = newStructureObj.AddComponent<SpriteRenderer>();
            sr.sprite = SkinManager.Instance.GetSkinSprite(selectedStructure.skinId);  // Usa skinId

            cell.placedStructure = selectedStructure;
            placedStructures[cellObject] = selectedStructure;

            SaveBuildingToSupabase(cell, selectedStructure);
            selectedStructure = null;
        }
    }

    void SelectStructure(int index)
    {
        if (index >= 0 && index < availableStructures.Length)
        {
            selectedStructure = availableStructures[index];

            PopupManager popupManager = FindObjectOfType<PopupManager>();
            if (popupManager != null)
            {
                popupManager.ClosePopup();
            }
        }
    }

    private async void SaveBuildingToSupabase(Cell cell, Structure structure)
    {
        try
        {
            var client = await SupabaseManager.Instance.GetClient();

            var edificio = new Edificio
            {
                TipoEdificio = structure.structureName,
                Vida = structure.health,
                Dano = structure.damage,
                IdCiudad = 1,
                IdSkin = structure.skinId,
                Cuadrado = cell.cellID
            };

            await client.From<Edificio>().Insert(edificio);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al guardar edificio en Supabase: {ex.Message}");
        }
    }
}
