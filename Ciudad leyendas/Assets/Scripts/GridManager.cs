using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int rows = 10;
    public int cols = 10;
    public float cellSize = 1.0f;
    public GameObject cellPrefab;  // Prefab de la celda (opcional)
    public GameObject objectToPlace; // Objeto que se colocará en las celdas

    private Vector2 gridOrigin;
    private GameObject[,] gridArray; // Almacena las celdas creadas

    void Start()
    {
        CalculateGridSize();
        GenerateGrid();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detección de toque o clic
        {
            DetectCellClick();
        }
    }

    void CalculateGridSize()
    {
        Camera cam = Camera.main;

        // Obtiene el tamaño visible de la pantalla en unidades del mundo
        float screenHeight = cam.orthographicSize * 2;
        float screenWidth = screenHeight * cam.aspect;

        // Calcula el tamaño total del grid en Unity
        float gridWidth = cols * cellSize;
        float gridHeight = rows * cellSize;

        // Centrar correctamente en la pantalla
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
            PlaceObjectInCell(clickedCell);
        }
    }

    void PlaceObjectInCell(GameObject cell)
    {
        if (objectToPlace != null)
        {
            Instantiate(objectToPlace, cell.transform.position, Quaternion.identity);
        }
    }
}
