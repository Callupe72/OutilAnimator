using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

//https://www.youtube.com/watch?v=0HHeIUGsuW8


public class StringListSearchProvider : ScriptableObject, ISearchWindowProvider
{
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> searchList = new List<SearchTreeEntry> ();
        searchList.Add(new SearchTreeGroupEntry(new GUIContent("List"), 0));
        foreach (Animator item in FindObjectsOfType<Animator>())
        {
            searchList.Add(new SearchTreeGroupEntry(new GUIContent(item.name)));
        }
        return searchList;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        return true;
    }
}
