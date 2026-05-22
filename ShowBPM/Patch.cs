using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using ShowBPM;
using UnityEngine;

internal static class Patch
{
    private const double MinAngleRadians = 1.56905098538846;
    private const double MinTimeSeconds = 0.03;
    private const double BpmComparisonEpsilon = 0.001;

    [HarmonyPatch(typeof(scnCalibration), "Start")]
    internal static class CalibrationStartPatch
    {
        private static void Postfix()
        {
            if (Main.IsEnabled)
            {
                Main.gui.TextObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(scrUIController), "WipeToBlack")]
    internal static class UiControllerWipeToBlackPatch
    {
        private static void Postfix()
        {
            if (Main.IsEnabled)
            {
                Main.gui.TextObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(scnEditor), "ResetScene")]
    internal static class EditorResetScenePatch
    {
        private static void Postfix()
        {
            if (Main.IsEnabled)
            {
                Main.gui.TextObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(scrController), "StartLoadingScene")]
    internal static class ControllerStartLoadingScenePatch
    {
        private static void Postfix()
        {
            if (Main.IsEnabled)
            {
                Main.gui.TextObject.SetActive(false);
            }
        }
    }



    [HarmonyPatch(typeof(scrController), "Awake")]
    internal static class ControllerAwakePatch
    {
        public static void Prefix()
        {
            if (!Main.IsEnabled) return;

            Main.language = Main.languages.TryGetValue(RDString.language.ToString(), out var lang)
                ? lang
                : Main.languages["English"];

            if (Main.gui == null) return;

            Main.gui.text.font = RDString.GetFontDataForLanguage(RDString.language).font;
            Main.gui.SyncKpsStyle();
        }
    }

    [HarmonyPatch(typeof(scnGame), "Play")]
    internal static class GamePlayPatch
    {
        private static void Postfix(scnGame __instance)
        {
            if (Main.IsEnabled && scrController.instance.gameworld && scnGame.instance != null)
            {
                LevelStart(scrController.instance);
            }
        }
    }

    [HarmonyPatch(typeof(scrPressToStart), "ShowText")]
    internal static class BossLevelStartPatch
    {
        private static void Postfix(scrPressToStart __instance)
        {
            if (Main.IsEnabled && scrController.instance.gameworld && scnGame.instance == null)
            {
                LevelStart(scrController.instance);
            }
        }
    }

    public static CallRateTracker callRateTracker = new CallRateTracker();

    [HarmonyPatch(typeof(scrPlanet), "MoveToNextFloor")]
    internal static class MoveToNextFloorPatch
    {
        public static void Postfix(scrPlanet __instance, scrFloor floor)
        {
            if (!Main.IsEnabled || !__instance.controller.gameworld || floor == null || floor.nextfloor == null)
                return;

            var displayItems = new List<(int order, int index, string text)>();
            int insertIndex = 0;

            double planetSpeed = __instance.controller.planetarySystem.speed;
            double curFloorBpm = GetRealBpm(floor, bpm) * playbackSpeed * pitch;
            double nextFloorBpm = GetRealBpm(floor.nextfloor, bpm) * playbackSpeed * pitch;
            bool isSpeedTransition = DetermineIfSpeedTransition(__instance, floor, nextFloorBpm, curFloorBpm);

            if (isSpeedTransition || isPrevSpeedTransition)
            {
                curFloorBpm = prevFloorBpm;
            }

            if (Main.setting.onTileBpm)
            {
                displayItems.Add((Main.setting.tileBpmOrder, insertIndex++, Main.setting.text1.Replace("{value}", FormatValue((float)(bpm * planetSpeed)))));
            }

            if (Main.setting.onCurBpm)
            {
                displayItems.Add((Main.setting.realBpmOrder, insertIndex++, Main.setting.text2.Replace("{value}", FormatValue((float)curFloorBpm))));
            }

            if (Main.setting.onRecommandKPS)
            {
                double kps = curFloorBpm / 60.0;
                displayItems.Add((Main.setting.kpsOrder, insertIndex++, Main.setting.text3.Replace("{value}", Math.Round(kps).ToString())));
            }

            if (Main.setting.onNextBpm)
            {
                var nextChange = GetNextSpeedChangeFloor(floor);
                if (nextChange != null)
                {
                    double nextBpm = nextChange.speed * bpm * playbackSpeed * pitch;
                    displayItems.Add((Main.setting.nextBpmOrder, insertIndex++, Main.setting.text5.Replace("{value}", FormatValue((float)nextBpm))));
                }
            }

            displayItems.Sort((a, b) =>
            {
                int cmp = a.order.CompareTo(b.order);
                return cmp != 0 ? cmp : a.index.CompareTo(b.index);
            });

            if (scnGame.instance.levelData.angleData[scrController.instance.currentSeqID] != 999)
            {
                callRateTracker.TrackCall();
            }

            Main.gui.SetText(string.Join("\n", displayItems.ConvertAll(x => x.text)));
            isPrevSpeedTransition = isSpeedTransition;
            prevFloorBpm = curFloorBpm;
        }

