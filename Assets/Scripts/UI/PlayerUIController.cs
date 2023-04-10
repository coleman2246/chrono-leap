using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset pauseAsset;
    [SerializeField] private VisualTreeAsset playerAsset;

    PlayerTimeAbilities timeAbilities;
    PlayerWorldInteractions worldInteractions;

    private UIDocument doc;

    private ProgressBar playerTimeCharges;
    private ProgressBar playerObjectsFrozen;
    private Label playerChargesLabel;
    private Label playerFrozenLabel;


    private Button pauseResumeButton;
    private Button pauseRestartButton;
    private Button pauseQuitButton;


    private bool isPaused;


    void Start()
    {
        isPaused = false;

        timeAbilities = GetComponentInParent<PlayerTimeAbilities>();
        worldInteractions = GetComponentInParent<PlayerWorldInteractions>();

        doc = GetComponent<UIDocument>();

        doc.visualTreeAsset = playerAsset;

        InitPlayerUI();

    }

    void InitPauseUI()
    {
        VisualElement root = doc.rootVisualElement;

        pauseResumeButton = root.Q<Button>("ResumeButton");
        pauseRestartButton = root.Q<Button>("RestartButton");
        pauseQuitButton = root.Q<Button>("QuitButton");

        pauseResumeButton.RegisterCallback<ClickEvent>( _ => PauseButtonCallback());
        pauseRestartButton.RegisterCallback<ClickEvent>( _ => RestartButtonCallback());
        pauseQuitButton.RegisterCallback<ClickEvent>( _ => QuitButtonCallback());

    }

    void PauseButtonCallback()
    {
        if(worldInteractions.isPaused)
        {
            worldInteractions.UnPause();
            isPaused = false;
        }
    }

    void RestartButtonCallback()
    {
        worldInteractions.RestartLevel();
        isPaused = false;
    }

    void QuitButtonCallback()
    {

        worldInteractions.Quit();
        isPaused = false;
    }

    void InitPlayerUI()
    {
        VisualElement root = doc.rootVisualElement;

        playerTimeCharges = root.Q<ProgressBar>("TimeCharges");
        playerObjectsFrozen = root.Q<ProgressBar>("ObjectsFrozen");

        playerChargesLabel = root.Q<Label>("ChargesLabel");
        playerFrozenLabel = root.Q<Label>("FrozenLabel");
        
        playerTimeCharges.highValue = timeAbilities.maxTimeStopCharges;
        playerObjectsFrozen.highValue = timeAbilities.maxStoppedObjects;
 
    }

    public void Pause()
    {
        if(!isPaused)
        {
            doc.visualTreeAsset = pauseAsset;
            InitPauseUI(); 
            isPaused = true;
        }


    }

    public void UnPause()
    {
        if(isPaused)
        {
            doc.visualTreeAsset = playerAsset;
            InitPlayerUI();
            isPaused = false;
        }


    }

    public void GameOverMenu()
    {
        Pause();
        pauseResumeButton.visible = false;
        // hide the resume button 
    }

    void PlayerUIUpdate()
    {
        playerTimeCharges.value = timeAbilities.maxTimeStopCharges - timeAbilities.remainingTimeStopCharges;
        playerObjectsFrozen.value = timeAbilities.numberOfStoppedObjects;
        
        playerChargesLabel.text = $"Charges : {timeAbilities.remainingTimeStopCharges}";
        playerFrozenLabel.text = $"Enemies Frozen : {timeAbilities.numberOfStoppedObjects}";
    }

    void PauseUIUpdate()
    {

    }

    
    void Update()
    {
        if(isPaused)
        {
            PauseUIUpdate();
        }
        else
        {
            PlayerUIUpdate();
        }
                
    }

}
