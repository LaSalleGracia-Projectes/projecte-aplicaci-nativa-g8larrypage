using UnityEngine;
using UnityEngine.EventSystems;

public class StructureHoldDetector : MonoBehaviour
{
    public Structure structure; // <- Asegúrate que sea pública

    private float holdTime = 0.5f;
    private float timer;
    private bool isHolding;

    void Update()
    {
        if (IsPointerOverThisObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                isHolding = true;
                timer = 0;
            }

            if (isHolding && Input.GetMouseButton(0))
            {
                timer += Time.deltaTime;
                if (timer >= holdTime)
                {
                    ShowStructureInfo();
                    isHolding = false; // Evitar múltiples llamadas
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isHolding = false;
            }
        }

        // Aquí agregamos la verificación si se hace clic fuera del panel
        if (Input.GetMouseButtonDown(0))
        {
            // Si el clic no está en el panel, lo ocultamos
            if (!IsPointerOverStructure())
            {
                StructureInfoPanel.Instance.HideInfo();
            }
        }
    }

    bool IsPointerOverThisObject()
    {
        Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(touchPos);
        return hit != null && hit.gameObject == gameObject;
    }

    bool IsPointerOverStructure()
    {
        Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(touchPos);
        return hit != null && hit.gameObject == gameObject;
    }

    void ShowStructureInfo()
    {
        if (structure != null)
        {
            StructureInfoPanel.Instance.ShowInfo(structure.structureName, structure.health, structure.damage);
        }
        else
        {
            Debug.LogWarning("No hay structure asignado en StructureHoldDetector.");
        }
    }
}

