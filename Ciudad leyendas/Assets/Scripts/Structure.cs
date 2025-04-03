using UnityEngine;

[System.Serializable]
public class Structure
{
    public string structureName;
    public Sprite structureSprite;
    public int health;
    public int damage;
    public int price;

    public Structure(string name, Sprite sprite, int health, int damage, int price)
    {
        structureName = name;
        structureSprite = sprite;
        this.health = health;
        this.damage = damage;
        this.price = price;
    }
}
