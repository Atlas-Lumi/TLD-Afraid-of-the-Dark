using HarmonyLib;
using Il2Cpp;

namespace AnxietyAtNight
{
    internal class Patches
    {
        [HarmonyPatch(typeof(Anxiety), "Update")]
        internal class Anxiety_Update
        {
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
            private static void Play_AfflictionAnxietyCured()
            {
                GameManager.GetPlayerVoiceComponent().Play("Play_VOCatchBreath", (Il2CppVoice.Priority)PlayerVoice.Options.None);
            }

            private static void Postfix(Anxiety __instance)
            {
                // ty Fuar
                __instance.m_AudioEvent = "";

                void AnxietyCuredPopup()
                {
                    PlayerDamageEvent.SpawnAfflictionEvent(__instance.m_AnxietyLocalizedString.m_LocalizationID, "GAMEPLAY_Healed", __instance.m_AnxietyIcon, InterfaceManager.m_FirstAidBuffColor);
                }

                if (IsNight())
                {
                    bool scaredConditionFire = GetDistanceToClosestFire() < 3f;
                    bool scaredConditionTorch = HoldingLitTorch();
                    bool scaredConditionInCar = IsInCar();
                    bool scaredConditionShelter = IsInShelter();
                    bool scaredConditionBuilding = IsInBuilding();

                    // None of the conditions are met, anxiety added
                    if (!__instance.m_HasAffliction && (!scaredConditionFire && !scaredConditionTorch && !scaredConditionInCar && !scaredConditionShelter && !scaredConditionBuilding))
                    {
                        __instance.StartAffliction();
                        __instance.m_HasAffliction = true;
                    }
                    // mmm. comfy.
                    else if (__instance.m_HasAffliction && (scaredConditionTorch || scaredConditionInCar || scaredConditionShelter || scaredConditionFire || scaredConditionBuilding))
                    {
                        __instance.StopAffliction(true);
                        AnxietyCuredPopup();
                        Play_AfflictionAnxietyCured();
                        __instance.m_HasAffliction = false;
                    }
                }
                // Day time, anxiety removed
                else if (IsDay() && __instance.m_HasAffliction)
                {
                    __instance.StopAffliction(true);
                    AnxietyCuredPopup();
                    Play_AfflictionAnxietyCured();
                    __instance.m_HasAffliction = false;
                }
            }
        }
    }
}