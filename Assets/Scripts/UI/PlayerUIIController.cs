using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUIIController : MonoBehaviour
{
    PlayerTimeAbilities timeAbilities;
    PlayerWorldInteractions worldInteractions;

    private UIDocument doc;
    private VisualElement root;

    private ProgressBar timeCharges;
    private ProgressBar objectsFrozen;

    private Label chargesLabel;
    private Label frozenLabel;

    void Start()
    {
        timeAbilities = GetComponentInParent<PlayerTimeAbilities>();
        worldInteractions = GetComponentInParent<PlayerWorldInteractions>();

        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;



        timeCharges = root.Q<ProgressBar>("TimeCharges");
        objectsFrozen = root.Q<ProgressBar>("ObjectsFrozen");

        chargesLabel = root.Q<Label>("ChargesLabel");
        frozenLabel = root.Q<Label>("FrozenLabel");
        
        InitUI();

    }


    void InitUI()
    {
        timeCharges.highValue = timeAbilities.maxTimeStopCharges;
        objectsFrozen.highValue = timeAbilities.maxStoppedObjects;
    }

    void Update()
    {
        timeCharges.value = timeAbilities.maxTimeStopCharges - timeAbilities.remainingTimeStopCharges;
        objectsFrozen.value = timeAbilities.numberOfStoppedObjects;
        
        chargesLabel.text = $"Charges : {timeAbilities.remainingTimeStopCharges}";
        frozenLabel.text = $"Enemies Frozen : {timeAbilities.numberOfStoppedObjects}";
        
    }

}
