using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private UIDocument doc;
    private VisualElement root;

    private Button playButton;
    private Button controlButton;
    private Button quitButton;

    // basically reused from first project
    void Start()
    {
        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;
        
        playButton = root.Q<Button>("PickLevel");
        controlButton = root.Q<Button>("Controls");
        quitButton = root.Q<Button>("Quit");

        playButton.RegisterCallback<ClickEvent>(_ => PlayButtonCallback());
        controlButton.RegisterCallback<ClickEvent>( _ => ControlButtonCallback());
        quitButton.RegisterCallback<ClickEvent>(_ => QuitButtonCallback());

    }

    void PlayButtonCallback()
    {
        SceneManager.LoadScene("PickLevel");
    }

    void ControlButtonCallback()
    {
        SceneManager.LoadScene("Controls");
    }

    void QuitButtonCallback()
    {
        Application.Quit();
    }
}
