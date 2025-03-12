using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int rows = 10;
    public int cols = 10;
    public float cellSize = 1.0f;
    public GameObject cellPrefab;  // Prefab de la celda (opcional)

    private Vector2 screenSize;
    private Vector2 gridOrigin;

    void Start()
    {
        CalculateGridSize();
        GenerateGrid();
    }

    void CalculateGridSize()
    {
        // Obtiene el tamaño de la pantalla en unidades del mundo
        Camera cam = Camera.main;
        float screenHeight = cam.orthographicSize * 2;
        float screenWidth = screenHeight * cam.aspect;

        // Calcula el tamaño del grid en base a la pantalla
        float gridWidth = cols * cellSize;
        float gridHeight = rows * cellSize;

        // Centra el grid en la pantalla
        float startX = (screenWidth - gridWidth) / 2 - (screenWidth / 2);
        float startY = (screenHeight - gridHeight) / 2 - (screenHeight / 2);

        gridOrigin = new Vector2(startX, startY);
    }

    void GenerateGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector2 cellPosition = new Vector2(
                    gridOrigin.x + col * cellSize + (cellSize / 2),
                    gridOrigin.y + row * cellSize + (cellSize / 2)
                );

                CreateCell(cellPosition);
            }
        }
    }

    void CreateCell(Vector2 position)
    {
        if (cellPrefab != null)
        {
            Instantiate(cellPrefab, position, Quaternion.identity, transform);
        }
        else
        {
            GameObject cell = new GameObject("Cell");
            cell.transform.position = position;
            cell.transform.parent = transform;

            // Agregar un SpriteRenderer opcional para ver las celdas
            SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
            sr.color = new Color(1, 1, 1, 0.2f); // Color semitransparente
        }
    }
}
