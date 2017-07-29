using System;
using System.Collections.Generic;

using Microsoft.Win32;

namespace CustomDataTip
{
    public enum VsTheme
    {
        Unknown = 0,
        Light,
        Dark,
        Blue
    }

    public class ThemeUtil
    {
        private static readonly IDictionary<string, VsTheme> Themes = new Dictionary<string, VsTheme>()
        {
            { "de3dbbcd-f642-433c-8353-8f1df4370aba", VsTheme.Light },
            { "1ded0138-47ce-435e-84ef-9ec1f439b749", VsTheme.Dark },
            { "a4d6a176-b948-4b29-8c66-53c97a1ed7d0", VsTheme.Blue }
        };

        public static VsTheme GetCurrentTheme()
        {
            string themeId = GetThemeId();
            if (string.IsNullOrWhiteSpace(themeId) == false)
            {
                themeId = themeId.Replace("{", "").Replace("}", "");
                VsTheme theme;
                if (Themes.TryGetValue(themeId, out theme))
                {
                    return theme;
                }
            }

            return VsTheme.Unknown;
        }

        public static string GetThemeId()
        {
            string keyName = string.Format(@"Software\Microsoft\VisualStudio\14.0\ApplicationPrivateSettings\Microsoft\VisualStudio");

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
            {
                var keyText = (string) key?.GetValue("ColorTheme", string.Empty);

                if (!string.IsNullOrEmpty(keyText))
                {
                    var keyTextValues = keyText.Split('*');
                    if (keyTextValues.Length > 2)
                    {
                        return keyTextValues[2];
                    }
                }
            }

            return null;
        }
    }
}