        private static bool DetermineIfSpeedTransition(scrPlanet planet, scrFloor floor, double nextFloorBpm, double curFloorBpm)
        {
            if (!Main.setting.ignoreMultipress)
                return false;

            double angleMoved = scrMisc.GetAngleMoved(floor.entryangle, floor.exitangle, !floor.isCCW);
            double sectionTime = scrMisc.AngleToTime(angleMoved, planet.conductor.bpm * planet.controller.planetarySystem.speed);
            bool isWideAngle = angleMoved > MinAngleRadians;
            double frameTimeBuffer = planet.controller.averageFrameTime * 2.5;
            bool isLongEnough = isWideAngle && sectionTime > frameTimeBuffer;
            bool isNotMultiPress = !(isLongEnough && sectionTime > MinTimeSeconds);

            return isNotMultiPress && !DoubleEqual(nextFloorBpm, curFloorBpm);
        }
    }

    [HarmonyPatch(typeof(RDString), "ChangeLanguage")]
    internal static class ChangeLanguagePatch
    {
        public static void Prefix(SystemLanguage language)
        {
            Main.language = Main.languages.TryGetValue(language.ToString(), out var lang)
                ? lang
                : Main.languages["English"];

            if (Main.gui == null) return;

            Main.gui.text.font = RDString.GetFontDataForLanguage(language).font;
            Main.gui.SyncKpsStyle();
        }
    }

    [HarmonyPatch(typeof(scnEditor), "DrawFloorNums")]
    internal static class DrawFloorNumsPatch
    {
        public static bool Prefix(scnEditor __instance)
        {
            if (!Main.setting.showSpeedText)
                return true;

            foreach (var floor in __instance.floors)
            {
                if (!floor.enabled)
                    continue;

                if (string.IsNullOrEmpty(floor.editorNumText.letterText.text))
                    continue;

                char firstChar = floor.editorNumText.letterText.text[0];

                if (firstChar == 'x' || floor.editorNumText.letterText.text == "t")
                {
                    floor.editorNumText.letterText.text = floor.seqID.ToString();
                    floor.editorNumText.letterText.color = basicColor;
                }

                floor.editorNumText.gameObject.SetActive(__instance.showFloorNums && !__instance.playMode && !floor.isFake);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(scrFloor), "LateUpdate")]
    internal static class LateUpdatePatch
    {
        public static void Postfix(scrFloor __instance)
        {
            if (!Main.setting.showSpeedText)
                return;

            bool isValidContext = scrController.instance.gameworld
                || scrController.instance.currFloor == null
                || scrController.instance.currFloor.freeroamGenerated;

            if (!isValidContext)
                return;

            if (__instance.editorNumText == null)
                return;

            if (string.IsNullOrEmpty(__instance.editorNumText.letterText.text))
                return;

            if (__instance.editorNumText.letterText.text[0] != 'x')
                return;

            if (!__instance.isFading)
                return;

            var color = __instance.editorNumText.letterText.color;
            __instance.editorNumText.letterText.color = new Color(color.r, color.g, color.b, __instance.opacity);
        }
    }

