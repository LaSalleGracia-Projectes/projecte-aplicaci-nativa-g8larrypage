using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Services;
using Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public class GridManager : MonoBehaviour
{
    public int rows = 10;
    public int cols = 10;
    public float cellSize = 1.0f;
    public GameObject cellPrefab;
    public Structure[] availableStructures;
    public Button[] structureButtons;
    public Transform placedStructuresParent;

    public long ciudadId = 2; // ID de ciudad a cargar. Puedes cambiarlo dinámicamente según el jugador.

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

        LoadBuildingsFromSupabase(); // Carga de edificios al iniciar
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
        GameObject cell;

        if (cellPrefab != null)
        {
            cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
        }
        else
        {
            cell = new GameObject("Cell");
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
                PlaceStructureInCell(clickedCell, selectedStructure);
            }
        }
    }

    void PlaceStructureInCell(GameObject cellObject, Structure structure)
    {
        Cell cell = cellObject.GetComponent<Cell>();
        if (cell != null && cell.placedStructure == null)
        {
            GameObject newStructureObj = new GameObject(structure.structureName);
            newStructureObj.transform.position = cell.transform.position + new Vector3(0, 0, -1);
            newStructureObj.transform.parent = placedStructuresParent;

            // Usamos la skin desde SkinManager
            Sprite structureSprite = SkinManager.Instance.GetSpriteForSkin(structure.skinId);
            if (structureSprite != null)
            {
                SpriteRenderer sr = newStructureObj.AddComponent<SpriteRenderer>();
                sr.sprite = structureSprite;
            }

            cell.placedStructure = structure;
            placedStructures[cellObject] = structure;

            SaveBuildingToSupabase(cell, structure);
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

            // Verificar si ya existe un edificio en esta celda para esta ciudad
            var existingResponse = await client
                .From<Edificio>()
                .Where(x => x.IdCiudad == ciudadId)
                .Where(x => x.Cuadrado == cell.cellID)
                .Get();

            if (existingResponse.Models.Count > 0)
            {
                // Ya existe, así que lo actualizamos
                var existing = existingResponse.Models[0];

                existing.TipoEdificio = structure.structureName;
                existing.Vida = structure.health;
                existing.Dano = structure.damage;
                existing.IdSkin = structure.skinId;

                await UpdateBuildingInSupabase(existing);
                Debug.Log($"Edificio actualizado en celda {cell.cellID}");
            }
            else
            {
                // No existe, así que lo insertamos
                var edificio = new Edificio
                {
                    TipoEdificio = structure.structureName,
                    Vida = structure.health,
                    Dano = structure.damage,
                    IdCiudad = ciudadId,
                    IdSkin = structure.skinId,
                    Cuadrado = cell.cellID
                };

                await client.From<Edificio>().Insert(edificio);
                Debug.Log($"Edificio insertado en celda {cell.cellID}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al guardar edificio en Supabase: {ex.Message}");
        }
    }

    private async Task UpdateBuildingInSupabase(Edificio edificio)
    {
        try
        {
            var client = await SupabaseManager.Instance.GetClient();
            await client.From<Edificio>().Update(edificio);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al actualizar edificio en Supabase: {ex.Message}");
        }
    }

    private async void LoadBuildingsFromSupabase()
    {
        try
        {
            var client = await SupabaseManager.Instance.GetClient();
            var response = await client.From<Edificio>().Where(x => x.IdCiudad == ciudadId).Get();

            foreach (var edificio in response.Models)
            {
                int row = (int)(edificio.Cuadrado / cols);
                int col = (int)(edificio.Cuadrado % cols);

                if (row < rows && col < cols)
                {
                    Cell cell = gridArray[row, col];
                    Structure structure = FindStructureByName(edificio.TipoEdificio);

                    if (structure != null)
                    {
                        structure = structure.CloneWithSkin(edificio.IdSkin); // Aplica la skin correcta
                        PlaceStructureInCell(cell.gameObject, structure);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cargar edificios desde Supabase: {ex.Message}");
        }
    }

    private Structure FindStructureByName(string name)
    {
        foreach (var s in availableStructures)
        {
            if (s.structureName == name)
                return s;
        }
        return null;
    }
}
