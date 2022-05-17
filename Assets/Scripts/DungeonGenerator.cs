using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using System.Linq;


public class DungeonGenerator : MonoBehaviour
{
    #region data
    [SerializeField]
    private Vector2Int dungeonSize, minRoomSize, maxRoomSize;
    [SerializeField]
    bool randomizeSeed;
    [SerializeField]
    public int seed;
    [SerializeField]
    private int roomCount, gapSize, attempts;
    [SerializeField]
    private GameObject zero, one, two;
    [SerializeField] float FractionOfRestEdgesAdded = 0;



    private List<IPoint> points = new List<IPoint>();
    private Delaunator delaunator;
    private IEnumerable<IEdge> edges;
    private List<Edge> PrimEdges = new List<Edge>();

    private Transform PointsContainer;
    private Transform TileContainer;
    private Transform PrimContainer;
    private Transform TrianglesContainer;
    [SerializeField] Color triangleEdgeColor, primColor = Color.black;
    [SerializeField] Material meshMaterial;
    [SerializeField] Material lineMaterial;
    [SerializeField] float triangleEdgeWidth, primEdgeWidth = .01f;
    [SerializeField] GameObject trianglePointPrefab;
 
    private List<room> rooms;
    private room newRoom;
    private int[,] grid;
    private GameObject spawnedTile;

    enum TileType
    {
        room,
        border,
        corridor
    }
    #endregion

    void Start()
    {
        if (randomizeSeed)
        {
            seed = (int)System.DateTime.Now.Ticks; 
        }
        Random.InitState(seed);

        grid = new int[dungeonSize.x, dungeonSize.y];
        rooms = new List<room>();

        GenerateRooms();
        Delaunay();
        ConnectRooms();




        CreateNewContainers();
        DrawDungeon();
        DrawTriangles();
        DrawPrims();
        //DebugListRooms();
    }

    private void GenerateRooms() 
    {
        int j = 0; //room placement attempt counter
        for (int i = 0; i < roomCount; i++)
        {
            //randomize coords, size
            Vector2Int newCoords = new Vector2Int(Random.Range(0, dungeonSize.x), Random.Range(0, dungeonSize.y));
            Vector2Int newSize = new Vector2Int(Random.Range(minRoomSize.x, maxRoomSize.x), Random.Range(minRoomSize.y, maxRoomSize.y));
            newRoom = new room(newCoords, newSize-Vector2Int.one);

            if(RoomInGrid(newRoom) && RoomNotOverlapping(newRoom))
            {
                for (int x = newRoom.info.x; x <= newRoom.info.xMax; x++)
                {
                    for (int y = newRoom.info.y; y <= newRoom.info.yMax; y++)
                    {
                        grid[x, y] = 1;
                    }
                }
                rooms.Add(newRoom);
                j = 0;
            }
            else
            {
                if (j >= attempts) { break;}
                i--;
                j++;
                
            }           
        }
    }
    private void Delaunay()
    {
        foreach (var i in rooms)
        {
            points.Add(new Point(i.info.center.x, i.info.center.y));
        }
        delaunator = new Delaunator(points.ToArray());
        edges = delaunator.GetEdges();
    }



