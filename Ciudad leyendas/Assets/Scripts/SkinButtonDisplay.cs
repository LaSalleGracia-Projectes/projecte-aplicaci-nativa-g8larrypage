using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SkinButtonDisplay : MonoBehaviour
{
    [SerializeField] private Image skinImage;
    [SerializeField] private TMP_Text precioText;

    // Lista de IDs disponibles
    private readonly List<long> skinIDsDisponibles = new List<long> { 2, 3, 5, 6, 8, 9, 11, 12, 14, 15 };

    private long skinIDSeleccionada;

    void Start()
    {
        MostrarSkinAleatoria();
        StartCoroutine(ActualizarCada30Segundos());
    }

    public void MostrarSkinAleatoria()
    {
        if (skinIDsDisponibles.Count == 0 || SkinManager.Instance == null) return;

        // Elegir un ID aleatorio
        int indiceAleatorio = Random.Range(0, skinIDsDisponibles.Count);
        skinIDSeleccionada = skinIDsDisponibles[indiceAleatorio];

        // Obtener sprite y precio desde SkinManager
        Sprite sprite = SkinManager.Instance.GetSpriteForSkin(skinIDSeleccionada);
        int precio = SkinManager.Instance.GetPrecioForSkin(skinIDSeleccionada);

        // Asignar sprite y precio al botón
        if (sprite != null) skinImage.sprite = sprite;
        if (precio >= 0) precioText.text = precio.ToString();
        else precioText.text = "¿?";
    }

    IEnumerator ActualizarCada30Segundos()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            MostrarSkinAleatoria();
        }
    }

    public long GetSkinID()
    {
        return skinIDSeleccionada;
    }
}
