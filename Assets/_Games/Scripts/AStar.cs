using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStar : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;

    private const int MOVE_DIAGONAL_COST = 14;

    public GridMA GridMaster;
    [SerializeField] Vector2Int StartPos;
    [SerializeField] Vector2Int EndPos;

    HashSet<NodePath> Path = new HashSet<NodePath>();

    //Danh sách chứa các node chưa kiểm tra và sẽ được kiểm tra.
    List<NodePath> openList = new List<NodePath>();

    //Danh sách chứa các node đã được duyệt.
    HashSet<NodePath> closeList = new HashSet<NodePath>();

    private List<NodePath> availible = new List<NodePath>();

    List<NodePath> neighbours = new List<NodePath>();

    private NodePath lowestFCostNode = null;

    private Coroutine coro = null;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Ấn phím Space để tìm đường
        {
            if (availible.Count < 1)
            {
                foreach (var node in GridMaster.node)
                {
                    if (!node.walkable) continue;
                    availible.Add(node);
                }
            }

            if (StartPos == EndPos)
            {
                Debug.Log("Start and End positions are the same.");
                return;
            }
            ClearColorPath();

            var starPos = GridMaster.GetPointInGrid(StartPos);
            var endPos = GridMaster.GetPointInGrid(EndPos);
            if (starPos == null || endPos == null)
            {
                Debug.Log("Start or End position is out of bounds.");
                return;
            }
            if (coro != null) return;
            //EndPos = availible[ranE].Position;
            coro = StartCoroutine(FindPathIE(starPos.Position, endPos.Position));
            //FindPath(availible[ranS].Position, availible[ranE].Position);

        }
    }

    #region IE

    IEnumerator FindPathIE(Vector2Int startPos, Vector2Int targetPos)
    {
        openList.Clear();
        closeList.Clear();
        Path.Clear();
        NodePath startNode = GridMaster.GetPointInGrid(startPos);

        NodePath endNode = GridMaster.GetPointInGrid(targetPos);

        openList.Add(startNode);


        startNode.gCost = 0;
        startNode.hCost = CaculateDistance(startNode, endNode);

        if (!startNode.walkable || !endNode.walkable)
        {
            Debug.Log("No path found");
            coro = null;
            yield break;
        }

        while (openList.Count > 0)
        {
            NodePath currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                ClearColorPath();
                OnChangeColorPath();
                Debug.Log("Xong! số ô đi là: " + Path.Count);
                Debug.Log("Danh sách các node");
                for (int i = Path.Count - 1; i >= 0; i--)
                {
                    Debug.Log(Path.ElementAt(i).Position);
                }

                //Path = CalculatePath(startNode,currentNode);

                coro = null;
                yield break;
            }


            openList.Remove(currentNode);
            closeList.Add(currentNode);

            foreach (var neighbourNode in GetNeighboursList(currentNode))
            {
                if (closeList.Contains(neighbourNode) || !neighbourNode.walkable) continue;

                int newGCost = currentNode.gCost + CaculateDistance(currentNode, neighbourNode);

                if (newGCost < neighbourNode.gCost || !openList.Contains(neighbourNode))
                {
                    neighbourNode.gCost = newGCost;
                    neighbourNode.hCost = CaculateDistance(neighbourNode, endNode);
                    neighbourNode.Parent = currentNode;
                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }

            Path = CalculatePath(startNode, currentNode);
            OnChangeColorPath();
            yield return new WaitForSeconds(0.05f);
            yield return null;
        }
        coro = null;
        yield return null;
    }


    #endregion
    List<NodePath> GetNeighboursList(NodePath currentNode)
    {
        neighbours.Clear();
        //Check left
        if (currentNode.Position.x - 1 >= 0)
        {
            //Left
            neighbours.Add(
                GridMaster.GetPointInGrid(new Vector2Int(currentNode.Position.x - 1, currentNode.Position.y)));
            //LeftDown
            if (currentNode.Position.y - 1 >= 0)
                neighbours.Add(GridMaster.GetPointInGrid(new Vector2Int(currentNode.Position.x - 1,
                    currentNode.Position.y - 1)));
            //LeftUp
            if (currentNode.Position.y + 1 < GridMaster.Height)
                neighbours.Add(GridMaster.GetPointInGrid(new Vector2Int(currentNode.Position.x - 1,
                    currentNode.Position.y + 1)));
        }

        //Check right
        if (currentNode.Position.x + 1 < GridMaster.Width)
        {
            //Right
            neighbours.Add(
                GridMaster.GetPointInGrid(new Vector2Int(currentNode.Position.x + 1, currentNode.Position.y)));
            //RightDown
            if (currentNode.Position.y - 1 >= 0)
                neighbours.Add(GridMaster.GetPointInGrid(new Vector2Int(currentNode.Position.x + 1,
                    currentNode.Position.y - 1)));
            //RightUp
            if (currentNode.Position.y + 1 < GridMaster.Height)
                neighbours.Add(GridMaster.GetPointInGrid(new Vector2Int(currentNode.Position.x + 1,
                    currentNode.Position.y + 1)));
        }

        //Up
        if (currentNode.Position.y + 1 < GridMaster.Height)
            neighbours.Add(
                GridMaster.GetPointInGrid(new Vector2Int(currentNode.Position.x, currentNode.Position.y + 1)));

        //Down
        if (currentNode.Position.y - 1 >= 0)
            neighbours.Add(
                GridMaster.GetPointInGrid(new Vector2Int(currentNode.Position.x, currentNode.Position.y - 1)));
        return neighbours;
    }

    //Eulid distance.
    int CaculateDistance(NodePath n1, NodePath n2)
    {
        int xDistance = Mathf.Abs(n1.Position.x - n2.Position.x);

        int yDistance = Mathf.Abs(n1.Position.y - n2.Position.y);

        int remaining = Mathf.Abs(xDistance - yDistance);



        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    NodePath GetLowestFCostNode(List<NodePath> NodeList)
    {
        lowestFCostNode = NodeList[0];
        for (int i = 0; i < NodeList.Count; i++)
        {
            if (NodeList[i].fCost < lowestFCostNode.fCost || (NodeList[i].fCost == lowestFCostNode.fCost &&
                                                              NodeList[i].hCost < lowestFCostNode.hCost))
            {
                lowestFCostNode = NodeList[i];
            }
        }

        return lowestFCostNode;
    }

    HashSet<NodePath> CalculatePath(NodePath startNode, NodePath endNode)
    {
        Path.Clear();
        Path.Add(endNode);
        NodePath currentNode = endNode;
        while (currentNode.Parent != null && currentNode.Parent != startNode)
        {
            Path.Add(currentNode.Parent);
            currentNode = currentNode.Parent;
        }
        Path.Reverse();

        return Path;
    }
    #region Color
    void OnChangeColorPath()
    {

        if (Path != null && Path.Count > 0)
        {
            Color color = Color.yellow;
            foreach (NodePath node in Path)
            {
                if (node == null) return;
                node.ObjectMove.color = color;
            }
        }
        Color colorNpc = Color.green;
        Color endColor = Color.red;
        if (GridMaster.GetPointInGrid(EndPos) != null)
        {
            GridMaster.GetPointInGrid(EndPos).ObjectMove.color = endColor;
        }
        if (GridMaster.GetPointInGrid(StartPos) != null)
        {
            GridMaster.GetPointInGrid(StartPos).ObjectMove.color = colorNpc;
        }
    }
    void ClearColorPath()
    {
        if (availible != null && availible.Count > 0)
        {
            Color color = Color.white;
            foreach (NodePath node in availible)
            {
                if (node == null) return;
                node.ObjectMove.color = color;
            }
        }
    }
    #endregion
}