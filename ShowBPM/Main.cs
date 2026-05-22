using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;
using UnityEngine;

namespace ShowBPM
{
    public static class Main
    {
        public static bool IsEnabled { get; private set; }

        internal static TextBehaviour gui;
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

        public static Harmony harmony;
        public static Setting setting;

        public static Dictionary<string, Language> languages = new Dictionary<string, Language>()
        {
            {"Korean", new Korean()},
            {"English", new English()},
            {"中文", new Chinese()}
        };

        public static Language language = new English();
        private static GUIStyle alignButtonStyle;
        private static string[] alignTexts;

        internal static void Setup(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            setting = UnityModManager.ModSettings.Load<Setting>(modEntry);
            modEntry.OnToggle = OnToggle;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            IsEnabled = value;

            if (value)
            {
                Start(modEntry);
                gui = new GameObject().AddComponent<TextBehaviour>();
                UnityEngine.Object.DontDestroyOnLoad(gui);
                modEntry.OnGUI = OnGUI;
                modEntry.OnSaveGUI = OnSaveGUI;
                gui.TextObject.SetActive(false);
            }
            else
            {
                gui.TextObject.SetActive(false);
                UnityEngine.Object.Destroy(gui);
                gui = null;
                Stop(modEntry);
            }

            return true;
        }

        public static void InitLanguage()
        {
            switch (RDString.language)
            {
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    language = languages["中文"];
                    break;
                case SystemLanguage.Korean:
                    language = languages["Korean"];
                    break;
                case SystemLanguage.English:
                    language = languages["English"];
                    break;
            }
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            DrawToggleTextField(ref setting.onTileBpm, language.showTileBPM, ref setting.text1, language.setTileBPM);
            DrawToggleTextField(ref setting.onCurBpm, language.showRealBPM, ref setting.text2, language.setRealBPM);
            DrawToggleTextField(ref setting.onRecommandKPS, language.showKPS, ref setting.text3, language.setKPS);

            DrawSpeedTextToggle();

            DrawToggleTextField(ref setting.showRealKPS, language.showRealKPS, ref setting.text4, language.setRealKPS);

            if (!setting.onTileBpm && !setting.onCurBpm && !setting.onRecommandKPS)
            {
                return;
            }

            GUILayout.Label("   ");

            setting.useShadow = GUILayout.Toggle(setting.useShadow, language.setShadow);
            gui.shadowText.enabled = setting.useShadow;
            gui.SyncKpsStyle();

            setting.useBold = GUILayout.Toggle(setting.useBold, language.setBold);
            gui.text.fontStyle = setting.useBold ? FontStyle.Bold : FontStyle.Normal;
            gui.SyncKpsStyle();

            setting.zero = GUILayout.Toggle(setting.zero, language.setZeroPlaceHolder);
            setting.ignoreMultipress = GUILayout.Toggle(setting.ignoreMultipress, language.ignoreMultipress);

            DrawSliderSection();

            DrawAlignSection();

            gui.text.alignment = gui.ToTextAnchor(setting.align);
            gui.SyncKpsStyle();
        }

        private static void DrawToggleTextField(ref bool toggle, string toggleLabel, ref string text, string textLabel)
        {
            toggle = GUILayout.Toggle(toggle, toggleLabel);

            if (toggle)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30f);
                text = MoreGUILayout.NamedTextField(textLabel, text, 300f);
                GUILayout.EndHorizontal();
            }
        }

        private static void DrawSpeedTextToggle()
        {
            setting.showSpeedText = GUILayout.Toggle(setting.showSpeedText, language.showSpeedText);

            if (!setting.showSpeedText)
                return;

            GUILayout.BeginHorizontal();
            GUILayout.Space(30f);
            setting.showSpeedTextMode = GUILayout.Toggle(setting.showSpeedTextMode == 0, language.setTileBPM) ? 0 : 1;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30f);
            setting.showSpeedTextMode = GUILayout.Toggle(setting.showSpeedTextMode == 1, language.setRealBPM) ? 1 : 0;
            GUILayout.EndHorizontal();
        }

        private static void DrawSliderSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            int decimalPlaces = (int)MoreGUILayout.NamedSlider(language.showDecimal, setting.showDecimal, 0f, 6f, 300f, 1f, 0f, "{0:0.##}");
            if (decimalPlaces != setting.showDecimal)
            {
                setting.showDecimal = decimalPlaces;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            float newX = MoreGUILayout.NamedSlider(language.setX, setting.x, -0.1f, 1.1f, 300f, 0.01f, 0f, "{0:0.##}");
            if (newX != setting.x)
            {
                setting.x = newX;
                gui.SetPosition(setting.x, setting.y);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            float newY = MoreGUILayout.NamedSlider(language.setY, setting.y, -0.1f, 1.1f, 300f, 0.01f, 0f, "{0:0.##}");
            if (newY != setting.y)
            {
                setting.y = newY;
                gui.SetPosition(setting.x, setting.y);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            float newSize = MoreGUILayout.NamedSlider(language.setSize, setting.size, 1f, 100f, 300f, 1f, 0f, "{0:0.##}");
            if ((int)newSize != setting.size)
            {
                setting.size = (int)newSize;
                gui.SetSize(setting.size);
                gui.SyncKpsStyle();
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawAlignSection()
        {
            if (alignTexts == null)
            {
                alignTexts = new[] { language.alignLeft, language.alignCenter, language.alignRight };
            }

            alignButtonStyle ??= new GUIStyle(GUI.skin.button);

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label(language.setAlign);

            for (int i = 0; i < alignTexts.Length; i++)
            {
                alignButtonStyle.fontStyle = setting.align == i ? FontStyle.Bold : FontStyle.Normal;

                if (GUILayout.Button(alignTexts[i], alignButtonStyle))
                {
                    setting.align = i;
                    gui.SetPosition(setting.x, setting.y);
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            setting.Save(modEntry);
        }

        private static void Start(UnityModManager.ModEntry modEntry)
        {
            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void Stop(UnityModManager.ModEntry modEntry)
        {
            harmony.UnpatchAll(modEntry.Info.Id);
        }
    }
}
