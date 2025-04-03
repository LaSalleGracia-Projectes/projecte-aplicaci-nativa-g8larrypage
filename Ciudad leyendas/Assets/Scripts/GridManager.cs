using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    private Dictionary<int, Structure> placedStructures = new Dictionary<int, Structure>(); // Relación ID-Estructura
    private Structure selectedStructure = null;
    private int nextCellID = 1; // ID autoincremental para las celdas

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
                Cell cellComponent = cellObject.AddComponent<Cell>();
                cellComponent.cellID = nextCellID++; // Asigna un ID único
                gridArray[row, col] = cellComponent;
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
            Cell clickedCell = hit.collider.gameObject.GetComponent<Cell>();

            if (selectedStructure != null && clickedCell.placedStructure == null)
            {
                PlaceStructureInCell(clickedCell);
            }
        }
    }

    void PlaceStructureInCell(Cell cell)
    {
        GameObject newStructureObj = new GameObject(selectedStructure.structureName);
        newStructureObj.transform.position = cell.transform.position + new Vector3(0, 0, -1);

        SpriteRenderer sr = newStructureObj.AddComponent<SpriteRenderer>();
        sr.sprite = selectedStructure.structureSprite;

        if (FindObjectOfType<PopupManager>() != null)
        {
            newStructureObj.transform.SetParent(FindObjectOfType<PopupManager>().placedStructuresParent);
        }

        cell.AssignStructure(selectedStructure);
        placedStructures[cell.cellID] = selectedStructure; // Guarda la estructura con el ID de la celda
        selectedStructure = null;
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
}
