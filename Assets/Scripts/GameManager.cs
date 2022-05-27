using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int dollars = 0;
    public int fullProgress = 0;
    public int actualProgress = 0;

    private void Awake()
    {
        if(GameManager.instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    public void OnSceneLoaded(Scene s, LoadSceneMode l)
    {
        //after loading scene do this
    }

}
