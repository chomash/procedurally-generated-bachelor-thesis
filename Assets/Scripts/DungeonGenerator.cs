using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using System.Linq;
using UnityEngine.Tilemaps;


public class GridLocation
{
    public int x;
    public int y;
    public byte type; //0 - empty, 1 - room, 2 - border, 3 - corridor
    public room parentRoom;

    public GridLocation()
    {
        type = 0;
    }
    public GridLocation(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
    public GridLocation(int _x, int _y, byte _type)
    {
        x = _x;
        y = _y;
        type = _type;
    }
    public GridLocation(int _x, int _y, byte _type, room parent)
    {
        x = _x;
        y = _y;
        type = _type;
        parentRoom = parent;
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }

    public static GridLocation operator +(GridLocation a, GridLocation b) => new GridLocation(a.x + b.x, a.y + b.y);

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return x == ((GridLocation)obj).x && y == ((GridLocation)obj).y;
        }
    }

    public override int GetHashCode()
    {
        return 0;
    }
}

public class DungeonGenerator : MonoBehaviour
{
    #region data
    [SerializeField] public Pathfinding_AStar AStar;

    [Header("Generation properties")]
    public Vector2Int dungeonSize;
    public Vector2Int minRoomSize, maxRoomSize;
    public int roomCount, gapSize, attempts;
    public bool shorestSpareEdges = false;
    public float percentOfSpareEdgesAdded = 0;
    public bool randomizeSeed;
    public int seed;

    [Header("Tile Properties")]
    [SerializeField] public Tilemap floorTM;
    [SerializeField] public Tilemap wallTM, decosTM;
    [SerializeField] public RuleTile floor, walls, decos;

    [Header("Triangulation Properties")]
    public bool showVertexes = true;
    public bool showTriangleEdges = true;
    public bool showPrimEdges = true;
    public Color triangleEdgeColor = new Color(50, 100, 200, 120);
    public Color primColor = Color.black;
    public Material meshMaterial;
    public Material lineMaterial;
    public float triangleEdgeWidth = 0.1f;
    public float primEdgeWidth = 0.08f;
    public GameObject trianglePointPrefab;

    private List<IPoint> points = new List<IPoint>();
    private Delaunator delaunator;
    private IEnumerable<IEdge> edges;
    private List<Edge> PrimEdges = new List<Edge>();
    private Transform PointsContainer;
    private Transform PrimContainer;
    private Transform TrianglesContainer;
    private List<room> rooms;
    private room newRoom;
    public GridLocation[,] gridMap;
    public List<GridLocation> directions = new List<GridLocation>() { new GridLocation(1,0),
                                                                      new GridLocation(-1,0),
                                                                      new GridLocation(0,1),
                                                                      new GridLocation (0,-1)};
    
    #endregion

    void Start()
    {
        if (randomizeSeed)
        {
            seed = (int)System.DateTime.Now.Ticks; 
        }
        Random.InitState(seed);
        
        gridMap = new GridLocation[dungeonSize.x, dungeonSize.y];
        rooms = new List<room>();

        GenerateRooms();
        Delaunay();
        ConnectRooms();
        Pathfinding();

        CreateNewContainers();
        DrawDungeon();

        if (showVertexes)
        {
            DrawVertexes();
        }
        if (showTriangleEdges)
        {
            DrawTriangles();
        }
        if (showPrimEdges)
        {

            DrawPrims();
        }
    }

