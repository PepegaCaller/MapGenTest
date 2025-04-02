using UnityEngine;

public class Enemy
{
    public Vector2Int position; 
    public string type; 
    public int health; 

    public Enemy(Vector2Int position, string type, int health)
    {
        this.position = position;
        this.type = type;
        this.health = health;
    }
}
