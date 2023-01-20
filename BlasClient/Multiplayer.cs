﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Gameplay.UI;
using Framework.Managers;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Penitent;

namespace BlasClient
{
    public class Multiplayer
    {
        private Client client;
        private string playerName;

        private int frameDelay = 120;
        private int currentFrame = 0;

        public void Initialize()
        {
            LevelManager.OnLevelLoaded += onLevelLoaded;
        }
        public void Dispose()
        {
            LevelManager.OnLevelLoaded -= onLevelLoaded;
        }

        private void onLevelLoaded(Level oldLevel, Level newLevel)
        {

            createNewPlayer();
        }

        private void createNewPlayer()
        {
            // Create new player based on test playerStatus
            PlayerStatus status = getCurrentStatus();

            GameObject obj = new GameObject("Test player", typeof(SpriteRenderer), typeof(Animator));
            obj.transform.position = new Vector3(status.xPos, status.yPos, 0);

            SpriteRenderer render = obj.GetComponent<SpriteRenderer>();
            render.flipX = !status.facingDirection;
            render.sortingLayerName = Core.Logic.Penitent.SpriteRenderer.sortingLayerName;
            render.sprite = Core.Logic.Penitent.SpriteRenderer.sprite;

            Animator anim = obj.GetComponent<Animator>();
            anim.runtimeAnimatorController = Core.Logic.Penitent.Animator.runtimeAnimatorController;
            if (status.animation != null)
            {
                anim.Play(status.animation, 0);
            }
        }

        public void update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                Connect();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                createNewPlayer();
            }

            if (client != null && client.connected)
            {
                currentFrame++;
                if (currentFrame > frameDelay)
                {
                    // Send player status
                    Main.UnityLog("Sending player status");
                    client.sendPlayerUpdate(getCurrentStatus());
                    currentFrame = 0;
                }
            }
        }

        private PlayerStatus getCurrentStatus()
        {
            PlayerStatus status = new PlayerStatus();
            status.name = playerName;

            Penitent penitent = Core.Logic.Penitent;
            if (penitent != null)
            {
                status.xPos = penitent.transform.position.x;
                status.yPos = penitent.transform.position.y;
                status.facingDirection = penitent.GetOrientation() == Framework.FrameworkCore.EntityOrientation.Right ? true : false;

                Animator anim = penitent.Animator;
                //anim.GetCurrentAnimatorStateInfo(0).hash
                int length = anim.GetCurrentAnimatorClipInfo(0).Length;
                for (int i = 0; i < length; i++)
                {
                    status.animation = anim.GetCurrentAnimatorClipInfo(0)[i].clip.name;
                }
            }
            if (Core.LevelManager.currentLevel != null && Core.LevelManager.currentLevel.LevelName != "MainMenu")
            {
                status.sceneName = Core.LevelManager.currentLevel.LevelName;
            }
            return status;
        }

        public void Connect()
        {
            playerName = "Test";
            client = new Client("localhost");
            client.Connect();
        }

        public void displayNotification(string message)
        {
            UIController.instance.ShowPopUp(message, "", 0, false);
        }
    }
}
