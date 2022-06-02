using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class AnimatorCustomWindow : EditorWindow
{
    string animatorName = "New Animator";
    public RuntimeAnimatorController newController;
    List<GameObject> selectedObj = new List<GameObject>();
    string[] texuresString = { "Play", "Pause"};
    bool searchWithFindObjectOfType = true;
    Color selectedColor = Color.red;

    float time = 0;

    AnimationClip currentClip = null;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Paul/Animator Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        AnimatorCustomWindow window = (AnimatorCustomWindow)EditorWindow.GetWindow(typeof(AnimatorCustomWindow));
        window.Show();
    }

    void OnGUI()
    {
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("This window does not work in play mode", MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField("Setup");

        selectedColor = EditorGUILayout.ColorField(selectedColor);

        EditorGUILayout.LabelField("All animators in scene");

        searchWithFindObjectOfType = EditorGUILayout.Toggle("Search with FindObjectOfType", searchWithFindObjectOfType);

        Animator[] anims = GetAllAnimatorsInScene();

        GUIStyle style = new GUIStyle(GUI.skin.button);

        style.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.BeginHorizontal();
        SelectAll(anims);
        UnSelectAll();
        EditorGUILayout.EndHorizontal();

        for (int i = anims.Length - 1; i >= 0; --i)
        {
            Animator currentAnim = anims[i];
            bool isSelected = Selection.Contains(currentAnim.gameObject);

            GUI.backgroundColor = isSelected ? selectedColor : Color.white;
            style.fontStyle = isSelected ? FontStyle.Italic : FontStyle.Bold;

            EditorGUILayout.Space(10);

            ButtonSelectAnim(currentAnim, style, isSelected);
            if (isSelected)
            {
                SeeAllAnimsOfAnimController(currentAnim);
            }
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.Space(20);

        PlayAnimation();

        CreateNewAnimator();
    }

    #region Select
    void SelectAll(Animator[] anims)
    {
        if (GUILayout.Button("Select All"))
        {
            selectedObj.Clear();
            foreach (var item in anims)
            {
                selectedObj.Add(item.gameObject);
            }
            ActualizeSelection();
        }
    }
    void UnSelectAll()
    {
        if (GUILayout.Button("Unselect All"))
        {
            selectedObj.Clear();
            ActualizeSelection();
            ChangeCurrentClip(null);
        }
    }
    #endregion

    Animator[] GetAllAnimatorsInScene()
    {
        if (searchWithFindObjectOfType)
        {
            return FindObjectsOfType<Animator>();
        }
        else
        {
            List<Animator> animators = new List<Animator>();
            foreach (var item in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Transform child in item.transform)
                {
                    if (child.GetComponent<Animator>())
                    {
                        animators.Add(child.GetComponent<Animator>());
                    }
                }
                if (item.GetComponent<Animator>())
                {
                    animators.Add(item.GetComponent<Animator>());
                }
            }
            return animators.ToArray();
        }
    }

    void ActualizeSelection()
    {
        //Je fais ça pour ne pas que la sélection se fasse par dessus la selection par la hierarchy
        Selection.objects = selectedObj.ToArray();
    }
    void ResetSelection()
    {
        Selection.objects = null;
    }

    void CreateNewAnimator()
    {
        EditorGUILayout.HelpBox("Create Animators", MessageType.Info);
        animatorName = EditorGUILayout.TextField("Object Name: ", animatorName);
        newController = EditorGUILayout.ObjectField(newController, typeof(RuntimeAnimatorController), true) as RuntimeAnimatorController;
        if (GUILayout.Button("Add Animator"))
        {
            GameObject animatorGO = new GameObject();
            Animator anim = animatorGO.AddComponent<Animator>();
            anim.runtimeAnimatorController = newController;
            animatorGO.name = animatorName;
        }
    }
    void ButtonSelectAnim(Animator currentAnim, GUIStyle style, bool isSelected)
    {
        if (GUILayout.Button(currentAnim.name))
        {
            if (isSelected)
            {
                selectedObj.Remove(currentAnim.gameObject);
            }
            else
            {
                selectedObj.Add(currentAnim.gameObject);
            }
            ActualizeSelection();
        }
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
            GUI.backgroundColor = ac == currentClip ? selectedColor : Color.white;

            if (GUILayout.Button(ac.name))
            {
                ChangeCurrentClip(ac);
            }
            GUI.backgroundColor = Color.white;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(20);
    }

    void PlayAnimation()
    {
        if (currentClip != null)
        {
            float startTime = 0.0f;
            float stopTime = currentClip.length;
            AnimationMode.BeginSampling();
            AnimationMode.EndSampling();
            time = EditorGUILayout.Slider(time, startTime, stopTime);
            foreach (GameObject item in Selection.objects)
            {
                AnimationMode.SampleAnimationClip(item, currentClip, time);
            }

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < 2; i++)
            {
                Texture2D play = EditorGUIUtility.Load("Assets/Editor/Icon/"+ texuresString[i] + ".png") as Texture2D;
                Rect rect = EditorGUILayout.GetControlRect();
                if (GUI.Button(rect, play))
                {

                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    void ChangeCurrentClip(AnimationClip newClip)
    {
        if (currentClip == newClip)
        {
            currentClip = null;
        }
        else
        {
            currentClip = newClip;
        }
    }
}
