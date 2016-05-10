using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Ageless {

    public abstract class Setting {
        public string name;

        public Setting(string name) {
            this.name = name;
        }

        public abstract string valueToString();

        public abstract bool setValue(string c);
    }

    public class SettingRange {}

    public class SettingRangeBool : SettingRange {}

    public class SettingRangeInt : SettingRange {
        int min;
        int max;
        int delta;
        bool wrap;

        public SettingRangeInt(int min, int max, int delta, bool wrap) {
            this.min = min;
            this.max = max;
            this.delta = delta;
            this.wrap = wrap;
        }
    }

    public class SettingRangeFloat : SettingRange {
        float min;
        float max;
        float delta;
        bool multiplicative;
        bool wrap;

        public SettingRangeFloat(float min, float max, float delta, bool multiplicative, bool wrap) {
            this.min = min;
            this.max = max;
            this.delta = delta;
            this.multiplicative = multiplicative;
            this.wrap = wrap;
        }
    }


    public class SettingBool : Setting {
        public bool value;

        public SettingBool(string name, bool v) : base(name) {
            value = v;
        }

        public override string valueToString() {
            return value ? "true" : "false" ;
        }

        public override bool setValue(string c) {
            value = c.StartsWith("true");
            return c.StartsWith("true") || c.StartsWith("false");
        }

        public static implicit operator bool(SettingBool s) {
            return s.value;
        }
    }


    public class SettingInt : Setting {
        public int value;

        public SettingInt(string name, int v) : base(name) {
            value = v;
        }

        public override string valueToString() {
            return value.ToString();
        }

        public override bool setValue(string c) {
            try {
                value = int.Parse(c);
                return true;
            } catch (FormatException) {
                return false;
            }
        }

        public static implicit operator int(SettingInt s) {
            return s.value;
        }
    }


    public class SettingFloat : Setting {
        public float value;

        public SettingFloat(string name, float v) : base(name) {
            value = v;
        }

        public override string valueToString() {
            return value.ToString();
        }

        public override bool setValue(string c) {
            try {
                value = float.Parse(c);
                return true;
            } catch (FormatException) {
                return false;
            }
        }

        public static implicit operator float(SettingFloat s) {
            return s.value;
        }
    }


    public class Settings {

        public readonly Dictionary<string, Setting> settingList = new Dictionary<string, Setting>();
        public readonly Dictionary<Setting, SettingRange> settingEditableList = new Dictionary<Setting, SettingRange>();


        public readonly SettingBool invertCameraX = new SettingBool("Invert Camera X", false);
        public readonly SettingBool invertCameraY = new SettingBool("Invert Camera Y", false);
        public readonly SettingFloat cameraScrollSpeed = new SettingFloat("Camera Rotation Speed", (float)(Math.PI / 180));
		public readonly SettingFloat cameraZoomSpeed = new SettingFloat("Camera Zoom Speed", 1.05f);
        
        public readonly SettingInt windowWidth = new SettingInt("Window Width", 640);
        public readonly SettingInt windowHeight = new SettingInt("Window Height", 480);

        private string fileName;

		public Settings(string fileName) {
            this.fileName = fileName;

            settingList.Add(invertCameraX.name, invertCameraX); settingEditableList.Add(invertCameraX, new SettingRangeBool());
            settingList.Add(invertCameraY.name, invertCameraY); settingEditableList.Add(invertCameraY, new SettingRangeBool());
            settingList.Add(cameraScrollSpeed.name, cameraScrollSpeed); settingEditableList.Add(cameraScrollSpeed, new SettingRangeFloat((float)(Math.PI / 360), (float)(Math.PI / 16), 0.187622894589f, false, false));
            settingList.Add(cameraZoomSpeed.name, cameraZoomSpeed); settingEditableList.Add(cameraZoomSpeed, new SettingRangeFloat(1, 2, 0.01f, false, false));

            settingList.Add(windowWidth.name, windowWidth);
            settingList.Add(windowHeight.name, windowHeight);
        }

        public void load() {
            try {
                string[] lines = File.ReadAllLines(fileName);

                foreach (string line in lines) {
                    if (line.Length > 0) {
                        if (line[0] != '#') {
                            if (line.Contains(':')) {
                                string[] sections = line.Split(':');
                                if (sections.Length == 2) {
                                    if (settingList.ContainsKey(sections[0])) {
                                        if (!settingList[sections[0]].setValue(sections[1])) {
                                            Console.WriteLine("Failed to read value: {0}", sections[1]);
                                        }
                                    } else {
                                        Console.WriteLine("No such setting called: {0}", sections[0]);
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("Loaded Settings");

            } catch (FileNotFoundException) {
                Console.WriteLine("No settings file");
            }

        }

        public void save() {

            string text = "# Ageless Settings" + Environment.NewLine;

            foreach (Setting s in settingList.Values) {
                text += s.name + ":" + s.valueToString() + Environment.NewLine;
            }

            File.WriteAllText(fileName, text);

            Console.WriteLine("Saved Settings");
        }
    }
}
