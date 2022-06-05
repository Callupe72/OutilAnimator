using UnityEditor;
using UnityEngine;

public class ListOfAllAnimatorWindowCustomEditor : EditorWindow
{
    public AnimatorCustomWindow animatorCustomWindow;

    string searchString = "";

    public void RunAtStart()
    {
        animatorCustomWindow = GetWindow<AnimatorCustomWindow>();
    }

    void SeeAllAnimators()
    {
        if (GUILayout.Button("Select All"))
        {
            animatorCustomWindow.SelectAll();
        }
        if (GUILayout.Button("Unselect All"))
        {
            animatorCustomWindow.UnSelectAll();
        }
    }

    void RemoveFromSearch()
    {
        searchString = "";
        GUI.FocusControl(null);
    }

    private void OnGUI()
    {
        if (animatorCustomWindow.anims == null)
        {
            return;
        }

        if (animatorCustomWindow.anims.Count == 0)
        {
            return;
        }

        GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("V", GUILayout.Width(50)))
        {
            if (searchString != "")
            {
                foreach (var item in animatorCustomWindow.anims)
                {
                    if (item.name.ToLower().Contains(searchString))
                    {
                        animatorCustomWindow.AddInSelection(item.gameObject);
                    }
                    else
                    {
                        animatorCustomWindow.RemoveFromSelection(item.gameObject);
                    }
                }
            }
        }
        GUI.backgroundColor = Color.white;
        searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("X", GUILayout.Width(50)))
        {
            RemoveFromSearch();
        }
        GUI.backgroundColor = Color.white;


        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("Selected item", GUILayout.ExpandWidth(false), GUILayout.Width(250));

        //if (GUILayout.Button(searchString, EditorStyles.popup))
        //{
        //    SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), new StringListSearchProvider());
        //}
        GUILayout.EndHorizontal();

        SeeAllAnimators();
        if (animatorCustomWindow.anims.Count > 0)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);

            style.alignment = TextAnchor.MiddleCenter;

            for (int i = animatorCustomWindow.anims.Count - 1; i >= 0; --i)
            {
                Animator currentAnim = animatorCustomWindow.anims[i];
                bool isSelected = Selection.Contains(currentAnim.gameObject);

                GUI.backgroundColor = isSelected ? animatorCustomWindow.selectedColor : Color.white;
                style.fontStyle = isSelected ? FontStyle.Italic : FontStyle.Bold;

                EditorGUILayout.Space(10);

                ButtonSelectAnim(currentAnim, style, isSelected);
                if (isSelected)
                {
                    SeeAllAnimsOfAnimController(currentAnim);
                }
                GUI.backgroundColor = Color.white;
            }
        }




        EditorGUILayout.Space(20);
    }

    void SeeAllAnimsOfAnimController(Animator currentAnim)
    {
        if (currentAnim.runtimeAnimatorController == null)
        {
            EditorGUILayout.HelpBox("There is no controller", MessageType.Error);
            return;
        }
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();

        foreach (AnimationClip ac in currentAnim.runtimeAnimatorController.animationClips)
        {
            GUI.backgroundColor = ac == animatorCustomWindow.currentClip ? animatorCustomWindow.selectedColor : Color.white;

            if (GUILayout.Button(ac.name))
            {
                animatorCustomWindow.ChangeCurrentClip(ac);
            }
            GUI.backgroundColor = Color.white;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(20);
    }


    void ButtonSelectAnim(Animator currentAnim, GUIStyle style, bool isSelected)
    {
        if (GUILayout.Button(currentAnim.name))
        {
            if (isSelected)
            {
                animatorCustomWindow.selectedObj.Remove(currentAnim.gameObject);
            }
            else
            {
                animatorCustomWindow.selectedObj.Add(currentAnim.gameObject);
            }
            animatorCustomWindow.ActualizeSelection();
        }
    }
}