    [HarmonyPatch(typeof(scrFloor), "OnBecameVisible")]
    internal static class OnBecameVisiblePatch
    {
        public static bool Prefix(scrFloor __instance)
        {
            if (!Main.setting.showSpeedText)
                return true;

            __instance.enabled = true;

            if (scrController.instance != null && scrController.instance.gameworld)
            {
                if (__instance.nextfloor != null && __instance.holdLength > -1)
                {
                    __instance.nextfloor.enabled = true;
                }

                if (__instance.prevfloor != null && __instance.prevfloor.holdLength > -1)
                {
                    __instance.prevfloor.enabled = true;
                }
            }

            if (__instance.editorNumText == null)
                return false;

            if (ADOBase.editor != null && ADOBase.editor.showFloorNums && !ADOBase.editor.playMode && ADOBase.isLevelEditor)
            {
                __instance.editorNumText.letterText.text = __instance.seqID.ToString();
                bool active = ADOBase.editor.showFloorNums && !ADOBase.editor.playMode && !__instance.isFake;
                __instance.editorNumText.gameObject.SetActive(active);
            }

            if (!string.IsNullOrEmpty(__instance.editorNumText.letterText.text)
                && __instance.editorNumText.letterText.text[0] == 'x')
            {
                __instance.editorNumText.gameObject.SetActive(true);
            }

            return false;
        }
    }

    private static float bpm;
    private static float pitch;
    private static float playbackSpeed = 1f;
    private static bool isPrevSpeedTransition;
    private static double prevFloorBpm;
    private static readonly Color basicColor = new Color(1f, 0f, 1f, 1f);

    private static bool DoubleEqual(double f1, double f2)
    {
        return Math.Abs(f1 - f2) < BpmComparisonEpsilon;
    }

    [HarmonyPatch(typeof(RDString), "Setup")]
    private static class RdStringSetupPatch
    {
        public static void Postfix()
        {
            Main.InitLanguage();
        }
    }

    private static double GetRealBpm(scrFloor floor, float bpm)
    {
        if (floor == null || floor.seqID == 0)
            return bpm;

        if (floor.nextfloor == null)
            return floor.speed * bpm;

        return 60.0 / (floor.nextfloor.entryTime - floor.entryTime);
    }

    private static scrFloor GetNextSpeedChangeFloor(scrFloor floor)
    {
        if (floor == null) return null;
        double currentSpeed = floor.speed;
        scrFloor next = floor.nextfloor;
        while (next != null)
        {
            if (!DoubleEqual(next.speed, currentSpeed))
                return next;
            next = next.nextfloor;
        }
        return null;
    }

    public static string Repeat(string value, int count)
    {
        return new StringBuilder(value.Length * count).Insert(0, value, count).ToString();
    }

    private static string cachedFormat;
    private static int cachedDecimalPlaces = -1;
    private static bool cachedZero;

    public static string FormatValue(float v)
    {
        int decimalPlaces = Main.setting.showDecimal;
        bool useZeroPlaceholder = Main.setting.zero;

        if (cachedFormat == null || decimalPlaces != cachedDecimalPlaces || useZeroPlaceholder != cachedZero)
        {
            cachedDecimalPlaces = decimalPlaces;
            cachedZero = useZeroPlaceholder;
            cachedFormat = "{0:0." + Repeat(useZeroPlaceholder ? "0" : "#", decimalPlaces) + "}";
        }

        return string.Format(cachedFormat, v);
    }

