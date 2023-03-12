﻿using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Framework.Map;
using Framework.Managers;
using Gameplay.UI.Others.MenuLogic;

namespace BlasClient.Patches
{
    // Show other players on map screen
    [HarmonyPatch(typeof(NewMapMenuWidget), "OnShow")]
    public class MapMenuWidget_Patch
    {
        public static void Postfix()
        {
            Main.Multiplayer.mapScreenManager.createPlayerMarks(true);
        }
    }

    // Get center position of room
    [HarmonyPatch(typeof(NewMapManager), "GetCellKeyFromPosition", typeof(string), typeof(Vector2))]
    public class MapManagerRoom_Patch
    {
        public static bool Prefix(string scene, Vector2 position, MapData ___CurrentMap)
        {
            if (position.x > -999f) return true;

            if (scene != "" && scene.Length == 9)
            {
                ZoneKey zone = new ZoneKey(scene.Substring(0, 3), scene.Substring(3, 3), scene.Substring(6, 3));
                if (___CurrentMap.CellsByZone.ContainsKey(zone))
                {
                    // Loop through each cell and find average position
                    Vector2 totalPosition = Vector2.zero;
                    foreach (CellData cell in ___CurrentMap.CellsByZone[zone])
                    {
                        totalPosition += new Vector2(cell.CellKey.X, cell.CellKey.Y);
                    }

                    // Calculate average position and send it to map manager
                    int totalCells = ___CurrentMap.CellsByZone[zone].Count;
                    Vector2 averagePosition = new Vector2(totalPosition.x / totalCells, totalPosition.y / totalCells);
                    Main.Multiplayer.mapScreenManager.setActivePlayerPosition(averagePosition);
                    return false;
                }
            }

            // Scene is not a valid one on the map
            Main.Multiplayer.mapScreenManager.setActivePlayerPosition(new Vector2(-1, -1));
            Main.Multiplayer.Log("Player zone '" + scene + "' does not exist!");
            return false;
        }
    }

    // Remove confessor icons from map once beaten
    [HarmonyPatch(typeof(NewMapManager), "GetAllMarks")]
    public class MapManagerConfessor_Patch
    {
        public static void Prefix(NewMapManager __instance, MapData ___CurrentMap)
        {
            if (Core.Events.GetFlag("CONFESSOR_1_ARENACOMPLETED")) RemoveConfessorMarker(new Vector2(-100, -13));
            if (Core.Events.GetFlag("CONFESSOR_2_ARENACOMPLETED")) RemoveConfessorMarker(new Vector2(-480, 96));
            if (Core.Events.GetFlag("CONFESSOR_3_ARENACOMPLETED")) RemoveConfessorMarker(new Vector2(-660, -170));
            if (Core.Events.GetFlag("CONFESSOR_4_ARENACOMPLETED")) RemoveConfessorMarker(new Vector2(-880, -4));
            if (Core.Events.GetFlag("CONFESSOR_5_ARENACOMPLETED")) RemoveConfessorMarker(new Vector2(260, 42));
            if (Core.Events.GetFlag("CONFESSOR_6_ARENACOMPLETED")) RemoveConfessorMarker(new Vector2(280, 10));
            if (Core.Events.GetFlag("CONFESSOR_7_ARENACOMPLETED")) RemoveConfessorMarker(new Vector2(0, 120));

            void RemoveConfessorMarker(Vector2 position)
            {
                CellKey cellKey = __instance.GetCellKeyFromPosition(position);
                if (___CurrentMap.CellsByCellKey.ContainsKey(cellKey))
                {
                    ___CurrentMap.CellsByCellKey[cellKey].Type = EditorMapCellData.CellType.Normal;
                }
            }
        }
    }
}