    private void ConnectRooms()
    {
        Prim prim = new Prim(edges, points);
        PrimEdges = prim.MinimumSpanningTree(FractionOfRestEdgesAdded);
        

    }
    #region check overlapping
    private bool RoomInGrid(room newRoom)
    {
        if (newRoom.info.min.x >= 0 &&
            newRoom.info.min.y >= 0 &&
            newRoom.info.max.x <= dungeonSize.x - 1 &&
            newRoom.info.max.y <= dungeonSize.y - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private bool RoomNotOverlapping(room newRoom)
    {
        bool isOverlap = true;
        //additional +1 because of how overlap function works.
        room newRoomWithBorders = new room(newRoom.info.min + new Vector2Int(-(gapSize + 1), -(gapSize + 1)), newRoom.info.size + new Vector2Int(2 * (gapSize + 1), 2 * (gapSize + 1)));

        if (rooms.Count > 0)
        {
            foreach (var room in rooms)
            {
                if (newRoomWithBorders.info.Overlaps(room.info))
                {
                    isOverlap = true;
                    break;
                }
                else
                {
                    isOverlap = false;
                }
            }
        }
        else
        {
            isOverlap = false;
        }



        if (isOverlap)
        {
            return false;
        }
        else
        {
            return true;
        }

    }
    #endregion
    private void DrawDungeon()
    {
        for (int x = 0; x < dungeonSize.x; x++)
        {
            for (int y = 0; y < dungeonSize.y; y++)
            {
                if(grid[x,y]== 0)
                {
                    spawnedTile = Instantiate(zero);
                }
                else if (grid[x,y]== 2)
                {
                    spawnedTile = Instantiate(two);
                }
                else
                {
                    spawnedTile = Instantiate(one);
                }

                spawnedTile.transform.parent = TileContainer;
                spawnedTile.transform.position = new Vector3((float)x, (float)y, 0);
            }
        }
    }

    private void DrawPrims()
    {
        if (PrimEdges == null) return;
        PrimEdges.ForEach(edge =>
        {
            CreateLine(PrimContainer, $"TriangleEdge - {edge.Index}", new Vector3[] { edge.P.ToVector3(), edge.Q.ToVector3() }, primColor, primEdgeWidth, 11);
        });
    }
    private void DrawTriangles()
    {
        if (delaunator == null) return;
        delaunator.ForEachTriangleEdge(edge =>
        {
            CreateLine(TrianglesContainer, $"TriangleEdge - {edge.Index}", new Vector3[] { edge.P.ToVector3(), edge.Q.ToVector3() }, triangleEdgeColor, triangleEdgeWidth, 10);
            
            //draw poins
            //var pointGameObject = Instantiate(trianglePointPrefab, PointsContainer);
            //pointGameObject.transform.SetPositionAndRotation(edge.P.ToVector3(), Quaternion.identity);

        });
    }
    private void CreateLine(Transform container, string name, Vector3[] points, Color color, float width, int order)
    {
        var lineGameObject = new GameObject(name);
        lineGameObject.transform.parent = container;
        var lineRenderer = lineGameObject.AddComponent<LineRenderer>();

        lineRenderer.SetPositions(points);

        lineRenderer.material = lineMaterial ?? new Material(Shader.Find("Standard"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.sortingOrder = order;
    }
    
    #region containers
    private void CreateNewContainers()
    {
        CreateNewPointsContainer();
        CreateNewTrianglesContainer();
        CreateNewPrimContainer();
        CreateNewTileContainer();
    }
    private void CreateNewPointsContainer()
    {
        if (PointsContainer != null)
        {
            Destroy(PointsContainer.gameObject);
        }

        PointsContainer = new GameObject(nameof(PointsContainer)).transform;
    }
    private void CreateNewTrianglesContainer()
    {
        if (TrianglesContainer != null)
        {
            Destroy(TrianglesContainer.gameObject);
        }

        TrianglesContainer = new GameObject(nameof(TrianglesContainer)).transform;
    }
    private void CreateNewPrimContainer()
    {
        if (PrimContainer != null)
        {
            Destroy(PrimContainer.gameObject);
        }

        PrimContainer = new GameObject(nameof(PrimContainer)).transform;
    }
    private void CreateNewTileContainer()
    {
        if (TileContainer != null)
        {
            Destroy(TileContainer.gameObject);
        }

        TileContainer = new GameObject(nameof(TileContainer)).transform;
    }
    #endregion


    private void DebugListRooms()
    {

        int i = 0;
        foreach (var room in rooms)
        {
            i++;
            Debug.Log($"{i}. min:{room.info.min} || max:{room.info.max}");
        }


        foreach (var edge in edges)
        {
            Debug.Log(edge.Index + ". " +edge.P + "/" + edge.Q);
        }
        
    }
}
