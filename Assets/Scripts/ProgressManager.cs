using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{

    public static string progressJson = "progress.json";
    public static SerializableDictionary<string, bool> progress = InitProgress();

    public static Dictionary<string,string> nextLevelMap = new Dictionary<string,string>
    {
        {"Tutorial" , "Level 1"},
        {"Level 1" , "Level 2"}
    };

    public static SerializableDictionary<string, bool> InitProgress()
    {
        if(JSONManager.verifySavePathFile(progressJson))
        {
            return JSONLoadManager<SerializableDictionary<string, bool>>.LoadFromJson(progressJson);
        }

        return new SerializableDictionary<string,bool>();
    
    }

    static public bool GetProgress(string level)
    {
        
        if(progress.ContainsKey(level))
        {
            return progress[level];
        }

        else
        {
            progress.Add(level,false);

            if(JSONManager.verifySavePathFile(progressJson))
            {
                JSONSaveManager<SerializableDictionary<string,bool>>.SaveToJson(progressJson, progress);
            }

        }

        return false;

    }
    
    static public void UnlockLevel(string level)
    {
        progress[level] = true;
        JSONSaveManager<SerializableDictionary<string,bool>>.SaveToJson(progressJson, progress);
    }

    static public void UnlockNextLevel(string currentLevel)
    {
        if(nextLevelMap.ContainsKey(currentLevel))
        {
            UnlockLevel(nextLevelMap[currentLevel]);
        }
    }
}

