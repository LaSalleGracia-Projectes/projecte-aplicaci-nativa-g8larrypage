using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance;

    [System.Serializable]
    public class SkinEntry
    {
        public long id;
        public Sprite sprite;
        public int precio; // Nuevo atributo de precio
    }

    public List<SkinEntry> skins;

    private Dictionary<long, SkinEntry> skinDict;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            skinDict = new Dictionary<long, SkinEntry>();

            foreach (var skin in skins)
            {
                if (!skinDict.ContainsKey(skin.id))
                    skinDict.Add(skin.id, skin);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Sprite GetSpriteForSkin(long id)
    {
        if (skinDict.ContainsKey(id))
        {
            return skinDict[id].sprite;
        }
        Debug.LogWarning($"Skin ID {id} no encontrada.");
        return null;
    }

    public int GetPrecioForSkin(long id)
    {
        if (skinDict.ContainsKey(id))
        {
            return skinDict[id].precio;
        }
        Debug.LogWarning($"Precio para Skin ID {id} no encontrado.");
        return -1;
    }
}