    public static void LevelStart(scrController controller)
    {
        Main.gui.TextObject.SetActive(true);

        var displayItems = new List<(int order, int index, string text)>();
        int insertIndex = 0;

        if (scnGame.instance != null)
        {
            pitch = scnGame.instance.levelData.pitch / 100f;

            if (ADOBase.isScnGame)
            {
                pitch *= GCS.currentSpeedTrial;
            }

            playbackSpeed = scnEditor.instance?.playbackSpeed ?? 1f;
            bpm = scnGame.instance.levelData.bpm * playbackSpeed * pitch;
        }
        else
        {
            pitch = scrConductor.instance.song.pitch;
            playbackSpeed = 1f;
            bpm = scrConductor.instance.bpm * pitch;
        }

        if (Main.setting.showSpeedText)
        {
            SetSpeedTextOnFloors();
        }

        float tileBpm = bpm;

        if (controller.currentSeqID != 0)
        {
            double speed = scrController.instance.planetarySystem.speed;
            tileBpm = (float)(bpm * speed);
        }

        if (Main.setting.onTileBpm)
        {
            displayItems.Add((Main.setting.tileBpmOrder, insertIndex++, Main.setting.text1.Replace("{value}", FormatValue(tileBpm))));
        }

        if (Main.setting.onCurBpm)
        {
            displayItems.Add((Main.setting.realBpmOrder, insertIndex++, Main.setting.text2.Replace("{value}", FormatValue(tileBpm))));
        }

        if (Main.setting.onRecommandKPS)
        {
            float kps = tileBpm / 60f;
            displayItems.Add((Main.setting.kpsOrder, insertIndex++, Main.setting.text3.Replace("{value}", Math.Round(kps).ToString())));
        }

        if (Main.setting.onNextBpm && controller.currFloor != null)
        {
            var nextChange = GetNextSpeedChangeFloor(controller.currFloor);
            if (nextChange != null)
            {
                double nextBpm = nextChange.speed * bpm * playbackSpeed * pitch;
                displayItems.Add((Main.setting.nextBpmOrder, insertIndex++, Main.setting.text5.Replace("{value}", FormatValue((float)nextBpm))));
            }
        }

        displayItems.Sort((a, b) =>
        {
            int cmp = a.order.CompareTo(b.order);
            return cmp != 0 ? cmp : a.index.CompareTo(b.index);
        });
        Main.gui.SetText(string.Join("\n", displayItems.ConvertAll(x => x.text)));
        Main.gui.SetSize(Main.setting.size);
    }

    private static void SetSpeedTextOnFloors()
    {
        foreach (var floor in scrLevelMaker.instance.listFloors)
        {
            if (floor.isFake)
                continue;

            var nextFloor = floor.nextfloor;

            if (nextFloor == null || nextFloor.editorNumText == null)
                continue;

            if (DoubleEqual(floor.speed, nextFloor.speed))
                continue;

            bool shouldShow = Main.setting.showSpeedTextMode switch
            {
                1 => !DoubleEqual(GetRealBpm(floor, bpm), GetRealBpm(nextFloor, bpm)),
                _ => true
            };

            if (!shouldShow)
                continue;

            double ratio = Main.setting.showSpeedTextMode switch
            {
                0 => nextFloor.speed / floor.speed,
                _ => GetRealBpm(nextFloor, bpm) / Math.Abs(GetRealBpm(floor, bpm))
            };

            nextFloor.editorNumText.letterText.text = $"x{ratio:0.##}";
            nextFloor.editorNumText.letterText.color = GetSpeedColor(ratio);

            bool isVisible = (floor.floorRenderer != null && floor.floorRenderer.renderer.isVisible)
                || (floor.legacyFloorSpriteRenderer != null && floor.legacyFloorSpriteRenderer.isVisible);

            if (isVisible)
            {
                nextFloor.editorNumText.gameObject.SetActive(true);
            }
        }
    }

    private static Color GetSpeedColor(double ratio)
    {
        if (ratio > 1.0)
        {
            return new Color32(
                (byte)Math.Max(240.0 - ratio * 7.0, 110.0),
                (byte)Math.Max(96.0 - ratio * 5.0, 0.0),
                (byte)Math.Max(96.0 - ratio * 5.0, 0.0),
                byte.MaxValue
            );
        }

        return new Color32(
            (byte)Math.Max(96.0 - ratio * 5.0, 0.0),
            (byte)Math.Max(96.0 - ratio * 5.0, 0.0),
            (byte)Math.Max(240.0 - ratio * 7.0, 110.0),
            byte.MaxValue
        );
    }
}
