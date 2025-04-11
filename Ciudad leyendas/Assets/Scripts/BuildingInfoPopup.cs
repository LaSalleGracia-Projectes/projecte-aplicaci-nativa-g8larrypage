using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuildingInfoPopup : MonoBehaviour
{
    public GameObject infoPanel;   // Panel de UI que muestra la informaci�n
    public Text nameText;          // Texto para el nombre del edificio
    public Text healthText;        // Texto para la vida del edificio
    public Text damageText;        // Texto para el da�o del edificio
    public Text priceText;         // Texto para el precio del edificio
    private Structure currentStructure;
    private bool isPressing = false;
    private float pressTime = 0f;
    private const float pressThreshold = 1f; // Tiempo m�nimo de presi�n para mostrar el popup (en segundos)

    void Update()
    {
        if (isPressing)
        {
            pressTime += Time.deltaTime;

            // Si el tiempo de presi�n supera el umbral, mostramos la ventana emergente
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
        pressTime = 0f; // Reiniciar el tiempo de presi�n
    }

    public void OnPointerUp()
    {
        isPressing = false; // Cuando el bot�n se suelta, desactivar el estado de presi�n
        HideInfoPanel();    // Ocultar el panel si se suelta el clic antes de la duraci�n
    }

    void ShowInfoPanel()
    {
        // Mostrar la ventana emergente con los stats del edificio
        if (currentStructure != null)
        {
            nameText.text = "Nombre: " + currentStructure.structureName;
            healthText.text = "Vida: " + currentStructure.health.ToString();
            damageText.text = "Da�o: " + currentStructure.damage.ToString();
            priceText.text = "Precio: " + currentStructure.price.ToString();

            infoPanel.SetActive(true);
        }
    }

    void HideInfoPanel()
    {
        infoPanel.SetActive(false); // Ocultar el panel de informaci�n
    }
}
