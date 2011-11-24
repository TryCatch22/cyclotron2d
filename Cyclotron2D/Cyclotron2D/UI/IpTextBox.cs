using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cyclotron2D.Screens.Base;
using Cyclotron2D.UI.UIElements;
using Microsoft.Xna.Framework;

namespace Cyclotron2D.UI
{
    public class IpTextBox : LabelTextBox
    {
        public IpTextBox(Game game, Screen screen) : base(game, screen)
        {
            Element.ValueChanged += OnValueChanged;
        }

        private void OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            BoxText = AutoCompleteIP(BoxText, e.OldValue);
        }

        private string AutoCompleteIP(string text, string oldText)
        {
            // Case 1: Text has been deleted
            if (oldText.Length > text.Length)
            {
                // If they deleted a ".", delete the character before it too.
                if (oldText[oldText.Length - 1] == '.' && oldText.Length >= 2)
                    return oldText.Substring(0, oldText.Length - 2);
                return text;
            }

            // Case 2: Text has been entered

            // Case 2a: They started off by entering a ".". The fools...
            if (text.StartsWith("."))
                return "";

            // Get rid of any other unwanted crap.
            text = text.Replace("..", ".");
            text = Regex.Replace(text, @"[^\d.]", "");

            // Auto add the period if 3 digits have been typed, and if it isn't the last group of digits.
            if (text.Length >= 3 && Regex.IsMatch(text, @"\d\d\d$"))
            {
                var value = Int32.Parse(text.Substring(text.Length - 3));
                // Stop those bastards from typing too-big numbers
                if (value > 255)
                    return oldText;

                if (text.Count(x => x == '.') < 3)
                    text += ".";
            }
            else if (text.Length >= 2 && text.Count(x => x == '.') < 3 && Regex.IsMatch(text, @"\d\d$"))
            {
                // 2-digit numbers between 26 and 99 are already going to be greater than 255 if
                // allowed to add a third digit, so we'll add the period immediately.
                var value = Int32.Parse(text.Substring(text.Length - 2));
                if (value >= 26 && value <= 99)
                    text += ".";
            }

            // Once it's valid, stop them from typing anything else.
            var ipRegex = @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$";
            if (Regex.IsMatch(oldText, ipRegex) && !Regex.IsMatch(text, ipRegex))
                return oldText;

            return text;
        }
    }
}
