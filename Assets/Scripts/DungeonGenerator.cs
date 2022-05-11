using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField]
    private Vector2Int dungeonSize, minRoomSize, maxRoomSize;
    [SerializeField]
    bool randomizeSeed;
    [SerializeField]
    private int seed, roomCount, gapSize, attempts;
    [SerializeField]
    private GameObject zero, one, two;



    private room newRoom;
    private List<room> rooms;
    private int[,] grid;
    private GameObject spawnedTile;

    enum TileType
    {
        room,
        border,
        corridor
    }
    
    void Start()
    {
        if (randomizeSeed)
        {
            seed = (int)System.DateTime.Now.Ticks; 
        }
        Random.seed = seed;
        grid = new int[dungeonSize.x, dungeonSize.y];
        rooms = new List<room>();


        GenerateRooms();
        //Triangulate here
        //Make Create Links
        //Create Paths
        //
        DrawDungeon();


        
        //debug
        DebugListRooms();
    }

    private void GenerateRooms() 
    {
        int j = 0; //room placement attempt counter
        for (int i = 0; i < roomCount; i++)
        {

            //randomize coords, size
            Vector2Int newCoords = new Vector2Int(Random.Range(0, dungeonSize.x), Random.Range(0, dungeonSize.y));
            Vector2Int newSize = new Vector2Int(Random.Range(minRoomSize.x, maxRoomSize.x), Random.Range(minRoomSize.y, maxRoomSize.y));
            newRoom = new room(newCoords, newSize);

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
                
                spawnedTile.transform.position = new Vector3((float)x, (float)y, 0);
            }
        }
    }


    private bool RoomInGrid(room newRoom)
    {
        if (newRoom.info.min.x >= 0 &&
            newRoom.info.min.y >= 0 &&
            newRoom.info.max.x <= dungeonSize.x-1 &&
            newRoom.info.max.y <= dungeonSize.y-1)
        {
            return true;
        }else
        {
            return false;
        }
    }

    private bool RoomNotOverlapping(room newRoom)
    {
        bool isOverlap = true;
        //additional +1 because of how overlap function works.
        room newRoomWithBorders = new room (newRoom.info.min + new Vector2Int(-(gapSize+1),-(gapSize+1)), newRoom.info.size + new Vector2Int(2*(gapSize+1),2*(gapSize+1)));

        if(rooms.Count > 0)
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
            isOverlap=false;
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


    private void DebugListRooms()
    {
       int i = 0;
        foreach(var room in rooms)
        {
            i++;
            Debug.Log($"{i}. min:{room.info.min} || max:{room.info.max}");
        }
    }
}
