using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct Page
{
    public string pageName;
    public List<GameObject> pageComponents;
}

/// <summary>
/// Class for UI menus
/// </summary>
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
        pages[page].pageComponents.ForEach(item => item.SetActive(true));
    }

    public void OpenPage(string page)
    {
        ResetActiveMenus();
        for (int i = 0; i < pages[i].pageComponents.Count; i++)
        {
            if (pages[i].pageName == page)
            {
                pages[i].pageComponents[i].SetActive(true);
            }            
        }
    }

    public void ResetActiveMenus()
    {
        pages.ForEach(item => item.pageComponents.ForEach(subItem => subItem.SetActive(false)));
    }

}
