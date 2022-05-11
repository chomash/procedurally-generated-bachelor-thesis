using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class room
{

    public RectInt info;

    public room (Vector2Int coords, Vector2Int size)
    {
        info = new RectInt(coords, size);
    }
   
}
