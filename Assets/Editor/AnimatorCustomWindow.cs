using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


[ExecuteInEditMode]
public class AnimatorCustomWindow : EditorWindow
{
    ListOfAllAnimatorWindowCustomEditor listWindow;

    //Setup
    public Color selectedColor = Color.blue;
    private float _lastEditorTime = 0f;
    bool autoRefresh = false;

    //Create animator
    string animatorName = "New Animator";
    public RuntimeAnimatorController newController;


    public List<GameObject> selectedObj = new List<GameObject>();
    bool searchWithFindObjectOfType = true;
    List<string> textureString = new List<string>();

    public List<Animator> anims = new List<Animator>();

    public AnimationClip currentClip = null;
    bool animationCanLoop;
    bool animationIsPlaying;

    float animSpeed = 1;
    float animTime;
    float waitTimeBetween2Loop = 0;
    bool isWaitingForDelayUpdate;

    //Add menu named "My Window" to the Window menu
    [MenuItem("Paul/Animator Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        AnimatorCustomWindow window = (AnimatorCustomWindow)EditorWindow.GetWindow(typeof(AnimatorCustomWindow));
        window.Show();
    }
    public void RunAtStart()
    {
        DestroyListWindow();
        RefreshAnimatorInScene();
    }

    void DestroyListWindow()
    {
        if (listWindow)
        {
            listWindow.Close();
        }
        else
        {
            if (GetWindow<ListOfAllAnimatorWindowCustomEditor>())
            {
                GetWindow<ListOfAllAnimatorWindowCustomEditor>().Close();
            }
        }
    }

    async void DelayUseAsync()
    {
        isWaitingForDelayUpdate = true;
        await Task.Delay(Mathf.RoundToInt(waitTimeBetween2Loop * 1000));
        isWaitingForDelayUpdate = false;
        if (animationIsPlaying && AnimationMode.InAnimationMode())
        {
            StartAnimation();
        }
    }

    void CreateButtonsList()
    {
        textureString.Clear();
        textureString.Add("Play");
        textureString.Add("Pause");
        textureString.Add("Loop");
        textureString.Add("Stop");
    }

    void RefreshAnimatorInScene()
    {
        anims.Clear();
        if (searchWithFindObjectOfType)
        {
            foreach (var item in FindObjectsOfType<Animator>())
            {
                anims.Add(item);
            }
        }
        else
        {
            foreach (var item in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Transform child in item.transform)
                {
                    if (child.GetComponent<Animator>())
                    {
                        anims.Add(child.GetComponent<Animator>());
                    }
                }
                if (item.GetComponent<Animator>())
                {
                    anims.Add(item.GetComponent<Animator>());
                }
            }
        }
    }

    void OpenNewWindowForList()
    {
        GUI.backgroundColor = listWindow != null ? selectedColor : Color.white;
        if (GUILayout.Button("See list in another window"))
        {
            if (listWindow == null)
            {
                listWindow = GetWindow<ListOfAllAnimatorWindowCustomEditor>();
                listWindow.animatorCustomWindow = this;
                listWindow.Show();
                RefreshAnimatorInScene();
            }
            else
            {
                listWindow.Close();
                listWindow = null;
            }
        }
        GUI.backgroundColor = Color.white;
    }

    void OnGUI()
    {
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("This window does not work in play mode", MessageType.Error);
            return;
        }

        //GUILayout.BeginScrollView(Vector2.zero);

        CreateCategory("Setup", true);
        selectedColor = EditorGUILayout.ColorField(selectedColor);

        GUI.backgroundColor = autoRefresh ? selectedColor : Color.white;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Auto Refresh"))
        {
            autoRefresh = !autoRefresh;
        }

        GUI.backgroundColor = Color.white;

        if (!autoRefresh)
        {
            if (GUILayout.Button("Refresh"))
            {
                RefreshAnimatorInScene();
            }
        }
        else
        {
            RefreshAnimatorInScene();
        }

        EditorGUILayout.EndHorizontal();

        OpenNewWindowForList();

        searchWithFindObjectOfType = EditorGUILayout.Toggle("Search with FindObjectOfType", searchWithFindObjectOfType);

        EditorGUILayout.EndVertical();

        GUIStyle style = new GUIStyle(GUI.skin.button);

        style.alignment = TextAnchor.MiddleCenter;


