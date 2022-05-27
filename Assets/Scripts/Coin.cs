using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : Collectable
{
    public int amout;

    protected override void OnCollect()
    {
        if (!collected)
        {
            collected = true;
            GameManager.instance.dollars += amout;
            GameManager.instance.actualProgress++;
            Destroy(gameObject);
        }
    }
}
