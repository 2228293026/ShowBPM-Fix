using UnityEngine;
using UnityEngine.UI;

namespace ShowBPM
{
    internal class TextBehaviour : MonoBehaviour
    {
        public GameObject TextObject;
        public Text text;
        public Shadow shadowText;
        public RectTransform rectTransform;

        public Text realKPSText;
        public Shadow realKPSShadow;
        private RectTransform realKPSRectTransform;
        private GameObject realKPSObject;
        private string lastRealKPSText;

        public void SetSize(int size)
        {
            text.fontSize = size;
            text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
        }

        public void SetText(string content)
        {
            text.text = content;
            text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
            SyncKpsStyle();
        }

        public void SetPosition(float x, float y)
        {
            var pos = new Vector2(x, y);
            rectTransform.anchorMin = pos;
            rectTransform.anchorMax = pos;
            rectTransform.pivot = pos;
            SyncKpsStyle();
        }

        public void SyncKpsStyle()
        {
            if (realKPSText == null) return;

            float alignX = GetAlignFloat(Main.setting.align);
            realKPSRectTransform.anchorMin = new Vector2(alignX, 0);
            realKPSRectTransform.anchorMax = new Vector2(alignX, 0);
            realKPSRectTransform.pivot = new Vector2(alignX, 1);
            realKPSRectTransform.anchoredPosition = new Vector2(0, -5);
            realKPSText.fontSize = Main.setting.size;
            realKPSText.fontStyle = Main.setting.useBold ? FontStyle.Bold : FontStyle.Normal;
            realKPSText.font = text.font;
            realKPSShadow.enabled = Main.setting.useShadow;
            realKPSText.alignment = ToTextAnchor(Main.setting.align);
        }

        private static float GetAlignFloat(int align)
        {
            return align switch
            {
                0 => 0f,
                1 => 0.5f,
                _ => 1f
            };
        }

        void Awake()
        {
            var mainCanvas = gameObject.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 10001;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            TextObject = new GameObject();
            TextObject.transform.SetParent(transform);
            TextObject.AddComponent<Canvas>();
            rectTransform = TextObject.GetComponent<RectTransform>();

            var textObject = new GameObject();
            textObject.transform.SetParent(TextObject.transform);

            text = textObject.AddComponent<Text>();
            text.font = RDString.GetFontDataForLanguage(RDString.language).font;
            text.alignment = ToTextAnchor(Main.setting.align);
            text.fontSize = Main.setting.size;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;

            shadowText = textObject.AddComponent<Shadow>();
            shadowText.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadowText.effectDistance = new Vector2(2f, -2f);

            var pos = new Vector2(Main.setting.x, Main.setting.y);
            rectTransform.anchorMin = pos;
            rectTransform.anchorMax = pos;
            rectTransform.pivot = pos;
            rectTransform.anchoredPosition = Vector2.zero;

            var rkpsObject = new GameObject("RealKPS");
            realKPSObject = rkpsObject;
            rkpsObject.transform.SetParent(textObject.transform);
            realKPSRectTransform = rkpsObject.AddComponent<RectTransform>();

            realKPSText = rkpsObject.AddComponent<Text>();
            realKPSText.font = RDString.GetFontDataForLanguage(RDString.language).font;
            realKPSText.fontSize = Main.setting.size;
            realKPSText.alignment = ToTextAnchor(Main.setting.align);
            realKPSText.color = Color.white;
            realKPSText.horizontalOverflow = HorizontalWrapMode.Overflow;

            realKPSShadow = rkpsObject.AddComponent<Shadow>();
            realKPSShadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            realKPSShadow.effectDistance = new Vector2(2f, -2f);

            SyncKpsStyle();
        }

        void Update()
        {
            if (!Main.IsEnabled)
                return;

            if (TextObject.activeSelf && !IsInGameContext())
            {
                TextObject.SetActive(false);
                return;
            }

            if (!TextObject.activeInHierarchy)
                return;

            realKPSObject.SetActive(Main.setting.showRealKPS);

            if (!Main.setting.showRealKPS)
                return;

            int kps = Patch.callRateTracker.GetCallsPerSecond();
            string kpsStr = kps.ToString();

            if (kpsStr != lastRealKPSText)
            {
                lastRealKPSText = kpsStr;
                realKPSText.text = Main.setting.text4.Replace("{value}", kpsStr);
            }
        }

        private static bool IsInGameContext()
        {
            return scrController.instance != null && scrController.instance.gameworld;
        }

        public TextAnchor ToTextAnchor(int align)
        {
            return align switch
            {
                0 => TextAnchor.UpperLeft,
                1 => TextAnchor.UpperCenter,
                _ => TextAnchor.UpperRight
            };
        }
    }
}
