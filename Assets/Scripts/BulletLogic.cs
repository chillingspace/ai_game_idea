/*******************************************************************************
File:      BulletLogic.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is added to the bullet and controls all of its behavior,
    including how to handle when different objects are hit.

*******************************************************************************/
using UnityEngine;

public enum Teams { Player, Enemy }

public class BulletLogic : MonoBehaviour
{
    public Teams Team = Teams.Player;
    public int Power = 1;

    private void OnTriggerEnter2D(Collider2D col)
    {
        // Skip friendly fire
        if (col.isTrigger || col.tag == Team.ToString())
            return;

        //Player hit
        if (col.CompareTag("Player"))
        {
            PlayerControl pc = col.GetComponent<PlayerControl>();
            pc.player_hit = true;
        }

        // Add more logic here for damaging player/enemy if needed
        Destroy(gameObject);
    }

}
