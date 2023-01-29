﻿using UnityEngine;
using System.Collections.Generic;
using Framework.Managers;
using Framework.FrameworkCore;
using BlasClient.Structures;
using Tools.Level.Interactables;

namespace BlasClient.Managers
{
    public class ProgressManager
    {
        // Only enabled when processing & applying the queued progress updates
        public static bool updatingProgress;

        private List<ProgressUpdate> queuedProgressUpdates = new List<ProgressUpdate>();
        private static readonly object progressLock = new object();

        public void sceneLoaded()
        {
            foreach (PersistentObject persistence in Object.FindObjectsOfType<PersistentObject>())
            {
                if (Main.Multiplayer.checkPersistentObject(persistence.GetPersistenID()))
                {
                    // Calling setPersistence() with null data means that the object has been interacted with
                    persistence.SetCurrentPersistentState(null, false, null);
                }
            }
        }

        public void updateProgress()
        {
            if (!Main.Multiplayer.inLevel)
                return;

            lock (progressLock)
            {
                updatingProgress = true;

                for (int i = 0; i < queuedProgressUpdates.Count; i++)
                {
                    applyProgress(queuedProgressUpdates[i]);
                }
                queuedProgressUpdates.Clear();

                updatingProgress = false;
            }
        }

        public void receiveProgress(string id, byte type, byte value)
        {
            lock (progressLock)
            {
                Main.UnityLog("Received new game progress: " + id);
                queuedProgressUpdates.Add(new ProgressUpdate(id, type, value));
            }
        }

        // TODO - Check for value to determine whether to remove or add
        // TODO - For stats - value will contain the current level of the stat
        private void applyProgress(ProgressUpdate progress)
        {
            switch (progress.type)
            {
                case 0:
                    Core.InventoryManager.AddRosaryBead(progress.id); return;
                case 1:
                    Core.InventoryManager.AddPrayer(progress.id); return;
                case 2:
                    Core.InventoryManager.AddRelic(progress.id); return;
                case 3:
                    Core.InventoryManager.AddSword(progress.id); return;
                case 4:
                    Core.InventoryManager.AddCollectibleItem(progress.id); return;
                case 5:
                    Core.InventoryManager.AddQuestItem(progress.id); return;
                case 6:
                    Core.Logic.Penitent.Stats.Life.Upgrade();
                    Core.Logic.Penitent.Stats.Life.SetToCurrentMax(); return;
                case 7:
                    Core.Logic.Penitent.Stats.Fervour.Upgrade();
                    Core.Logic.Penitent.Stats.Fervour.SetToCurrentMax(); return;
                case 8:
                    Core.Logic.Penitent.Stats.Strength.Upgrade(); return;
                case 9:
                    Core.Logic.Penitent.Stats.MeaCulpa.Upgrade(); return;
                case 10:
                    Core.Logic.Penitent.Stats.BeadSlots.Upgrade(); return;
                case 11:
                    Core.Logic.Penitent.Stats.Flask.Upgrade();
                    Core.Logic.Penitent.Stats.Flask.SetToCurrentMax(); return;
                case 12:
                    Core.Logic.Penitent.Stats.FlaskHealth.Upgrade(); return;
                case 13:
                    Core.SkillManager.UnlockSkill(progress.id); return;
                case 14:
                    Core.Events.SetFlag(progress.id, true, false); return;
                case 15:
                    updatePersistentObject(progress.id); return;
                case 16:
                    Core.SpawnManager.SetTeleportActive(progress.id, true); return;
                case 17:
                    Core.NewMapManager.RevealCellInPosition(new Vector2(int.Parse(progress.id), 0)); return;

                // Unlocked teleports - flags ?
                // Church donations
                default:
                    Main.UnityLog("Error: Progress type doesn't exist: " + progress.type); return;
            }
        }

        // When receiving a pers. object update, the object is immediately updated
        // Their setPersState() is also overriden to update them on scene load
        private void updatePersistentObject(string persistentId)
        {
            Main.Multiplayer.addPersistentObject(persistentId);

            PersistenceState persistence = StaticObjects.GetPersistenceState(persistentId);
            if (persistence != null && Core.LevelManager.currentLevel.LevelName == persistence.scene)
            {
                // Player just received a pers. object in the same scene - find it and set value immediately
                switch (persistence.type)
                {
                    case 0: // Prie Dieu
                        foreach (PrieDieu priedieu in Object.FindObjectsOfType<PrieDieu>())
                        {
                            if (priedieu.GetPersistenID() == persistentId)
                            {
                                // Maybe play activation animation
                                priedieu.Ligthed = true;
                                break;
                            }
                        }
                        return;
                    case 1: // Collectible item
                        foreach (CollectibleItem item in Object.FindObjectsOfType<CollectibleItem>())
                        {
                            if (item.GetPersistenID() == persistentId)
                            {
                                item.Consumed = true;
                                item.transform.GetChild(2).gameObject.SetActive(false);
                                break;
                            }
                        }
                        return;
                    case 2: // Chest
                        foreach (Chest chest in Object.FindObjectsOfType<Chest>())
                        {
                            if (chest.GetPersistenID() == persistentId)
                            {
                                chest.Consumed = true;
                                chest.transform.GetChild(2).GetComponent<Animator>().SetBool("USED", true);
                                break;
                            }
                        }
                        return;
                    case 3: // Cherub
                        foreach (CherubCaptorPersistentObject cherub in Object.FindObjectsOfType<CherubCaptorPersistentObject>())
                        {
                            if (cherub.GetPersistenID() == persistentId)
                            {
                                cherub.destroyed = true;
                                cherub.spawner.DisableCherubSpawn();
                                cherub.spawner.DestroySpawnedCherub();
                                break;
                            }
                        }
                        return;
                    // Lever
                    // Gate
                }
            }
        }
    }
}
