// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerToys.PreviewHandler.Markdown
{
    internal sealed class Settings
    {
        /// <summary>
        /// Gets the color of the window background.
        /// Even though this is not a setting yet, it's retrieved from a "Settings" class to be aligned with other preview handlers that contain this setting.
        /// It's possible it can be converted into a setting in the future.
        /// </summary>
        public static Color BackgroundColor
        {
            get
            {
                if (GetTheme() == "dark")
                {
                    return Color.FromArgb(30, 30, 30); // #1e1e1e
                }
                else
                {
                    return Color.White;
                }
            }
        }

        /// <summary>
        /// Returns the theme.
        /// </summary>
        /// <returns>Theme that should be used.</returns>
        public static string GetTheme()
        {
            return Common.UI.ThemeManager.GetWindowsBaseColor().ToLowerInvariant();
        }

        /// <summary>
        /// Returns whether local images should be displayed in the Markdown preview.
        /// GPO policy takes precedence over user setting.
        /// </summary>
        public static bool GetLocalImagesEnabled()
        {
            int? gpoValue = GetGpoValue("ConfigureEnabledUtilityFileExplorerMarkdownLocalImages");
            if (gpoValue == 1)
            {
                return true;
            }

            if (gpoValue == 0)
            {
                return false;
            }

            try
            {
                string settingsPath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft", "PowerToys", "File Explorer", "settings.json");
                if (!System.IO.File.Exists(settingsPath))
                {
                    return false;
                }

                string json = System.IO.File.ReadAllText(settingsPath);
                int idx = json.IndexOf("\"md-previewer-local-images-setting\"", StringComparison.Ordinal);
                if (idx < 0)
                {
                    return false;
                }

                int valueIdx = json.IndexOf("\"value\"", idx, StringComparison.Ordinal);
                if (valueIdx < 0)
                {
                    return false;
                }

                return json.IndexOf("true", valueIdx, Math.Min(20, json.Length - valueIdx), StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private static int? GetGpoValue(string valueName)
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\PowerToys");
                if (key != null)
                {
                    object val = key.GetValue(valueName);
                    if (val is int intVal)
                    {
                        return intVal;
                    }
                }

                using var userKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Policies\PowerToys");
                if (userKey != null)
                {
                    object val = userKey.GetValue(valueName);
                    if (val is int intVal)
                    {
                        return intVal;
                    }
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
