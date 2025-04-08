using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Services;  // Para la conexi�n a Supabase
using Models;   // Para el modelo Edificio
using System;   // Para excepciones

public class GridManager : MonoBehaviour
{
    public int rows = 10;
    public int cols = 10;
    public float cellSize = 1.0f;
    public GameObject cellPrefab;
    public Structure[] availableStructures; // Lista de estructuras disponibles
    public Button[] structureButtons; // Array de botones de las estructuras

    private Vector2 gridOrigin;
    private Cell[,] gridArray; // Usamos Cell en vez de GameObject
    private Dictionary<GameObject, Structure> placedStructures = new Dictionary<GameObject, Structure>();
    private Structure selectedStructure = null; // La estructura seleccionada para colocar

    void Start()
    {
        CalculateGridSize();
        GenerateGrid();

        // Asignar eventos a los botones
        for (int i = 0; i < structureButtons.Length; i++)
        {
            int index = i; // Necesario para evitar problemas con las lambdas
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
        gridArray = new Cell[rows, cols]; // Cambiado a Cell

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector2 cellPosition = new Vector2(
                    gridOrigin.x + col * cellSize + (cellSize / 2),
                    gridOrigin.y + row * cellSize + (cellSize / 2)
                );

                GameObject cellObject = CreateCell(cellPosition);
                Cell cell = cellObject.GetComponent<Cell>(); // Obtener el componente Cell

                // Asigna el ID de celda �nico
                cell.cellID = row * cols + col;

                // Agrega la celda al array
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
        cell.AddComponent<Cell>(); // Aseg�rate de agregar el componente Cell a cada celda
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
        Cell cell = cellObject.GetComponent<Cell>();  // Obtener el componente Cell
        if (cell != null && cell.placedStructure == null) // Si no tiene estructura, colocamos una nueva
        {
            GameObject newStructureObj = new GameObject(selectedStructure.structureName);
            newStructureObj.transform.position = cell.transform.position + new Vector3(0, 0, -1);

            SpriteRenderer sr = newStructureObj.AddComponent<SpriteRenderer>();
            sr.sprite = selectedStructure.structureSprite;

            cell.placedStructure = selectedStructure;  // Asigna la estructura a la celda

            placedStructures[cellObject] = selectedStructure;

            // Guardar la estructura en Supabase
            SaveBuildingToSupabase(cell, selectedStructure);

            selectedStructure = null; // Vuelve a deseleccionar la estructura
        }
    }

    void SelectStructure(int index)
    {
        if (index >= 0 && index < availableStructures.Length)
        {
            selectedStructure = availableStructures[index];

            // Cerrar el PopupPanel al seleccionar una estructura
            PopupManager popupManager = FindObjectOfType<PopupManager>();
            if (popupManager != null)
            {
                popupManager.ClosePopup();
            }
        }
    }

    // Funci�n para guardar el edificio en Supabase
    private async void SaveBuildingToSupabase(Cell cell, Structure structure)
    {
        try
        {
            var client = await SupabaseManager.Instance.GetClient();

            // Crear el objeto del edificio para insertar
            var edificio = new Edificio
            {
                TipoEdificio = structure.structureName,
                Vida = structure.health,
                Dano = structure.damage,
                IdCiudad = 1, // Asigna el ID de ciudad correspondiente
                IdSkin = 1, // Asigna el ID de skin correspondiente
                Cuadrado = cell.cellID
            };

            // Insertar el edificio en la base de datos
            var insertResponse = await client.From<Edificio>().Insert(edificio);

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al guardar edificio en Supabase: {ex.Message}");
        }
    }


}
