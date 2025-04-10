using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance;

    [System.Serializable]
    public class SkinEntry
    {
        public long skinId;
        public Sprite sprite;
    }

    public SkinEntry[] skins;
    private Dictionary<long, Sprite> skinDictionary;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        skinDictionary = new Dictionary<long, Sprite>();
        foreach (var skin in skins)
        {
            skinDictionary[skin.skinId] = skin.sprite;
        }
    }

    public Sprite GetSkinSprite(long skinId)
    {
        if (skinDictionary.TryGetValue(skinId, out Sprite sprite))
            return sprite;

        Debug.LogWarning($"No skin found for ID: {skinId}");
        return null;
    }
}
