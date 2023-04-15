using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset pauseAsset;
    [SerializeField] private VisualTreeAsset playerAsset;
    [SerializeField] private float interactTimeout = 0.1f;

    PlayerTimeAbilities timeAbilities;
    PlayerWorldInteractions worldInteractions;

    private UIDocument doc;

    private ProgressBar playerTimeCharges;
    private ProgressBar playerObjectsFrozen;
    private Label playerChargesLabel;
    private Label playerFrozenLabel;
    private Label interactMessageLabel;


    private Button pauseResumeButton;
    private Button pauseRestartButton;
    private Button pauseQuitButton;
    private Label pauseMessage;


    private bool isPaused;


    private float lastInteractMessageTime;
    private string interactMessage;

    void Start()
    {
        isPaused = false;

        timeAbilities = GetComponentInParent<PlayerTimeAbilities>();
        worldInteractions = GetComponentInParent<PlayerWorldInteractions>();

        doc = GetComponent<UIDocument>();

        doc.visualTreeAsset = playerAsset;

        InitPlayerUI();

    }

    public void SetInteractMessage(string newMsg)
    {
        interactMessage = newMsg;
        lastInteractMessageTime = Time.time;
    }

    void InitPauseUI()
    {
        VisualElement root = doc.rootVisualElement;

        pauseResumeButton = root.Q<Button>("ResumeButton");
        pauseRestartButton = root.Q<Button>("RestartButton");
        pauseQuitButton = root.Q<Button>("QuitButton");
        pauseMessage = root.Q<Label>("PauseMessage");

        pauseMessage.visible = false;

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
        interactMessageLabel = root.Q<Label>("InteractionLabel");
        
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

    public void GameOverMenu(string message)
    {
        Pause();
        pauseResumeButton.visible = false;

        if(message != "")
        {
            pauseMessage.visible = true;
            pauseMessage.text = message;

        }
        // hide the resume button 
    }

    void PlayerUIUpdate()
    {
        playerTimeCharges.value = timeAbilities.maxTimeStopCharges - timeAbilities.remainingTimeStopCharges;
        playerObjectsFrozen.value = timeAbilities.numberOfStoppedObjects;
        
        playerChargesLabel.text = $"Charges : {timeAbilities.remainingTimeStopCharges}";
        playerFrozenLabel.text = $"Enemies Frozen : {timeAbilities.numberOfStoppedObjects}";



        if(Time.time - lastInteractMessageTime < interactTimeout)
        {


            interactMessageLabel.visible = true;
            interactMessageLabel.text = interactMessage;
        }
        else
        {
            interactMessageLabel.visible = false;
            interactMessageLabel.text = "";
        }

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
