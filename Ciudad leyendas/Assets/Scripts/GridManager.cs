using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int rows = 10;
    public int cols = 10;
    public float cellSize = 1.0f;
    public GameObject cellPrefab;
    public Structure[] availableStructures; // Lista de estructuras disponibles
    public Button[] structureButtons; // Array de botones de las estructuras

    private Vector2 gridOrigin;
    private GameObject[,] gridArray;
    private Dictionary<int, Structure> placedStructures = new Dictionary<int, Structure>(); // Diccionario con ID de celda y estructura
    private Structure selectedStructure = null; // La estructura seleccionada para colocar
    private int cellIdCounter = 0; // Contador de IDs únicos para cada celda

    void Start()
    {
        CalculateGridSize();
        GenerateGrid();

        // Asignar eventos a los botones
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
        gridArray = new GameObject[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector2 cellPosition = new Vector2(
                    gridOrigin.x + col * cellSize + (cellSize / 2),
                    gridOrigin.y + row * cellSize + (cellSize / 2)
                );

                GameObject cell = CreateCell(cellPosition);
                CellData cellData = cell.AddComponent<CellData>(); // Agregar script para ID
                cellData.cellId = cellIdCounter++; // Asignar ID único a la celda

                gridArray[row, col] = cell;
                placedStructures[cellData.cellId] = null; // Inicialmente sin estructura
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
        return cell;
    }

    void DetectCellClick()
    {
        Vector2 touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

        if (hit.collider != null)
        {
            GameObject clickedCell = hit.collider.gameObject;
            PlaceStructureInCell(clickedCell);
        }
    }

    void PlaceStructureInCell(GameObject cell)
    {
        CellData cellData = cell.GetComponent<CellData>();
        if (cellData == null || placedStructures[cellData.cellId] != null) return; // Si ya tiene estructura, no colocar

        GameObject newStructureObj = new GameObject(selectedStructure.structureName);
        newStructureObj.transform.position = cell.transform.position + new Vector3(0, 0, -1);

        SpriteRenderer sr = newStructureObj.AddComponent<SpriteRenderer>();
        sr.sprite = selectedStructure.structureSprite;

        if (FindObjectOfType<PopupManager>() != null)
        {
            newStructureObj.transform.SetParent(FindObjectOfType<PopupManager>().placedStructuresParent);
        }

        placedStructures[cellData.cellId] = selectedStructure; // Asignar la estructura al ID de la celda
        selectedStructure = null; // Se debe volver a seleccionar otra estructura para seguir colocando
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
}
