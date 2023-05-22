using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SkiaSharp;

namespace FrameControlEx.Converters {
    [ValueConversion(typeof(string), typeof(SKColor))]
    public class SKColorConverter : IValueConverter {
        public static SKColorConverter Instance { get; } = new SKColorConverter();

        private static readonly object BlackColour = SKColors.Black;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string str && str.Length > 0) {
                return StringToColour(str) ?? DependencyProperty.UnsetValue;
            }
            else if (value is SKColor col) {
                return ColourToString(col);
            }
            else {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string str && str.Length > 0) {
                return StringToColour(str) ?? DependencyProperty.UnsetValue;
            }
            else if (value is SKColor col) {
                return ColourToString(col);
            }
            else {
                return DependencyProperty.UnsetValue;
            }
        }

        public static SKColor? StringToColour(string text) {
            switch (text.ToLower()) {
                case "aliceblue":             return SKColors.AliceBlue;
                case "antiquewhite":          return SKColors.AntiqueWhite;
                case "aqua":                  return SKColors.Aqua;
                case "aquamarine":            return SKColors.Aquamarine;
                case "azure":                 return SKColors.Azure;
                case "beige":                 return SKColors.Beige;
                case "bisque":                return SKColors.Bisque;
                case "black":                 return SKColors.Black;
                case "blanchedalmond":        return SKColors.BlanchedAlmond;
                case "blue":                  return SKColors.Blue;
                case "blueviolet":            return SKColors.BlueViolet;
                case "brown":                 return SKColors.Brown;
                case "burlywood":             return SKColors.BurlyWood;
                case "cadetblue":             return SKColors.CadetBlue;
                case "chartreuse":            return SKColors.Chartreuse;
                case "chocolate":             return SKColors.Chocolate;
                case "coral":                 return SKColors.Coral;
                case "cornflowerblue":        return SKColors.CornflowerBlue;
                case "cornsilk":              return SKColors.Cornsilk;
                case "crimson":               return SKColors.Crimson;
                case "cyan":                  return SKColors.Cyan;
                case "darkblue":              return SKColors.DarkBlue;
                case "darkcyan":              return SKColors.DarkCyan;
                case "darkgoldenrod":         return SKColors.DarkGoldenrod;
                case "darkgray":              return SKColors.DarkGray;
                case "darkgreen":             return SKColors.DarkGreen;
                case "darkkhaki":             return SKColors.DarkKhaki;
                case "darkmagenta":           return SKColors.DarkMagenta;
                case "darkolivegreen":        return SKColors.DarkOliveGreen;
                case "darkorange":            return SKColors.DarkOrange;
                case "darkorchid":            return SKColors.DarkOrchid;
                case "darkred":               return SKColors.DarkRed;
                case "darksalmon":            return SKColors.DarkSalmon;
                case "darkseagreen":          return SKColors.DarkSeaGreen;
                case "darkslateblue":         return SKColors.DarkSlateBlue;
                case "darkslategray":         return SKColors.DarkSlateGray;
                case "darkturquoise":         return SKColors.DarkTurquoise;
                case "darkviolet":            return SKColors.DarkViolet;
                case "deeppink":              return SKColors.DeepPink;
                case "deepskyblue":           return SKColors.DeepSkyBlue;
                case "dimgray":               return SKColors.DimGray;
                case "dodgerblue":            return SKColors.DodgerBlue;
                case "firebrick":             return SKColors.Firebrick;
                case "floralwhite":           return SKColors.FloralWhite;
                case "forestgreen":           return SKColors.ForestGreen;
                case "fuchsia":               return SKColors.Fuchsia;
                case "gainsboro":             return SKColors.Gainsboro;
                case "ghostwhite":            return SKColors.GhostWhite;
                case "gold":                  return SKColors.Gold;
                case "goldenrod":             return SKColors.Goldenrod;
                case "gray":                  return SKColors.Gray;
                case "green":                 return SKColors.Green;
                case "greenyellow":           return SKColors.GreenYellow;
                case "honeydew":              return SKColors.Honeydew;
                case "hotpink":               return SKColors.HotPink;
                case "indianred":             return SKColors.IndianRed;
                case "indigo":                return SKColors.Indigo;
                case "ivory":                 return SKColors.Ivory;
                case "khaki":                 return SKColors.Khaki;
                case "lavender":              return SKColors.Lavender;
                case "lavenderblush":         return SKColors.LavenderBlush;
                case "lawngreen":             return SKColors.LawnGreen;
                case "lemonchiffon":          return SKColors.LemonChiffon;
                case "lightblue":             return SKColors.LightBlue;
                case "lightcoral":            return SKColors.LightCoral;
                case "lightcyan":             return SKColors.LightCyan;
                case "lightgoldenrodyellow":  return SKColors.LightGoldenrodYellow;
                case "lightgray":             return SKColors.LightGray;
                case "lightgreen":            return SKColors.LightGreen;
                case "lightpink":             return SKColors.LightPink;
                case "lightsalmon":           return SKColors.LightSalmon;
                case "lightseagreen":         return SKColors.LightSeaGreen;
                case "lightskyblue":          return SKColors.LightSkyBlue;
                case "lightslategray":        return SKColors.LightSlateGray;
                case "lightsteelblue":        return SKColors.LightSteelBlue;
                case "lightyellow":           return SKColors.LightYellow;
                case "lime":                  return SKColors.Lime;
                case "limegreen":             return SKColors.LimeGreen;
                case "linen":                 return SKColors.Linen;
                case "magenta":               return SKColors.Magenta;
                case "maroon":                return SKColors.Maroon;
                case "mediumaquamarine":      return SKColors.MediumAquamarine;
                case "mediumblue":            return SKColors.MediumBlue;
                case "mediumorchid":          return SKColors.MediumOrchid;
                case "mediumpurple":          return SKColors.MediumPurple;
                case "mediumseagreen":        return SKColors.MediumSeaGreen;
                case "mediumslateblue":       return SKColors.MediumSlateBlue;
                case "mediumspringgreen":     return SKColors.MediumSpringGreen;
                case "mediumturquoise":       return SKColors.MediumTurquoise;
                case "mediumvioletred":       return SKColors.MediumVioletRed;
                case "midnightblue":          return SKColors.MidnightBlue;
                case "mintcream":             return SKColors.MintCream;
                case "mistyrose":             return SKColors.MistyRose;
                case "moccasin":              return SKColors.Moccasin;
                case "navajowhite":           return SKColors.NavajoWhite;
                case "navy":                  return SKColors.Navy;
                case "oldlace":               return SKColors.OldLace;
                case "olive":                 return SKColors.Olive;
                case "olivedrab":             return SKColors.OliveDrab;
                case "orange":                return SKColors.Orange;
                case "orangered":             return SKColors.OrangeRed;
                case "orchid":                return SKColors.Orchid;
                case "palegoldenrod":         return SKColors.PaleGoldenrod;
                case "palegreen":             return SKColors.PaleGreen;
                case "paleturquoise":         return SKColors.PaleTurquoise;
                case "palevioletred":         return SKColors.PaleVioletRed;
                case "papayawhip":            return SKColors.PapayaWhip;
                case "peachpuff":             return SKColors.PeachPuff;
                case "peru":                  return SKColors.Peru;
                case "pink":                  return SKColors.Pink;
                case "plum":                  return SKColors.Plum;
                case "powderblue":            return SKColors.PowderBlue;
                case "purple":                return SKColors.Purple;
                case "red":                   return SKColors.Red;
                case "rosybrown":             return SKColors.RosyBrown;
                case "royalblue":             return SKColors.RoyalBlue;
                case "saddlebrown":           return SKColors.SaddleBrown;
                case "salmon":                return SKColors.Salmon;
                case "sandybrown":            return SKColors.SandyBrown;
                case "seagreen":              return SKColors.SeaGreen;
                case "seashell":              return SKColors.SeaShell;
                case "sienna":                return SKColors.Sienna;
                case "silver":                return SKColors.Silver;
                case "skyblue":               return SKColors.SkyBlue;
                case "slateblue":             return SKColors.SlateBlue;
                case "slategray":             return SKColors.SlateGray;
                case "snow":                  return SKColors.Snow;
                case "springgreen":           return SKColors.SpringGreen;
                case "steelblue":             return SKColors.SteelBlue;
                case "tan":                   return SKColors.Tan;
                case "teal":                  return SKColors.Teal;
                case "thistle":               return SKColors.Thistle;
                case "tomato":                return SKColors.Tomato;
                case "turquoise":             return SKColors.Turquoise;
                case "violet":                return SKColors.Violet;
                case "wheat":                 return SKColors.Wheat;
                case "white":                 return SKColors.White;
                case "whitesmoke":            return SKColors.WhiteSmoke;
                case "yellow":                return SKColors.Yellow;
                case "yellowgreen":           return SKColors.YellowGreen;
                case "transparent":           return SKColors.Transparent;
            }

            if (text.StartsWith("#")) {
                text = text.Substring(1);
            }

            if (text.Length == 6 || text.Length == 8) {
                if (!uint.TryParse(text.Substring(0, 2), NumberStyles.HexNumber, null, out uint p0))
                    return null;
                if (!uint.TryParse(text.Substring(2, 2), NumberStyles.HexNumber, null, out uint p1))
                    return null;
                if (!uint.TryParse(text.Substring(4, 2), NumberStyles.HexNumber, null, out uint p2))
                    return null;
                if (text.Length == 6) {
                    return new SKColor(p0 | (p1 << 8) | (p2 << 16));
                }
                else {
                    if (!uint.TryParse(text.Substring(6, 2), NumberStyles.HexNumber, null, out uint p3))
                        return null;
                    return new SKColor(p0 | (p1 << 8) | (p2 << 16) | (p3 << 24));
                }
            }

            string[] split = text.Split(',');
            if (split.Length == 3 || split.Length == 4) {
                uint[] array = new uint[split.Length];
                for (int i = 0; i < split.Length; i++) {
                    if (uint.TryParse(split[i], NumberStyles.Integer, null, out uint i0)) {
                        array[i] = i0;
                    }
                    else if (uint.TryParse(split[i], NumberStyles.Integer, null, out uint i1)) {
                        array[i] = i1;
                    }
                    else {
                        return null;
                    }
                }

                if (array.Length == 3) {
                    return new SKColor(array[0] | (array[1] << 8) | (array[2] << 16));
                }
                else {
                    return new SKColor(array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24));
                }
            }

            return null;
        }

        public static string ColourToString(SKColor col) {
            if (col == SKColors.AliceBlue)            return "AliceBlue";
            if (col == SKColors.AntiqueWhite)         return "AntiqueWhite";
            if (col == SKColors.Aqua)                 return "Aqua";
            if (col == SKColors.Aquamarine)           return "Aquamarine";
            if (col == SKColors.Azure)                return "Azure";
            if (col == SKColors.Beige)                return "Beige";
            if (col == SKColors.Bisque)               return "Bisque";
            if (col == SKColors.Black)                return "Black";
            if (col == SKColors.BlanchedAlmond)       return "BlanchedAlmond";
            if (col == SKColors.Blue)                 return "Blue";
            if (col == SKColors.BlueViolet)           return "BlueViolet";
            if (col == SKColors.Brown)                return "Brown";
            if (col == SKColors.BurlyWood)            return "BurlyWood";
            if (col == SKColors.CadetBlue)            return "CadetBlue";
            if (col == SKColors.Chartreuse)           return "Chartreuse";
            if (col == SKColors.Chocolate)            return "Chocolate";
            if (col == SKColors.Coral)                return "Coral";
            if (col == SKColors.CornflowerBlue)       return "CornflowerBlue";
            if (col == SKColors.Cornsilk)             return "Cornsilk";
            if (col == SKColors.Crimson)              return "Crimson";
            if (col == SKColors.Cyan)                 return "Cyan";
            if (col == SKColors.DarkBlue)             return "DarkBlue";
            if (col == SKColors.DarkCyan)             return "DarkCyan";
            if (col == SKColors.DarkGoldenrod)        return "DarkGoldenrod";
            if (col == SKColors.DarkGray)             return "DarkGray";
            if (col == SKColors.DarkGreen)            return "DarkGreen";
            if (col == SKColors.DarkKhaki)            return "DarkKhaki";
            if (col == SKColors.DarkMagenta)          return "DarkMagenta";
            if (col == SKColors.DarkOliveGreen)       return "DarkOliveGreen";
            if (col == SKColors.DarkOrange)           return "DarkOrange";
            if (col == SKColors.DarkOrchid)           return "DarkOrchid";
            if (col == SKColors.DarkRed)              return "DarkRed";
            if (col == SKColors.DarkSalmon)           return "DarkSalmon";
            if (col == SKColors.DarkSeaGreen)         return "DarkSeaGreen";
            if (col == SKColors.DarkSlateBlue)        return "DarkSlateBlue";
            if (col == SKColors.DarkSlateGray)        return "DarkSlateGray";
            if (col == SKColors.DarkTurquoise)        return "DarkTurquoise";
            if (col == SKColors.DarkViolet)           return "DarkViolet";
            if (col == SKColors.DeepPink)             return "DeepPink";
            if (col == SKColors.DeepSkyBlue)          return "DeepSkyBlue";
            if (col == SKColors.DimGray)              return "DimGray";
            if (col == SKColors.DodgerBlue)           return "DodgerBlue";
            if (col == SKColors.Firebrick)            return "Firebrick";
            if (col == SKColors.FloralWhite)          return "FloralWhite";
            if (col == SKColors.ForestGreen)          return "ForestGreen";
            if (col == SKColors.Fuchsia)              return "Fuchsia";
            if (col == SKColors.Gainsboro)            return "Gainsboro";
            if (col == SKColors.GhostWhite)           return "GhostWhite";
            if (col == SKColors.Gold)                 return "Gold";
            if (col == SKColors.Goldenrod)            return "Goldenrod";
            if (col == SKColors.Gray)                 return "Gray";
            if (col == SKColors.Green)                return "Green";
            if (col == SKColors.GreenYellow)          return "GreenYellow";
            if (col == SKColors.Honeydew)             return "Honeydew";
            if (col == SKColors.HotPink)              return "HotPink";
            if (col == SKColors.IndianRed)            return "IndianRed";
            if (col == SKColors.Indigo)               return "Indigo";
            if (col == SKColors.Ivory)                return "Ivory";
            if (col == SKColors.Khaki)                return "Khaki";
            if (col == SKColors.Lavender)             return "Lavender";
            if (col == SKColors.LavenderBlush)        return "LavenderBlush";
            if (col == SKColors.LawnGreen)            return "LawnGreen";
            if (col == SKColors.LemonChiffon)         return "LemonChiffon";
            if (col == SKColors.LightBlue)            return "LightBlue";
            if (col == SKColors.LightCoral)           return "LightCoral";
            if (col == SKColors.LightCyan)            return "LightCyan";
            if (col == SKColors.LightGoldenrodYellow) return "LightGoldenrodYellow";
            if (col == SKColors.LightGray)            return "LightGray";
            if (col == SKColors.LightGreen)           return "LightGreen";
            if (col == SKColors.LightPink)            return "LightPink";
            if (col == SKColors.LightSalmon)          return "LightSalmon";
            if (col == SKColors.LightSeaGreen)        return "LightSeaGreen";
            if (col == SKColors.LightSkyBlue)         return "LightSkyBlue";
            if (col == SKColors.LightSlateGray)       return "LightSlateGray";
            if (col == SKColors.LightSteelBlue)       return "LightSteelBlue";
            if (col == SKColors.LightYellow)          return "LightYellow";
            if (col == SKColors.Lime)                 return "Lime";
            if (col == SKColors.LimeGreen)            return "LimeGreen";
            if (col == SKColors.Linen)                return "Linen";
            if (col == SKColors.Magenta)              return "Magenta";
            if (col == SKColors.Maroon)               return "Maroon";
            if (col == SKColors.MediumAquamarine)     return "MediumAquamarine";
            if (col == SKColors.MediumBlue)           return "MediumBlue";
            if (col == SKColors.MediumOrchid)         return "MediumOrchid";
            if (col == SKColors.MediumPurple)         return "MediumPurple";
            if (col == SKColors.MediumSeaGreen)       return "MediumSeaGreen";
            if (col == SKColors.MediumSlateBlue)      return "MediumSlateBlue";
            if (col == SKColors.MediumSpringGreen)    return "MediumSpringGreen";
            if (col == SKColors.MediumTurquoise)      return "MediumTurquoise";
            if (col == SKColors.MediumVioletRed)      return "MediumVioletRed";
            if (col == SKColors.MidnightBlue)         return "MidnightBlue";
            if (col == SKColors.MintCream)            return "MintCream";
            if (col == SKColors.MistyRose)            return "MistyRose";
            if (col == SKColors.Moccasin)             return "Moccasin";
            if (col == SKColors.NavajoWhite)          return "NavajoWhite";
            if (col == SKColors.Navy)                 return "Navy";
            if (col == SKColors.OldLace)              return "OldLace";
            if (col == SKColors.Olive)                return "Olive";
            if (col == SKColors.OliveDrab)            return "OliveDrab";
            if (col == SKColors.Orange)               return "Orange";
            if (col == SKColors.OrangeRed)            return "OrangeRed";
            if (col == SKColors.Orchid)               return "Orchid";
            if (col == SKColors.PaleGoldenrod)        return "PaleGoldenrod";
            if (col == SKColors.PaleGreen)            return "PaleGreen";
            if (col == SKColors.PaleTurquoise)        return "PaleTurquoise";
            if (col == SKColors.PaleVioletRed)        return "PaleVioletRed";
            if (col == SKColors.PapayaWhip)           return "PapayaWhip";
            if (col == SKColors.PeachPuff)            return "PeachPuff";
            if (col == SKColors.Peru)                 return "Peru";
            if (col == SKColors.Pink)                 return "Pink";
            if (col == SKColors.Plum)                 return "Plum";
            if (col == SKColors.PowderBlue)           return "PowderBlue";
            if (col == SKColors.Purple)               return "Purple";
            if (col == SKColors.Red)                  return "Red";
            if (col == SKColors.RosyBrown)            return "RosyBrown";
            if (col == SKColors.RoyalBlue)            return "RoyalBlue";
            if (col == SKColors.SaddleBrown)          return "SaddleBrown";
            if (col == SKColors.Salmon)               return "Salmon";
            if (col == SKColors.SandyBrown)           return "SandyBrown";
            if (col == SKColors.SeaGreen)             return "SeaGreen";
            if (col == SKColors.SeaShell)             return "SeaShell";
            if (col == SKColors.Sienna)               return "Sienna";
            if (col == SKColors.Silver)               return "Silver";
            if (col == SKColors.SkyBlue)              return "SkyBlue";
            if (col == SKColors.SlateBlue)            return "SlateBlue";
            if (col == SKColors.SlateGray)            return "SlateGray";
            if (col == SKColors.Snow)                 return "Snow";
            if (col == SKColors.SpringGreen)          return "SpringGreen";
            if (col == SKColors.SteelBlue)            return "SteelBlue";
            if (col == SKColors.Tan)                  return "Tan";
            if (col == SKColors.Teal)                 return "Teal";
            if (col == SKColors.Thistle)              return "Thistle";
            if (col == SKColors.Tomato)               return "Tomato";
            if (col == SKColors.Turquoise)            return "Turquoise";
            if (col == SKColors.Violet)               return "Violet";
            if (col == SKColors.Wheat)                return "Wheat";
            if (col == SKColors.White)                return "White";
            if (col == SKColors.WhiteSmoke)           return "WhiteSmoke";
            if (col == SKColors.Yellow)               return "Yellow";
            if (col == SKColors.YellowGreen)          return "YellowGreen";
            if (col == SKColors.Transparent)          return "Transparent";

            // could convert col.GetHashCode() to hex as the GetHashCode function
            // just returns the internal ARGB value, but this is safer
            return $"#{col.Alpha:X}{col.Red:X}{col.Green:X}{col.Blue:X}";
        }
    }
}
