using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp.Unity.Extensions;
using DelaunatorSharp;
using System.Linq;


public class Prim
{
    private List<IEdge> allEdges;
    private List<PVertex> vertexes;
    private List<PVertex> dumpVertexes;

    private List<PVertex> adjacentVertexes;
    private List<Edge> newEdges = null;
    private List<Edge> spareEdges;

    public Prim(IEnumerable<IEdge> inEdges, List<IPoint> inVertexes)
    {
        allEdges = inEdges.ToList();
        vertexes = new List<PVertex>();
        foreach (var item in inVertexes)
        {
            vertexes.Add(new PVertex(item.ToVector3()));
        }
    }



    public List<Edge> MinimumSpanningTree()
    {
        vertexes[Random.Range(0, vertexes.Count)].key = 0f;
        dumpVertexes = new List<PVertex>(vertexes);
        
        while (dumpVertexes.Count > 0)
        {
            int u = MinValueIndex(dumpVertexes);
            PVertex temp = dumpVertexes[u];
            dumpVertexes.Remove(dumpVertexes[u]);

            if (temp.parentIndex >= 0)
            {
                Edge tempEdge = new Edge(newEdges.Count - 1, temp.position, vertexes[temp.parentIndex].position);
                newEdges.Add(tempEdge);
            }

            foreach (var adjVert in Adjacent(temp))
            {
                float distance = Vector3.Distance(adjVert.position, temp.position);
                int dumpIndex = GetIndex(dumpVertexes, adjVert);
                if (dumpIndex>=0 && distance < adjVert.key)
                {
                    dumpVertexes[dumpIndex].key = distance;
                    dumpVertexes[dumpIndex].parentIndex = GetIndex(vertexes, temp);
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
            else if( allEdges[i].Q.ToVector3() == testVertex.position)
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
            if(p.position == vertex.position &&
                p.parentIndex == vertex.parentIndex &&
                p.key == vertex.key)
            {
                return index;
            }
            index++;
        }
        return -1;

    }


    //private float Distance(PVertex a, PVertex b)
    //{
    //    float dist = Vector3.Distance(a.position, b.position);
    //    return dist;
    //}
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
