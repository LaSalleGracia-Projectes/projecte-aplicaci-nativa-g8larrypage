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
    }

    public List<SkinEntry> skins;

    private Dictionary<long, Sprite> skinDict;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            skinDict = new Dictionary<long, Sprite>();

            foreach (var skin in skins)
            {
                if (!skinDict.ContainsKey(skin.id))
                    skinDict.Add(skin.id, skin.sprite);
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
            return skinDict[id];
        }
        Debug.LogWarning($"Skin ID {id} no encontrada.");
        return null;
    }
}
