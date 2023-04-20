using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class ControlUIController : MonoBehaviour
{

    private Button backButton;

    void Start()
    {
        
        UIDocument doc = GetComponent<UIDocument>();

        VisualElement root = doc.rootVisualElement;

        backButton = root.Q<Button>("BackButton");
        backButton.RegisterCallback<ClickEvent>( _ => NavigateBack());
    }

    void NavigateBack()
    {

        SceneManager.LoadScene("MainMenu");
    }
}
