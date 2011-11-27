using System.Collections.Generic;
using System.Linq;
using Cyclotron2D.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Cyclotron2D.State
{
    public class InputState : CyclotronComponent
    {

        private readonly Keys[] ModifierKeys = new Keys[]
        {
            Keys.LeftAlt, Keys.RightAlt, 
            Keys.LeftControl, Keys.RightControl,
            Keys.LeftShift, Keys.RightShift  
        };

        #region Properties

        public KeyboardState CurrentKeyState { get; private set; }
        public KeyboardState PreviousKeyState { get; private set; }

        public MouseState CurrentMouseState { get; private set; }
        public MouseState PreviousMouseState { get; private set; }

        public Point MousePosition { get { return new Point(CurrentMouseState.X, CurrentMouseState.Y); } }

        public bool IsNewLeftClick { get { return CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released; } }

        #endregion

        public InputState(Game game) : base(game)
        {
            UpdateOrder = 0; //input device state change get updated before anything else

            CurrentKeyState = PreviousKeyState = Keyboard.GetState();
            CurrentMouseState = PreviousMouseState = Mouse.GetState();
        }

        #region Public Methods

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            PreviousKeyState = CurrentKeyState;
            PreviousMouseState = CurrentMouseState;

            CurrentKeyState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();
        }


        public bool IsKeyDown(Keys key)
        {
            return CurrentKeyState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return CurrentKeyState.IsKeyUp(key);
        }

        public bool IsNewCharPress(out char? c)
        {
            c = null;
            List<Keys> keys = CurrentKeyState.GetPressedKeys().Where(x => x != Keys.None).ToList();
            List<Keys> modKeys = new List<Keys>();
            foreach (var modKey in ModifierKeys)
            {
                if (keys.Contains(modKey))
                {
                    keys.Remove(modKey);
                    modKeys.Add(modKey);
                }
            }
            Keys k;
            if (keys.Count >= 1 && PreviousKeyState.IsKeyUp(k = keys[0]))
            {
                c = GetCharacter(k, modKeys.Contains(Keys.LeftShift) || modKeys.Contains(Keys.RightShift));

                if (k == Keys.Space)
                {
                    c = ' ';
                }
                else if (string.IsNullOrEmpty(c.Value.ToString()) || c == '\0')
                {
                    c = GetNumPadChar(k);
                }

                if (!string.IsNullOrEmpty(c.Value.ToString()) && c != '\0')
                {
                    return true;
                }
                c = null;

            }

            return false;
        }

        private static char GetNumPadChar(Keys key)
        {
            switch (key)
            {
                case Keys.NumPad0:
                    return '0';
                case Keys.NumPad1:
                    return '1';
                case Keys.NumPad2:
                    return '2';
                case Keys.NumPad3:
                    return '3';
                case Keys.NumPad4:
                    return '4';
                case Keys.NumPad5:
                    return '5';
                case Keys.NumPad6:
                    return '6';
                case Keys.NumPad7:
                    return '7';
                case Keys.NumPad8:
                    return '8';
                case Keys.NumPad9:
                    return '9';
                default:
                    return '\0';

            }
        }

        /// \brief Converts a key from the enum Keys to a char
        /// <param name="key">The key to convert.</param>
        /// <param name="shift"></param>
        /// <returns>If possible, the character for the key being pressed. Otherwise the null character.</returns>
        private static char GetCharacter(Keys key, bool shift)
        {
            // if the shift key is being pressed, return a capital letter.
            if ((char)key >= 'A' && (char)key <= 'Z')
                if (shift)  // if shift key is pressed
                    return (char)key;
                else
                    return (char)((int)key + 32);
            else if ((char)key >= '0' && (char)key <= '9')
                if (!shift)
                    return (char)key;
                else
                {
                    switch (key)
                    {
                        case Keys.D0:
                            return ')';
                        case Keys.D1:
                            return '!';
                        case Keys.D2:
                            return '@';
                        case Keys.D3:
                            return '#';
                        case Keys.D4:
                            return '$';
                        case Keys.D5:
                            return '%';
                        case Keys.D6:
                            return '^';
                        case Keys.D7:
                            return '&';
                        case Keys.D8:
                            return '*';
                        case Keys.D9:
                            return '(';

                    }
                    // Shift + number keys
                    
                }
            else if (key == Keys.OemPeriod)
                return '.';
            else if (key == Keys.OemMinus)
                return '-';
            else if (key == Keys.OemComma)
                return ',';

            return (char)0;
        }

        public bool IsNewKeyPress(Keys key)
        {
            return CurrentKeyState.IsKeyDown(key) && PreviousKeyState.IsKeyUp(key);
        }

        public bool IsNewKeyRelease(Keys key)
        {
            return CurrentKeyState.IsKeyUp(key) && PreviousKeyState.IsKeyDown(key);
        }

        #endregion
    }


}