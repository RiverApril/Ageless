using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK.Input;

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

    public class SettingRangeBool : SettingRange { }

    public class SettingRangeKey : SettingRange { }

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


    public class SettingKey : Setting {

        public bool isMouse;
        public MouseButton button;
        public Key key;
        public KeyModifiers mod;

        public SettingKey(string name, Key key, KeyModifiers mod) : base(name) {
            this.key = key;
            this.mod = mod;
            this.isMouse = false;
        }

        public SettingKey(string name, MouseButton button, KeyModifiers mod) : base(name) {
            this.button = button;
            this.mod = mod;
            this.isMouse = true;
        }

        public override bool setValue(string c) {
            try {
                if (c.StartsWith(" Mouse ")) {
                    int p1 = c.IndexOf('"');
                    int p2 = c.IndexOf('"', p1 + 1);
                    button = (MouseButton)Enum.Parse(button.GetType(), c.Substring(p1 + 1, p2 - p1 - 1));
                } else {
                    int p1 = c.IndexOf('"');
                    int p2 = c.IndexOf('"', p1 + 1);
                    key = (Key)Enum.Parse(key.GetType(), c.Substring(p1 + 1, p2 - p1 - 1));
                }
                mod = 0;
                if (c.Contains("Shift")) {
                    mod |= KeyModifiers.Shift;
                }
                if (c.Contains("Control")) {
                    mod |= KeyModifiers.Control;
                }
                if (c.Contains("Alt")) {
                    mod |= KeyModifiers.Alt;
                }
                return true;
            } catch (ArgumentException) {
                return false;
            }
        }

        public override string valueToString() {
            string m = "";
            if ((mod & KeyModifiers.Shift) != 0) {
                m += " + Shift";
            }
            if ((mod & KeyModifiers.Control) != 0) {
                m += " + Control";
            }
            if ((mod & KeyModifiers.Alt) != 0) {
                m += " + Alt";
            }
            if (isMouse) {
                return " Mouse \"" + Enum.GetName(button.GetType(), button) + "\"" + m;
            } else {
                return " \"" + Enum.GetName(key.GetType(), key) + "\"" + m;
            }
        }

        public bool testModifiers(KeyboardState ks) {
            return ((ks.IsKeyDown(Key.ShiftLeft) || ks.IsKeyDown(Key.ShiftRight)) == ((mod & KeyModifiers.Shift) != 0)) &&
                    ((ks.IsKeyDown(Key.ControlLeft) || ks.IsKeyDown(Key.ControlRight)) == ((mod & KeyModifiers.Control) != 0)) &&
                    ((ks.IsKeyDown(Key.AltLeft) || ks.IsKeyDown(Key.AltRight)) == ((mod & KeyModifiers.Alt) != 0));
        }

        public bool test(KeyboardKeyEventArgs ke, MouseButtonEventArgs me) {
            if (ke != null && !isMouse) {
                if (ke.Key == key && ke.Modifiers == mod) {
                    return true;
                }
            }
            if (me != null && isMouse) {
                KeyboardState ks = Keyboard.GetState();
                if (me.Button == button && testModifiers(ks)) {
                    return true;
                }
            }
            return false;
        }

        public bool test(KeyboardState ks, MouseState ms) {
            if (ks != null && !isMouse) {
                if (ks.IsKeyDown(key) && testModifiers(ks)) {
                    return true;
                }
            }
            if (ms != null && isMouse) {
                if (ms.IsButtonDown(button) && testModifiers(ks)) {
                    return true;
                }
            }
            return false;
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


        public readonly SettingKey bindCameraUp = new SettingKey("Camera Up", Key.W, 0);
        public readonly SettingKey bindCameraDown = new SettingKey("Camera Down", Key.S, 0);
        public readonly SettingKey bindCameraLeft = new SettingKey("Camera Left", Key.A, 0);
        public readonly SettingKey bindCameraRight = new SettingKey("Camera Right", Key.D, 0);
        public readonly SettingKey bindCameraIn = new SettingKey("Camera In", Key.Plus, 0);
        public readonly SettingKey bindCameraOut = new SettingKey("Camera Out", Key.Minus, 0);

        public readonly SettingKey bindMoveToMouse = new SettingKey("Move", MouseButton.Left, 0);
        public readonly SettingKey bindExit = new SettingKey("Exit", Key.Escape, 0);

        public readonly SettingKey editorBindToggle = new SettingKey("EDITOR Toggle", Key.Tab, 0);
        public readonly SettingKey editorBindFocusOnPlayer = new SettingKey("EDITOR Focus on Player", Key.P, 0);
        public readonly SettingKey editorBindSave = new SettingKey("EDITOR Save", Key.S, KeyModifiers.Control);
        public readonly SettingKey editorBindLoad = new SettingKey("EDITOR Load", Key.L, KeyModifiers.Control);


        private string fileName;

        public Settings(string fileName) {
            this.fileName = fileName;

            addSetting(invertCameraX, new SettingRangeBool());
            addSetting(invertCameraY, new SettingRangeBool());
            addSetting(cameraScrollSpeed, new SettingRangeFloat((float)(Math.PI / 360), (float)(Math.PI / 16), 0.187622894589f, false, false));
            addSetting(cameraZoomSpeed, new SettingRangeFloat(1, 2, 0.01f, false, false));

            addSetting(windowWidth);
            addSetting(windowHeight);


            addSetting(bindCameraUp, new SettingRangeKey());
            addSetting(bindCameraDown, new SettingRangeKey());
            addSetting(bindCameraLeft, new SettingRangeKey());
            addSetting(bindCameraRight, new SettingRangeKey());
            addSetting(bindCameraIn, new SettingRangeKey());
            addSetting(bindCameraOut, new SettingRangeKey());

            addSetting(bindMoveToMouse, new SettingRangeKey());
            addSetting(bindExit);

            addSetting(editorBindToggle, new SettingRangeKey());
            addSetting(editorBindFocusOnPlayer, new SettingRangeKey());
            addSetting(editorBindSave, new SettingRangeKey());
            addSetting(editorBindLoad, new SettingRangeKey());
        }

        public void addSetting(Setting setting, SettingRange range) {
            addSetting(setting);
            settingEditableList.Add(setting, range);
        }

        public void addSetting(Setting setting) {
            settingList.Add(setting.name, setting);
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