    private void GenerateRooms() 
    {
        InitialGrid();
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
                        //0 - empty/wall, 1 - room, 2 - border, 3 - corridor
                        gridMap[x, y] = new GridLocation(x, y, 1, newRoom);
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
    private void InitialGrid()
    {
        for (int x = 0; x < dungeonSize.x; x++)
        {
            for (int y = 0; y < dungeonSize.y; y++)
            {
                gridMap[x, y] = new GridLocation(x,y,0);
            }
        }
    }
    private void ConnectRooms()
    {
        Prim prim = new Prim(edges, points);
        PrimEdges = prim.MinimumSpanningTree(percentOfSpareEdgesAdded, shorestSpareEdges);
        

    }
    private void Pathfinding()
    {
        foreach (Edge e in PrimEdges)
        {
            Vector2Int eP = Vector2Int.CeilToInt(new Vector2((float)e.P.X, (float)e.P.Y));
            Vector2Int eQ = Vector2Int.CeilToInt(new Vector2((float)e.Q.X, (float)e.Q.Y));
            bool foundP = false;
            bool foundQ = false;
            Vector2Int sLoc = Vector2Int.zero;
            Vector2Int eLoc = Vector2Int.zero;

            for (int x = 0; x < dungeonSize.x; x++)
            {
                if (foundP && foundQ)
                {
                    break;
                }
                for (int y = 0; y < dungeonSize.y; y++)
                {
                    if (gridMap[x, y].parentRoom == null) { continue; }

                    RectInt imHere = new RectInt(gridMap[x, y].parentRoom.info.min, gridMap[x, y].parentRoom.info.size + Vector2Int.one);
                    if (imHere.Contains(eP) && !foundP)
                    {
                        sLoc = Vector2Int.CeilToInt(gridMap[x, y].parentRoom.info.center);
                        foundP = true;
                    }
                    if (imHere.Contains(eQ) && !foundQ)
                    {
                        eLoc = Vector2Int.CeilToInt(gridMap[x, y].parentRoom.info.center);
                        foundQ = true;
                    }
                }
            }

            if(foundP && foundQ)
            {
                AStar.SearchForConnection(gridMap[sLoc.x, sLoc.y], gridMap[eLoc.x, eLoc.y]);
            }            
        }
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
        room newRoomWithBorders = new room(newRoom.info.min + new Vector2Int(-(gapSize+1), -(gapSize + 1)), newRoom.info.size + new Vector2Int(2 * (gapSize + 1), 2 * (gapSize + 1)));

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
            //shrinking by 1, to negate previous changes
            for (int x = newRoomWithBorders.info.x+1; x <= newRoomWithBorders.info.xMax-1; x++)
            {
                for (int y = newRoomWithBorders.info.y+1; y <= newRoomWithBorders.info.yMax-1; y++)
                {
                    if(x < 0 || x > dungeonSize.x -1 || y < 0 || y > dungeonSize.y - 1) { continue; }
                    //0 - empty/wall, 1 - room, 2 - border, 3 - corridor
                    gridMap[x, y] = new GridLocation(x, y, 2, null);
                }
            }
            return true;
        }

    }
    #endregion
    private void DrawDungeon()
    {
        wallTM.ClearAllTiles();
        floorTM.ClearAllTiles();
        decosTM.ClearAllTiles();


        for (int x = 0; x < dungeonSize.x; x++)
        {
            for (int y = 0; y < dungeonSize.y; y++)
            {
                //0 - empty/wall, 1 - room, 2 - border, 3 - corridor
                if (gridMap[x,y].type == 0)
                {

                    wallTM.SetTile(new Vector3Int(x, y, 0), walls);
                }
                else if (gridMap[x, y].type == 1)
                {
                    floorTM.SetTile(new Vector3Int(x, y, 0), floor);
                    decosTM.SetTile(new Vector3Int(x, y, 0), decos);
                }
                else if (gridMap[x, y].type == 2)
                {
                    wallTM.SetTile(new Vector3Int(x, y, 0), walls);
                }
                else if(gridMap[x, y].type == 3)
                {
                    floorTM.SetTile(new Vector3Int(x, y, 0), floor);
                    decosTM.SetTile(new Vector3Int(x, y, 0), decos);
                    
                }
            }
        }

        //added some empty tiles above, so we can't see walls at the very top
        for (int x = 0; x < dungeonSize.x; x++)
        {
            for (int y = dungeonSize.y; y < dungeonSize.y+10; y++)
            {
                wallTM.SetTile(new Vector3Int(x, y, 0), walls);
            }
        }
    }
    private void DrawVertexes()
    {
        if (delaunator == null) return;
        delaunator.ForEachTriangleEdge(edge =>
        {
            var pointgameobject = Instantiate(trianglePointPrefab, PointsContainer);
            pointgameobject.transform.SetPositionAndRotation(edge.P.ToVector3(), Quaternion.identity);
        });
    }
    private void DrawTriangles()
    {
        if (delaunator == null) return;
        delaunator.ForEachTriangleEdge(edge =>
        {
            CreateLine(TrianglesContainer, $"TriangleEdge - {edge.Index}", new Vector3[] { edge.P.ToVector3(), edge.Q.ToVector3() }, triangleEdgeColor, triangleEdgeWidth, 10);
        });
    }
    private void DrawPrims()
    {
        if (PrimEdges == null) return;
        PrimEdges.ForEach(edge =>
        {
            CreateLine(PrimContainer, $"TriangleEdge - {edge.Index}", new Vector3[] { edge.P.ToVector3(), edge.Q.ToVector3() }, primColor, primEdgeWidth, 11);
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
    #endregion
}
