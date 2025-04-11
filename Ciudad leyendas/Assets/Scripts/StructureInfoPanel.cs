using UnityEngine;
using TMPro;

public class StructureInfoPanel : MonoBehaviour
{
    public static StructureInfoPanel Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text structureNameText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Canvas canvas;  // Aquí ya tienes una referencia al Canvas

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            panel.SetActive(false);

            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main;
                canvas.overrideSorting = true;
                canvas.sortingOrder = -10; // Más al fondo
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void ShowInfo(string name, int health, int damage)
    {
        structureNameText.text = name;
        statsText.text = $"Vida: {health}\nDaño: {damage}";
        panel.SetActive(true);
    }

    public void HideInfo()
    {
        panel.SetActive(false);
    }
}
