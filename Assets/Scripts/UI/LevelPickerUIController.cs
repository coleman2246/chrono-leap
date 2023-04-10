using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


class LevelItem
{
    public bool isUnlocked;
    public string sceneName;
    public string buttonName;
    public Button button;

    public LevelItem(string buttonName, string sceneName, UIDocument doc, bool isUnlocked)
    {
        this.buttonName = buttonName; 
        this.sceneName = sceneName;
        this.isUnlocked = isUnlocked;

        VisualElement root = doc.rootVisualElement;

        this.button = root.Q<Button>(buttonName);

        Debug.Log(sceneName);

        if(isUnlocked)
        {
            this.button.RegisterCallback<ClickEvent>( _ => SwitchToLevel(this.sceneName));
        }
        else
        {
            this.button.style.unityBackgroundImageTintColor = Color.red; 
        }

    }

    
    void SwitchToLevel(string name)
    {
        SceneManager.LoadScene(name);
    }


}

public class LevelPickerUIController : MonoBehaviour
{
    private UIDocument doc;
    private VisualElement root;

    private Button playButton;
    private Button settingsButton;
    private Button quitButton;

    // this should be read from a json
    private Dictionary<string,bool> progressMap = new Dictionary<string,bool>();

    private Dictionary<string,string> levelButtonMap = new Dictionary<string,string>()
    {
        {"Tutorial", "Tutorial"},
        {"Level 1", "Level1"},
        {"Level 2", "Level2"},
        {"MainMenu", "BackButton"},
        
    };

    void Start()
    {
        
        UIDocument doc = GetComponent<UIDocument>();

        progressMap["MainMenu"] = true;

        foreach(KeyValuePair<string,string> kvp in levelButtonMap)
        {
            string levelName = kvp.Key;
            string buttonName = kvp.Value;

            LevelItem _ = new LevelItem(buttonName, levelName, doc, true);
        }
    }

    
    

    void Update()
    {
        
    }
}
