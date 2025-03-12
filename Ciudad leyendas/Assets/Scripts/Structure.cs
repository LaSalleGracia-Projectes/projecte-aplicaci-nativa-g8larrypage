using UnityEngine;

[System.Serializable]  // Esta línea es importante
public class Structure
{
    public string structureName;
    public Sprite structureSprite;  // Este campo es donde se asignará el sprite
    public int health;
    public int damage;
    public int price;

    // Constructor para la estructura
    public Structure(string name, Sprite sprite, int health, int damage, int price)
    {
        structureName = name;
        structureSprite = sprite;
        this.health = health;
        this.damage = damage;
        this.price = price;
    }
}
