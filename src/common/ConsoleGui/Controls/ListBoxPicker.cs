//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System;
using System.Linq;

namespace ConsoleGui.Controls
{
    /// <summary>
    /// Interactive list picker control that allows users to select an item from a list.
    /// Supports keyboard navigation (arrows, page up/down, home/end), selection (Enter/Escape),
    /// and type-to-filter search (press '?' or Ctrl+F to open, type to filter, Tab to cycle matches).
    /// </summary>
    public class ListBoxPicker : SpeedSearchListBoxControl
    {
        /// <summary>
        /// Displays an interactive picker and returns the selected index.
        /// </summary>
        /// <param name="choices">Array of strings to choose from</param>
        /// <param name="select">Initial selected index (default: 0)</param>
        /// <returns>Selected index, or -1 if cancelled</returns>
        public static int PickIndexOf(string[] choices, int select = 0)
        {
            var width = Math.Max(choices.Max(x => x.Length) + 4, 29);
            return ListBoxPicker.PickIndexOf(choices, width, 30, new Colors(ConsoleColor.White, ConsoleColor.Blue), new Colors(ConsoleColor.White, ConsoleColor.Red), select);
        }

        /// <summary>
        /// Displays an interactive picker with custom dimensions and colors.
        /// </summary>
        /// <param name="lines">Array of strings to choose from</param>
        /// <param name="width">Width of the picker window</param>
        /// <param name="height">Height of the picker window</param>
        /// <param name="normal">Colors for normal (unselected) items</param>
        /// <param name="selected">Colors for the selected item</param>
        /// <param name="select">Initial selected index (default: 0)</param>
        /// <returns>Selected index, or -1 if cancelled</returns>
        public static int PickIndexOf(string[] lines, int width, int height, Colors normal, Colors selected, int select = 0)
        {
            // TODO: Add InOutPipeServer support when that component is ported
            // if (InOutPipeServer.IsInOutPipeServer)
            // {
            //     return InOutPipeServer.GetSelectionFromUser(lines, select);
            // }

            if (height > lines.Length + 2) height = lines.Length + 2;
            if (width == int.MinValue)
            {
                foreach (var line in lines)
                {
                    if (line.Length > width)
                    {
                        width = line.Length;
                    }
                }
                width += 2;
            }

            var rect = Screen.Current.MakeSpaceAtCursor(width, height);
            var border = rect.Height > 2 ? Window.Borders.SingleLine : null;
            var picker = new ListBoxPicker(null, rect, normal, selected, border)
            {
                Items = lines,
            };

            picker.SetSelectedRow(select);
            picker.Run();

            return picker._picked;
        }

        /// <summary>
        /// Displays an interactive picker and returns the selected string.
        /// </summary>
        /// <param name="choices">Array of strings to choose from</param>
        /// <param name="select">Initial selected index (default: 0)</param>
        /// <returns>Selected string, or null if cancelled</returns>
        public static string? PickString(string[] choices, int select = 0)
        {
            var width = Math.Max(choices.Max(x => x.Length) + 4, 29);
            return ListBoxPicker.PickString(choices, width, 30, new Colors(ConsoleColor.White, ConsoleColor.Blue), new Colors(ConsoleColor.White, ConsoleColor.Red), select);
        }

        /// <summary>
        /// Displays an interactive picker with custom dimensions and colors, returning the selected string.
        /// </summary>
        /// <param name="lines">Array of strings to choose from</param>
        /// <param name="width">Width of the picker window</param>
        /// <param name="height">Height of the picker window</param>
        /// <param name="normal">Colors for normal (unselected) items</param>
        /// <param name="selected">Colors for the selected item</param>
        /// <param name="select">Initial selected index (default: 0)</param>
        /// <returns>Selected string, or null if cancelled</returns>
        public static string? PickString(string[] lines, int width, int height, Colors normal, Colors selected, int select = 0)
        {
            var picked = PickIndexOf(lines, width, height, normal, selected, select);
            return picked >= 0 && picked < lines.Length
                ? lines[picked]
                : null;
        }

        /// <summary>
        /// Processes keyboard input for the picker.
        /// Supports speed search (type to filter), navigation, and selection.
        /// </summary>
        /// <param name="key">The console key info from user input</param>
        /// <returns>True if the key was processed, false otherwise</returns>
        public override bool ProcessKey(ConsoleKeyInfo key)
        {
            // Process speed search keys (?, Ctrl+F, typing, Tab, etc.)
            var processed = ProcessSpeedSearchKey(key);
            if (processed) return processed;

            // Handle Enter and Escape for selection/cancellation
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    _picked = -1;
                    processed = true;
                    break;

                case ConsoleKey.Enter:
                    _picked = SelectedRow;
                    processed = true;
                    break;
            }

            if (processed)
            {
                Close();
                return true;
            }

            return base.ProcessKey(key);
        }

        #region protected methods

        /// <summary>
        /// Protected constructor - use static Pick methods instead.
        /// </summary>
        protected ListBoxPicker(Window? parent, Rect rect, Colors colorNormal, Colors colorSelected, string? border = null, bool fEnabled = true) 
            : base(parent, rect, colorNormal, colorSelected, border, fEnabled)
        {
        }

        #endregion

        #region private data

        private int _picked;

        #endregion
    }
}
