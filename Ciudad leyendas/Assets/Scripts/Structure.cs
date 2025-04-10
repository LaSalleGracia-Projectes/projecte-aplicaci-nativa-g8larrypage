using UnityEngine;

[System.Serializable]
public class Structure
{
    public string structureName;
    public Sprite structureSprite;
    public int health;
    public int damage;
    public int price;
    public long skinId; // Nueva propiedad

    public Structure(string name, Sprite sprite, int health, int damage, int price, long skinId = 1)
    {
        structureName = name;
        structureSprite = sprite;
        this.health = health;
        this.damage = damage;
        this.price = price;
        this.skinId = skinId;
    }

    public Structure CloneWithSkin(long skinId)
    {
        var sprite = SkinManager.Instance.GetSpriteForSkin(skinId);
        return new Structure(structureName, sprite, health, damage, price, skinId);
    }
}
