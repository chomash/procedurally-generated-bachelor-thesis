using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

class PathNode
{
    public GridLocation location;
    public float G;
    public float H;
    public float F;
    public PathNode parent;

    public PathNode(GridLocation l, float g, float h, float f, PathNode p)
    {
        location = l;
        G = g;
        H = h;
        F = f;
        parent = p;
    }
    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return location.Equals(((PathNode)obj).location);
        }
    }
    public override int GetHashCode()
    {
        return 0;
    }
}

public class Pathfinding_AStar : MonoBehaviour
{
    [SerializeField] DungeonGenerator dungGenerator;
    [Header("weights")]
    public float empty;
    public float rightRoom, wrongRoom, corridor, border, distanceMulti;

    List<PathNode> openNodes = new List<PathNode>();
    List<PathNode> closeNodes = new List<PathNode>();
    PathNode goalNode;
    PathNode startNode;
    PathNode lastNode;
    GridLocation startL, endL;
    bool done = false;

    public void SearchForConnection(GridLocation _startL, GridLocation _endL)
    {
        startL = _startL;
        endL = _endL;
        BeginSearch();
        do{Search(lastNode);
        }while (!done);

        GetPath();
    }
    public void BeginSearch()
    {
        done = false;

        //Vector3 startLocation = new Vector3(startL.x, startL.y, 0);
        //Vector3 endLocation = new Vector3(endL.x, endL.y, 0);
        startNode = new PathNode(new GridLocation(startL.x, startL.y),0, 0, 0, null);
        goalNode = new PathNode(new GridLocation(endL.x, endL.y), 0, 0, 0, null);

        openNodes.Clear();
        closeNodes.Clear();
        openNodes.Add(startNode);
        lastNode = startNode;
    }

    void Search(PathNode thisNode)
    {
        if (thisNode.Equals(goalNode))//goal has been reached
        {
            done = true;
            return;
        }

        foreach(GridLocation dir in dungGenerator.directions)
        {
            GridLocation neighbour = dir + thisNode.location;
            if (neighbour.x < 0 || neighbour.y < 0 || neighbour.x >= dungGenerator.dungeonSize.x || neighbour.y >= dungGenerator.dungeonSize.y) { continue; }
            if (isClosed(neighbour)) continue;


            float G = NeighbourCost(neighbour) + thisNode.G; 
            float H = distanceMulti * Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            float F = G + H;
            if (!UpdateMarker(neighbour, G, H, F, thisNode))
                openNodes.Add(new PathNode(neighbour, G, H, F, thisNode));
        }

        openNodes = openNodes.OrderBy(p=>p.F).ToList<PathNode>();
        PathNode pN = (PathNode) openNodes.ElementAt(0);
        closeNodes.Add(pN);
        openNodes.RemoveAt(0);
        lastNode = pN;
    }
    public void GetPath()
    {
        PathNode beginNode = lastNode;
        while (!startNode.Equals(beginNode) && beginNode != null)
        {
            //0 - empty/wall, 1 - room, 2 - border, 3 - corridor
            if (dungGenerator.gridMap[beginNode.location.x, beginNode.location.y].type != 1)
            {
                dungGenerator.gridMap[beginNode.location.x, beginNode.location.y] = new GridLocation(beginNode.location.x, beginNode.location.y, 3, null);
            }
            beginNode = beginNode.parent;
        }
    }
    bool isClosed(GridLocation marker)
    {
        foreach (PathNode p in closeNodes)
        {
            if (p.location.Equals(marker)) return true;
        }
        return false;
    }
    bool UpdateMarker(GridLocation pos, float g, float h, float f, PathNode prt)
    {
        foreach (PathNode p in openNodes)
        {
            if (p.location.Equals(pos))
            {
                if (f < p.F)
                {
                    p.G = g;
                    p.H = h;
                    p.F = f;
                    p.parent = prt;
                }
                return true;
            }
        }
        return false;
    }
    float NeighbourCost(GridLocation point)
    {
        float newG = float.PositiveInfinity;
        //0 - empty/wall, 1 - room, 2 - border, 3 - corridor
        if (dungGenerator.gridMap[point.x, point.y].type == 0)
        {
            newG = empty;
        }
        else if(dungGenerator.gridMap[point.x, point.y].type == 1)
        {
            if(dungGenerator.gridMap[point.x, point.y].parentRoom.info.Contains(new Vector2Int(endL.x, endL.y)))
            {
                newG = rightRoom;
            }
            else
            {
                newG = wrongRoom;
            }
            
        }
        else if(dungGenerator.gridMap[point.x, point.y].type == 2)
        {
            newG = border;
        }
        else if(dungGenerator.gridMap[point.x, point.y].type == 3)
        {
            newG = corridor;
        }

        return newG;
    }
}
