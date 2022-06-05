using UnityEditor;
using UnityEngine;

class PopupReadMe : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        PopupReadMePaul window = (PopupReadMePaul)EditorWindow.GetWindow(typeof(PopupReadMePaul), true, "Paul Leduc Read Me");
        window.maxSize = new Vector2(615f, 530f);
        window.minSize = window.maxSize;
    }
}

public class PopupReadMePaul : EditorWindow
{
    string[] objectives = new string[]
    {
        "Accélerer / Ralentir la vitesse de l'animation",
        "Pouvoir sampler l’animation simulée via un slider",
        "Afficher des informations supplémentaires sur l’animation sélectionnée",
        "Délais configurable entre 2 loops",
        "Les animations peuvent être selectionnées dans une autre fenêtre et une barre de recherche permet de les filtrer : Si tu tapes A et que tu valides avec le bouton vert, il te sélectionnera toutes les animations contenant un a. Désolé, l'érgonomie n'est pas au point",
        "Remettre à jour la liste quand la hierarchy est update",
        "Mettre à jour quand la scène change",
        "Pouvoir jouer des animations sur plusieurs objets en même temps",
    };

    void OnGUI()
    {
        EditorGUILayout.LabelField("Bonsoir Jérôme !", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Comme tu peux le voir, j'ai juste un petit bug qui arrive au lancement que je n'ai pas réussi à fixer, désolé d'avance \nMême si le git est en public j'ai préféré t'ajouter en collaborateur. \nTous les objectifs principaux sont fonctionnels et j'ai rajouté quelques objectifs secondaires :", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(5);
        foreach (var obj in objectives)
        {
            EditorGUILayout.LabelField("- " + obj);
        }
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Pour supprimer ce popup tu peux supprimer le script PopupReadMePaul, dans le dossier Editor");
        EditorGUILayout.LabelField("Cordialement");
        EditorGUILayout.LabelField("Bonne soirée");
        EditorGUILayout.LabelField("Paul Leduc");
    }
}
