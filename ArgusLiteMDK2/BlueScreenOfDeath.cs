using System;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    internal static class BlueScreenOfDeath
    {
        private const int MAX_BSOD_WIDTH = 50;

        private const string BSOD_TEMPLATE =
            "{0} - v{1}\n\n" +
            "A fatal exception has occured at\n" +
            "{2}. The current\n" +
            "program will be terminated.\n" +
            "\n" +
            "EXCEPTION:\n" +
            "{3}\n" +
            "\n" +
            "* Please screenshot this crash screen and\n" +
            "  DM to DeltaWing or\n" +
            "  Post in the #script-babble channel\n" +
            "\n" +
            "* Press RECOMPILE to restart the program";

        private static readonly StringBuilder bsodBuilder = new StringBuilder(256);

        public static void Show(IMyTextSurface surface, string scriptName, string version, Exception e)
        {
            if (surface == null) return;
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.Alignment = TextAlignment.LEFT;
            var scaleFactor = 512f / Math.Min(surface.TextureSize.X, surface.TextureSize.Y);
            surface.FontSize = scaleFactor * surface.TextureSize.X / (19.5f * MAX_BSOD_WIDTH);
            surface.FontColor = Color.White;
            surface.BackgroundColor = Color.Blue;
            surface.Font = "Monospace";
            var exceptionStr = e.ToString();
            var exceptionLines = exceptionStr.Split('\n');
            bsodBuilder.Clear();
            foreach (var line in exceptionLines)
                if (line.Length <= MAX_BSOD_WIDTH)
                {
                    bsodBuilder.Append(line).Append("\n");
                }
                else
                {
                    var words = line.Split(' ');
                    var lineLength = 0;
                    foreach (var word in words)
                    {
                        lineLength += word.Length;
                        if (lineLength >= MAX_BSOD_WIDTH)
                        {
                            bsodBuilder.Append("\n");
                            lineLength = word.Length;
                        }

                        bsodBuilder.Append(word).Append(" ");
                        lineLength += 1;
                    }

                    bsodBuilder.Append("\n");
                }

            surface.WriteText(string.Format(BSOD_TEMPLATE,
                scriptName.ToUpperInvariant(),
                version,
                DateTime.Now,
                bsodBuilder));
        }
    }
}