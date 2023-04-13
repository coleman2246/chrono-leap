using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevel : PlayerCollissionWrapper
{

    public override void PlayerCollision(GameObject player)
    {

        PlayerWorldInteractions playInteract = player.GetComponent<PlayerWorldInteractions>();

        // unlock next level
        // save progress
        playInteract.LevelEnd();
    }
}
