using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_Elements : MonoBehaviour
{
    public RectTransform progressBar;
    public Color highlight = new Color(20,255,50,255);
    public Image button, bar;
    private bool canPress;

    public void reloadScene()
    {
        Debug.Log(SceneManager.GetActiveScene().name);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void nextLevel()
    {
        if (canPress)
        {
            GameManager.instance.fullProgress = 0;
            GameManager.instance.actualProgress = 0;
            SceneManager.LoadScene("SampleScene");
        }
    }

    public void LateUpdate()
    {
        float ratio = (float)GameManager.instance.actualProgress / (float)GameManager.instance.fullProgress;
        if (ratio > 0)
        {
            progressBar.localScale = new Vector3(1, ratio, 1);
        }
        else
        {
            progressBar.localScale = new Vector3(1, 0, 1);
        }

        if (ratio > 0.7)
        {
            button.color = highlight;
            bar.color = highlight;
            canPress = true;
        }
    }
}
