using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scene", menuName = "Data/Scene")]
public class SceneData : ScriptableObject
{
    public int BuildIndex;
    public string MapName;
    public Sprite LoadingBG;

    public static SceneData getSceneByIndex(int id)
    {
        SceneData[] objects = Resources.LoadAll<SceneData>("Scenes/");
        foreach (SceneData g in objects)
        {
            if (g.BuildIndex == id)
            {
                return g;
            }
        }
        return null;
    }

    public static SceneData getSceneByName(string name)
    {
        SceneData[] objects = Resources.LoadAll<SceneData>("Scenes/");
        foreach (SceneData g in objects)
        {
            if (g.MapName.ToLower().Equals(name.ToLower()))
            {
                return g;
            }
        }
        return null;
    }
}
