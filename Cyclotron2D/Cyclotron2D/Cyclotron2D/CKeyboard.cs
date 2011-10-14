#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Tron
{
    /// <summary>
    /// This class provides additional functionality for dealing with keyboard input. It can detect
    /// key presses and can apply auto-repeat functionality when typing.
    /// </summary>
    public static class CKeyboard
    {
        private static KeyboardState m_keyboardState;
        /// The current keyboard state.
        public static KeyboardState State
        {
            get { return m_keyboardState; }
        }

        private static KeyboardState m_lastKeyboardState;
        /// The last keyboard state.  Used to detect if a key was just pressed.
        public static KeyboardState LastState
        {
            get { return m_lastKeyboardState; }
        }

        // Variables to allow a key to auto repeat if the user holds it down for a certain amount of time.
        static Keys pressedKey;
        static bool keyWasPressed;
        static float keyPressTime;
        static float keyRepeatTime;  // time the key was repeated
        static float m_repeatDelay;
        static bool repeatKey;       // Tells the PressDown method it's time to repeat the key.
        /// The amount of time in seconds the user must hold down the key before it starts repeating.
        public static float RepeatDelay
        {
            get { return m_repeatDelay; }
            set { m_repeatDelay = value; }
        }
        static float m_repeatRate;
        /// Once the repeat delay is over, this specifies how often (in seconds) the key acts like it was pressed again.
        public static float RepeatRate
        {
            get { return m_repeatRate; }
            set { m_repeatRate = value; }
        }

        static bool m_bAutoRepeat;
        public static bool AutoRepeat
        {
            get { return m_bAutoRepeat; }
            set { m_bAutoRepeat = value; }
        }

        ////////Methods////////

        /// Constructor
        static CKeyboard()
        {
            m_keyboardState = Keyboard.GetState();
            m_lastKeyboardState = m_keyboardState;
            m_repeatDelay = 0.5f; // half a second delay
            m_repeatRate = 0.05f; // retype character every 1/20th of a second after the delay
            pressedKey = (Keys)(-1);
            keyWasPressed = false;
            keyRepeatTime = 0f;
            repeatKey = false;
            m_bAutoRepeat = true;
        }


        /// Called at the beginning of each frame. Records the keyboard states.
        public static void Update(GameTime gametime)
        {
            m_lastKeyboardState = m_keyboardState;
            m_keyboardState = Keyboard.GetState();

            // Make sure letters aren't mistakenly typed due to outdated key repeats
            repeatKey = false;

            // A key was just pressed. Start keeping track of it for repeating.
            if (keyWasPressed)
            {
                keyWasPressed = false;
                keyPressTime = (float)gametime.TotalGameTime.TotalSeconds;
            }

            // if a key was pressed, make sure it's still pressed, and then check if it's time to repeat it.
            if ((int)pressedKey != -1 && m_bAutoRepeat)
            {
                // Make sure it's still pressed
                if (m_keyboardState.IsKeyUp(pressedKey))
                    pressedKey = (Keys)(-1);
                else
                {
                    if (gametime.TotalGameTime.TotalSeconds - keyPressTime > m_repeatDelay)
                        if (gametime.TotalGameTime.TotalSeconds - keyRepeatTime > m_repeatRate)
                        {
                            // press key
                            repeatKey = true;
                            keyRepeatTime = (float)gametime.TotalGameTime.TotalSeconds;
                        }
                }

            }

        }



        /// \brief Detects if a key was just pressed this frame.
        /// <param name="key">The key that you want to check.</param>
        /// <returns>Returns true if the key was just pressed.</returns>
        public static bool WasPressed(Keys key)
        {
            // We're only gonna check for pressed keys if the program is interested in them. Therfore,
            // we only need to check when this function is called.
            if (m_keyboardState.IsKeyDown(key) && m_lastKeyboardState.IsKeyUp(key))
            {
                pressedKey = key;
                keyWasPressed = true;
                return true;
            }
            // If no key was actually just pressed, check if it's time to repeat a key that's being held down.
            if (repeatKey && (key == pressedKey))
                return true;

            return false;
        }

        /// \brief Detects if a key was just released this frame.
        /// <param name="key">The key that you want to check.</param>
        /// <returns>Returns true if the key was just released.</returns>
        public static bool WasReleased(Keys key)
        {
            // We're only gonna check for released keys if the program is interested in them. Therfore,
            // we only need to check when this function is called.
            if (m_keyboardState.IsKeyUp(key) && m_lastKeyboardState.IsKeyDown(key))
                return true;

            return false;
        }

        /// \brief Converts a key from the enum Keys to a char
        /// <param name="key">The key to convert.</param>
        /// <returns>If possible, the character for the key being pressed. Otherwise the null character.</returns>
        public static char GetCharacter(Keys key)
        {
            // if the shift key is being pressed, return a capital letter.
            if ((char)key >= 'A' && (char)key <= 'Z')
                if (Shift)  // if shift key is pressed
                    return (char)key;
                else 
                    return (char)((int)key + 32);
            else if ((char)key >= '0' && (char)key <= '9')
                if (!Shift)
                    return (char)key;
                else
                {                       // Shift + number keys
                    if (key == Keys.D9)
                        return '(';
                    if (key == Keys.D0)
                        return ')';
                }
            else if (key == Keys.OemPeriod)
                return '.';
            else if (key == Keys.OemMinus)
                return '-';
            else if (key == Keys.OemComma)
                return ',';
            
            return (char)0;
        }

        public static bool Shift
        {
            get { return m_keyboardState.IsKeyDown(Keys.RightShift) || m_keyboardState.IsKeyDown(Keys.LeftShift); }
        }

        public static bool Control
        {
            get { return m_keyboardState.IsKeyDown(Keys.RightControl) || m_keyboardState.IsKeyDown(Keys.LeftControl); }
        }

        /// \brief Returns a string containing the alphanumeric keys currently being held down. So far, I just used this
        /// to test if my CKeyboard class was working. 
        /// <returns>Returns a string of all the pressed alphanumeric characters.</returns>
        public static String GetKeyDownChars()
        {
            // Don't get the string if the user is holding the control key
            if (m_keyboardState.IsKeyDown(Keys.LeftControl) || m_keyboardState.IsKeyDown(Keys.RightControl))
                return "";

            String result = "";
            char temp;
            Keys[] keys = m_keyboardState.GetPressedKeys();

            for (int i = 0; i < keys.Length; i++)
                if (m_keyboardState.IsKeyDown(keys[i]))
                    if ((temp = GetCharacter(keys[i])) != (char)0)
                        result += temp;

            return result;
        }


        /// \brief Finds the keys that were just pressed this frame and returns them in a string. Only
        /// finds alphanumeric characters and the '.'.
        /// <returns>String containing the characters that were typed.</returns>
        public static String GetPressedChars()
        {
            // Don't get the string if the user is holding the control key
            if (m_keyboardState.IsKeyDown(Keys.LeftControl) || m_keyboardState.IsKeyDown(Keys.RightControl))
                return "";

            String result = "";
            char temp;
            Keys[] keys = m_keyboardState.GetPressedKeys();

            for (int i = 0; i < keys.Length; i++)
                if (WasPressed(keys[i]))
                    if ((temp = GetCharacter(keys[i])) != (char)0)
                        result += temp;

            return result;
        }


        /// \brief Finds the first key in the Keys enum that was just pressed this frame.
        /// <returns>The key that was pressed.</returns>
        public static Keys GetPressedKey()
        {
            Keys[] keys = m_keyboardState.GetPressedKeys();

            for (int i = 0; i < keys.Length; i++)
                if (WasPressed(keys[i]))
                    return keys[i];
            return (Keys)(-1);

            //foreach (object i in Enum.GetValues(typeof(Keys)))
        }

        /// \brief Finds the first key in the Keys enum that is currently held down.
        /// <returns>The key that was pressed.</returns>
        public static Keys GetDownKey()
        {
            //foreach (object i in Enum.GetValues(typeof(Keys)))
            Keys[] temp = m_keyboardState.GetPressedKeys();
            if (temp.Length > 0)
                return temp[0];
            else
                return (Keys)(-1);

        }
    }

    public class MappedKeyboard<T>
    {
        Dictionary<T, List<Keys>> map = new Dictionary<T, List<Keys>>();
        public Dictionary<T, List<Keys>> Map
        {
            get { return map; }
            set 
            {
                if (value == null)
                    throw new ArgumentNullException("Setting Map to null is not allowed. Consider using an empty " +
                        "dictionary instead.");

                map = value; 
            }
        }

        public IEnumerable<Keys> this[T key]
        {
            get { return map[key]; }
            set { map[key] = new List<Keys>(value); }
        }

        public void Add(T key, IEnumerable<Keys> values)
        {
            map[key].AddRange(values);
        }

        public void Add(T key, Keys value)
        {
            map[key].Add(value);
        }

        public void Remove(T key, IEnumerable<Keys> values)
        {
            foreach (Keys val in values)
                map[key].Remove(val);
        }

        public void Remove(T key, Keys value)
        {
            map[key].Remove(value);
        }

        public bool PressDown(T key)
        {
            foreach (Keys keyboardKey in map[key])
                if (CKeyboard.WasPressed(keyboardKey))
                    return true;

            return false;
        }

        public bool ReleaseKey(T key)
        {
            foreach (Keys keyboardKey in map[key])
                if (CKeyboard.WasReleased(keyboardKey))
                    return true;

            return false;
        }

        public bool IsKeyDown(T key)
        {
            foreach (Keys keyboardKey in map[key])
                if (CKeyboard.State.IsKeyDown(keyboardKey))
                    return true;

            return false;
        }

        public bool IsKeyUp(T key)
        {
            return !IsKeyDown(key);
        }
    }
}
