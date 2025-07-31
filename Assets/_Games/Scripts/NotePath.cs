using UnityEngine;

public class NodePath
{
    public int gCost = 0;
    public int hCost = 0;
    public int fCost => gCost + hCost;
    public Vector2Int Position;
    public bool walkable = true;
    public NodePath Parent;
    public SpriteRenderer ObjectMove;
    public NodePath(int x, int y, bool walkable = true)
    {
        Position = new Vector2Int(x, y);
        this.walkable = walkable;
    }
    
}
