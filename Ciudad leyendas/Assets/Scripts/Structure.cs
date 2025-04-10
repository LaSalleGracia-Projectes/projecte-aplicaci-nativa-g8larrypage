using UnityEngine;

[System.Serializable]
public class Structure
{
    public string structureName;
    public int skinId;  // ID de skin en vez del sprite
    public int health;
    public int damage;
    public int price;

    public Structure(string name, int skinId, int health, int damage, int price)
    {
        structureName = name;
        this.skinId = skinId;
        this.health = health;
        this.damage = damage;
        this.price = price;
    }
}
