using UnityEngine;

public class Cell : MonoBehaviour
{
    public int cellID; // ID �nico de la celda
    public Structure placedStructure; // Estructura colocada en la celda (si la hay)

    public void AssignStructure(Structure structure)
    {
        placedStructure = structure;
    }
}
