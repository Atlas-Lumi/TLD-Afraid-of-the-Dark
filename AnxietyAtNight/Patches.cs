using HarmonyLib;
using UnityEngine;
using System;

namespace AnxietyAtNight
{
    internal class Patches
    {
        [HarmonyPatch(typeof(Anxiety), "Update")]
        internal class Anxiety_Update
        {
            private static bool affliction;

            public static bool HoldingLitTorch()
            {
                return (GameManager.GetPlayerManagerComponent().m_ItemInHands && GameManager.GetPlayerManagerComponent().m_ItemInHands.m_TorchItem && GameManager.GetPlayerManagerComponent().m_ItemInHands.m_TorchItem.IsBurning());
            }
            private static bool IsNight()
            {
                return GameManager.GetTimeOfDayComponent().IsNight();
            }
            private static bool IsDay()
            {
                return GameManager.GetTimeOfDayComponent().IsDay();
            }
            private static bool IsInShelter()
            {
                return GameManager.GetSnowShelterManager().PlayerInShelter();
            }
            private static bool IsInBuilding()
            {
                if (GameManager.GetWeatherComponent().IsIndoorScene())
                {
                    return true;
                }
                else if (GameManager.GetWeatherComponent().IsIndoorEnvironment())
                {
                    return true;
                }
                else 
                { 
                    return false; 
                }
            }
            private static bool IsInCar()
            {
                return GameManager.GetPlayerInVehicle().IsInside();
            }
            private static float GetDistanceToClosestFire()
            {
                return GameManager.GetFireManagerComponent().GetDistanceToClosestFire(GameManager.GetPlayerTransform().position);
            }
            private static void Play_AfflictionAnxiety()
            {
                GameManager.GetPlayerVoiceComponent().Play("PLAY_ANXIETYAFFLICTION", Voice.Priority.Normal);
            }
            private static void Play_AfflictionAnxietyCured()
            {
                GameManager.GetPlayerVoiceComponent().Play("Play_VOCatchBreath", Voice.Priority.Normal);
            }

            private static void Prefix(Anxiety __instance)
            {
                void AnxietyCuredPopup()
                {
                    PlayerDamageEvent.SpawnAfflictionEvent(__instance.m_AnxietyLocalizedString.m_LocalizationID, "GAMEPLAY_Healed", __instance.m_AnxietyIcon, InterfaceManager.m_FirstAidBuffColor);
                }

                if (IsNight())
                {
                    bool scaredConditionFire = GetDistanceToClosestFire() > 3f;
                    bool scaredConditionTorch = HoldingLitTorch();
                    bool scaredConditionInCar = IsInCar();
                    bool scaredConditionShelter = IsInShelter();
                    bool scaredConditionBuilding = IsInBuilding();

                    // None of the conditions are met, anxiety added
                    if (!affliction && scaredConditionFire && !scaredConditionTorch && !scaredConditionInCar && !scaredConditionShelter && !scaredConditionBuilding)
                    {
                        __instance.StartAffliction();
                        Play_AfflictionAnxiety();
                        affliction = true;
                    }
                    // Close to fire, anxiety removed
                    else if (affliction && !scaredConditionFire && !scaredConditionTorch && !scaredConditionInCar)
                    {
                        __instance.StopAffliction(true);
                        Play_AfflictionAnxietyCured();
                        AnxietyCuredPopup();
                        affliction = false;
                    }
                    // Holding lit torch, anxiety removed
                    else if (affliction && scaredConditionFire && scaredConditionTorch && !scaredConditionInCar)
                    {
                        __instance.StopAffliction(true);
                        Play_AfflictionAnxietyCured();
                        AnxietyCuredPopup();
                        affliction = false;
                    }
                    // Inside a car, anxiety removed
                    else if (affliction && scaredConditionInCar)
                    {
                        __instance.StopAffliction(true);
                        Play_AfflictionAnxietyCured();
                        AnxietyCuredPopup();
                        affliction = false;
                    }
                    // Inside snow shelter, anxiety removed
                    else if (affliction && scaredConditionShelter)
                    {
                        __instance.StopAffliction(true);
                        Play_AfflictionAnxietyCured();
                        AnxietyCuredPopup();
                        affliction = false;
                    }
                    // Inside a building, shelter, or cave, anxiety removed
                    else if (affliction && scaredConditionBuilding)
                    {
                        __instance.StopAffliction(true);
                        Play_AfflictionAnxietyCured();
                        AnxietyCuredPopup();
                        affliction = false;
                    }
                }           
                // Day time, anxiety removed
                else if (IsDay()&& affliction)
                {
                    __instance.StopAffliction(true);
                    Play_AfflictionAnxietyCured();
                    AnxietyCuredPopup();
                    affliction = false;
                }              
            }
        }
    }
}