using HarmonyLib;
using Kitchen;
using KitchenLib.Utils;
using TMPro;
using UnityEngine;

namespace FireEvent.Patches
{
    [HarmonyPatch]
    static class LocalViewRouter_Patch
    {
        const float SCALE = 0.008f;
        const float ELEMENT_VERTICAL_OFFSET = -0.345f;

        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
        [HarmonyPostfix]
        static void GetPrefab_Postfix(ref GameObject __result, ViewType view_type)
        {
            if (view_type == ViewType.DayDisplay && !(__result?.HasComponent<FireScoreView>() ?? true))
            {
                Transform containerTransform = __result.transform.Find("GameObject (1)");
                if (containerTransform == null)
                {
                    Main.LogError("Failed to find \"GameObject (1)\" in DayDisplay.");
                    return;
                }

                GameObject speedrunTimer = containerTransform.Find("Speedrun Duration")?.gameObject;
                if (speedrunTimer == null)
                {
                    Main.LogError("Failed to find \"Speedrun Duration\" in DayDisplay/GameObject (1).");
                    return;
                }

                GameObject fireScoreContainer = new GameObject("FireScore");
                fireScoreContainer.transform.ParentTo(containerTransform);

                GameObject offsetContainer = new GameObject("Offset");
                offsetContainer.transform.ParentTo(fireScoreContainer);
                offsetContainer.transform.localPosition = new Vector3(-0.86f, -3f, -0.035f);

                FireScoreView fireScoreView = __result.AddComponent<FireScoreView>();

                GameObject scoreText = GameObject.Instantiate(speedrunTimer);
                scoreText.name = "Score";
                scoreText.transform.ParentTo(offsetContainer);
                scoreText.transform.localScale = Vector3.one * SCALE;
                if (scoreText.TryGetComponent<TextMeshPro>(out TextMeshPro scoreTMP))
                {
                    fireScoreView.Score = scoreTMP;
                }

                GameObject multiplier = GameObject.Instantiate(speedrunTimer);
                multiplier.name = "FireMultiplier";
                multiplier.transform.ParentTo(offsetContainer);
                multiplier.transform.localPosition = new Vector3(-0f, ELEMENT_VERTICAL_OFFSET, 0f);
                multiplier.transform.localScale = Vector3.one * SCALE;
                if (multiplier.TryGetComponent<TextMeshPro>(out TextMeshPro fireMultiplierTMP))
                {
                    fireScoreView.FireMultiplier = fireMultiplierTMP;
                }

                GameObject targetDay = GameObject.Instantiate(speedrunTimer);
                targetDay.name = "Target";
                targetDay.transform.ParentTo(offsetContainer);
                targetDay.transform.localPosition = new Vector3(-0f, ELEMENT_VERTICAL_OFFSET * 2, 0f);
                targetDay.transform.localScale = Vector3.one * SCALE;
                if (targetDay.TryGetComponent<TextMeshPro>(out TextMeshPro targetDayTMP))
                {
                    fireScoreView.Target = targetDayTMP;
                }

                // To refresh drawing and correct local position (?) of previous instantiated GO for some reason
                GameObject temp = GameObject.Instantiate(speedrunTimer);
                temp.transform.ParentTo(offsetContainer);
                GameObject.Destroy(temp);

                Main.LogInfo("Added FireScore GameObjects to DayDisplay.");
            }
        }
    }
}
