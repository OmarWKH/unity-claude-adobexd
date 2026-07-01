using UnityEngine;

namespace Nostrivia
{
    /// <summary>
    /// The Nostrivia design color tokens, read from the XD prototype's Specs mode.
    /// Single source of truth for every color used in the UI.
    /// </summary>
    public static class NostriviaColors
    {
        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        public static readonly Color White        = Hex("#FFFFFF"); // option buttons, card fills
        public static readonly Color SkyBlue      = Hex("#A9C4F0"); // screen background
        public static readonly Color MintGreen    = Hex("#A8E0C2"); // correct answer, Play Again, "Excellent!"
        public static readonly Color Pink         = Hex("#DDA8D2"); // Play / Submit(enabled) / Results
        public static readonly Color Coral        = Hex("#E2917F"); // wrong answer, WARNING bar, Leave
        public static readonly Color Periwinkle   = Hex("#6F7FE0"); // selected option, dialog title bars, badge
        public static readonly Color Navy         = Hex("#26233B"); // all text and outlines
        public static readonly Color LavenderGray = Hex("#D0CBDD"); // disabled Submit
        public static readonly Color Gold         = Hex("#EFC583"); // timer pill, ornament
    }
}
