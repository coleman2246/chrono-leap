using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathObject : PlayerCollissionWrapper
{

    public override void PlayerCollision(GameObject player)
    {
        PlayerWorldInteractions playInteract = player.GetComponent<PlayerWorldInteractions>();
        playInteract.KillPlayer();
    }
}
