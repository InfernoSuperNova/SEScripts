using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;

namespace IngameScript
{
    public static class ALDebug
    {
        public static List<IMyTextPanel> panels;
        public static MyGridProgram program;

        public static StringBuilder sb = new StringBuilder();

        public static void InitializePanels(List<IMyTextPanel> panels)
        {
            ALDebug.panels = panels;
            foreach (var panel in panels) panel.ContentType = ContentType.TEXT_AND_IMAGE;
        }

        public static void AddText(string text)
        {
            sb.AppendLine(text);
        }

        public static void WriteText()
        {
            var text = sb.ToString();
            program.Echo(text);
        }

        public static void Echo(object obj)
        {
            string thing = obj?.ToString();
            program.Echo(thing);
        }
    }
}