using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameManager gm = (GameManager)target;

        GUILayout.Space(10);

        if (GUILayout.Button("LOAD ALL EMOJIS"))
        {
            LoadAllEmojis(gm);
        }

        if (GUILayout.Button("CLEAR EMOJIS"))
        {
            Undo.RecordObject(gm, "Clear Emoji Library");
            gm.masterEmojiLibrary.Clear();
            EditorUtility.SetDirty(gm);

            Debug.Log("Cleared emoji library.");
        }
    }

    void LoadAllEmojis(GameManager gm)
    {
        string[] searchFolders = { "Assets/Emojis" }; // 👈 CHANGE IF NEEDED

        string[] guids = AssetDatabase.FindAssets("t:Sprite", searchFolders);

        List<Sprite> sprites = new List<Sprite>();
        HashSet<string> seen = new HashSet<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Get sliced sprites if they exist
            Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            bool foundSub = false;

            foreach (Object obj in subAssets)
            {
                if (obj is Sprite s)
                {
                    string key = path + s.name;

                    if (!seen.Contains(key))
                    {
                        sprites.Add(s);
                        seen.Add(key);
                    }

                    foundSub = true;
                }
            }

            // If not a spritesheet, grab single sprite
            if (!foundSub)
            {
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (s != null)
                {
                    string key = path + s.name;

                    if (!seen.Contains(key))
                    {
                        sprites.Add(s);
                        seen.Add(key);
                    }
                }
            }
        }

        sprites = sprites.OrderBy(s => s.name).ToList();

        Undo.RecordObject(gm, "Load Emojis");
        gm.masterEmojiLibrary = sprites;
        EditorUtility.SetDirty(gm);

        Debug.Log($"✅ Loaded {sprites.Count} emojis into masterEmojiLibrary");
    }
}