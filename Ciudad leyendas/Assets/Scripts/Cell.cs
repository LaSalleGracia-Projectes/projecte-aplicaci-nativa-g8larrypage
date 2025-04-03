using UnityEngine;

public class Cell : MonoBehaviour
{
    public long cellID;  // El ID único para cada celda.
    public Structure placedStructure;  // La estructura colocada en esta celda (puede ser null si no hay estructura).
}
