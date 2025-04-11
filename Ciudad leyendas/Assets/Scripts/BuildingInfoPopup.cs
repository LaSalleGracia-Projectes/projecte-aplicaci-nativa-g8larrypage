using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuildingInfoPopup : MonoBehaviour
{
    public GameObject infoPanel;   // Panel de UI que muestra la información
    public Text nameText;          // Texto para el nombre del edificio
    public Text healthText;        // Texto para la vida del edificio
    public Text damageText;        // Texto para el daño del edificio
    public Text priceText;         // Texto para el precio del edificio
    private Structure currentStructure;
    private bool isPressing = false;
    private float pressTime = 0f;
    private const float pressThreshold = 1f; // Tiempo mínimo de presión para mostrar el popup (en segundos)

    void Update()
    {
        if (isPressing)
        {
            pressTime += Time.deltaTime;

            // Si el tiempo de presión supera el umbral, mostramos la ventana emergente
            if (pressTime >= pressThreshold && currentStructure != null)
            {
                ShowInfoPanel();
            }
        }
    }

    public void OnPointerDown(Structure structure)
    {
        currentStructure = structure;
        isPressing = true;
        pressTime = 0f; // Reiniciar el tiempo de presión
    }

    public void OnPointerUp()
    {
        isPressing = false; // Cuando el botón se suelta, desactivar el estado de presión
        HideInfoPanel();    // Ocultar el panel si se suelta el clic antes de la duración
    }

    void ShowInfoPanel()
    {
        // Mostrar la ventana emergente con los stats del edificio
        if (currentStructure != null)
        {
            nameText.text = "Nombre: " + currentStructure.structureName;
            healthText.text = "Vida: " + currentStructure.health.ToString();
            damageText.text = "Daño: " + currentStructure.damage.ToString();
            priceText.text = "Precio: " + currentStructure.price.ToString();

            infoPanel.SetActive(true);
        }
    }

    void HideInfoPanel()
    {
        infoPanel.SetActive(false); // Ocultar el panel de información
    }
}
