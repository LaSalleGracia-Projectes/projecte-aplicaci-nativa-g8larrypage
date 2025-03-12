using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int rows = 10;
    public int cols = 10;
    public float cellSize = 1.0f;
    public GameObject cellPrefab;  // Prefab de la celda
    public Structure[] availableStructures;  // Esta propiedad debe ser pública o [SerializeField] para aparecer en el Inspector


    private Vector2 gridOrigin;
    private GameObject[,] gridArray; // Almacena las celdas creadas
    private Dictionary<GameObject, Structure> placedStructures = new Dictionary<GameObject, Structure>(); // Relación Celda-Estructura

    void Start()
    {
        CalculateGridSize();
        GenerateGrid();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detección de clic/tap
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
                placedStructures[cell] = null; // Inicialmente, cada celda está vacía
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

            // Agregar un SpriteRenderer opcional para ver las celdas
            SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
            sr.color = new Color(1, 1, 1, 0.2f);
        }

        cell.AddComponent<BoxCollider2D>(); // Agregar colisionador para detectar clics
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
        // Si la celda no tiene una estructura, colocar una nueva
        if (placedStructures.ContainsKey(cell) && placedStructures[cell] == null)
        {
            Structure structureToPlace = availableStructures[Random.Range(0, availableStructures.Length)]; // Elegir una estructura aleatoria

            // Instanciar la estructura y colocarla en la celda
            GameObject newStructureObj = new GameObject(structureToPlace.structureName);
            newStructureObj.transform.position = cell.transform.position + new Vector3(0, 0, -1); // Colocar encima de la celda

            // Asignar el sprite de la estructura
            SpriteRenderer sr = newStructureObj.AddComponent<SpriteRenderer>();
            sr.sprite = structureToPlace.structureSprite;  // Asignamos el sprite correctamente

            // Almacenar la estructura en la celda
            placedStructures[cell] = structureToPlace;
        }
    }

}
