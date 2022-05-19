using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//my implementation of https://www.youtube.com/watch?v=z1L3rMzG1_A


public class Prim
{
    private List<IEdge> allEdges;
    private List<PVertex> allVertexes;
    private List<PVertex> avaliableVertexes;

    private List<PVertex> adjacentVertexes;
    private List<Edge> newEdges = new List<Edge>();
    private List<Edge> unusedEdges = new List<Edge>();

    private bool shortestEdges;

    public Prim(IEnumerable<IEdge> inEdges, List<IPoint> inVertexes)
    {
        allEdges = inEdges.ToList();
        allVertexes = new List<PVertex>();
        foreach (var item in inVertexes)
        {
            allVertexes.Add(new PVertex(item.ToVector3()));
        }
    }

    public List<Edge> MinimumSpanningTree(float additionalEdges, bool shortestEdges)
    {
        this.shortestEdges = shortestEdges;
        allVertexes[Random.Range(0, allVertexes.Count)].key = 0f;//randomize starting point
        avaliableVertexes = new List<PVertex>(allVertexes);
        while (avaliableVertexes.Count > 0)
        {
            int minValueI = MinValueIndex(avaliableVertexes);
            PVertex tempVertex = avaliableVertexes[minValueI];
            avaliableVertexes.Remove(avaliableVertexes[minValueI]);

            if (tempVertex.parentIndex >= 0)
            {

                Point x = new Point(tempVertex.position.x, tempVertex.position.y);
                Point y = new Point(allVertexes[tempVertex.parentIndex].position.x, allVertexes[tempVertex.parentIndex].position.y);
                int i = newEdges.Count;
                Edge tempEdge = new Edge(i, x, y);
                newEdges.Add(tempEdge);

            }

            foreach (var adjVertex in Adjacent(tempVertex))
            {
                float distance = Vector3.Distance(adjVertex.position, tempVertex.position);
                int avaVerIndex = GetIndex(avaliableVertexes, adjVertex);
                if (avaVerIndex >= 0 && distance < avaliableVertexes[avaVerIndex].key)
                {
                    avaliableVertexes[avaVerIndex].key = distance;
                    avaliableVertexes[avaVerIndex].parentIndex = GetIndex(allVertexes, tempVertex);
                }
            }

        }


        if (additionalEdges > 0)
        {
            unusedEdges = new List<Edge>(Difference());
            int no_Add = Mathf.RoundToInt(additionalEdges * unusedEdges.Count());
            for (int i = 0; i < no_Add; i++)
            {
                if (shortestEdges)
                {
                    newEdges.Add(unusedEdges[0]);
                    unusedEdges.Remove(unusedEdges[0]);
                }
                else
                {
                    int rng = Random.Range(0, unusedEdges.Count());
                    newEdges.Add(unusedEdges[rng]);
                    unusedEdges.Remove(unusedEdges[rng]);
                }

            }
        }
        return newEdges;
    }


    private int MinValueIndex(List<PVertex> array)
    {
        int Index = 0;
        if (array.Count <= 1)
        {
            return Index;
        }
        for (int i = 0; i < array.Count; i++)
        {
            if (array[i].key < array[Index].key)
            {
                Index = i;
            }
        }
        return Index;
    }

    private List<PVertex> Adjacent(PVertex testVertex)
    {
        if (adjacentVertexes == null)
        {
            adjacentVertexes = new List<PVertex>();
        }
        else
        {
            adjacentVertexes.Clear();
        }

        for (int i = 0; i < allEdges.Count; i++)
        {
            if (allEdges[i].P.ToVector3() == testVertex.position)
            {
                adjacentVertexes.Add(new PVertex(allEdges[i].Q.ToVector3()));
            }
            else if (allEdges[i].Q.ToVector3() == testVertex.position)
            {
                adjacentVertexes.Add(new PVertex(allEdges[i].P.ToVector3()));
            }
        }
        return adjacentVertexes;
    }
    private int GetIndex(List<PVertex> list, PVertex vertex)
    {
        int index = 0;
        foreach (PVertex p in list)
        {
            if (p.position == vertex.position/* &&
                p.parentIndex == vertex.parentIndex &&
                p.key == vertex.key*/)
            {
                return index;
            }
            index++;
        }
        return -1;

    }
    public List<Edge> Difference()
    {
        List<unusedEdge> unsorted = new List<unusedEdge>();
        List<Edge> list = new List<Edge>();
        int n = 0;
        bool canAdd = false;
        for (int i = 0; i < allEdges.Count; i++)
        {
            for (int j = 0; j < newEdges.Count; j++)
            {
                
                if (allEdges[i].P.ToVector3() == newEdges[j].P.ToVector3() &&
                    allEdges[i].Q.ToVector3() == newEdges[j].Q.ToVector3() ||
                    allEdges[i].P.ToVector3() == newEdges[j].Q.ToVector3() &&
                    allEdges[i].Q.ToVector3() == newEdges[j].P.ToVector3() )
                {
                    canAdd = false;
                    break;
                }
                canAdd = true;
            }
            if (canAdd)
            {
                unsorted.Add(new unusedEdge(new Edge(n, allEdges[i].P, allEdges[i].Q)));
                n++;
            }

        }

        if (shortestEdges)
        {
            var sorted = unsorted.OrderBy(d => d.distance);
            foreach (var sortedEdge in sorted)
            {
                list.Add(sortedEdge.edge);
            }
        }
        else
        {
            foreach (var unsortedEdge in unsorted)
            {
                list.Add(unsortedEdge.edge);
            }
        }

          

        return list;
    }

}

public class unusedEdge
{
    public Edge edge;
    public float distance;
    public int index;
    public unusedEdge(Edge edge)
    {
        this.edge = edge;
        distance = Vector3.Distance(edge.P.ToVector3(), edge.Q.ToVector3());
    }

}

public class PVertex
{
    public float key;
    public int parentIndex;
    public Vector3 position;
    public PVertex(Vector3 pos)
    {
        key = float.PositiveInfinity;
        parentIndex = -1;
        position = pos;
    }
}
