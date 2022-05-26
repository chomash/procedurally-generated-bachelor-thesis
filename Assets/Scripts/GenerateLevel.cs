using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    public DungeonGenerator dungGenerator;
    public bool randomizeSeed;
    public int seed;
    
    public GameObject player;
    public GameObject coinObject;
    public Vector2Int coinsInRoom = new Vector2Int(2, 4);

    private int playerRoomIndex;
    private bool[] usedRoom;
    private int no_coin=0;


    void Start()
    {
        if (randomizeSeed)
        {
            seed = (int)System.DateTime.Now.Ticks;
        }
        Random.InitState(seed);
        dungGenerator.Initialize();
        
        SpawnPlayer();
        GenerateCoins();
        
    }

    void SpawnPlayer()
    {
        playerRoomIndex = Random.Range(0, dungGenerator.rooms.Count);
        Vector3 randomLocation = new Vector3(dungGenerator.rooms[playerRoomIndex].info.center.x, dungGenerator.rooms[playerRoomIndex].info.center.y, 0);
        Instantiate(player, randomLocation, Quaternion.identity);
        
    }
    void GenerateCoins()
    {
        usedRoom = new bool[dungGenerator.roomCount];
        usedRoom[playerRoomIndex] = true;

        for (int i = 0; i < dungGenerator.roomCount; i++)
        {
            if (usedRoom[i] == true) continue;
            SpawnObjectsInRoom(dungGenerator.rooms[i].info, coinObject, coinsInRoom);
            usedRoom[i] = true;
        }
    }
    void SpawnObjectsInRoom(RectInt spawnBoundries, GameObject spawnedObject, Vector2Int numberOfSpawns)
    {
        int count = Random.Range(numberOfSpawns.x, numberOfSpawns.y+1);
        for (int i = 0; i < count; i++)
        {
            Vector3 coords = new Vector3(Random.Range(spawnBoundries.min.x, spawnBoundries.max.x), Random.Range(spawnBoundries.min.y, spawnBoundries.max.y), 0);
            Instantiate(spawnedObject, coords, Quaternion.identity);
            no_coin++;
        }
    }
}
