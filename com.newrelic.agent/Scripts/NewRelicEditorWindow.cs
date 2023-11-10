#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NewRelic { 

public class NewRelicEditorWindow : EditorWindow
{

    string androidAppToken = "";
    string iosAppToken = "";
    GameObject gameObject;
    NewRelicAgent newRelicAgent;

    [MenuItem("Tools/NewRelic")]
    public static void ShowWindow()
    {
        GetWindow(typeof(NewRelicEditorWindow));
    }

    private void OnGUI()
    {
        GUILayout.Label("NewRelic Configuration", EditorStyles.boldLabel);

        androidAppToken = EditorGUILayout.TextField("Android App Token", androidAppToken);
        iosAppToken = EditorGUILayout.TextField("iOS App Token", iosAppToken);

        if (GUILayout.Button("Add Token"))
        {
            AddNewRelic();
        }
    }

    private void AddNewRelic()
    {
        if (gameObject == null)
        {
            gameObject = new GameObject("NewRelic");
            newRelicAgent = gameObject.AddComponent<NewRelicAgent>();
        }

        newRelicAgent = gameObject.GetComponent<NewRelicAgent>();
        newRelicAgent.androidApplicationToken = androidAppToken;
        newRelicAgent.iOSApplicationToken = iosAppToken;
    }
}
}
#endif