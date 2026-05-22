using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityModManagerNet;

namespace ShowBPM
{
    public class Setting : UnityModManager.ModSettings
    {
        // BPM display toggles
        public bool onTileBpm = true;
        public bool onCurBpm = true;
        public bool onRecommandKPS = true;
        public bool onNextBpm = false;
        public bool showRealKPS = false;

        // Text appearance
        public bool useShadow = true;
        public bool useBold = false;
        public int size = 35;
        public int align = 2;
        public int showDecimal = 0;
        public bool zero = true;

        // BPM calculation
        public bool ignoreMultipress = false;

        // Speed text in editor
        public bool showSpeedText = false;
        public int showSpeedTextMode = 0;

        // Position
        public float x = 0.96f;
        public float y = 0.98f;

        // Display text templates
        public string text1 = "타일 BPM - {value}";
        public string text2 = "체감 BPM - {value}";
        public string text3 = "초당 클릭 수 - {value}";
        public string text4 = "Real KPS - {value}";
        public string text5 = "Next BPM - {value}";

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            string path = GetPath(modEntry);

            try
            {
                using (var textWriter = new StreamWriter(path))
                {
                    var xmlSerializer = new XmlSerializer(GetType());
                    xmlSerializer.Serialize(textWriter, this);
                }
            }
            catch (Exception ex)
            {
                modEntry.Logger.Error("Can't save " + path + ".");
                modEntry.Logger.LogException(ex);
            }
        }

        public override string GetPath(UnityModManager.ModEntry modEntry)
        {
            return Path.Combine(modEntry.Path, GetType().Name + ".xml");
        }
    }
}