        if (anims.Count > 0 && listWindow == null)
        {
            CreateCategory("Select All animations", false);
            SelectAllButton();
            UnSelectAllButton();
            EditorGUILayout.EndHorizontal();

            for (int i = anims.Count - 1; i >= 0; --i)
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
        }

        PlayAnimation();
        CreateNewAnimator();
    }

    #region Select
    void SelectAllButton()
    {
        if (GUILayout.Button("Select All"))
        {
            SelectAll();
        }
    }
    void UnSelectAllButton()
    {
        if (GUILayout.Button("Unselect All"))
        {
            UnSelectAll();
        }
    }

    public void SelectAll()
    {
        selectedObj.Clear();
        foreach (var item in anims)
        {
            selectedObj.Add(item.gameObject);
        }
        ActualizeSelection();
    }

    public void UnSelectAll()
    {
        selectedObj.Clear();
        ActualizeSelection();
        ChangeCurrentClip(null);
    }
    #endregion

    void FakeUpdate()
    {
        if (currentClip == null)
            return;

        if (isWaitingForDelayUpdate)
            return;

        if (animationIsPlaying)
        {
            animTime = (Time.realtimeSinceStartup - _lastEditorTime) * animSpeed;
            if (animTime >= currentClip.length)
            {
                if (!animationCanLoop)
                {
                    StopAnimation();
                }
                else
                {
                    _lastEditorTime = Time.realtimeSinceStartup;
                    if (waitTimeBetween2Loop > 0)
                    {
                        DelayUseAsync();
                    }
                }
            }
            else
            {
                if (AnimationMode.InAnimationMode())
                {
                    foreach (var go in Selection.gameObjects)
                    {
                        if (go.GetComponent<Animator>())
                        {
                            AnimationMode.SampleAnimationClip(go, currentClip, animTime);
                        }
                    }
                }
            }
        }

        Repaint();
    }

    #region Enable/Disable Scene

    void OnEnable()
    {
        CreateButtonsList();
        EditorApplication.playModeStateChanged += _OnPlayModeStateChange;
        EditorSceneManager.sceneOpened += _ChangeScene;
        EditorApplication.hierarchyChanged += _ChangedHierarchy;
    }
    void OnDisable()
    {
        EditorApplication.playModeStateChanged -= _OnPlayModeStateChange;
        EditorSceneManager.sceneOpened -= _ChangeScene;
        EditorApplication.hierarchyChanged -= _ChangedHierarchy;
        StopAnimation();
    }

    void _ChangedHierarchy()
    {
        RefreshAnimatorInScene();
    }

    #endregion

    void _ChangeScene(Scene currentScene, OpenSceneMode sceneMod)
    {
        currentClip = null;
        StopAnimation();
        RefreshAnimatorInScene();
        if (listWindow == null)
        {
            GetListWindow();
            if (listWindow != null)
            {
                listWindow.Close();
            }
        }
    }


    void GetListWindow()
    {
        listWindow = GetWindow<ListOfAllAnimatorWindowCustomEditor>();
    }

    void _OnPlayModeStateChange(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            StopAnimation();
        }
    }

    void StartAnimation()
    {
        AnimationMode.StartAnimationMode();
        EditorApplication.update -= FakeUpdate;
        EditorApplication.update += FakeUpdate;
        _lastEditorTime = Time.realtimeSinceStartup;
        isWaitingForDelayUpdate = false;
        animationIsPlaying = true;
    }

    void StopAnimation()
    {
        animationIsPlaying = false;
        AnimationMode.StopAnimationMode();
        EditorApplication.update -= FakeUpdate;
    }

    public void ActualizeSelection()
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
        EditorGUILayout.Space(20);
        CreateCategory("Create new Animator", true);
        animatorName = EditorGUILayout.TextField("Object Name : ", animatorName);
        newController = EditorGUILayout.ObjectField("Object Controller : ", newController, typeof(RuntimeAnimatorController), true) as RuntimeAnimatorController;
        if (GUILayout.Button("Add Animator"))
        {
            GameObject animatorGO = new GameObject();
            Animator anim = animatorGO.AddComponent<Animator>();
            anim.runtimeAnimatorController = newController;
            animatorGO.name = animatorName;
        }

        EditorGUILayout.EndVertical();

    }

    void CreateCategory(string titleName, bool isVertical)
    {
        EditorGUILayout.Space(10);
        if (isVertical)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        }
        else
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        }
        EditorGUILayout.LabelField(titleName, EditorStyles.boldLabel);
        EditorGUILayout.Space(7);
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
        if (Selection.gameObjects.Length > 0 && selectedObj.Count > 0)
        {
            if (currentClip != null)
            {
                AnimationMode.StartAnimationMode();
                CreateCategory("Play Animations", true);
                CreateAnimationButtons();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginVertical();
                animSpeed = EditorGUILayout.Slider("Anim Speed", animSpeed, 0f, 10f);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                animTime = EditorGUILayout.Slider("Anim Time", animTime, 0, currentClip.length);
                string clipName = currentClip != null ? currentClip.name : "";
                EditorGUILayout.LabelField(" / " + (currentClip.length).ToString("F3") + " | " + Mathf.RoundToInt((animTime / currentClip.length) * 100) + " % " + " | " + clipName);
                if (EditorGUI.EndChangeCheck())
                {
                    StopAnimation();
                    foreach (var go in Selection.gameObjects)
                    {
                        if (go.GetComponent<Animator>())
                        {
                            AnimationMode.StartAnimationMode();
                            AnimationMode.SampleAnimationClip(go, currentClip, animTime);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (!animationIsPlaying)
                {
                    waitTimeBetween2Loop = EditorGUILayout.Slider("Time between 2 loop", waitTimeBetween2Loop, 0f, 5f);
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Select an animation to use it", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Select a game Object with animator to use it", MessageType.Info);
        }
    }

    public void AddInSelection(GameObject objectToAdd)
    {
        if (!selectedObj.Contains(objectToAdd))
        {
            selectedObj.Add(objectToAdd);
            ActualizeSelection();
        }
    }
    public void RemoveFromSelection(GameObject objectToAdd)
    {
        if (selectedObj.Contains(objectToAdd))
        {
            selectedObj.Remove(objectToAdd);
            ActualizeSelection();
        }
    }

    void CreateAnimationButtons()
    {
        AnimationPlayButton();
        AnimationPauseButton();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        AnimationLoopButton();
        AnimationStopButton();
    }

    void AnimationPlayButton()
    {
        Texture2D texture = EditorGUIUtility.Load("Assets/Editor/Icon/ButtonsControlAnimations/" + textureString[0] + ".png") as Texture2D;
        Rect rect = EditorGUILayout.GetControlRect();

        GUI.backgroundColor = animationIsPlaying ? selectedColor : Color.white;

        if (GUI.Button(rect, texture))
        {
            StartAnimation();
        }

        GUI.backgroundColor = Color.white;
    }
    void AnimationPauseButton()
    {
        Texture2D texture = EditorGUIUtility.Load("Assets/Editor/Icon/ButtonsControlAnimations/" + textureString[1] + ".png") as Texture2D;
        Rect rect = EditorGUILayout.GetControlRect();

        GUI.backgroundColor = !animationIsPlaying ? selectedColor : Color.white;
        if (GUI.Button(rect, texture))
        {
            StopAnimation();
        }
        GUI.backgroundColor = Color.white;
    }
    void AnimationLoopButton()
    {
        Texture2D texture = EditorGUIUtility.Load("Assets/Editor/Icon/ButtonsControlAnimations/" + textureString[2] + ".png") as Texture2D;
        Rect rect = EditorGUILayout.GetControlRect();

        GUI.backgroundColor = animationCanLoop ? selectedColor : Color.white;
        if (GUI.Button(rect, texture))
        {
            animationCanLoop = !animationCanLoop;
        }

        GUI.backgroundColor = Color.white;
    }
    void AnimationStopButton()
    {
        Texture2D texture = EditorGUIUtility.Load("Assets/Editor/Icon/ButtonsControlAnimations/" + textureString[3] + ".png") as Texture2D;
        Rect rect = EditorGUILayout.GetControlRect();

        if (GUI.Button(rect, texture))
        {
            animTime = 0;
            StopAnimation();
        }
    }

    public void ChangeCurrentClip(AnimationClip newClip)
    {
        currentClip = currentClip == newClip ? null : newClip;
    }
}

[InitializeOnLoad]
public class RunAtLaunchAtList : EditorWindow
{
    static void Launch()
    {
        Debug.Log("Launch");
        GetWindow<AnimatorCustomWindow>().RunAtStart();
    }
}
