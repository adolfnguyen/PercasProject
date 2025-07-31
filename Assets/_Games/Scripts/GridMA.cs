using UnityEngine;
using Random = UnityEngine.Random;

public class GridMA : MonoBehaviour
{
    public int Width;
    public int Height;

    [SerializeField] SpriteRenderer objectMovePref;
    [SerializeField] Transform holder;
    public NodePath[,] node;

    private void Start()
    {
        CreateGrid(Width, Height);
    }
    void CreateGrid(int width, int height)
    {
        Camera mainCamera = Camera.main;
        
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        node = new NodePath[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                node[i, j] = new NodePath(i, j);
                node[i, j].gCost = int.MaxValue;
                bool walkable = Random.value > 0.2f;
                node[i, j].walkable = walkable;
                node[i, j].Parent = null;
                GameObject objMove = Instantiate(objectMovePref.gameObject, new Vector2(node[i, j].Position.x + worldPoint.x*2+2f, node[i, j].Position.y+worldPoint.y*2+0.5f), Quaternion.identity,holder);
                node[i, j].ObjectMove = objMove.transform.GetChild(0).GetComponent<SpriteRenderer>();
                Color32 color = walkable ? Color.white : Color.gray;
                node[i, j].ObjectMove.color = color;
            }
        }
        holder.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
    public NodePath GetPointInGrid(Vector2Int position)
    {
        if (node == null) return null;
        if (position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height)
        {
            return node[position.x, position.y];
        }
        else
        {
            return null;
        }
    }
}
