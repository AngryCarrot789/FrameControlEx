﻿using System;

namespace FrameControlEx.Themes {
    public enum ThemeType {
        SoftDark,
        SoftDarkAndBlue,
        RedBlackTheme,
        DeepDark,
    }

    public static class ThemeTypeExtension {
        public static string GetName(this ThemeType type) {
            switch (type) {
                case ThemeType.SoftDark:        return "SoftDark";
                case ThemeType.SoftDarkAndBlue: return "SoftDarkAndBlue";
                case ThemeType.RedBlackTheme:   return "RedBlackTheme";
                case ThemeType.DeepDark:        return "DeepDark";
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}