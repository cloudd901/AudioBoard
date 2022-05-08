using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PCAFFINITY
{
    /*
     * - Reference HotkeyCommands (WinForms).dll in your Forms.cs.
     * - Set the Extension method next to your Form class.
     * - Create new instance of HotkeyCommands.
     * - Set Action KeyActionCall (Returns Current Form and Key Pressed)
     */

    /// <summary>Initialize the HotkeyCommand class.</summary>
    /// <seealso cref="System.IDisposable" />
    /// <example>This is how to quickly Initiate Hotkeys:
    ///     <code>
    ///         <para>using PCAFFINITY;</para>
    ///         <para>using System;</para>
    ///         <para>using System.Windows.Forms;</para>
    ///         <para>namespace YourHotkeyProgram</para>
    ///         <para>{</para>
    ///         <para>    public partial class Form1 : HotkeysExtensionForm</para>
    ///         <para>    {</para>
    ///         <para>        public Form1()</para>
    ///         <para>        {</para>
    ///         <para>            InitializeComponent();</para>
    ///         <para>            HotkeyCommand hotkeyComm = new HotkeyCommand(this, new string[] { "F1", "Escape", "{CTRL}{Shift}A" });</para>
    ///         <para>            hotkeyComm.KeyActionCall += onKeyAction;</para>
    ///         <para>            hotkeyComm.StartHotkeys();</para>
    ///         <para>        }</para>
    ///         <para>    }</para>
    ///         <para>}</para>
    ///     </code>
    /// </example>
    public class HotkeyCommand : IDisposable
    {
        private const uint WM_KEYDOWN = 0x100;
        private const uint WM_KEYUP = 0x101;
        private readonly Window _form;
        private bool IsDisposed;

        /// <summary>Initializes a new instance of the <see cref="HotkeyCommand"/> class.</summary>
        /// <param name="form">The Form listening for Hotkeys.</param>
        /// <param name="newKeyList">New String list of Hotkeys. Keymodifiers in brackets {}."</param>
        /// <example><c>HotkeyCommand hotkeyComm = new HotkeyCommand(this, new string[] { "F1", "Escape", "{CTRL}{Shift}A" });</c></example>
        /// <exception cref="InvalidCastException">Unable to subscribe to KeyCalled event. Please ensure your Form is using the HotkeysExtension with KeyPressedCall event.</exception>
        public HotkeyCommand(Window form, string[] newKeyList = null)
        {
            _form = form;
            if (_form is HotkeysExtensionWindow f)
            {
                f.KeyPressedCall += OnKeyActionCall;
            }
            else if (!SetSuppressExceptions)
            {
                throw new InvalidCastException("Unable to subscribe to KeyCalled event. Please ensure your Form is using the HotkeysExtension with KeyPressedCall event.");
            }
            else
            {
                return;
            }

            if (newKeyList != null)
            {
                newKeyList = newKeyList.Distinct().ToArray();
                for (int i = 0; i < newKeyList.Length; i++)
                {
                    HotkeyDictionary.Add((short)(i + 1), newKeyList[i]);
                }
            }
        }

        /// <summary>EventHandler for Hotkey Pressed events.</summary>
        /// <param name="form">The form.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="key">The key.</param>
        public delegate void KeyActionCallEventHandler(Window form, short id, string key);

        /// <summary>EventHandler for Hotkey Registration events.</summary>
        /// <param name="result"><c>true</c> if the registration was successfull.</param>
        /// <param name="key">The Hotkey being registered.</param>
        /// <param name="id">They ID of the Hotkey being registered</param>
        public delegate void KeyRegisteredEventHandler(bool result, string key, short id);

        /// <summary>EventHandler for Hotkey Unregistration events.</summary>
        /// <param name="key">The Hotkey being unregistered.</param>
        /// <param name="id">They ID of the Hotkey being unregistered</param>
        public delegate void KeyUnregisteredEventHandler(string key, short id);

        /// <summary>Occurs when Hotkey is called.</summary>
        public event KeyActionCallEventHandler KeyActionCall;

        /// <summary>Occurs when Hotkey is Registered.</summary>
        public event KeyRegisteredEventHandler KeyRegisteredCall;

        /// <summary>Occurs when Hotkey is Unregistered.</summary>
        public event KeyUnregisteredEventHandler KeyUnregisteredCall;

        /// <summary>Key Register Modifiers.</summary>
        [Flags]
        public enum KeyModifier
        {
            /// <exclude />
            None = 0x0000,

            /// <exclude />
            Alt = 0x0001,

            /// <exclude />
            Ctrl = 0x0002,

            /// <exclude />
            Shift = 0x0004,

            /// <exclude />
            Win = 0x0008,

            /// <exclude />
            NoRepeat = 0x4000
        }

        /// <summary>Gets the Dictionary of all current Hotkeys.</summary>
        /// <value>The Dictionary of Hotkeys.</value>
        public Dictionary<short, string> HotkeyDictionary { get; } = new Dictionary<short, string>();

        /// <summary>Check if Hotkeys are active.</summary>
        /// <value>Get the Hotkey status.</value>
        public bool IsRegistered { get; private set; }

        /// <summary>Gets or sets a value indicating whether Hotkeys trigger regardless of active window.</summary>
        /// <value>
        ///   <c>true</c> if [set Hotkeys globally]; otherwise, <c>false</c>.</value>
        public bool SetHotkeysGlobally { get; set; } = true;

        /// <summary>Gets or sets a value indicating whether Exceptions are thown or ignored.</summary>
        /// <value>
        ///   <c>true</c> if [Ignore Exceptions]; otherwise, <c>false</c>.</value>
        public bool SetSuppressExceptions { get; set; }

        public static uint FilterKeytoUint(string keyString)
        {
            keyString = keyString.Trim().ToUpper();

            if (keyString.Contains("PRINTSCREEN"))
            {
                return (uint)Keys.VK_PRINT;
            }
            else if (keyString.Contains("PRINTSCRN"))
            {
                return (uint)Keys.VK_PRINT;
            }
            else if (keyString.Contains("PRINT"))
            {
                return (uint)Keys.VK_PRINT;
            }
            else if (keyString.Contains("PLAY"))
            {
                return (uint)Keys.VK_PLAY;
            }
            else if (keyString.Contains("PAUSE"))
            {
                return (uint)Keys.VK_PAUSE;
            }
            else if (keyString.Contains("LWIN"))
            {
                return (uint)Keys.VK_LWIN;
            }
            else if (keyString.Contains("RWIN"))
            {
                return (uint)Keys.VK_RWIN;
            }
            else if (keyString.Contains("WIN"))
            {
                return (uint)Keys.VK_LWIN;
            }
            else if (keyString.Contains("UP"))
            {
                return (uint)Keys.VK_UP;
            }
            else if (keyString.Contains("DOWN"))
            {
                return (uint)Keys.VK_DOWN;
            }
            else if (keyString.Contains("LEFT"))
            {
                return (uint)Keys.VK_LEFT;
            }
            else if (keyString.Contains("RIGHT"))
            {
                return (uint)Keys.VK_RIGHT;
            }
            else if (keyString.Contains("SPACE"))
            {
                return (uint)Keys.VK_SPACE;
            }
            else if (keyString.Contains("SPC"))
            {
                return (uint)Keys.VK_SPACE;
            }
            else if (keyString.Contains("ESCAPE"))
            {
                return (uint)Keys.VK_ESCAPE;
            }
            else if (keyString.Contains("ESC"))
            {
                return (uint)Keys.VK_ESCAPE;
            }
            else if (keyString.Contains("CLEAR"))
            {
                return (uint)Keys.VK_CLEAR;
            }
            else if (keyString.Contains("CLR"))
            {
                return (uint)Keys.VK_CLEAR;
            }
            else if (keyString.Contains("CAPSLOCK"))
            {
                return (uint)Keys.VK_CAPITAL;
            }
            else if (keyString.Contains("END"))
            {
                return (uint)Keys.VK_END;
            }
            else if (keyString.Contains("HOME"))
            {
                return (uint)Keys.VK_HOME;
            }
            else if (keyString.Contains("INSERT"))
            {
                return (uint)Keys.VK_INSERT;
            }
            else if (keyString.Contains("PAGEUP"))
            {
                return (uint)Keys.VK_PRIOR;
            }
            else if (keyString.Contains("PGUP"))
            {
                return (uint)Keys.VK_PRIOR;
            }
            else if (keyString.Contains("PAGEDOWN"))
            {
                return (uint)Keys.VK_NEXT;
            }
            else if (keyString.Contains("PGDOWN"))
            {
                return (uint)Keys.VK_NEXT;
            }
            else if (keyString == "]")
            {
                return (uint)Keys.VK_OEM_6;
            }
            else if (keyString == "[")
            {
                return (uint)Keys.VK_OEM_4;
            }
            else if (keyString == ",")
            {
                return (uint)Keys.VK_OEM_COMMA;
            }
            else if (keyString == ".")
            {
                return (uint)Keys.VK_OEM_PERIOD;
            }
            else if (keyString == "?")
            {
                return (uint)Keys.VK_OEM_2;
            }
            else if (keyString == ";")
            {
                return (uint)Keys.VK_OEM_1;
            }
            else if (keyString == ":")
            {
                return (uint)Keys.VK_OEM_1;
            }
            else if (keyString == "\"")
            {
                return (uint)Keys.VK_OEM_7;
            }
            else if (keyString == "|")
            {
                return (uint)Keys.VK_OEM_5;
            }
            else if (keyString == "+")
            {
                return (uint)Keys.VK_OEM_PLUS;
            }
            else if (keyString == "-")
            {
                return (uint)Keys.VK_OEM_MINUS;
            }
            else if (keyString == "_")
            {
                return (uint)Keys.VK_OEM_MINUS;
            }
            else if (keyString == "*")
            {
                return (uint)Keys.VK_MULTIPLY;
            }
            else if (keyString == "`")
            {
                return (uint)Keys.VK_OEM_3;
            }
            else if (keyString == "~")
            {
                return (uint)Keys.VK_OEM_3;
            }
            else if (keyString == "1")
            {
                return (uint)Keys.VK_1;
            }
            else if (keyString == "!")
            {
                return (uint)Keys.VK_1;
            }
            else if (keyString == "2")
            {
                return (uint)Keys.VK_2;
            }
            else if (keyString == "@")
            {
                return (uint)Keys.VK_2;
            }
            else if (keyString == "3")
            {
                return (uint)Keys.VK_3;
            }
            else if (keyString == "#")
            {
                return (uint)Keys.VK_3;
            }
            else if (keyString == "4")
            {
                return (uint)Keys.VK_4;
            }
            else if (keyString == "$")
            {
                return (uint)Keys.VK_4;
            }
            else if (keyString == "5")
            {
                return (uint)Keys.VK_5;
            }
            else if (keyString == "%")
            {
                return (uint)Keys.VK_5;
            }
            else if (keyString == "6")
            {
                return (uint)Keys.VK_6;
            }
            else if (keyString == "^")
            {
                return (uint)Keys.VK_6;
            }
            else if (keyString == "7")
            {
                return (uint)Keys.VK_7;
            }
            else if (keyString == "&")
            {
                return (uint)Keys.VK_7;
            }
            else if (keyString == "8")
            {
                return (uint)Keys.VK_8;
            }
            else if (keyString == "9")
            {
                return (uint)Keys.VK_9;
            }
            else if (keyString == "(")
            {
                return (uint)Keys.VK_9;
            }
            else if (keyString == "0")
            {
                return (uint)Keys.VK_0;
            }
            else if (keyString == ")")
            {
                return (uint)Keys.VK_0;
            }
            else if (keyString == "NUMPAD1")
            {
                return (uint)Keys.VK_NUMPAD1;
            }
            else if (keyString == "NUMPAD2")
            {
                return (uint)Keys.VK_NUMPAD2;
            }
            else if (keyString == "NUMPAD3")
            {
                return (uint)Keys.VK_NUMPAD3;
            }
            else if (keyString == "NUMPAD4")
            {
                return (uint)Keys.VK_NUMPAD4;
            }
            else if (keyString == "NUMPAD5")
            {
                return (uint)Keys.VK_NUMPAD5;
            }
            else if (keyString == "NUMPAD6")
            {
                return (uint)Keys.VK_NUMPAD6;
            }
            else if (keyString == "NUMPAD7")
            {
                return (uint)Keys.VK_NUMPAD7;
            }
            else if (keyString == "NUMPAD8")
            {
                return (uint)Keys.VK_NUMPAD8;
            }
            else if (keyString == "NUMPAD9")
            {
                return (uint)Keys.VK_NUMPAD9;
            }
            else if (keyString == "NUMPAD0")
            {
                return (uint)Keys.VK_NUMPAD0;
            }
            else if (keyString == "NUMPAD+")
            {
                return (uint)Keys.VK_ADD;
            }
            else if (keyString == "NUMPAD-")
            {
                return (uint)Keys.VK_SUBTRACT;
            }
            else if (keyString == "NUMPAD*")
            {
                return (uint)Keys.VK_MULTIPLY;
            }
            else if (keyString == "NUMPAD/")
            {
                return (uint)Keys.VK_DIVIDE;
            }
            else if (keyString == "NUMPAD.")
            {
                return (uint)Keys.VK_DECIMAL;
            }
            else
            {
                return (uint)Enum.Parse(typeof(Keys), keyString, true);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Register a single Hotkey to <see cref="HotkeyDictionary"/>.</summary>
        /// <param name="key">The Hotkey.</param>
        /// <param name="id">The Hotkey ID.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">'{key}' Key is already in HotkeyDictionary.
        /// or
        /// ID '{id}' is already registered to key '{key}' in HotkeyDictionary.</exception>
        public HotkeyCommand HotkeyAddKey(string key, short? id = null)
        {
            key = key.Trim().ToUpper();
            if (HotkeyDictionary.ContainsValue(key))
            {
                return !SetSuppressExceptions
                    ? throw new InvalidOperationException($"'{key}' Key is already in HotkeyDictionary.")
                    : this;
            }

            short idK;
            try
            {
                idK = id == null ? (short)(HotkeyDictionary.Keys.Count + 1) : (short)id;
            }
            catch
            {
                idK = 1;
            }

            short? knownId = null;
            _ = HotkeyDictionary.TryGetValue(idK, out string knownKey);

            if (knownKey != null)
            {
                if (knownKey == key)
                {
                    return !SetSuppressExceptions
                        ? throw new InvalidOperationException($"{key} is already in HotkeyDictionary.")
                        : this;
                }
                else if (knownId == idK)
                {
                    return !SetSuppressExceptions
                        ? throw new InvalidOperationException($"ID '{idK}' is already registered to key '{knownKey}' in HotkeyDictionary.")
                        : this;
                }
            }

            HotkeyDictionary.Add(idK, key);
            return this;
        }

        /// <summary>Register a list of Hotkeys.</summary>
        /// <param name="newKeyList">The new Hotkey list.</param>
        /// <param name="replaceCurrentKeys">if set to <c>true</c> [replace current List of Hotkeys]. see <see cref="HotkeyDictionary"/></param>
        public void HotkeyAddKeyList(string[] newKeyList, bool replaceCurrentKeys = false)
        {
            if (replaceCurrentKeys)
            {
                HotkeyUnregisterAll(true);
                HotkeyDictionary.Clear();
            }

            foreach (string s in newKeyList)
            {
                if (HotkeyDictionary.ContainsValue(s))
                {
                    short id = HotkeyDictionary.FirstOrDefault(x => x.Value == s).Key;
                    _ = HotkeyDictionary.Remove(id);
                }
            }

            foreach (string s in newKeyList)
            {
                _ = HotkeyAddKey(s);
            }
        }

        /// <summary>Register a list of Hotkeys.</summary>
        /// <param name="newKeyList">The new Hotkey list.</param>
        /// <param name="newIDList">The new Hotkey ID list.</param>
        /// <param name="replaceCurrentKeys">if set to <c>true</c> [replace current List of Hotkeys]. see <see cref="HotkeyDictionary"/></param>
        /// <exception cref="InvalidOperationException">
        /// Size of newKeyList is not the same as newIDList.
        /// or
        /// newKeyList cannot contain duplicate keys.
        /// or
        /// '{s}' Key is already in HotkeyDictionary.
        /// or
        /// '{s}' ID is already in HotkeyDictionary.
        /// </exception>
        public void HotkeyAddKeyList(string[] newKeyList, short[] newIDList, bool replaceCurrentKeys = false)
        {
            if (newIDList == null)
            {
                HotkeyAddKeyList(newKeyList, replaceCurrentKeys);
                return;
            }

            if (newIDList.Length != newKeyList.Length)
            {
                if (!SetSuppressExceptions)
                {
                    throw new InvalidOperationException("Size of newKeyList is not the same as newIDList.");
                }

                return;
            }

            newKeyList = newKeyList.Distinct().ToArray();
            if (newIDList.Length != newKeyList.Length)
            {
                if (!SetSuppressExceptions)
                {
                    throw new InvalidOperationException("newKeyList cannot contain duplicate keys.");
                }

                return;
            }

            if (replaceCurrentKeys)
            {
                HotkeyUnregisterAll(true); HotkeyDictionary.Clear();
            }

            for (int i = 0; i < newKeyList.Length; i++)
            {
                _ = HotkeyAddKey(newKeyList[i], newIDList[i]);
            }
        }

        /// <summary>Unregister a single Hotkey from <see cref="HotkeyDictionary"/>.</summary>
        /// <param name="key">The Hotkey.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Unable to Unregister '{key}' key.</exception>
        public HotkeyCommand HotkeyRemoveKey(string key)
        {
            key = key.Trim().ToUpper();
            short? knownId = null;

            if (!HotkeyDictionary.ContainsValue(key))
            {
                return !SetSuppressExceptions
                    ? throw new InvalidOperationException($"Unable to Unregister '{key}' key. Key Not Found")
                    : this;
            }

            knownId = HotkeyDictionary.FirstOrDefault(x => x.Value == key).Key;
            _ = HotkeyDictionary.Remove((short)knownId);
            return this;
        }

        /// <summary>Unregisters all Hotkeys from <see cref="HotkeyDictionary"/>.</summary>
        /// <param name="removeCurrentKeys">if set to <c>true</c> [Delete all from HotkeyDictionary], else <c>false</c> to [keep HotkeyDictionary].</param>
        public void HotkeyUnregisterAll(bool removeCurrentKeys = true)
        {
            foreach (KeyValuePair<short, string> p in new Dictionary<short, string>(HotkeyDictionary))
            {
                string keyString = p.Value;
                KeyModifier km = new KeyModifier();
                while (keyString.Contains("{"))
                {
                    int loc1 = keyString.IndexOf('{');
                    int loc2 = keyString.IndexOf('}');
                    string mod = keyString.Substring(loc1 + 1, loc2 - loc1 - 1);
                    keyString = keyString.Replace("{" + mod + "}", "");
                    mod = mod.ToUpper();
                    if (mod is "SHFT" or "SHIFT")
                    {
                        km |= KeyModifier.Shift;
                    }

                    if (mod is "CTRL" or "CONTROL")
                    {
                        km |= KeyModifier.Ctrl;
                    }

                    if (mod == "ALT")
                    {
                        km |= KeyModifier.Alt;
                    }

                    if (mod == "WIN")
                    {
                        km |= KeyModifier.Win;
                    }
                }
                try
                {
                    if (removeCurrentKeys)
                    {
                        _ = HotkeyDictionary.Remove(p.Key);
                    }

                    uint keys = (uint)Keys.None;
                    if (!string.IsNullOrEmpty(keyString))
                    {
                        keys = FilterKeytoUint(keyString);
                    }

                    KeyUnregisteredCall?.Invoke(p.Value, p.Key);

                    //if (_form is HotkeysExtensionWindow f)
                    //{
                    //    _ = new KeyHandler(keys, f._windowHandle, p.Key, km).Unregister();
                    //     KeyUnregisteredCall?.Invoke(p.Value, p.Key);
                    //}
                }
                catch
                {
                }
            }
        }

        /// <summary>Quickly Stop and Restart the hotkeys. Only use if already Started.</summary>
        public void RestartHotkeys()
        {
            if (IsRegistered)
            {
                StopHotkeys();
            }

            StartHotkeys();
        }

        /// <summary>Start using hotkeys.</summary>
        /// <exception cref="InvalidOperationException">HotkeyCommands is already Initiated. Try stopping first.</exception>
        public void StartHotkeys()
        {
            if (IsRegistered)
            {
                if (!SetSuppressExceptions)
                {
                    throw new InvalidOperationException("HotkeyCommands is already Initiated. Try stopping first.");
                }

                return;
            }

            IsRegistered = true;
            RegAllDictionary();
        }

        /// <summary>Stop using hotkeys.</summary>
        /// <exception cref="InvalidOperationException">HotkeyCommands is not started. Try starting it first.</exception>
        public void StopHotkeys()
        {
            if (!IsRegistered)
            {
                if (!SetSuppressExceptions)
                {
                    throw new InvalidOperationException("HotkeyCommands is not started. Try starting it first.");
                }

                return;
            }

            IsRegistered = false;
            HotkeyUnregisterAll(false);
        }

        /// <summary>
        /// Dispose of Hotkeys
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                if (IsRegistered)
                {
                    StopHotkeys();
                }

                if (_form is HotkeysExtensionWindow f)
                {
                    f.KeyPressedCall -= OnKeyActionCall;
                }

                HotkeyDictionary.Clear();
                IsDisposed = true;
            }
        }

        private void OnKeyActionCall(Window form, short id)
        {
            if (form is HotkeysExtensionWindow f)
            {
                if (!SetHotkeysGlobally)
                {
                    IntPtr window = NativeMethods.GetForegroundWindow();
                    if (f._windowHandle != window)
                    {
                        try
                        {
                            HotkeyDictionary.TryGetValue(id, out string keyString);
                            _ = Enum.TryParse(keyString, out Keys keyKeys);
                            NativeMethods.PostMessage(window, WM_KEYDOWN, (IntPtr)keyKeys, IntPtr.Zero);
                            NativeMethods.PostMessage(window, WM_KEYUP, (IntPtr)keyKeys, IntPtr.Zero);
                        }
                        catch
                        {
                        }

                        return;
                    }
                }

                HotkeyDictionary.TryGetValue(id, out string key);
                KeyActionCall?.Invoke(f, id, key);
            }
        }//Received from 'HotkeysExtension'. Alert new event to HotkeyCommand instance.

        private void RegAllDictionary()
        {
            foreach (KeyValuePair<short, string> p in HotkeyDictionary)
            {
                string keyString = p.Value;
                KeyModifier km = new KeyModifier();
                while (keyString.Contains("{"))
                {
                    int loc1 = keyString.IndexOf('{');
                    int loc2 = keyString.IndexOf('}');
                    string mod = keyString.Substring(loc1 + 1, loc2 - loc1 - 1);
                    keyString = keyString.Replace("{" + mod + "}", "");
                    mod = mod.ToUpper();
                    if (mod == "SHFT" || mod == "SHIFT")
                    {
                        km |= KeyModifier.Shift;
                    }

                    if (mod == "CTRL" || mod == "CONTROL")
                    {
                        km |= KeyModifier.Ctrl;
                    }

                    if (mod == "ALT")
                    {
                        km |= KeyModifier.Alt;
                    }

                    if (mod == "WIN")
                    {
                        km |= KeyModifier.Win;
                    }
                }
                try
                {
                    uint keys = (uint)Keys.None;

                    if (!string.IsNullOrEmpty(keyString))
                    {
                        keys = FilterKeytoUint(keyString);
                    }

                    if (_form is HotkeysExtensionWindow f)
                    {
                        bool test = new KeyHandler(keys, f._windowHandle, p.Key, km).Register();
                        KeyRegisteredCall?.Invoke(test, p.Value, p.Key);
                    }
                }
                catch (Exception e)
                {
                    HotkeyUnregisterAll(false);
                    if (!SetSuppressExceptions)
                    {
                        throw new InvalidOperationException(e.Message);
                    }
                }
            }
        }
    }
}