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
    public Structure ayuntamiento;  // Referencia al ayuntamiento
    public Button botonAyuntamiento;  // Botón para colocar el ayuntamiento

    private Vector2 gridOrigin;
    private GameObject[,] gridArray;
    private Dictionary<GameObject, Structure> placedStructures = new Dictionary<GameObject, Structure>();
    private bool placingAyuntamiento = false;  // Control para colocar solo 1 ayuntamiento

    void Start()
    {
        CalculateGridSize();
        GenerateGrid();

        if (botonAyuntamiento != null)
        {
            botonAyuntamiento.onClick.AddListener(EnableAyuntamientoPlacement);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && placingAyuntamiento)
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
                gridArray[row, col] = cell;
                placedStructures[cell] = null;
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

            if (placingAyuntamiento)
            {
                PlaceAyuntamiento(clickedCell);
            }
        }
    }

    void PlaceAyuntamiento(GameObject cell)
    {
        if (placedStructures.ContainsKey(cell) && placedStructures[cell] == null)
        {
            GameObject newStructureObj = new GameObject(ayuntamiento.structureName);
            newStructureObj.transform.position = cell.transform.position + new Vector3(0, 0, -1);

            SpriteRenderer sr = newStructureObj.AddComponent<SpriteRenderer>();
            sr.sprite = ayuntamiento.structureSprite;

            if (FindObjectOfType<PopupManager>() != null)
            {
                newStructureObj.transform.SetParent(FindObjectOfType<PopupManager>().placedStructuresParent);
            }

            placedStructures[cell] = ayuntamiento;

            placingAyuntamiento = false; // Desactivar el modo de colocación después de poner 1 ayuntamiento
        }
    }

    void EnableAyuntamientoPlacement()
    {
        placingAyuntamiento = true;  // Activar el modo para colocar el ayuntamiento
    }
}
