using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct Page
{
    public string pageName;
    public GameObject[] pageComponents;
}

public class UIManager : MonoBehaviour {

    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

    public List<Page> pages = new List<Page>();
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        OpenPage(0);
    }

    public void OpenPage(int page)
    {
        ResetActiveMenus();
        for (int i = 0; i < pages[page].pageComponents.Length; i++)
        {
            pages[page].pageComponents[i].SetActive(true);
        }
    }

    public void OpenPage(string page)
    {
        ResetActiveMenus();
        for (int i = 0; i < pages[i].pageComponents.Length; i++)
        {
            if (pages[i].pageName == page)
            {
                pages[i].pageComponents[i].SetActive(true);
            }            
        }
    }

    public void ResetActiveMenus()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            for (int j = 0; j < pages[i].pageComponents.Length; j++)
            {
                pages[i].pageComponents[j].SetActive(false);
            }
        }
    }

}
