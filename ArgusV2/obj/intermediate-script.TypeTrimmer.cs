using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using System.Collections;
using System.Text;
using VRage.Game.ModAPI.Ingame;
using System.Linq;
using Color = VRageMath.Color;
using VRage.Game.ModAPI.Ingame.Utilities;
using Sandbox.Game.EntityComponents;
using VRage.Game;
using VRage.Game.ObjectBuilders.Definitions;
using SpaceEngineers.Game.ModAPI.Ingame;
using Base6Directions = VRageMath.Base6Directions;
using VRage.Library.Collections;
using VRage.Utils;
class Program : MyGridProgram
{
    public static Program I;
    public static DebugAPI Debug;
    public static Random RNG;
    public TimedLog _Log = new TimedLog(10);
        
    int _frame;
    public Program()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            _Log.Add("Beginning script setup", LogLevel.Info);
            I = this;
            Debug = new DebugAPI(this);
            RNG = new Random();
            LogLine("Setup debug and RNG", LogLevel.Debug);
            Config.Setup(Me);
            Program.LogLine("Creating this ship as controllable ship", LogLevel.Info);
            ShipManager.CreateControllableShip(Me.CubeGrid, GridTerminalSystem);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
                
            var elapsed = DateTime.UtcNow - startTime;
            LogLine($"Setup completed in {elapsed.TotalMilliseconds:F1} ms", LogLevel.Highlight);
        }
        catch (Exception ex)
        {
            Echo("Crashed: " + ex);
        }
        Echo(_Log.ToString());
    }
        
    public void Main(string argument, UpdateType updateSource)
    {
        try
        {
            if ((updateSource & UpdateType.Update1) != 0) RunUpdate();
            if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0) RunCommand(argument);
        }
        catch (Exception ex)
        {
            Echo(ex.ToString());
            Runtime.UpdateFrequency = UpdateFrequency.None;
        }
    }



    void RunUpdate()
    {
            
        TimerManager.TickAll();
        ShipManager.EarlyUpdate(_frame);
        ShipManager.LateUpdate(_frame++);
            
        _Log.Update();
        Log(_Log);
    }
    void RunCommand(string argument)
    {
        Action action;
        if (Config.Commands.TryGetValue(argument, out action))
        {
            action();
        }
        else
        {
            // Optional: handle unknown command
            // e.g., Log("Unknown command: " + argument);
        }
    }
    public static void Log(object obj)
    {
        I.Echo(obj.ToString());
    }

    public static void Log(TimeSpan elapsed, string hint)
    {
        double microseconds = elapsed.Ticks / 10.0;
        I.Echo($"{hint}: {microseconds} µs");
    }
        
    public static void LogLine(object echo, LogLevel level = LogLevel.Info)
    {
        I._Log.Add(echo.ToString(), level);
    }
}
public class DebugAPI
    {
        public readonly bool ModDetected;

        public void RemoveDraw() => _rmd?.Invoke(_pb);
        Action<IMyProgrammableBlock> _rmd;

        public void RemoveAll() => _rma?.Invoke(_pb);
        Action<IMyProgrammableBlock> _rma;

        public void Remove(int id) => _rm?.Invoke(_pb, id);
        Action<IMyProgrammableBlock, int> _rm;

        public int DrawPoint(Vector3D origin, Color color, float radius = 0.2f, float seconds = DefaultSeconds, bool? onTop = null) => _p?.Invoke(_pb, origin, color, radius, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, Vector3D, Color, float, float, bool, int> _p;

        public int DrawLine(Vector3D start, Vector3D end, Color color, float thickness = DefaultThickness, float seconds = DefaultSeconds, bool? onTop = null) => _ln?.Invoke(_pb, start, end, color, thickness, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, Vector3D, Vector3D, Color, float, float, bool, int> _ln;

        public int DrawAABB(BoundingBoxD bb, Color color, Style style = Style.Wireframe, float thickness = DefaultThickness, float seconds = DefaultSeconds, bool? onTop = null) => _bb?.Invoke(_pb, bb, color, (int)style, thickness, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, BoundingBoxD, Color, int, float, float, bool, int> _bb;

        public int DrawOBB(MyOrientedBoundingBoxD obb, Color color, Style style = Style.Wireframe, float thickness = DefaultThickness, float seconds = DefaultSeconds, bool? onTop = null) => _obb?.Invoke(_pb, obb, color, (int)style, thickness, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, MyOrientedBoundingBoxD, Color, int, float, float, bool, int> _obb;

        public int DrawSphere(BoundingSphereD sphere, Color color, Style style = Style.Wireframe, float thickness = DefaultThickness, int lineEveryDegrees = 15, float seconds = DefaultSeconds, bool? onTop = null) => _sph?.Invoke(_pb, sphere, color, (int)style, thickness, lineEveryDegrees, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, BoundingSphereD, Color, int, float, int, float, bool, int> _sph;

        public int DrawMatrix(MatrixD matrix, float length = 1f, float thickness = DefaultThickness, float seconds = DefaultSeconds, bool? onTop = null) => _m?.Invoke(_pb, matrix, length, thickness, seconds, onTop ?? _defTop) ?? -1;
        Func<IMyProgrammableBlock, MatrixD, float, float, float, bool, int> _m;

        public int DrawGPS(string name, Vector3D origin, Color? color = null, float seconds = DefaultSeconds) => _gps?.Invoke(_pb, name, origin, color, seconds) ?? -1;
        Func<IMyProgrammableBlock, string, Vector3D, Color?, float, int> _gps;

        public int PrintHUD(string message, Font font = Font.Debug, float seconds = 2) => _hud?.Invoke(_pb, message, font.ToString(), seconds) ?? -1;
        Func<IMyProgrammableBlock, string, string, float, int> _hud;

        public void PrintChat(string message, string sender = null, Color? senderColor = null, Font font = Font.Debug) => _chat?.Invoke(_pb, message, sender, senderColor, font.ToString());
        Action<IMyProgrammableBlock, string, string, Color?, string> _chat;

        public void DeclareAdjustNumber(out int id, double initial, double step = 0.05, Input modifier = Input.Control, string label = null) => id = _adj?.Invoke(_pb, initial, step, modifier.ToString(), label) ?? -1;
        Func<IMyProgrammableBlock, double, double, string, string, int> _adj;

        public double GetAdjustNumber(int id, double noModDefault = 1) => _getAdj?.Invoke(_pb, id) ?? noModDefault;
        Func<IMyProgrammableBlock, int, double> _getAdj;

        public int GetTick() => _tk?.Invoke() ?? -1;
        Func<int> _tk;

        public TimeSpan GetTimestamp() => _ts?.Invoke() ?? TimeSpan.Zero;
        Func<TimeSpan> _ts;

        public MeasureToken Measure(Action<TimeSpan> call) => new MeasureToken(this, call);
        public struct MeasureToken : IDisposable
        {
            DebugAPI A; TimeSpan S; Action<TimeSpan> C;
            public MeasureToken(DebugAPI api, Action<TimeSpan> call) { A = api; C = call; S = A.GetTimestamp(); }
            public void Dispose() { C?.Invoke(A.GetTimestamp() - S); }
        }

        public enum Style { Solid, Wireframe, SolidAndWireframe }
        public enum Input { MouseLeftButton, MouseRightButton, MouseMiddleButton, MouseExtraButton1, MouseExtraButton2, LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt, Tab, Shift, Control, Alt, Space, PageUp, PageDown, End, Home, Insert, Delete, Left, Up, Right, Down, D0, D1, D2, D3, D4, D5, D6, D7, D8, D9, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, NumPad0, NumPad1, NumPad2, NumPad3, NumPad4, NumPad5, NumPad6, NumPad7, NumPad8, NumPad9, Multiply, Add, Separator, Subtract, Decimal, Divide, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12 }
        public enum Font { Debug, White, Red, Green, Blue, DarkBlue }
        const float DefaultThickness = 0.02f;
        const float DefaultSeconds = -1;
        IMyProgrammableBlock _pb;
        bool _defTop;
        public DebugAPI(MyGridProgram program, bool drawOnTopDefault = false)
        {
            if(program == null) throw new Exception("Pass `this` into the API, not null.");
            _defTop = drawOnTopDefault;
            _pb = program.Me;
            var methods = _pb.GetProperty("DebugAPI")?.As<IReadOnlyDictionary<string, Delegate>>()?.GetValue(_pb);
            if(methods != null)
            {
                Assign(out _rma, methods["RemoveAll"]);
                Assign(out _rmd, methods["RemoveDraw"]);
                Assign(out _rm, methods["Remove"]);
                Assign(out _p, methods["Point"]);
                Assign(out _ln, methods["Line"]);
                Assign(out _bb, methods["AABB"]);
                Assign(out _obb, methods["OBB"]);
                Assign(out _sph, methods["Sphere"]);
                Assign(out _m, methods["Matrix"]);
                Assign(out _gps, methods["GPS"]);
                Assign(out _hud, methods["HUDNotification"]);
                Assign(out _chat, methods["Chat"]);
                Assign(out _adj, methods["DeclareAdjustNumber"]);
                Assign(out _getAdj, methods["GetAdjustNumber"]);
                Assign(out _tk, methods["Tick"]);
                Assign(out _ts, methods["Timestamp"]);
                RemoveAll();
                ModDetected = true;
            }
        }
        void Assign<T>(out T field, object method) => field = (T)method;
    }
    public static class Dwon
    {
        public class Comment
        {
            public object Obj;
            public string Com;

            public Comment(object obj, string comment)
            {
                Obj = obj;
                Com = comment;
            }
        }


        public static string Serialize(object obj, int indent = -1)
        {
            var sb = new StringBuilder();
            Serialize(obj, sb, indent, null);
            Program.LogLine("Serialized successfully", LogLevel.Debug);
            return sb.ToString();
        }

        private static void Serialize(object obj, StringBuilder sb, int indent, string key)
        {
            string pad = new string(' ', Math.Max(indent, 0));

            if (obj == null)
            {
                if (key != null) sb.AppendLine(pad + key + " = null");
                return;
            }

            Comment commentObj = obj as Comment;
            if (commentObj != null)
            {
                bool isPrimitive = IsPrimitive(commentObj.Obj);
                if (isPrimitive && key != null)
                {
                    sb.AppendLine(pad + key + " = " + ValueToString(commentObj.Obj) + "   # " + commentObj.Com);
                }
                else
                {
                    if (!string.IsNullOrEmpty(commentObj.Com))
                        sb.AppendLine(pad + "# " + commentObj.Com);
                    Serialize(commentObj.Obj, sb, indent, key);
                }
                return;
            }

            IDictionary<string, object> dict = obj as IDictionary<string, object>;
            if (dict != null)
            {
                if (key != null) sb.AppendLine(pad + key + " = [");
                foreach (var kv in dict)
                {
                    Serialize(kv.Value, sb, indent + 2, kv.Key);
                }
                if (key != null) sb.AppendLine(pad + "]");
                return;
            }

            IEnumerable<object> list = obj as IEnumerable<object>;
            if (list != null)
            {
                if (key != null)
                {
                    sb.Append(pad + key + " = { ");
                    bool first = true;
                    foreach (object v in list)
                    {
                        if (!first) sb.Append(", ");
                        sb.Append(ValueToString(v));
                        first = false;
                    }
                    sb.AppendLine(" }");
                }
                else
                {
                    foreach (object v in list)
                        Serialize(v, sb, indent, null);
                }
                return;
            }

            if (key != null)
            {
                sb.AppendLine(pad + key + " = " + ValueToString(obj));
            }
        }

        private static string ValueToString(object obj)
        {
            if (obj == null) return "null";
            if (obj is string) return "\"" + Escape((string)obj) + "\"";
            if (obj is bool) return ((bool)obj ? "true" : "false");
            if (obj is float) return ((float)obj).ToString("0.#####", System.Globalization.CultureInfo.InvariantCulture);
            if (obj is double) return ((double)obj).ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture); 
            // ~10 decimal places, enough for SE precision
            if (obj is int || obj is long || obj is short || obj is byte) return obj.ToString();
            return "\"" + Escape(obj.ToString()) + "\"";
        }

        private static string Escape(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static bool IsPrimitive(object value)
        {
            return value is string || value is bool ||
                   value is int || value is long || value is short || value is byte ||
                   value is float || value is double;
        }



        public static object Parse(string dwon)
        {
            int idx = 0;
            var top = new Dictionary<string, object>();
            while (idx < dwon.Length)
            {
                SkipWhitespaceAndComments(dwon, ref idx);
                if (idx >= dwon.Length) break;

                string key = ParseKey(dwon, ref idx);
                Program.Log(key);
                ExpectChar(dwon, ref idx, '=');
                object value = ParseValue(dwon, ref idx);
                string comment = ParseInlineComment(dwon, ref idx);
                if (comment != null && IsPrimitive(value))
                    value = new Comment(value, comment);

                top[key] = value;
            }
            return top;
        }

        private static object ParseValue(string s, ref int idx)
        {
            SkipWhitespaceAndComments(s, ref idx);

            if (idx >= s.Length) return null;

            char c = s[idx];

            switch (c)
            {
                // Dictionary / object
                case '[':
                {
                    idx++;
                    var dict = new Dictionary<string, object>();
                    while (true)
                    {
                        SkipWhitespaceAndComments(s, ref idx);
                        if (idx >= s.Length) break;
                        if (s[idx] == ']') { idx++; break; }

                        string key = ParseKey(s, ref idx);
                        ExpectChar(s, ref idx, '=');
                        object value = ParseValue(s, ref idx);

                        // attach inline comment if exists
                        string comment = ParseInlineComment(s, ref idx);
                        if (comment != null && IsPrimitive(value))
                            value = new Comment(value, comment);

                        dict[key] = value;
                    }
                    return dict;
                }
                // Array / list
                case '{':
                {
                    idx++;
                    var list = new List<object>();
                    while (true)
                    {
                        SkipWhitespaceAndComments(s, ref idx);
                        if (idx >= s.Length) break;
                        if (s[idx] == '}') { idx++; break; }

                        object value = ParseValue(s, ref idx);
                        string comment = ParseInlineComment(s, ref idx);
                        if (comment != null && IsPrimitive(value))
                            value = new Comment(value, comment);

                        list.Add(value);

                        SkipWhitespaceAndComments(s, ref idx);
                        if (idx < s.Length && s[idx] == ',') idx++; // optional comma
                    }
                    return list;
                }
            }

            // Primitive or string
            string token = ParseToken(s, ref idx);
            string inlineComment = ParseInlineComment(s, ref idx);
            object primitive = ParsePrimitive(token);
            if (inlineComment != null && IsPrimitive(primitive))
                primitive = new Comment(primitive, inlineComment);
            return primitive;
        }

        private static void SkipWhitespaceAndComments(string s, ref int idx)
        {
            while (idx < s.Length)
            {
                if (s[idx] == ' ' || s[idx] == '\t' || s[idx] == '\r' || s[idx] == '\n')
                {
                    idx++;
                    continue;
                }

                if (s[idx] == '#')
                {
                    while (idx < s.Length && s[idx] != '\n') idx++;
                    continue;
                }
                break;
            }
        }

        private static string ParseKey(string s, ref int idx)
        {
            SkipWhitespace(s, ref idx);

            int start = idx;

            while (idx < s.Length)
            {
                char c = s[idx];

                // key ends ONLY at '='
                if (c == '=')
                    break;

                // stop if we hit newline before seeing '=' → malformed key
                if (c == '\n' || c == '\r')
                    throw new Exception("Unexpected newline while reading key");

                idx++;
            }

            if (idx == start)
                throw new Exception($"Empty key at index {idx}");

            string key = s.Substring(start, idx - start).Trim();

            return key;
        }

        private static string ParseToken(string s, ref int idx)
        {
            SkipWhitespace(s, ref idx);
            if (idx >= s.Length) return "";

            char c = s[idx];

            // Quoted string
            if (c == '"' || c == '\'')
            {
                char quote = c;
                idx++;
                int start = idx;
                while (idx < s.Length && s[idx] != quote) idx++;
                string str = s.Substring(start, idx - start);
                if (idx < s.Length) idx++; // skip closing quote
                return str;
            }

            // Unquoted token
            int startToken = idx;
            while (idx < s.Length && s[idx] != '\n' && s[idx] != '\r' && s[idx] != '#' && s[idx] != ',' && s[idx] != '}' && s[idx] != ']') idx++;
            string token = s.Substring(startToken, idx - startToken).Trim();
            return token;
        }

        private static string ParseInlineComment(string s, ref int idx)
        {
            SkipWhitespace(s, ref idx);
            if (idx < s.Length && s[idx] == '#')
            {
                idx++; // skip #
                int start = idx;
                while (idx < s.Length && s[idx] != '\n' && s[idx] != '\r') idx++;
                return s.Substring(start, idx - start).Trim();
            }
            return null;
        }

        private static void SkipWhitespace(string s, ref int idx)
        {
            while (idx < s.Length && (s[idx] == ' ' || s[idx] == '\t' || s[idx] == '\r' || s[idx] == '\n')) idx++;
        }

        private static void ExpectChar(string s, ref int idx, char expected)
        {
            SkipWhitespace(s, ref idx);
            if (idx >= s.Length || s[idx] != expected) throw new Exception("Expected '" + expected + "' at index " + idx);
            idx++;
        }

        private static object ParsePrimitive(string token)
        {
            if (token.Length == 0) return null;

            if (token == "true") return true;
            if (token == "false") return false;

            int i;
            if (int.TryParse(token, out i)) return i;

            double f;
            if (double.TryParse(token, System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out f)) return f;

            return token; // string fallback
        }

        
        // Helper: recursively unwrap all Dwon.Comment objects
        public static void UnwrapAllComments(Dictionary<string, object> dict)
        {
            var keys = new List<string>(dict.Keys);
            foreach (var key in keys)
            {
                var val = dict[key];
                var c = val as Comment;
                var nestedDict = val as Dictionary<string, object>;
                var list = val as List<object>;
                if (c != null)
                    dict[key] = c.Obj;
                
                else if (nestedDict != null)
                    UnwrapAllComments(nestedDict);
                else if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var listItem = list[i];
                        var cl = listItem as Comment;
                        list[i] = cl != null ? cl.Obj : list[i];
                    }
                        
                }
            }
        }

// Helper: safely get value, returns default if missing
        public static T GetValue<T>(Dictionary<string, object> dict, string key, T defaultValue = default(T))
        {
            object value;
            if (dict.TryGetValue(key, out value))
            {
                // Handle value types and reference types separately
                if (value is T) return (T)value;

                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }
public enum Direction : byte
{
    /// <summary>
    /// 
    /// </summary>
    Forward,
    /// <summary>
    /// 
    /// </summary>
    Backward,
    /// <summary>
    /// 
    /// </summary>
    Left,
    /// <summary>
    /// 
    /// </summary>
    Right,
    /// <summary>
    /// 
    /// </summary>
    Up,
    /// <summary>
    /// 
    /// </summary>
    Down,
}
public static class DynamicAABBTreeDHelper
{
    private static readonly List<TrackableShip> Elements = new List<TrackableShip>();
    private static readonly List<BoundingBoxD> Boxes = new List<BoundingBoxD>();

    private static readonly Dictionary<TrackableShip, List<TrackableShip>> ReusedOverlaps =
        new Dictionary<TrackableShip, List<TrackableShip>>();

    private static readonly List<List<TrackableShip>> ReusedLists = new List<List<TrackableShip>>();

    private static int _rI = 0;
    // Diabolical
    private static List<TrackableShip> GetList()
    {
        if (_rI >= ReusedLists.Count)
            ReusedLists.Add(new List<TrackableShip>(8));

        var list = ReusedLists[_rI++];
        list.Clear();
        return list;
    }
        
    public static Dictionary<TrackableShip, List<TrackableShip>> GetAllOverlaps(MyDynamicAABBTreeD tree)
    {
        _rI = 0;
        ReusedOverlaps.Clear();
        // Get all entities and their AABBs
        tree.GetAll(Elements, clear: true, boxsList: Boxes);

        for (int i = 0; i < Elements.Count; i++)
        {
            var entityA = Elements[i];
            var boxA = Boxes[i];

            var overlapping = GetList();
            tree.OverlapAllBoundingBox(ref boxA, overlapping, 0U, false);

            foreach (var entityB in overlapping)
            {
                if (entityA == entityB) continue;
                    
                if (!ReusedOverlaps.ContainsKey(entityA)) ReusedOverlaps.Add(entityA, GetList());
                ReusedOverlaps[entityA].Add(entityB);
                    
                if (!ReusedOverlaps.ContainsKey(entityB)) ReusedOverlaps.Add(entityB, GetList());
                ReusedOverlaps[entityB].Add(entityA);
                    
            }
        }

        return ReusedOverlaps;
    }
    public static List<TrackableShip> GetOverlapsWithBox(MyDynamicAABBTreeD tree, BoundingBoxD box)
    {
        _rI = 0;
        // Get a temporary list from the pool
        var overlapping = GetList();

        // Query the tree for all leaf nodes overlapping the given box
        tree.OverlapAllBoundingBox(ref box, overlapping, 0U, false);

        return overlapping;
    }
}
public static class OBBReconstructor
{
        
    /// <summary>
    /// Checks if a hypothetical OBB (defined by its integer grid extents)
    /// is fully contained within the world-space AABB using SAT.
    /// </summary>
    private static bool IsObbContained(
        long i1, long i2, long i3,
        double gridSize,
        MatrixD obbRotation,
        Vector3D worldHalfExtents)
    {
        // 1. Calculate the OBB's continuous half-extents.
        double r1 = i1 * gridSize;
        double r2 = i2 * gridSize;
        double r3 = i3 * gridSize;
        
        // 2. Get the OBB's local axes and absolute components.
        Vector3D u1 = obbRotation.Right;
        Vector3D u2 = obbRotation.Up;
        Vector3D u3 = obbRotation.Forward;
        
        Vector3D abs_u1 = Vector3D.Abs(u1);
        Vector3D abs_u2 = Vector3D.Abs(u2);
        Vector3D abs_u3 = Vector3D.Abs(u3);
        
        // 3. Perform the Separating Axis Theorem (SAT) check.
        // Check projection onto World X, Y, Z axes.
            
        // Robust Tolerance: Prevents premature pruning due to floating point inaccuracies 
        // when OBB projections are extremely close to the AABB boundaries (hard diagonals).
        const double RobustTolerance = 1e-8; 
        
        // Projected Radius onto World X-axis: R_x = r1|u1.X| + r2|u2.X| + r3|u3.X|
        double projectedRadiusX = r1 * abs_u1.X + r2 * abs_u2.X + r3 * abs_u3.X;
        if (projectedRadiusX > worldHalfExtents.X + RobustTolerance) return false;
        
        // Projected Radius onto World Y-axis: R_y = r1|u1.Y| + r2|u2.Y| + r3|u3.Y|
        double projectedRadiusY = r1 * abs_u1.Y + r2 * abs_u2.Y + r3 * abs_u3.Y;
        if (projectedRadiusY > worldHalfExtents.Y + RobustTolerance) return false;
        
        // Projected Radius onto World Z-axis: R_z = r1|u1.Z| + r2 * abs_u2.Z + r3 * abs_u3.Z;
        double projectedRadiusZ = r1 * abs_u1.Z + r2 * abs_u2.Z + r3 * abs_u3.Z;
        if (projectedRadiusZ > worldHalfExtents.Z + RobustTolerance) return false;
        
        return true;
    }
        
    // Helper: Try to solve A * r = H for r = [r1,r2,r3].
    // A_{row, col} = abs(u_col.{X,Y,Z})
    private static bool TrySolveContinuousHalfExtents(
        Vector3D u1, Vector3D u2, Vector3D u3,
        Vector3D worldHalfExtents,
        out double r1, out double r2, out double r3)
    {
        var a11 = Math.Abs(u1.X); var a12 = Math.Abs(u2.X); var a13 = Math.Abs(u3.X);
        var a21 = Math.Abs(u1.Y); var a22 = Math.Abs(u2.Y); var a23 = Math.Abs(u3.Y);
        var a31 = Math.Abs(u1.Z); var a32 = Math.Abs(u2.Z); var a33 = Math.Abs(u3.Z);

        double Hx = worldHalfExtents.X, Hy = worldHalfExtents.Y, Hz = worldHalfExtents.Z;

        // Determinant
        double det = a11 * (a22 * a33 - a23 * a32)
                   - a12 * (a21 * a33 - a23 * a31)
                   + a13 * (a21 * a32 - a22 * a31);

        const double EPS = 1e-12;
        if (Math.Abs(det) < EPS)
        {
            r1 = r2 = r3 = 0.0;
            return false;
        }

        // Inverse (adjugate / det)
        double inv11 =  (a22 * a33 - a23 * a32) / det;
        double inv12 = -(a12 * a33 - a13 * a32) / det;
        double inv13 =  (a12 * a23 - a13 * a22) / det;

        double inv21 = -(a21 * a33 - a23 * a31) / det;
        double inv22 =  (a11 * a33 - a13 * a31) / det;
        double inv23 = -(a11 * a23 - a13 * a21) / det;

        double inv31 =  (a21 * a32 - a22 * a31) / det;
        double inv32 = -(a11 * a32 - a12 * a31) / det;
        double inv33 =  (a11 * a22 - a12 * a21) / det;

        r1 = inv11 * Hx + inv12 * Hy + inv13 * Hz;
        r2 = inv21 * Hx + inv22 * Hy + inv23 * Hz;
        r3 = inv31 * Hx + inv32 * Hy + inv33 * Hz;

        // All radii must be non-negative (tiny negative due to FP -> clamp)
        const double NEG_CLAMP = -1e-9;
        if (r1 < NEG_CLAMP || r2 < NEG_CLAMP || r3 < NEG_CLAMP) return false;

        r1 = Math.Max(0.0, r1);
        r2 = Math.Max(0.0, r2);
        r3 = Math.Max(0.0, r3);

        return true;
    }

    public static BoundingBoxD GetMaxInscribedOBB(
        BoundingBoxD worldAabb,
        MatrixD obbRotation,
        double gridSize)
    {
        gridSize /= 2;
            
        Vector3D worldHalfExtents = worldAabb.HalfExtents;
        Vector3D u1 = obbRotation.Right;
        Vector3D u2 = obbRotation.Up;
        Vector3D u3 = obbRotation.Forward;

        // Try direct linear solve first (this will typically recover the original slender box)
        double r1c, r2c, r3c;
        if (TrySolveContinuousHalfExtents(u1, u2, u3, worldHalfExtents, out r1c, out r2c, out r3c))
        {
            long i1 = Math.Max(0, (long)(r1c / gridSize + 1e-12));
            long i2 = Math.Max(0, (long)(r2c / gridSize + 1e-12));
            long i3 = Math.Max(0, (long)(r3c / gridSize + 1e-12));

                
            // Greedily grow each axis by +1 while it still fits (reclaim integer slack). This helps eliminate weird edge cases
            bool changed;
            do
            {
                changed = false;
                if (IsObbContained(i1 + 1, i2, i3, gridSize, obbRotation, worldHalfExtents)) { i1++; changed = true; }
                if (IsObbContained(i1, i2 + 1, i3, gridSize, obbRotation, worldHalfExtents)) { i2++; changed = true; }
                if (IsObbContained(i1, i2, i3 + 1, gridSize, obbRotation, worldHalfExtents)) { i3++; changed = true; }
            } while (changed);
                
                
            var obbHalfExtents = new Vector3D(i1 * gridSize, i2 * gridSize, i3 * gridSize);
            return new BoundingBoxD(-obbHalfExtents, obbHalfExtents);
        }
            
        double maxRadiusWorldX = worldHalfExtents.X / (Math.Abs(u1.X) + Math.Abs(u2.X) + Math.Abs(u3.X));
        double maxRadiusWorldY = worldHalfExtents.Y / (Math.Abs(u1.Y) + Math.Abs(u2.Y) + Math.Abs(u3.Y));
        double maxRadiusWorldZ = worldHalfExtents.Z / (Math.Abs(u1.Z) + Math.Abs(u2.Z) + Math.Abs(u3.Z));

        double maxFittingRadius = Math.Min(Math.Min(maxRadiusWorldX, maxRadiusWorldY), maxRadiusWorldZ);
        long I_max = Math.Max(0, (long)Math.Floor(maxFittingRadius / gridSize));
            
        var conservativeHalf = new Vector3D(I_max * gridSize, I_max * gridSize, I_max * gridSize);
        return new BoundingBoxD(-conservativeHalf, conservativeHalf);
    }
}
public class PIDController
{
    double integral;
    double lastInput;

    public double gain_p;
    double gain_i;
    public double gain_d;
    double upperLimit_i;
    double lowerLimit_i;
    double second;

    public PIDController(double pGain, double iGain, double dGain, double iUpperLimit = 0, double iLowerLimit = 0, double stepsPerSecond = 60)
    {
        gain_p = pGain;
        gain_i = iGain;
        gain_d = dGain;
        upperLimit_i = iUpperLimit;
        lowerLimit_i = iLowerLimit;
        second = stepsPerSecond;
    }

    public double Filter(double input, int round_d_digits)
    {
        double roundedInput = Math.Round(input, round_d_digits);

        integral = integral + (input / second);
        integral = (upperLimit_i > 0 && integral > upperLimit_i ? upperLimit_i : integral);
        integral = (lowerLimit_i < 0 && integral < lowerLimit_i ? lowerLimit_i : integral);

        double derivative = (roundedInput - lastInput) * second;
        lastInput = roundedInput;

        return (gain_p * input) + (gain_i * integral) + (gain_d * derivative);
    }
    public void Reset()
    {
        integral = lastInput = 0;
    }
}
    public static class Solver
    {
        private static double dGridSpeedLimit = 104; // TODO: Set this up properly bozo
        public static double MaxValueD => Double.MaxValue;
            
        public const double
            epsilon = 1e-6,

            cos120d = -0.5,
            root3 = 1.73205,
            sin120d = root3 / 2,

            inv3 = 1.0 / 3.0,
            inv9 = 1.0 / 9.0,
            inv6 = 1.0 / 6.0,
            inv54 = 1.0 / 54.0;

        public static Vector3D BallisticSolver(double maxSpeed, Vector3D missileVelocity, Vector3D missileAcceleration, Vector3D displacementVector, Vector3D targetPosition, Vector3D targetVelocity, Vector3D targetAcceleration, Vector3D targetJerk, bool isMissile, Vector3D gravity = default(Vector3D), bool hasGravity = false)
        {
            double tmaxT = 0;
            Vector3D
                dOffset = Vector3D.Zero,    
                targetAccelerationS = targetAcceleration,
                targetVelocityS = targetVelocity,
                relativeVelocity, relativeAcceleration;

            if (targetAcceleration.LengthSquared() > 1)
            {
                //Target Max Speed Math
                tmaxT = Math.Min((Vector3D.Normalize(targetAcceleration) * dGridSpeedLimit - Vector3D.ProjectOnVector(ref targetVelocity, ref targetAcceleration)).Length(), 2 * dGridSpeedLimit) / targetAcceleration.Length();
                targetVelocity = Vector3D.ClampToSphere(targetVelocity + targetAcceleration * tmaxT, dGridSpeedLimit);
                targetVelocityS += targetAcceleration * tmaxT * 0.5;
                targetAcceleration = Vector3D.Zero;
            }

            if (missileAcceleration.LengthSquared() > 1)
            {
                double
                    Tmax = Math.Max((Vector3D.Normalize(missileAcceleration) * maxSpeed - Vector3D.ProjectOnVector(ref missileVelocity, ref missileAcceleration)).Length(), 0) / missileAcceleration.Length(),
                    Dmax = (missileVelocity * Tmax + missileAcceleration * Tmax * Tmax).Length();
                Vector3D posAtTmax = displacementVector + targetVelocity * Tmax + 0.5 * targetAcceleration * Tmax * Tmax;
                if (posAtTmax.Length() > Dmax)
                {
                    missileAcceleration = Vector3D.Zero;
                    missileVelocity = Vector3D.ClampToSphere(missileVelocity + missileAcceleration * Tmax, maxSpeed);
                    displacementVector -= Vector3D.Normalize(displacementVector) * Dmax;
                }
            }

            //Quartic
            relativeVelocity = targetVelocity - missileVelocity;
            relativeAcceleration = targetAcceleration - missileAcceleration;
            double time = Solve(
                    relativeAcceleration.LengthSquared() * 0.25,
                    relativeAcceleration.X * relativeVelocity.X + relativeAcceleration.Y * relativeVelocity.Y + relativeAcceleration.Z * relativeVelocity.Z,
                    relativeVelocity.LengthSquared() - missileVelocity.LengthSquared() + displacementVector.X * relativeAcceleration.X + displacementVector.Y * relativeAcceleration.Y + displacementVector.Z * relativeAcceleration.Z,
                    2 * (displacementVector.X * relativeVelocity.X + displacementVector.Y * relativeVelocity.Y + displacementVector.Z * relativeVelocity.Z),
                    displacementVector.LengthSquared());
            if (time == MaxValueD || double.IsNaN(time) || time > 100)
                time = 100;

            if (tmaxT > time)
            {
                tmaxT = time;
                time = 0;
            }   
            else
                time -= tmaxT;
            //sbDebug.AppendLine($"{Math.Round(time, 2)} {Math.Round(relativeVelocity.Length(), 2)} {Math.Round(relativeAcceleration.Length(), 2)}\n");
            return isMissile ?
                displacementVector + targetVelocity * time + targetVelocityS * tmaxT + 0.5 * targetAccelerationS * tmaxT * tmaxT + 0.5 * targetAcceleration * time * time + dOffset :
                targetPosition + (targetVelocity - missileVelocity) * time + (targetVelocityS - missileVelocity) * tmaxT + 0.5 * targetAccelerationS * tmaxT * tmaxT + 0.5 * targetAcceleration * time * time + - 0.5 * gravity * (time + tmaxT) * (time + tmaxT) * Convert.ToDouble(hasGravity) + dOffset;
        }
        // public static Vector3D SelectOffsetPosition(PDCTarget target, bool isMissile)
        // {
        //     int
        //         RaycastPointCount = target.RaycastPoints.Count,
        //         TurretPointCount = target.TurretPoints.Count;
        //
        //     if (RaycastPointCount + TurretPointCount == 0 || target.Orientation == null)
        //         return Vector3D.Zero;
        //
        //     if (isMissile)
        //         return RaycastPointCount != 0 ?
        //             target.RaycastPoints.ElementAt(Program.RNG.Next(0, RaycastPointCount)) :
        //             target.TurretPoints.ElementAt(Program.RNG.Next(0, TurretPointCount));
        //     else
        //         return TurretPointCount != 0 ?
        //             target.TurretPoints.ElementAt(Program.RNG.Next(0, TurretPointCount)) :
        //             target.RaycastPoints.ElementAt(Program.RNG.Next(0, RaycastPointCount));
        // }

        //Shortcut Ignoring Of Complex Values And Return Smallest Real Number
        public static double Solve(double a, double b, double c, double d, double e)
        {
            if (Math.Abs(a) < epsilon) a = a >= 0 ? epsilon : -epsilon;
            double inva = 1 / a;

            b *= inva;
            c *= inva;
            d *= inva;
            e *= inva;

            double
                a3 = -c,
                b3 = b * d - 4 * e,
                c3 = -b * b * e - d * d + 4 * c * e,
                y;

            double[] result;
            bool chooseMaximal = SolveCubic(a3, b3, c3, out result);
            y = result[0];
            if (chooseMaximal)
            {
                if (Math.Abs(result[1]) > Math.Abs(y))
                    y = result[1];
                if (Math.Abs(result[2]) > Math.Abs(y))
                    y = result[2];
            }

            double q1, q2, p1, p2, squ;

            double u = y * y - 4 * e;
            if (Math.Abs(u) < epsilon)
            {
                q1 = q2 = y * 0.5;
                u = b * b - 4 * (c - y);

                if (Math.Abs(u) < epsilon)
                    p1 = p2 = b * 0.5;
                else
                {
                    squ = Math.Sqrt(u);
                    p1 = (b + squ) * 0.5;
                    p2 = (b - squ) * 0.5;
                }
            }
            else
            {
                squ = Math.Sqrt(u);
                q1 = (y + squ) * 0.5;
                q2 = (y - squ) * 0.5;

                double dm = 1 / (q1 - q2);
                p1 = (b * q1 - d) * dm;
                p2 = (d - b * q2) * dm;
            }

            double v1, v2;

            u = p1 * p1 - 4 * q1;
            if (u < 0)
                v1 = MaxValueD;
            else
            {
                squ = Math.Sqrt(u);
                v1 = MinPosNZ(-p1 + squ, -p1 - squ) * 0.5;
            }

            u = p2 * p2 - 4 * q2;
            if (u < 0)
                v2 = MaxValueD;
            else
            {
                squ = Math.Sqrt(u);
                v2 = MinPosNZ(-p2 + squ, -p2 - squ) * 0.5;
            }

            return MinPosNZ(v1, v2);
        }

        private static bool SolveCubic(double a, double b, double c, out double[] result)
        {
            result = new double[4];

            double
                a2 = a * a,
                q = (a2 - 3 * b) * inv9,
                r = (a * (2 * a2 - 9 * b) + 27 * c) * inv54,
                r2 = r * r,
                q3 = q * q * q;

            if (r2 < q3)
            {
                double
                    sqq = Math.Sqrt(q),
                    t = r / (sqq * sqq * sqq);

                if (t < -1)
                    t = -1;
                else if (t > 1)
                    t = 1;

                t = Math.Acos(t);

                a *= inv3;
                q = -2 * sqq;

                double
                    costv3 = Math.Cos(t * inv3),
                    sintv3 = Math.Sin(t * inv3);

                result[0] = q * costv3 - a;
                result[1] = q * ((costv3 * cos120d) - (sintv3 * sin120d)) - a;
                result[2] = q * ((costv3 * cos120d) + (sintv3 * sin120d)) - a;

                return true;
            }
            else
            {
                double
                    g = -Math.Pow(Math.Abs(r) + Math.Sqrt(r2 - q3), inv3),
                    h;

                if (r < 0)
                    g = -g;

                h = g == 0 ? 0 : q / g;

                a *= inv3;

                result[0] = g + h - a;
                result[1] = -0.5 * (g + h) - a;
                result[2] = 0.5 * root3 * (g - h);

                if (Math.Abs(result[2]) < epsilon)
                {
                    result[2] = result[1];
                    return true;
                }
                return false;
            }
        }

        private static double MinPosNZ(double a, double b)
        {
            if (a <= 0)
                return b > 0 ? b : MaxValueD;
            else if (b <= 0)
                return a;
            else
                return Math.Min(a, b);
        }
    }
public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Critical,
    Highlight
}
public class TimedLog
{
    Dictionary<LogLevel, string> _logColour = new Dictionary<LogLevel, string>()
    {
        { LogLevel.Trace, $"{ColorToHexARGB(Color.Gray)}" },
        { LogLevel.Debug, $"{ColorToHexARGB(Color.DarkSeaGreen)}"},
        { LogLevel.Info, $"{ColorToHexARGB(Color.White)}" },
        { LogLevel.Warning, $"{ColorToHexARGB(Color.Gold)}" },
        { LogLevel.Error, $"{ColorToHexARGB(Color.Red)}" },
        { LogLevel.Critical, $"{ColorToHexARGB(Color.DarkRed)}" },
        { LogLevel.Highlight, $"{ColorToHexARGB(Color.Aquamarine)}"}
    };
    private static string ColorToHexARGB(Color color)
    {
        return $"[color=#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}]";
    }

    private static string _footer = "[/color]\n";
    
        
    class Entry
    {
        internal readonly string Text;
        internal readonly double Timestamp; // seconds since epoch
        internal readonly LogLevel Level;
        public Entry(string text, double timestamp, LogLevel level)
        {
            Text = text;
            Timestamp = timestamp;
            Level = level;
        }
            
    }

    private readonly List<Entry> _entries = new List<Entry>();
    private readonly double _lifespan; // seconds

    public TimedLog(double lifespanSeconds)
    {
        _lifespan = lifespanSeconds;
    }

    public void Add(string message, LogLevel level)
    {
        if (level < Config.LogLevel) return;
        double now = (System.DateTime.UtcNow - new System.DateTime(1970,1,1)).TotalSeconds;
        _entries.Add(new Entry(message, now, level));
    }

    public void Update()
    {
        double now = (System.DateTime.UtcNow - new System.DateTime(1970,1,1)).TotalSeconds;
        _entries.RemoveAll(e => now - e.Timestamp > _lifespan);
    }

    public List<string> GetEntries()
    {
        return _entries.Select(e => e.Text).ToList();
    }

    public override string ToString()
    {
        string value = "";
        foreach (var entry in _entries)
        {
            value += _logColour[entry.Level] + entry.Text + _footer;
        }

        return value;
    }
}
public class SimpleTimer
{
    public int Remaining;
    public Action OnComplete;
}
public static class TimerManager
{
    private static readonly Dictionary<int, SimpleTimer> Timers = new Dictionary<int, SimpleTimer>();
    private static readonly Dictionary<int, SimpleTimer> _pendingAdditions = new Dictionary<int, SimpleTimer>();
    private static int _nextId = 1;

    /// <summary>
    /// Starts a timer for the given duration and optional callback.
    /// Returns a unique timer ID that is never reused.
    /// </summary>
    public static int Start(int duration, Action onComplete = null)
    {
        if (duration <= 0)
        {
            if (onComplete != null) onComplete();
            return -1;
        }

        int id = _nextId++;
        var timer = new SimpleTimer
        {
            Remaining = duration,
            OnComplete = onComplete
        };

        // Queue addition so TickAll can safely insert at the start
        _pendingAdditions[id] = timer;
        return id;
    }

    public static int StartSeconds(float duration, Action onComplete = null)
    {
        return Start((int)(duration * 60), onComplete);
    }

    /// <summary>
    /// Cancels a timer immediately.
    /// </summary>
    public static void Cancel(int id)
    {
        Timers.Remove(id);
        _pendingAdditions.Remove(id);
    }

    /// <summary>
    /// Tick all active timers. Call once per frame.
    /// </summary>
    public static void TickAll()
    {
        // First, add any timers that were scheduled since last tick
        if (_pendingAdditions.Count > 0)
        {
            foreach (var kvp in _pendingAdditions)
            {
                Timers[kvp.Key] = kvp.Value;
            }
            _pendingAdditions.Clear();
        }

        var finished = new List<int>();

        // Tick timers
        foreach (var kvp in Timers)
        {
            var timer = kvp.Value;
            timer.Remaining--;
            if (timer.Remaining <= 0)
            {
                // Safe to invoke callbacks here; any new timers go into _pendingAdditions
                timer.OnComplete?.Invoke();
                finished.Add(kvp.Key);
            }
        }

        // Remove finished timers after enumeration
        foreach (var id in finished)
        {
            Timers.Remove(id);
        }
    }

    public static bool IsActive(int id)
    {
        return Timers.ContainsKey(id) || _pendingAdditions.ContainsKey(id);
    }

    public static int GetRemaining(int id)
    {
        SimpleTimer timer;
        if (Timers.TryGetValue(id, out timer)) return timer.Remaining;
        if (_pendingAdditions.TryGetValue(id, out timer)) return timer.Remaining;
        return -1;
    }
}
public static class Config
{
        

    private static ConfigTool _configTool = new ConfigTool("General Config", "")
    {
        Get = GetConfig,
        Set = SetConfig
    };

        
    public static Dictionary<string, Action> Commands;
        
    public static LogLevel LogLevel = LogLevel.Trace; // TODO: Hook me up
        
    public static string ArgumentTarget = "Target";
    public static string ArgumentUnTarget = "Untarget";
    public static string GroupName = "ArgusV2";
    public static string TrackerGroupName = "TrackerGroup";

        
    public static double MaxWeaponRange = 2000;
    public static double LockRange = 3000; // m
    public static float LockAngle = 40; // deg
    public static double MinFireDot = 0.999999; // Should be updated further
        
        
    public static string TrackingName = "CTC: Tracking";
    public static string SearchingName = "CTC: Searching";
    public static string StandbyName = "CTC: Standby";
    public static int ScannedBlockMaximumValidTimeFrames = 3600; // 1 minutes

    public static double ProportialGain = 500;
    public static double IntegralGain = 0;
    public static double DerivativeGain = 30;
    public static double IntegralLowerLimit = -0.05;
    public static double IntegralUpperLimit = 0.05;
    public static double MaxAngularVelocityRpm = 30;


    public static int GdriveTimeoutFrames = 300; // TODO: Hook me up
    public static double GravityAcceleration = 9.81; // TODO: Hook me up
    public static bool DefaultPrecisionMode = false; // TODO: Hook me up
    public static bool DisablePrecisionModeOnEnemyDetected = false; // TODO: Hook me up

    public static void Setup(IMyProgrammableBlock me)
    {
        Program.LogLine("Setting up config");
        // We touch the static classes that have config data that needs collecting/setting
        GunData.Init();
        ProjectileData.Init();
            
        if (me.CustomData.Length > 0)
            ConfigTool.DeserializeAll(me.CustomData);
        me.CustomData = ConfigTool.SerializeAll();
        Program.LogLine("Written config to custom data", LogLevel.Debug);
        Commands = new Dictionary<string, Action>
        {
            { ArgumentTarget,     () => ShipManager.PrimaryShip.Target() },
            { ArgumentUnTarget,   () => ShipManager.PrimaryShip.UnTarget() },
            { "FireAllTest",             () => ShipManager.PrimaryShip.Guns.FireAll() },
            { "CancelAllTest",           () => ShipManager.PrimaryShip.Guns.CancelAll() }
        };
        if (LogLevel.Trace <= LogLevel)
        {
            foreach (var command in Commands)
            {
                Program.LogLine($"Command: {command.Key}", LogLevel.Trace);
            }
        }
        Program.LogLine("Commands set up", LogLevel.Debug);
        SetupGlobalState(me);
        Program.LogLine("Config setup done", LogLevel.Info);
            
    }

    public static void SetupGlobalState(IMyProgrammableBlock me)
    {
        Program.LogLine("Setting up global state", LogLevel.Debug);
        GlobalState.PrecisionMode = DefaultPrecisionMode;
        Program.LogLine($"Precision mode state is {GlobalState.PrecisionMode}", LogLevel.Trace);
    }

    static Config()
    {
            
    }


    private static Dictionary<string, object> GetConfig()
    {
        return new Dictionary<string, object>
        {
            ["String Config"] = new Dictionary<string, object>
            {
                ["ArgumentTarget"] = ArgumentTarget,
                ["ArgumentUnTarget"] = ArgumentUnTarget,
                ["GroupName"] = GroupName,
                ["TrackerGroupName"] = TrackerGroupName
            },
            ["Behavior Config"] = new Dictionary<string, object>
            {
                ["MaxWeaponRange"] = MaxWeaponRange,
                ["LockRange"] = LockRange,
                ["LockAngle"] = LockAngle,
                ["MinFireDot"] = MinFireDot,
            },
            ["Tracker Config"] = new Dictionary<string, object>
            {
                ["TrackingName"] = TrackingName,
                ["SearchingName"] = SearchingName,
                ["StandbyName"] = StandbyName,
                ["ScannedBlockMaxValidFrames"] = ScannedBlockMaximumValidTimeFrames
            },
            ["PID Config"] = new Dictionary<string, object>
            {
                ["ProportionalGain"] = ProportialGain,
                ["IntegralGain"] = IntegralGain,
                ["DerivativeGain"] = DerivativeGain,
                ["IntegralLowerLimit"] = IntegralLowerLimit,
                ["IntegralUpperLimit"] = IntegralUpperLimit,
                ["MaxAngularVelocityRPM"] = MaxAngularVelocityRpm
            }
        };
    }
    private static void SetConfig(Dictionary<string, object> obj)
    {
            
        // Unwrap any Dwon.Comment objects recursively first
        Dwon.UnwrapAllComments(obj);

            
        // String Config
        var stringConfig = obj.ContainsKey("String Config") ? obj["String Config"] as Dictionary<string, object> : null;
        if (stringConfig != null)
        {
            ArgumentTarget       = Dwon.GetValue(stringConfig, "ArgumentTarget", ArgumentTarget);
            ArgumentUnTarget     = Dwon.GetValue(stringConfig, "ArgumentUnTarget", ArgumentUnTarget);
            GroupName            = Dwon.GetValue(stringConfig, "GroupName", GroupName);
            TrackerGroupName     = Dwon.GetValue(stringConfig, "TrackerGroupName", TrackerGroupName);
        }

        // Behavior Config
        var behaviorConfig = obj.ContainsKey("Behavior Config") ? obj["Behavior Config"] as Dictionary<string, object> : null;
        if (behaviorConfig != null)
        {
            MaxWeaponRange = Dwon.GetValue(behaviorConfig, "MaxWeaponRange", MaxWeaponRange);
            LockRange      = Dwon.GetValue(behaviorConfig, "LockRange", LockRange);
            LockAngle      = Dwon.GetValue(behaviorConfig, "LockAngle", LockAngle);
            MinFireDot     = Dwon.GetValue(behaviorConfig, "MinFireDot", MinFireDot);
        }

        // Tracker Config
        var trackerConfig = obj.ContainsKey("Tracker Config") ? obj["Tracker Config"] as Dictionary<string, object> : null;
        if (trackerConfig != null)
        {
            TrackingName                 = Dwon.GetValue(trackerConfig, "TrackingName", TrackingName);
            SearchingName                = Dwon.GetValue(trackerConfig, "SearchingName", SearchingName);
            StandbyName                  = Dwon.GetValue(trackerConfig, "StandbyName", StandbyName);
            ScannedBlockMaximumValidTimeFrames = Dwon.GetValue(trackerConfig, "ScannedBlockMaxValidFrames", ScannedBlockMaximumValidTimeFrames);
        }

        // PID Config
        var pidConfig = obj.ContainsKey("PID Config") ? obj["PID Config"] as Dictionary<string, object> : null;
        if (pidConfig != null)
        {
            ProportialGain        = Dwon.GetValue(pidConfig, "ProportionalGain", ProportialGain);
            IntegralGain          = Dwon.GetValue(pidConfig, "IntegralGain", IntegralGain);
            DerivativeGain        = Dwon.GetValue(pidConfig, "DerivativeGain", DerivativeGain);
            IntegralLowerLimit    = Dwon.GetValue(pidConfig, "IntegralLowerLimit", IntegralLowerLimit);
            IntegralUpperLimit    = Dwon.GetValue(pidConfig, "IntegralUpperLimit", IntegralUpperLimit);
            MaxAngularVelocityRpm = Dwon.GetValue(pidConfig, "MaxAngularVelocityRPM", MaxAngularVelocityRpm);
        }
    }
}
public class ConfigTool
{
    private static readonly Dictionary<string, ConfigTool> Configs = new Dictionary<string, ConfigTool>();

    public static string SerializeAll()
    {
        Program.LogLine("Writing config", LogLevel.Info);
        var config = new Dictionary<string, object>();
        foreach (var kv in Configs)
        {
            Program.LogLine($"Collecting config: {kv.Key}", LogLevel.Debug);
            config.Add(kv.Key, kv.Value.Get());
        }

        //return TestDwon();
        return Dwon.Serialize(config);
    }

    public static void DeserializeAll(string config)
    {
        Program.LogLine("Reading config from custom data", LogLevel.Info);
        var parsed = Dwon.Parse(config);
        Program.LogLine("DeltaWing Object Notation: Parsed successfully", LogLevel.Debug);
        var dict = parsed as Dictionary<string, object>;
        if (dict == null)
        {
            Program.LogLine("Config malformed", LogLevel.Critical);
            throw new Exception();
        }
        foreach (var kv in dict)
        {
            ConfigTool config1;
            if (!Configs.TryGetValue(kv.Key, out config1)) continue;
            var dict3 = kv.Value as Dictionary<string, object>;
            if (dict3 != null)
            {
                Program.LogLine("Config set: " + kv.Key, LogLevel.Debug);
                config1.Set(dict3);
            }
        }
    }
        
        
        
    public Func<Dictionary<string, object>> Get { get; set; }
    public Action<Dictionary<string, object>> Set { get; set; }

    public ConfigTool(string name, string comment)
    {
        Name = name;
        Comment = comment;
        Configs.Add(name, this);
    }
        
    public string Name { get; }
    public string Comment { get; }
        
        
    public Dictionary<string, object> GetConfig() => Get?.Invoke();
    public void SetConfig( Dictionary<string, object> value) => Set?.Invoke(value);
}
public class ProjectileData
{
    private static readonly ConfigTool Config = new ConfigTool("Projectile Data",
        "The main list of known projectiles. Gun Data should reference these by name.")
    {
        Get = GetConfig,
        Set = SetConfig
    };
        
        
    public static Dictionary<string, ProjectileData> LookupTable = new Dictionary<string, ProjectileData>();
    public static readonly ProjectileData DefaultProjectile = new ProjectileData(0, 0, 0, 0);
        
    static ProjectileData()
    {
        LookupTable.Add("Default", DefaultProjectile);
            
        LookupTable.Add("LargeRailgun", new ProjectileData(2000, 2000, 2000, 0));
        LookupTable.Add("Artillery", new ProjectileData(500, 500, 2000, 0));
        LookupTable.Add("SmallRailgun", new ProjectileData(1000, 1000, 1400, 0));
        LookupTable.Add("Gatling", new ProjectileData(400, 400, 800, 0));
        LookupTable.Add("AssaultCannon", new ProjectileData(500, 500, 1400, 0));
        LookupTable.Add("Rocket", new ProjectileData(100, 200, 800, 1000));
    }

    public static void Init()
    {
        Program.LogLine("Projectile data loaded", LogLevel.Debug);
    }
        
        
    public static ProjectileData Get(string nameThing)
    {
        ProjectileData gun;
        return LookupTable.TryGetValue(nameThing, out gun) ? gun : DefaultProjectile;
    }
    public float ProjectileVelocity { get; private set; }
    public float MaxVelocity { get; private set;}
    public float MaxRange { get; private set; }
    public float Acceleration { get; private set; }

    public ProjectileData(float projectileVelocity, float maxVelocity, float maxRange, float acceleration)
    {
        ProjectileVelocity = projectileVelocity;
        MaxVelocity = maxVelocity;
        MaxRange = maxRange;
        Acceleration = acceleration;
    }
        
    private static Dictionary<string, object> GetConfig()
    {
        var root = new Dictionary<string, object>();

        foreach (var kv in LookupTable)
        {
            var name = kv.Key;
            var values = kv.Value;

            root[name] = new Dictionary<string, object>
            {
                ["ProjectileVelocity"] = values.ProjectileVelocity,
                ["MaxVelocity"] = values.MaxVelocity,
                ["MaxRange"] = values.MaxRange,
                ["Acceleration"] = values.Acceleration
            };
        }

        return root;
    }
    private static void SetConfig(Dictionary<string, object> config)
    {
        Dwon.UnwrapAllComments(config);

        foreach (var kv in config)
        {
            var data = (Dictionary<string, object>)kv.Value;
                
            var existing  = LookupTable[kv.Key] ?? DefaultProjectile;
                
            var projectileVelocity = Dwon.GetValue(data,  "ProjectileVelocity", existing.ProjectileVelocity);
            var maxVelocity = Dwon.GetValue(data,  "MaxVelocity", existing.MaxVelocity);
            var maxRange = Dwon.GetValue(data,  "MaxRange", existing.MaxRange);
            var acceleration = Dwon.GetValue(data, "Acceleration", existing.Acceleration);
                
                
                
            var gunData = new ProjectileData(projectileVelocity, maxVelocity, maxRange, acceleration);
            LookupTable[kv.Key] = gunData;
        }
    }
}
public class GunData
{
    private static readonly ConfigTool Config = new ConfigTool("Gun Data",
        "The main list of known gun types and their definition names. Should reference a known projectile type.")
    {
        Get = GetConfig,
        Set = SetConfig
    };
    public static Dictionary<string, GunData> LookupTable = new Dictionary<string, GunData>();
    public static readonly GunData DefaultGun = new GunData("Default", 0, 0, 0f, 0f);
        
    static GunData()
    {
        LookupTable.Add("Default", DefaultGun);
            
        // Large grid
        LookupTable.Add("LargeRailgun", new GunData("LargeRailgun", GunReloadType.NeedsCharging, GunFireType.Delay, 2.0f, 4.0f));
        LookupTable.Add("LargeBlockLargeCalibreGun", new GunData("Artillery", 0, 0, 0, 12));
        LookupTable.Add("LargeMissileLauncher", new GunData("Rocket", 0, 0, 0, 0.5f));
        // Small grid
        LookupTable.Add("SmallRailgun", new GunData("SmallRailgun", GunReloadType.NeedsCharging, GunFireType.Delay, 0.5f, 4.0f));
        LookupTable.Add("SmallBlockAutocannon", new GunData("Gatling", 0, 0, 0.0f, 0.4f));
        LookupTable.Add("SmallBlockMediumCalibreGun", new GunData("AssaultCannon", 0, 0, 0.0f, 6f));
        LookupTable.Add("MyObjectBuilder_SmallGatlingGun", new GunData("Gatling", 0, 0, 0.0f, 0.1f));
        LookupTable.Add("MyObjectBuilder_SmallMissileLauncher", new GunData("Rocket", 0, 0, 0.0f, 1f));
            
        LookupTable.Add("SmallRocketLauncherReload", Get("MyObjectBuilder_SmallMissileLauncher"));
        LookupTable.Add("SmallGatlingGunWarfare2", Get("MyObjectBuilder_SmallGatlingGun"));
        LookupTable.Add("SmallMissileLauncherWarfare2", Get("MyObjectBuilder_SmallMissileLauncher"));
            
    }
        
    public static void Init()
    {
        Program.LogLine("Gun data loaded", LogLevel.Debug);
    }
        
    public static GunData Get(string nameThing)
    {
        GunData gun;
        return LookupTable.TryGetValue(nameThing, out gun) ? gun : DefaultGun;
    }


    string _projectileDataString;
    public ProjectileData ProjectileData { get; }
    public GunReloadType ReloadType { get; }
    public GunFireType FireType { get; }
    public int FireTimeFrames { get; }
    public float FireTime => FireTimeFrames / 60.0f;
    public int ReloadTimeFrames { get; }
    public float ReloadTime => ReloadTimeFrames / 60.0f;

    public GunData(string projectileDataName, GunReloadType reloadType, GunFireType fireType, float fireTime, float reloadTime)
    {
        _projectileDataString = projectileDataName;
        ProjectileData = ProjectileData.Get(projectileDataName);
        ReloadType = reloadType;
        FireType = fireType;
        FireTimeFrames = (int)(fireTime * 60);
        ReloadTimeFrames = (int)(reloadTime * 60);
    }
        
        
    private static Dictionary<string, object> GetConfig()
    {
        var root = new Dictionary<string, object>();

        foreach (var kv in LookupTable)
        {
            var name = kv.Key;
            var values = kv.Value;

            var gunData = new Dictionary<string, object>();
            gunData["Projectile"] = values._projectileDataString;
            gunData["ReloadType"] = new Dwon.Comment((int)values.ReloadType, "0 = normal, 1 = charged");
            gunData["FireType"] = new Dwon.Comment((int)values.FireType,"0 = normal, 1 = delay before firing") ;
            gunData["FireTime"] = values.FireTime;
            gunData["ReloadTime"] = values.ReloadTime;

            root[name] = gunData;
        }

        return root;
    }
        
    private static void SetConfig(Dictionary<string, object> config)
    {
        Dwon.UnwrapAllComments(config);

        foreach (var kv in config)
        {
            var data = (Dictionary<string, object>)kv.Value;
                
            var existing  = LookupTable[kv.Key] ?? DefaultGun;


            var projectile = Dwon.GetValue(data, "Projectile", existing._projectileDataString);
            var gunReloadType = Dwon.GetValue(data,  "ReloadType", existing.ReloadType);
            var fireType = Dwon.GetValue(data,  "FireType", existing.FireType);
            var fireTime = Dwon.GetValue(data,  "FireTime", existing.FireTime);
            var reloadTime = Dwon.GetValue(data,  "ReloadTime", existing.ReloadTime);
                
                
            var gunData = new GunData(projectile, gunReloadType, fireType, fireTime, reloadTime);
            LookupTable[kv.Key] = gunData;
        }
    }
        
}
public static class GlobalState
{
    public static bool PrecisionMode;
}
public abstract class ArgusShip
{
    protected Vector3D CPreviousVelocity;
    protected Vector3D CVelocity;
    protected int RandomUpdateJitter;
    public SensorPollFrequency PollFrequency = SensorPollFrequency.Realtime;

    public ArgusShip()
    {
        Program.LogLine("New ArgusShip", LogLevel.Debug);
        RandomUpdateJitter = Program.RNG.Next() % 600;
    }
        
    public abstract Vector3D Position { get; }
    public abstract Vector3D Velocity { get; }
    public abstract Vector3D Acceleration { get; }
    public abstract float GridSize { get; }
    public abstract string Name { get; }


    /// <summary>
    /// The first update to be called. This should be for grabbing new data from the API, e.g. positions, velocities, targets.
    /// </summary>
    /// <param name="frame"></param>
    public abstract void EarlyUpdate(int frame);
        
    /// <summary>
    /// The second update to be called. This would be for anything that depends on up-to-date data from other ships, e.g. ballistic calculations, distance checks.
    /// </summary>
    /// <param name="frame"></param>
    public abstract void LateUpdate(int frame);
        
        
    /// <summary>
    /// Calculates a direction to aim in order to hit the specified target ship with a projectile
    /// traveling at the given velocity. 
    /// 
    /// WARNING: Should only be called during LateUpdate, when target and shooter velocities are up to date.
    /// Returns a vector pointing from this ship toward the predicted intercept point.
    /// </summary>
    /// <param name="target">The target ship to hit.</param>
    /// <param name="projectileVelocity">The speed of the projectile.</param>
    /// <returns>A normalized direction vector to aim at for impact.</returns>
    public Vector3D GetTargetLeadPosition(ArgusShip target, float projectileVelocity)
    {
        Vector3D shooterPos = this.Position;
        Vector3D shooterVel = this.Velocity;
            
        Vector3D targetPos = target.Position;
        Vector3D targetAcc = target.Acceleration;

        Vector3D relativeVel = target.Velocity - shooterVel; // target motion relative to shooter
            
        Vector3D displacement = targetPos - shooterPos;
        double s = projectileVelocity;

        // Quadratic coefficients: a t^2 + b t + c = 0
        double a = relativeVel.LengthSquared() - s * s;
        double b = 2.0 * displacement.Dot(relativeVel);
        double c = displacement.LengthSquared();

        double t;

        // Solve quadratic
        if (Math.Abs(a) < 1e-6) // handle edge case: target velocity ~= projectile speed
        {
            if (Math.Abs(b) < 1e-6)
                t = 0; // target on top of shooter
            else
                t = -c / b;
        }
        else
        {
            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0) return targetPos; // cannot reach target, aim at current pos

            double sqrtDisc = Math.Sqrt(discriminant);
            double t1 = (-b + sqrtDisc) / (2 * a);
            double t2 = (-b - sqrtDisc) / (2 * a);

            t = Math.Min(t1, t2) > 0 ? Math.Min(t1, t2) : Math.Max(t1, t2);
            if (t < 0) t = Math.Max(t1, t2); // if both negative, aim at current pos
        }

        // Include acceleration via a single-step approximation
        Vector3D intercept = targetPos + relativeVel * t + 0.5 * targetAcc * t * t;
        return intercept;
    }

        
        

        
}
public struct FiringSolution
{
    public readonly Vector3D DesiredForward;
    public readonly Vector3D TargetPosition;
    public readonly Vector3D ShooterPosition;
    public readonly Vector3D CurrentForward;
    public readonly double Dot;
    public readonly double Range;
    public MatrixD WorldMatrix;

    public FiringSolution(Vector3D desiredForward, Vector3D targetPosition, Vector3D shooterPosition, Vector3D currentForward, double dot, double range, MatrixD worldMatrix)
    {
        DesiredForward = desiredForward;
        TargetPosition = targetPosition;
        ShooterPosition = shooterPosition;
        CurrentForward = currentForward;
        Dot = dot;
        Range = range;
        WorldMatrix = worldMatrix;
    }
}
public class FireController
{
    ControllableShip _ship;
    GunManager _guns;

    public FireController(ControllableShip ship, GunManager guns)
    {
        Program.LogLine($"Setting up FCS", LogLevel.Info);
        _ship = ship;
        _guns = guns;
    }
    /// <summary>
    /// What a cool function name, that's how you know it's cool
    /// </summary>
    public FiringSolution ArbitrateFiringSolution()
    {

        _guns.SelectFiringGroup();
            
        int count = _guns.FireGroupCount;
        PositionValidity validity = _guns.FireGroupValidity;
        var pos = _guns.FireRefPos;

        var enemyPos = _ship.GetTargetPosition();
            
            
        var displacement = enemyPos - pos;

        var dist = displacement.Length();

        var dir = displacement / dist;

        var dot = dir.Dot(_ship.Forward);

        var solvedPos = _guns.GetBallisticSolution();
        var solvedForward = (solvedPos - pos).Normalized();
        Program.Log(solvedForward);

        if (dot > Config.MinFireDot && enemyPos != Vector3D.Zero) _guns.TryFire();
        else _guns.TryCancel();

        return new FiringSolution(solvedForward, enemyPos, pos, _ship.Forward, dot, dist, _ship.WorldMatrix);
    }
}
public enum GunReloadType
{
    Normal,
    NeedsCharging
}
public enum GunFireType
{
    Normal,
    Delay
}
public enum GunState
{
    Firing,
    Cancelling,
    ReadyToFire,
    Reloading,
    Recharging,
    NonFunctional
}
public class Gun
{
    private static readonly MyDefinitionId ElectricityId =
        new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");
    IMyUserControllableGun _gun;
    GunReloadType _reloadType;
    GunFireType _fireType;
    MyResourceSinkComponent _gunSinkComponent;
    int _reloadTimerId;
    int _firingTimerId;
    GunState _cachedState;
    bool _stateValid;
    bool _cancelled;
    GunData _gunData;
    GunManager _manager;

    public Gun(IMyUserControllableGun gun, GunManager manager)
    {
            
        var blockDefinition = gun.BlockDefinition;
        Program.LogLine($"Set up new gun {blockDefinition}", LogLevel.Info);
        var gunData = GunData.Get(blockDefinition.SubtypeIdAttribute);
        if (gunData == GunData.DefaultGun) gunData = GunData.Get(blockDefinition.TypeIdString);
        _gunData = gunData;
        _manager = manager;
            
        _gun = gun;
        _gunSinkComponent = gun.Components.Get<MyResourceSinkComponent>();
        _reloadType = _gunData.ReloadType;
        _fireType = _gunData.FireType;
    }
        
    public Vector3D GridPosition => (Vector3)(_gun.Min + _gun.Max) / 2 * _manager.ThisShip.GridSize;
    public Vector3D WorldPosition => _gun.GetPosition();
    public Vector3D Direction { get; set; }
    public float Velocity => _gunData.ProjectileData.ProjectileVelocity;
    public float Acceleration => _gunData.ProjectileData.Acceleration;
    public float MaxVelocity => _gunData.ProjectileData.MaxVelocity;
    public float MaxRange => _gunData.ProjectileData.MaxRange;

    public GunData GunData => _gunData;
        
    public GunState State
    {
        get
        {
            if (!_stateValid)
            {
                _cachedState = EvaluateState();
                _stateValid = true;
            }
            return _cachedState;

        }
    }

    public Vector3D Forward => _gun.WorldMatrix.Forward;

    public void EarlyUpdate(int frame)
    {
        _stateValid = false;
    }

    public void LateUpdate(int frame)
    {
            
    }

    public bool Fire()
    {
        if (State != GunState.ReadyToFire) return false;
            
        _gun.ShootOnce();
        _reloadTimerId = TimerManager.Start(_gunData.ReloadTimeFrames, OnReloadComplete);
        if (_fireType == GunFireType.Delay)
        {
            _firingTimerId = TimerManager.Start(_gunData.FireTimeFrames, OnFireComplete); // Temporary delay value
        }
        else
        {
            _firingTimerId = TimerManager.Start(0, OnFireComplete);
        }
        return true;
    }


    public bool ForceCancel()
    {
        if (State != GunState.Firing) return false;
        _gun.Enabled = false; // The only way to force a gun to not fire is to turn it off
        TimerManager.Start(0, ReenableGun);
        _cancelled = true;
        return true;
    }
        
    public bool CancelIfAboutToFire()
    {

        if (State != GunState.Firing) return false;


        if (TimerManager.GetRemaining(_firingTimerId) > 1) return false;
        _gun.Enabled = false; // The only way to force a gun to not fire is to turn it off
        TimerManager.Start(0, ReenableGun);
        _cancelled = true;
        return true;

    }
        
        
        
        
    public Vector3D GetFireVelocity(Vector3D shipVelocity)
    {
        Vector3D combinedVelocity = shipVelocity + Direction * Velocity;
        if (combinedVelocity.LengthSquared() > Velocity * Velocity)
            combinedVelocity = combinedVelocity.Normalized() * Velocity;
        return combinedVelocity;
    }
        
        
        
    GunState EvaluateState()
    {
        bool functional = _gun.IsFunctional;
        if (!functional) return GunState.NonFunctional;
                
        if (_fireType == GunFireType.Delay && TimerManager.IsActive(_firingTimerId)) return _cancelled ? GunState.Cancelling : GunState.Firing;
                
        switch (_reloadType)
        {
            case GunReloadType.Normal:
                if (TimerManager.IsActive(_reloadTimerId)) return GunState.Reloading;
                break;
            case GunReloadType.NeedsCharging:
                if (TimerManager.IsActive(_reloadTimerId)) return GunState.Reloading; // Railguns for example have a 4 second refractory period after attempting to fire so we start this timer too anyway
                if (_gunSinkComponent.CurrentInputByType(ElectricityId) > 0.02f) return GunState.Recharging;
                break;
        }
        return GunState.ReadyToFire;
    }

    void OnFireComplete()
    {
        if (_cancelled)
        {
            _cancelled = false;
            return;
        }
    }

    void OnReloadComplete()
    {
            
    }

    void ReenableGun()
    {
        _gun.Enabled = true;
    }
}
public enum PositionValidity
{
    Fallback,
    Ready,
    Firing
}
public enum FireType
{
    FireWhenReady,
    WaitForAll,
    Volley
}
public class GunManager
{
    List<Gun> _guns = new List<Gun>();
        
    List<Gun> _currentFiringGroup = new List<Gun>();

    Vector3D _firingReferencePosition;
    PositionValidity _fireGroupValidity;
    int _fireGroupCount;
    GunData _gunData;
        
    public GunManager(List<IMyTerminalBlock> blocks, ControllableShip thisShip)
    {
        Program.LogLine("Setting up gun manager", LogLevel.Info);
        foreach (var block in blocks)
        {
            var gun = block as IMyUserControllableGun;
            if (gun != null) _guns.Add(new Gun(gun, this));
        }
        if (_guns.Count <= 0) Program.LogLine($"No guns in group {Config.GroupName}", LogLevel.Warning);
        ThisShip = thisShip;
    }

    public ControllableShip ThisShip { get; }
    public IMyCubeGrid Grid => ThisShip.Controller.CubeGrid;
    public int GunCount => _guns.Count;
        
    public Vector3D FireRefPos => _firingReferencePosition;
    public PositionValidity FireGroupValidity => _fireGroupValidity;
    public int FireGroupCount => _fireGroupCount;

    public void EarlyUpdate(int frame)
    {
        foreach (var gun in _guns) gun.EarlyUpdate(frame);
    }

    public void SelectFiringGroup()
    {
        var validity = PositionValidity.Fallback;
            
        var fireGroupsReady = new Dictionary<GunData, List<Gun>>();
        var fireGroupsFiring = new Dictionary<GunData, List<Gun>>();
        var readyCount = 0;
        var firingCount = 0;
            
        foreach (var gun in _guns)
        {
            switch (gun.State)
            {
                case GunState.ReadyToFire:
                    if (!fireGroupsReady.ContainsKey(gun.GunData)) fireGroupsReady.Add(gun.GunData, new List<Gun>());
                    fireGroupsReady[gun.GunData].Add(gun);
                    readyCount++;
                    break;
                case GunState.Firing:
                    if (!fireGroupsFiring.ContainsKey(gun.GunData)) fireGroupsFiring.Add(gun.GunData, new List<Gun>());
                    fireGroupsFiring[gun.GunData].Add(gun);
                    firingCount++;
                    break;
            }
        }
        Program.Log(readyCount);
            
        var groupDict = fireGroupsFiring; // TODO: Make sure there's always a fallback
        if (readyCount > 0)
        {
            //_fireGroupCount = readyCount;
            _fireGroupValidity = PositionValidity.Ready;
            groupDict = fireGroupsReady;
            //_firingReferencePosition = Vector3D.Transform(readyPos / readyCount, ThisShip.WorldMatrix);
        }
            
            
            
            
            
        if (firingCount > 0) // We do firing last so it is always prioritized
        {
            //_fireGroupCount = firingCount;
            _fireGroupValidity = PositionValidity.Firing;
            groupDict = fireGroupsFiring;
            //_firingReferencePosition = Vector3D.Transform(firingPos / firingCount, ThisShip.WorldMatrix);
        }
            
            
            

        var target = ThisShip.GetTargetPosition();
        var ourPosApprox = ThisShip.Position;
        var rangeSquared = (target - ourPosApprox).LengthSquared();
            
        foreach (var group in groupDict)
        {
            var data = group.Key;
            var guns = group.Value;
            var maxRange = data.ProjectileData.MaxRange;
            if (maxRange * maxRange < rangeSquared) continue; // Completely ignore gun cases that are out of range
            _currentFiringGroup = guns;
            _gunData = data;
            break;
        }

        _fireGroupCount = _currentFiringGroup.Count;

        if (_fireGroupCount == 0)
        {
            _firingReferencePosition = ThisShip.Position;
            return;
        }
            
        Vector3D average = Vector3D.Zero;

        foreach (var gun in _currentFiringGroup)
        {
            average += gun.GridPosition;
        }

        average /= _fireGroupCount;

        _firingReferencePosition = Vector3D.Transform(average, ThisShip.WorldMatrix);

    }
        

    public void LateUpdate(int frame)
    {
        foreach (var gun in _guns) gun.LateUpdate(frame);
    }

        
        
    // API
    public void FireAll()
    {
        foreach (var gun in _guns) gun.Fire();
    }

    public void CancelAll()
    {
        foreach (var gun in _guns) gun.ForceCancel();
    }
        
    public void TryFire()
    {
        foreach (var gun in _guns) gun.Fire();
    }

    public void TryCancel()
    {
        foreach (var gun in _guns) gun.CancelIfAboutToFire();
    }


    public Vector3D GetBallisticSolution()
    {

        if (_gunData == null) return Vector3D.Zero;
        var maxSpeed = _gunData.ProjectileData.MaxVelocity;
        var target = ThisShip.GetTargetPosition();
        var displacement = target - _firingReferencePosition;

        var refGun = _currentFiringGroup[0];

        if (refGun == null) return Vector3D.Zero;

        var gravity = ThisShip.Gravity;
        var hasGravity = gravity.LengthSquared() != 0;

        return Solver.BallisticSolver(maxSpeed, refGun.GetFireVelocity(ThisShip.Velocity) / 60,
            _gunData.ProjectileData.Acceleration * refGun.Forward,
            displacement,
            ThisShip.CurrentTarget.Position, ThisShip.CurrentTarget.Velocity / 60, ThisShip.CurrentTarget.Acceleration / 60,
            Vector3D.Zero, false, gravity, hasGravity);
    }
}
public class GyroManager
{
    private readonly List<IMyGyro> _gyros;
    private readonly PIDController _pitch;
    private readonly PIDController _yaw;
        
        
    public GyroManager(List<IMyTerminalBlock> blocks)
    {
        Program.LogLine("Setting up gyro manager", LogLevel.Info);
        _gyros = new List<IMyGyro>();
        foreach (var b in blocks)
        {
            var gyro = b as IMyGyro;
            if (gyro != null)
            {
                _gyros.Add(gyro);
            }
        }
            
        if (_gyros.Count <= 0) Program.LogLine($"No gyroscopes found in group: {Config.GroupName}", LogLevel.Warning);
            
            
        _pitch = new PIDController(Config.ProportialGain, Config.IntegralGain, Config.DerivativeGain, 
            Config.IntegralUpperLimit, Config.IntegralLowerLimit);
        _yaw = new PIDController(Config.ProportialGain, Config.IntegralGain, Config.DerivativeGain,
            Config.IntegralUpperLimit, Config.IntegralLowerLimit);
    }
        
    public void Rotate(ref FiringSolution solution, double roll = 0)
    {
        int roundValue = 7;
        double rotationalGain = 1.0;
        if (solution.Dot > 0.9999)
        {
            rotationalGain *= 0.8;
            roundValue = 4;
        }

        if (solution.Dot > 0.99999)
        {
            rotationalGain *= 0.8;
            roundValue = 3;
        }
        if (solution.Dot > 0.999999)
        {
            rotationalGain *= 0.8;
            roundValue = 2;
        }
        if (solution.Dot > 0.9999999)
        {
            rotationalGain *= 0.8;
            roundValue = 1;
        }
            
        double gp;
        double gy;
        var gr = roll;

        //Rotate Toward forward

        var waxis = Vector3D.Cross(solution.CurrentForward, solution.DesiredForward);
        var axis = Vector3D.TransformNormal(waxis, MatrixD.Transpose(solution.WorldMatrix));
        var x = _pitch.Filter(-axis.X, roundValue);
        var y = _yaw.Filter(-axis.Y, roundValue);

        gp = MathHelper.Clamp(x, -Config.MaxAngularVelocityRpm, Config.MaxAngularVelocityRpm);
        gy = MathHelper.Clamp(y, -Config.MaxAngularVelocityRpm, Config.MaxAngularVelocityRpm);

        if (Math.Abs(gy) + Math.Abs(gp) > Config.MaxAngularVelocityRpm)
        {
            var adjust = Config.MaxAngularVelocityRpm / (Math.Abs(gy) + Math.Abs(gp));
            gy *= adjust;
            gp *= adjust;
        }
        gp *= rotationalGain;
        gy *= rotationalGain;
        ApplyGyroOverride(gp, gy, gr, solution.WorldMatrix);
    }


    void ApplyGyroOverride(double pitchSpeed, double yawSpeed, double rollSpeed, MatrixD worldMatrix)
    {
        var rotationVec = new Vector3D(pitchSpeed, yawSpeed, rollSpeed);
        var relativeRotationVec = Vector3D.TransformNormal(rotationVec, worldMatrix);
        foreach (var gyro in _gyros)
            if (gyro.IsFunctional && gyro.IsWorking && gyro.Enabled && !gyro.Closed)
            {
                var transformedRotationVec =
                    Vector3D.TransformNormal(relativeRotationVec, MatrixD.Transpose(gyro.WorldMatrix));
                gyro.Pitch = (float)transformedRotationVec.X;
                gyro.Yaw = (float)transformedRotationVec.Y;
                gyro.Roll = (float)transformedRotationVec.Z;
                gyro.GyroOverride = true;
                return;
            }
    }

    public void ResetGyroOverrides()
    {
        foreach (var gyro in _gyros)
            if (gyro.IsFunctional && gyro.IsWorking && gyro.Enabled && !gyro.Closed)
            {
                gyro.GyroOverride = false;
                return;
            }
    }
        
        
}
public class BalancedMassSystem
{
    private readonly List<BlockMass> _blocks;
    private readonly List<BallMass> _balls;



    public BalancedMassSystem(List<IMyTerminalBlock> blocks)
    {
        _blocks = new List<BlockMass>();
        _balls = new List<BallMass>();

        foreach (var block in blocks)
        {
            var ball = block as IMySpaceBall;
            if (ball != null)
            {
                _balls.Add(new BallMass(ball, this));
                continue;
            }

            var mass = block as IMyArtificialMassBlock;
            if (mass != null)
            {
                _blocks.Add(new BlockMass(mass, this));
            }

        }
    }
        
    public bool Enabled { get; set; }

    public void EarlyUpdate(int _frame)
    {
        // TODO: Balancer logic
    }

    public void LateUpdate(int _frame)
    {
        foreach (var block in _blocks)
        {
            block.UpdateState();
        }
        foreach (var ball in _balls)
        {
            ball.UpdateState();
        }
    }
}
internal class DirectionalDrive
{
    List<GravityGenerator> _generators;
    bool _previousEnabled;
    float _acceleration;
    float _previousAcceleration;
    int _framesOff;

    public DirectionalDrive(List<GravityGenerator> generators, Direction direction)
    {
        _generators = generators;
        TotalAcceleration = 0;
        Direction = direction;
        foreach (var generator in generators)
        {
            var linear = generator as GravityGeneratorLinear;

            if (linear != null) TotalAcceleration += Config.GravityAcceleration; // No better way to get acceleration?
        }
    }

    public void EarlyUpdate(int frame)
    {
        if (_acceleration == 0) _framesOff++;
        else _framesOff = 0;
        if (_framesOff > Config.GdriveTimeoutFrames) Enabled = false;
    }

    public void LateUpdate(int frame)
    {
        if (_previousEnabled != Enabled) 
            foreach (var generator in _generators) generator.Enabled = Enabled;
        _previousEnabled = Enabled;
        if (_previousAcceleration != _acceleration)
            foreach (var generator in _generators)
                generator.Acceleration = _acceleration;
        _previousAcceleration = _acceleration;
    }
        
    public Direction Direction { get; private set; }
    public bool Enabled { get; private set; }
    public double TotalAcceleration { get; private set; } // Probably not spherical inclusive for now

    public void SetAcceleration(float acceleration)
    {
        if (acceleration == _acceleration && acceleration == 0) return;
        Enabled = true;
        if (acceleration == _acceleration) return; // Could possibly round this to maybe 2 dp to reduce polling rate
        _acceleration = acceleration;
    }
}
public class GDrive
{
    private readonly DirectionalDrive _forwardBackward;
    private readonly DirectionalDrive _leftRight;
    private readonly DirectionalDrive _upDown;

    BalancedMassSystem _massSystem;
    ControllableShip _ship;


    bool _previousMassEnabled;
        

    public GDrive(List<IMyTerminalBlock> blocks, ControllableShip ship)
    {
        Program.LogLine($"Setting up gravity drive", LogLevel.Info);
        _ship = ship;
            
        var forwardBackward = new List<GravityGenerator>();
        var leftRight = new List<GravityGenerator>();
        var upDown = new List<GravityGenerator>();

        var genCastArray = new Dictionary<Direction, List<GravityGenerator>>
        {
            { Direction.Up, upDown },
            { Direction.Down, upDown },
            { Direction.Left, leftRight },
            { Direction.Right, leftRight },
            { Direction.Forward, forwardBackward },
            { Direction.Backward, forwardBackward }
        };
            
        foreach (var block in blocks)
        {
            var linearGen = block as IMyGravityGenerator;
            if (linearGen != null)
            {
                var dir = (Direction)linearGen.Orientation.Up;
                var list = genCastArray[dir];
                bool inverted = (int)dir % 2 == 0;
                list.Add(new GravityGeneratorLinear(linearGen, dir, inverted));
            }

            var sphericalGen = block as IMyGravityGeneratorSphere;
            if (sphericalGen != null)
            {
                var forward = _ship.LocalOrientationForward;
                var forwardDir = Base6Directions.Directions[(int)forward];
                var inverted = forwardDir.Dot(_ship.Position - sphericalGen.GetPosition()) > 0;
                var list = genCastArray[forward];
                list.Add(new GravityGeneratorSpherical(sphericalGen, forward, inverted));
                    
            }
        }

        if (forwardBackward.Count == 0) Program.LogLine($"No Forward/backward gravity generators", LogLevel.Warning);
        if (leftRight.Count == 0) Program.LogLine($"No Left/Right gravity generators", LogLevel.Warning);
        if (upDown.Count == 0) Program.LogLine($"No Up/Down gravity generators", LogLevel.Warning);
            
        _forwardBackward = new DirectionalDrive(forwardBackward, Direction.Forward);
        _leftRight = new DirectionalDrive(leftRight, Direction.Left);
        _upDown = new DirectionalDrive(upDown, Direction.Up);


        _massSystem = new BalancedMassSystem(blocks);
    }

    bool MassEnabled => _forwardBackward.Enabled || _leftRight.Enabled || _upDown.Enabled;

    public void EarlyUpdate(int frame)
    {
        _forwardBackward.EarlyUpdate(frame);
        _leftRight.EarlyUpdate(frame);
        _upDown.EarlyUpdate(frame);

        _massSystem.EarlyUpdate(frame);
    }

    public void LateUpdate(int frame)
    {
        _forwardBackward.LateUpdate(frame);
        _leftRight.LateUpdate(frame);
        _upDown.LateUpdate(frame);


        if (MassEnabled != _previousMassEnabled) _massSystem.Enabled = MassEnabled;
        Program.Log(_massSystem.Enabled);
        _previousMassEnabled = MassEnabled;
        _massSystem.LateUpdate(frame);
    }


    public void ApplyPropulsion(Vector3 propLocal)
    {
        propLocal *= (float)Config.GravityAcceleration;
        _forwardBackward.SetAcceleration(propLocal.Dot(Vector3D.Forward));
        _leftRight.SetAcceleration(propLocal.Dot(Vector3D.Left));
        _upDown.SetAcceleration(propLocal.Dot(Vector3D.Up));
    }
}
public class BallMass : Mass
{
    IMySpaceBall _ball;
    BalancedMassSystem _massSystem;
    public BallMass(IMySpaceBall ball, BalancedMassSystem massSystem)
    {
        _ball = ball;
        _massSystem = massSystem;
    }

    public bool BalancerAllowed { get; set; } = true;

    public bool GeneratorRequested => _massSystem.Enabled;

    public bool IsActive => BalancerAllowed && GeneratorRequested;
    public override double AbsoluteVirtualMass => _ball.VirtualMass;
    public override double BalancerVirtualMass => BalancerAllowed ? _ball.VirtualMass : 0;

    public void UpdateState()
    {
        _ball.Enabled = IsActive;
    }
}
public class BlockMass : Mass
{
    IMyArtificialMassBlock _mass;
    BalancedMassSystem _massSystem;

    bool _previousActive;
    public BlockMass(IMyArtificialMassBlock mass, BalancedMassSystem massSystem)
    {
        _mass = mass;
        _massSystem = massSystem;
    }
        
    public bool BalancerAllowed { get; set; } = true;

    public bool GeneratorRequested => _massSystem.Enabled;

    public bool IsActive => BalancerAllowed && GeneratorRequested;
    public override double AbsoluteVirtualMass => _mass.VirtualMass;
    public override double BalancerVirtualMass => BalancerAllowed ? _mass.VirtualMass : 0;

    public void UpdateState()
    {
            

        if (_previousActive != IsActive)
            _mass.Enabled = IsActive;
        _previousActive = IsActive;
    }
}
public abstract class GravityGenerator
{
    protected bool IsInverted;
        
    public IMyGravityGeneratorBase Generator { get; protected set; }
    public Direction Direction { get; protected set; }

        
    public bool Enabled
    {
        get { return Generator.Enabled; }
        set  { Generator.Enabled = value; }
    }

    // The managing classes should not have to concern themselves with what direction the gravity generator identifies as
    public float Acceleration
    {
        get
        {
            return Generator.GravityAcceleration * (IsInverted ? -1 : 1);
        }
        set
        {
            Generator.GravityAcceleration = value * (IsInverted ? -1 : 1);
        }
    }

}
public class GravityGeneratorLinear : GravityGenerator
{

    public GravityGeneratorLinear(IMyGravityGenerator gen, Direction dir, bool inverted) 
    {
            
        Generator = gen; 
        Direction = dir;
        IsInverted = inverted;
    }
}
public class GravityGeneratorSpherical : GravityGenerator
{

    public GravityGeneratorSpherical(IMyGravityGeneratorSphere sphericalGen, Direction dir, bool inverted)
    {
        Generator = sphericalGen;
        Direction = dir;
        IsInverted = inverted;
    }
}
public abstract class Mass
{
        
    public abstract double AbsoluteVirtualMass { get; }
    public abstract double BalancerVirtualMass { get; }
}
public class PropulsionController
{
    GDrive _gDrive;
    TDrive _tDrive;
    ControllableShip _ship;

    public PropulsionController(List<IMyTerminalBlock> blocks, ControllableShip ship)
    {
        Program.LogLine($"Setting up propulsion controller", LogLevel.Info);
        _ship = ship;
        _gDrive = new GDrive(blocks, ship);
        _tDrive = new TDrive(); // TODO
    }

    public void EarlyUpdate(int frame)
    {
        // Put logic to discern propulsion here?
            
        _gDrive.EarlyUpdate(frame);
        _tDrive.EarlyUpdate(frame);
    }

    public void LateUpdate(int frame)
    {
        var userInput = _ship.Controller.MoveIndicator;

        Matrix matrix; // TODO: Cache cockpit local orientation matrix
        _ship.Controller.Orientation.GetMatrix(out matrix);

        var desiredMovement = Vector3.Transform(userInput, matrix);

        if (_ship.Controller.DampenersOverride)
        {
            var velocity = _ship.Velocity;
            var localVelocity = Vector3D.TransformNormal(velocity, MatrixD.Invert(_ship.WorldMatrix)); // TODO: Cache inverted matrix somewhere in ship

            // TODO: Integrate ship mass and total force in each direction for proper dampening results
            var dampenValueForwardBackward = localVelocity * Vector3D.Forward;
            var dampenValueLeftRight = localVelocity * Vector3D.Left;
            var dampenValueUpDown = localVelocity * Vector3D.Down;

            if (desiredMovement.Dot(Vector3D.Forward) == 0) desiredMovement += dampenValueForwardBackward;
            if (desiredMovement.Dot(Vector3D.Left) == 0) desiredMovement += dampenValueLeftRight;
            if (desiredMovement.Dot(Vector3D.Down) == 0) desiredMovement += dampenValueUpDown;
        }
            
            
        // Put logic to resolve propulsion here?
        _gDrive.ApplyPropulsion(desiredMovement);
        _gDrive.LateUpdate(frame);
        _tDrive.LateUpdate(frame);
    }
        
}
public class TDrive
{
    // TODO: Expand this stub
    public void EarlyUpdate(int frame)
    {
    }

    public void LateUpdate(int frame)
    {
    }
}
public enum SensorPollFrequency
{
    Low,        // For targets veeery far away (>10x max weapon range)
    Medium,     // For targets out of immediate gun range (>max weapon range)
    High,       // For targets out of gun range but being tracked by missiles in a non terminal phase
    Realtime    // For targets in gun range or being tracked by missiles in a terminal phase
}
public class SensorPolling
{
    public static int GetFramesBetweenPolls(SensorPollFrequency freq)
    {
        switch (freq)
        {
            case SensorPollFrequency.Low:      return 600;  // 10 seconds
            case SensorPollFrequency.Medium:   return 60;   // 1 second
            case SensorPollFrequency.High:     return 10;   // every 10 frames
            case SensorPollFrequency.Realtime: return 1;    // every frame
            default: return Int32.MaxValue;                 // Fuck you
        }
    }

}
public class TargetTracker
{
    public TargetTracker(IMyTurretControlBlock block)
    {
        Block = block;
    }

    public IMyTurretControlBlock Block { get; }
    public bool Closed => Block.Closed;

    // Explicit state tracking
    bool _hadTarget;

    bool _wasValid = true;

    public bool HasTarget { get; private set; }
    public bool JustLostTarget { get; private set; }
    public bool Invalid { get; private set; }
    public bool JustInvalidated { get; private set; }

    public bool Enabled
    {
        get { return Block.Enabled; }
        set { Block.Enabled = value; }
    }

    public string CustomName
    {
        get { return Block.CustomName; }
        set { Block.CustomName = value; }
    }

    public long TargetedEntity { get; set; }
    public TrackableShip TrackedShip { get; set; }
        
        
    public Vector3D Position => Block.GetPosition();

    /// <summary>Updates the HasTarget / JustLostTarget / TargetedEntity state.</summary>
    public void UpdateState()
    {
        var currentHasTarget = Block.HasTarget;
        JustLostTarget = !currentHasTarget && _hadTarget;
        _hadTarget = currentHasTarget;

        HasTarget = currentHasTarget;

        var currentInvalid = TrackedShip != null && TrackedShip.IntersectsLargerShipAABB;
        JustInvalidated = currentInvalid && _wasValid;
        _wasValid = !currentInvalid;

        Invalid = currentInvalid; 
            

        if (!currentHasTarget)
            TargetedEntity = 0;
    }

    /// <summary>Gets the currently targeted entity and updates TargetedEntity.</summary>
    public MyDetectedEntityInfo GetTargetedEntity()
    {
        var info = Block.GetTargetedEntity();
        TargetedEntity = info.EntityId;
        return info;
    }
}
public class ControllableShip : SupportingShip
{
    GyroManager _gyroManager;
    public GunManager Guns;
    FireController _fireController;
    List<IMyLargeTurretBase> _turrets; // TODO: Abstract into a turrets handler
    PropulsionController _propulsionController;

    TrackableShip _currentTarget;
    bool _hasTarget;

    Vector3D _cachedGravity;
    bool _gravityValid = false;
        

    public ControllableShip(IMyCubeGrid grid, List<IMyTerminalBlock> blocks, List<IMyTerminalBlock> trackerBlocks) : base(grid, trackerBlocks)
    {
        Program.LogLine("New ControllableShip : SupportingShip : ArgusShip", LogLevel.Debug);
        _gyroManager = new GyroManager(blocks);
        Guns = new GunManager(blocks, this);
        _fireController = new FireController(this, Guns);
        foreach (var block in blocks) // TODO: Proper controller candidate system (and perhaps manager)
        {
            var controller = block as IMyShipController;
            // var aiFlight = block as IMyFlightMovementBlock;
            // var aiOffensive = block as IMyOffensiveCombatBlock;
            if (controller != null) Controller = controller;
        }
        if (Controller == null) Program.LogLine($"WARNING: Controller not present in group: {Config.GroupName}");
        _propulsionController = new PropulsionController(blocks, this);
    }

    public IMyShipController Controller { get; set; }
    //public IMyFlightMovementBlock AiFlight { get; set; }
    public Vector3D Forward => Controller.WorldMatrix.Forward;

    public Vector3 LocalForward => Base6Directions.Directions[(int)Controller.Orientation.Forward];
    public Direction LocalOrientationForward => (Direction)Controller.Orientation.Forward;
    public Vector3D Up => Controller.WorldMatrix.Up;
    public MatrixD WorldMatrix => _grid.WorldMatrix;
    public TrackableShip CurrentTarget => _currentTarget;
        
    public Vector3D Gravity 
    {
        get
        {
            if (!_gravityValid)
            {
                _cachedGravity = Controller.GetNaturalGravity();
                _gravityValid = true; 
            }

            return _cachedGravity;
        }
    }

    public override void EarlyUpdate(int frame)
    {
        base.EarlyUpdate(frame);
        Guns.EarlyUpdate(frame);
        _propulsionController.EarlyUpdate(frame);
    }

    public override void LateUpdate(int frame)
    {
        base.LateUpdate(frame);
        if (_hasTarget)
        {
            _gravityValid = false;
            
            
            var solution = _fireController.ArbitrateFiringSolution();
            if (solution.TargetPosition == Vector3D.Zero) _gyroManager.ResetGyroOverrides(); // TODO: Don't spam this
            else _gyroManager.Rotate(ref solution);
            Guns.LateUpdate(frame);
        }
            
        _propulsionController.LateUpdate(frame);
    }

    public void UnTarget()
    {
        _currentTarget = null;
        _hasTarget = false;
        _gyroManager.ResetGyroOverrides();
    }

    public void Target()
    {
            
        _currentTarget = ShipManager.GetForwardTarget(this, Config.LockRange, Config.LockAngle);
        _hasTarget = true;
        if (_currentTarget == null)
        {
            _hasTarget = false;
            _gyroManager.ResetGyroOverrides();
            Program.LogLine("Couldn't find new target", LogLevel.Warning);
        }
        else
        {
            Program.LogLine("Got new target", LogLevel.Info);
        }
    }

    public Vector3D GetTargetPosition()
    {
        return _currentTarget?.Position ?? Vector3D.Zero;
    }
}
public class SupportingShip : ArgusShip 
{
    private readonly List<TargetTracker> _targetTrackers;
    protected readonly IMyCubeGrid _grid;

        
    public SupportingShip(IMyCubeGrid grid, List<IMyTerminalBlock> trackerBlocks)
    {
        Program.LogLine("New SupportingShip : ArgusShip", LogLevel.Debug);
        IMyUserControllableGun gun = null;
        IMyMotorStator rotor = null;
        _targetTrackers = new List<TargetTracker>();
        foreach (var block in trackerBlocks)
        {
            var controller = block as IMyTurretControlBlock;
            if (controller != null)
            {
                    
                _targetTrackers.Add(new TargetTracker(controller));
                continue;
            }
            gun = gun ?? block as IMyUserControllableGun;
            rotor = rotor ?? block as IMyMotorStator;
        }

        // if (gun == null) throw new Exception($"FATAL: Grid '{grid.CustomName}' tracker group missing a gun.");
        // if (rotor == null) throw new Exception($"FATAL: Grid '{grid.CustomName}' tracker group missing a rotor.");


        if (gun != null && rotor != null)
        {
            Program.LogLine("Setting up trackers", LogLevel.Debug);
            foreach (var tracker in _targetTrackers)
            {
                Program.LogLine($"Set up tracker: {tracker.CustomName}");
                var block = tracker.Block;
                block.ClearTools();
                block.AddTool(gun);
                block.AzimuthRotor = null; // These aren't intended to actually shoot at anything, just used for target designation and scanning
                block.ElevationRotor = rotor;
                block.AIEnabled = true;
                block.CustomName = Config.SearchingName;
            }

            if (_targetTrackers.Count <= 0) Program.LogLine("No target trackers in group", LogLevel.Warning);
        }
        else Program.LogLine($"Gun/rotor not present in group: {Config.TrackerGroupName}, cannot setup trackers", LogLevel.Warning);
            
        _grid = grid;
    }
        
        
    public override Vector3D Position => _grid.GetPosition();
    public override Vector3D Velocity => CVelocity;
    public override Vector3D Acceleration => (CVelocity - CPreviousVelocity) * 60;
    public override float GridSize => _grid.GridSize;

    public override string Name => _grid.CustomName;
    public override string ToString() => Name;

    public override void EarlyUpdate(int frame)
    {
        CPreviousVelocity = CVelocity;
        CVelocity = _grid.LinearVelocity;
    }

    public override void LateUpdate(int frame)
    {
        for (int i = _targetTrackers.Count - 1; i >= 0; i--)
        {
            var tracker = _targetTrackers[i];

            // Cleanup destroyed trackers
            if (tracker.Closed)
            {
                _targetTrackers.RemoveAt(i);
                continue;
            }

            // Update target state
            tracker.UpdateState();
            // Tracker has no target
            if (!tracker.HasTarget || tracker.Invalid)
            {
                if (tracker.JustLostTarget && !tracker.Enabled)
                {
                    tracker.Enabled = true;
                    tracker.CustomName = Config.SearchingName;

                    if (tracker.TrackedShip != null)
                    {
                        tracker.TrackedShip.Defunct = true;
                        tracker.TrackedShip.Tracker = null;
                    }
                        
                    tracker.TrackedShip = null;

                        
                }
                else if (tracker.JustInvalidated)
                {
                    tracker.Enabled = true;
                    tracker.CustomName = Config.SearchingName;
                }

                continue;
            }
            // Tracker currently has a target
            if (tracker.TargetedEntity != 0) continue;
                
            var target = tracker.GetTargetedEntity();
                
            TargetTracker existingTracker;
            // Already tracked by this or another tracker
            if (ShipManager.HasNonDefunctTrackableShip(target.EntityId, out existingTracker))
            {
                if (existingTracker == tracker && tracker.Enabled) tracker.Enabled = false;
                existingTracker.TrackedShip.AddTrackedBlock(target, null, tracker);
                continue;
            }
            // Newly detected ship — register it
            var trackableShip = ShipManager.AddTrackableShip(tracker, target.EntityId, target);
            tracker.TrackedShip = trackableShip;
            tracker.CustomName = Config.TrackingName;
            tracker.Enabled = false;
            tracker.TargetedEntity = target.EntityId;   
                
        }
    }
}
enum ScannedBlockType
{
    Any,
    Weapons,
    Propulsion,
    PowerSystems,
    LikelyDecoy
}
class ScannedBlockTracker
{
    int _timer;
    private readonly int _initialTimer;
    public ScannedBlockType Type;

    public ScannedBlockTracker(int timer, ScannedBlockType type)
    {
        _timer = timer;
        _initialTimer = timer;
        Type = type;
    }
    public void ResetTimer()
    {
        _timer = _initialTimer;
    }

    public bool Countdown()
    {
        _timer--;
        return _timer <= 0;
    }
}
public class TrackableShip : ArgusShip
{
        
    Dictionary<Vector3I, ScannedBlockTracker> _scannedBlocks = new Dictionary<Vector3I, ScannedBlockTracker>();
    Dictionary<Vector3I, ScannedBlockTracker> _scannedBlocks_Swap = new Dictionary<Vector3I, ScannedBlockTracker>();

    public SupportingShip TrackingShip;

    public MyDetectedEntityInfo Info;

    public long EntityId;
    bool _aabbNeedsRecalc = true;
    BoundingBoxD _cachedAABB;
    Vector3D _cachedGridOffset;
    Vector3D _previousPosition;

    private readonly float _gridSize = 1f;

    public TrackableShip(TargetTracker tracker, long entityId, MyDetectedEntityInfo initial)
    {
        Tracker = tracker;
        EntityId = entityId;
        PollFrequency = SensorPollFrequency.Medium;

            
        switch (initial.Type)
        {
            case MyDetectedEntityType.SmallGrid:
                _gridSize = 0.5f;
                break;
            case MyDetectedEntityType.LargeGrid:
                _gridSize = 2.5f;
                break;
        }

        Info = initial;
        _worldMatrix = Info.Orientation;
        _worldMatrix.Translation = Info.Position;
    }


    public override Vector3D Position => Info.Position;
    public override Vector3D Velocity => CVelocity;
    public override Vector3D Acceleration => (CVelocity - CPreviousVelocity) * 60;
    public override float GridSize => _gridSize;

    public override string Name => $"Trackable ship {EntityId}";

    public bool Defunct { get; set; } = false;

    public override string ToString() => Name;

    public TargetTracker Tracker { get; set; }
    public bool IntersectsLargerShipAABB { get; set; }

    public BoundingBoxD WorldAABB => Info.BoundingBox;
        
    public Vector3D Extents => LocalAABB.Extents;
    public Vector3D HalfExtents => LocalAABB.HalfExtents;
    public int ProxyId { get; set; } = 0;
        

    public BoundingBoxD LocalAABB
    {
        get
        {
            if (_aabbNeedsRecalc) GenerateLocalAABB();
            return _cachedAABB;
        }
    }

    public Vector3D GridOffset
    {
        get
        {
            if (_aabbNeedsRecalc) GenerateLocalAABB();
            return _cachedGridOffset;
        }
    }



    Vector3D _displacement;
    MatrixD _worldMatrix;

    public Vector3D TakeDisplacement()
    {
        var temp = _displacement;
        _displacement = Vector3D.Zero;
        return temp;
    }

    void GenerateLocalAABB()
    {
        _cachedAABB = OBBReconstructor.GetMaxInscribedOBB(WorldAABB, Info.Orientation, _gridSize);
        _aabbNeedsRecalc = false;
        var extents = HalfExtents;
        var h = _gridSize / 2;
        _cachedGridOffset = new Vector3D(h - extents.X % _gridSize, h - extents.Y % _gridSize, h - extents.Z % _gridSize);
    }
    public override void EarlyUpdate(int frame)
    {
        if (Defunct) return;
        if ((frame + RandomUpdateJitter) % SensorPolling.GetFramesBetweenPolls(PollFrequency) != 0) return;
        Info = Tracker.GetTargetedEntity();
        if (Tracker.Closed || Info.EntityId != EntityId) Defunct = true;
        CPreviousVelocity = CVelocity;
        CVelocity = Info.Velocity;
        _displacement = Position - _previousPosition;

        if (PollFrequency == SensorPollFrequency.Realtime && (_displacement * 60 - CVelocity).LengthSquared() > 10000)
        {
            _aabbNeedsRecalc = true;
            Vector3I displacement =
                (Vector3I)(Vector3D.Transform(_displacement - (CVelocity / 60), MatrixD.Invert(_worldMatrix)) * 2);
            DisplaceTrackedBlocks(displacement);
        }
            
        _worldMatrix = Info.Orientation;
        _worldMatrix.Translation += Info.Position;
        _previousPosition = Position;
    }

    public override void LateUpdate(int frame)
    {
            
        _scannedBlocks_Swap.Clear();
        foreach (var kv in _scannedBlocks)
        {
            var worldPos = Vector3D.Transform(
                (Vector3D)(Vector3)kv.Key * (double)_gridSize + GridOffset,
                _worldMatrix
            );

            if (kv.Value.Countdown()) continue;
            _scannedBlocks_Swap[kv.Key] = kv.Value; // keep the entry if still valid
                
            Program.Debug.DrawPoint(worldPos, Color.White, 0.2f, 0.016f, true);
        }

        var temp = _scannedBlocks;
        _scannedBlocks = _scannedBlocks_Swap;
        _scannedBlocks_Swap = temp;

        Program.Debug.DrawAABB(WorldAABB, Color.Green, DebugAPI.Style.Wireframe, 0.02f, 0.016f);

        var tempobb = new MyOrientedBoundingBoxD(LocalAABB, Info.Orientation);
        tempobb.Center = WorldAABB.Center;
        Program.Debug.DrawOBB(tempobb, Color.Red, DebugAPI.Style.Wireframe, 0.02f, 0.016f);
            
    }

    public void AddTrackedBlock(MyDetectedEntityInfo info, IMyLargeTurretBase turret = null,
        TargetTracker controller = null)
    {
        if (info.EntityId != Info.EntityId || info.HitPosition == null) return;

        var targetBlockPosition = (Vector3D)info.HitPosition;
        //Program.Debug.DrawPoint(targetBlockPosition, Color.Red, 0.2f, 5f, true);
        var worldDirection = targetBlockPosition - Position;
        var positionInGridDouble =
            Vector3D.TransformNormal(worldDirection,
                MatrixD.Transpose(
                    _worldMatrix));
        positionInGridDouble-= GridOffset;
        var positionInGridInt = new Vector3I(
            (int)Math.Round(positionInGridDouble.X / _gridSize),
            (int)Math.Round(positionInGridDouble.Y / _gridSize),
            (int)Math.Round(positionInGridDouble.Z / _gridSize)
        );

        var type = ScannedBlockType.Any;
        var targetingGroup = turret != null ? turret.GetTargetingGroup() :
            controller != null ? controller.Block.GetTargetingGroup() : "";
        switch (targetingGroup)
        {
            case "Weapons":
                type = ScannedBlockType.Weapons;
                break;
            case "Propulsion":
                type = ScannedBlockType.Propulsion;
                break;
            case "Power Systems":
                type = ScannedBlockType.PowerSystems;
                break;
        }
        if (_scannedBlocks.ContainsKey(positionInGridInt))
        {
            var existing = _scannedBlocks[positionInGridInt];
            if (type != existing.Type) existing.Type = ScannedBlockType.LikelyDecoy;
            existing.ResetTimer();
            return;
        }
        else
        {
            _scannedBlocks.Add(positionInGridInt, new ScannedBlockTracker(Config.ScannedBlockMaximumValidTimeFrames, type));
        }
            
    }

    void DisplaceTrackedBlocks(Vector3I displacement)
    {
        var newBlocks = new Dictionary<Vector3I, ScannedBlockTracker>();

        foreach (var block in _scannedBlocks)
        {
            newBlocks.Add(block.Key + displacement, block.Value);
        }

        _scannedBlocks = newBlocks;
    }
}
public static class ShipManager
{
        
    public static readonly List<ArgusShip> AllShips = new List<ArgusShip>();
        
    private static List<TrackableShip> _reusedShipList = new List<TrackableShip>();
        
    //public static Dictionary<long, TargetTracker> EntityIdToTracker = new Dictionary<long, TargetTracker>();

    public static Dictionary<long, TrackableShip> EntityIdToTrackableShip = new Dictionary<long, TrackableShip>();
        
    private static IEnumerator<TrackableShip> _pollEnumerator;

    private static MyDynamicAABBTreeD _trackableShipTree = new MyDynamicAABBTreeD();

    static ShipManager()
    {
            
    }
    public static ControllableShip PrimaryShip { get; set; }

        
        
    public static void EarlyUpdate(int frame)
    {
        UpdatePollFrequency();
        for (var index = AllShips.Count - 1; index >= 0; index--)
        {
            var ship = AllShips[index];

            ship.EarlyUpdate(frame);
        }



        foreach (var ship in AllShips)
        {
            var trackable = ship as TrackableShip;
            if (trackable == null) continue;
            var box = trackable.WorldAABB;
                    
            if (trackable.ProxyId != 0)
            {
                var displacement = trackable.TakeDisplacement();
                _trackableShipTree.MoveProxy(trackable.ProxyId, ref box, displacement);
            }
            else
            {
                trackable.ProxyId = _trackableShipTree.AddProxy(ref box, trackable, 0U);
            }

        }
            

        foreach (var kvp in DynamicAABBTreeDHelper.GetAllOverlaps(_trackableShipTree))
        {
            var testShip = kvp.Key;
            var overlaps = kvp.Value;

            var shipSize = testShip.WorldAABB.Size.LengthSquared();

            foreach (var overlapShip in overlaps)
            {
                    

                var overlapShipSize = overlapShip.WorldAABB.Size.LengthSquared();

                // var outcome = overlapShipSize > shipSize ? "losing" : "winning";
                // Program.Log($"Ship {testShip?.Name} is overlapping ship {overlapShip?.Name} and {outcome}");
                    
                if (overlapShipSize > shipSize)
                {
                    testShip.IntersectsLargerShipAABB = true;
                    return;
                }
            }

            testShip.IntersectsLargerShipAABB = false;
        }
    }

    private static void UpdatePollFrequency()
    {
        if (!_pollEnumerator.MoveNext())
        {
            // reached the end, restart
            _pollEnumerator = UpdatePollFrequenciesOneByOne().GetEnumerator();
            _pollEnumerator.MoveNext();
        }
    }

    public static void LateUpdate(int frame)
    {
        for (var index = AllShips.Count - 1; index >= 0; index--)
        {
            var ship = AllShips[index];
            ship.LateUpdate(frame);
        }
    }
        
        
    private static IEnumerable<TrackableShip> UpdatePollFrequenciesOneByOne()
    {
        var maxSqr = Config.MaxWeaponRange * Config.MaxWeaponRange;

        for (var index = AllShips.Count - 1; index >= 0; index--)
        {
            if (index >= AllShips.Count) continue;
            var ship = AllShips[index];
            var trackableShip = ship as TrackableShip;
            if (trackableShip == null) continue;

            var pos = trackableShip.Position;
            var ourPos = PrimaryShip.Position;
            var distSqr = (pos - ourPos).LengthSquared();

            if (distSqr > maxSqr)
                trackableShip.PollFrequency = SensorPollFrequency.Medium;
            else
                trackableShip.PollFrequency = SensorPollFrequency.Realtime;

            yield return trackableShip; // yields one ship per call
        }
    }
        
        
        
    /// <summary>
    /// Gets trackable ships within a set range of this ship. Do not store the returned list, it WILL be mutated unexpectedly.
    /// </summary>
    /// <param name="ship">The target ship to test.</param>
    /// <param name="range">The search range.</param>
    /// <returns>A REUSED list containing all ships in range. This is REUSED internally and should not be persisted.</returns>
    public static List<TrackableShip> GetTargetsInRange(SupportingShip ship, double range)
    {
        // Just an O(n) lookup for now :)
        _reusedShipList.Clear();
        foreach (var otherShip in AllShips)
        {
            var enemyShip = otherShip as TrackableShip;
            if (enemyShip == null) continue;
            if ((enemyShip.Position - ship.Position).LengthSquared() < range * range) _reusedShipList.Add(enemyShip);
        }
            
        return _reusedShipList;
    }

    public static TrackableShip GetForwardTarget(ControllableShip ship, double range, float coneAngleDegrees)
    {
        var candidates = GetTargetsInRange(ship, range);
        if (candidates.Count < 1) return null;
            
            
        var forward = ship.Forward;
        double cosCone = Math.Cos(coneAngleDegrees * Math.PI / 180.0);
            
        double bestScore = double.MaxValue;
        TrackableShip bestCandidate = null;
            
        // This more or less gets the closest ship closest to the center of the crosshair
        foreach (var candidate in candidates)
        {
            var shipToCandidate = (candidate.Position - ship.Position);
            var length = shipToCandidate.Length();

            var dot = (shipToCandidate / length).Dot(forward);
            dot = MathHelperD.Clamp(dot, -1.0, 1.0);
            if (dot < cosCone) continue;
                
            var score = (1 - dot) * length;
            if (score < bestScore)
            {
                bestCandidate = candidate;
                bestScore = score;
            }
        }
        return bestCandidate;
    }

    public static void CreateControllableShip(IMyCubeGrid grid, IMyGridTerminalSystem gridTerminalSystem)
    {
        var group = gridTerminalSystem.GetBlockGroupWithName(Config.GroupName);
        Program.LogLine($"Getting group : {Config.GroupName}", LogLevel.Trace);
        var trackerGroup = gridTerminalSystem.GetBlockGroupWithName(Config.TrackerGroupName);
        Program.LogLine($"Getting group : {Config.TrackerGroupName}", LogLevel.Trace);
        var blocks = new List<IMyTerminalBlock>();
        var trackerBlocks = new List<IMyTerminalBlock>();
        if (group != null) {group.GetBlocks(blocks); Program.LogLine($"Got group: {Config.GroupName}", LogLevel.Debug);}
        else Program.LogLine($"Group not present: {Config.GroupName}", LogLevel.Warning);
        if (trackerGroup != null) {trackerGroup.GetBlocks(trackerBlocks); Program.LogLine($"Got group: {Config.TrackerGroupName}", LogLevel.Debug);}
        else Program.LogLine($"Group not present: {Config.TrackerGroupName}", LogLevel.Warning);
        var ship = new ControllableShip(grid, blocks, trackerBlocks);
        AllShips.Add(ship);
        PrimaryShip = ship;

            
        _pollEnumerator = UpdatePollFrequenciesOneByOne().GetEnumerator();
    }

    public static TrackableShip AddTrackableShip(TargetTracker tracker, long entityId, MyDetectedEntityInfo initial)
    {
        TrackableShip trackableShip;
        if (EntityIdToTrackableShip.TryGetValue(entityId, out trackableShip))
        {
            Program.LogLine("Restoring defunct ship" + entityId, LogLevel.Debug);
            if (!trackableShip.Defunct) return null;
            trackableShip.Tracker = tracker;
            trackableShip.Defunct = false;

            return trackableShip;
        }
        Program.LogLine("Creating new ship " + entityId, LogLevel.Debug);
        trackableShip = new TrackableShip(tracker, entityId, initial);
        AllShips.Add(trackableShip);
        //EntityIdToTracker.Add(entityId, tracker);
        EntityIdToTrackableShip.Add(entityId, trackableShip);
        return trackableShip;
    }
    public static void RemoveTrackableShip(TrackableShip trackableShip)
    {
        AllShips.Remove(trackableShip);
        //EntityIdToTracker.Remove(trackableShip.EntityId);
        EntityIdToTrackableShip.Remove(trackableShip.EntityId);
        _trackableShipTree.RemoveProxy(trackableShip.ProxyId);
    }


    public static bool HasNonDefunctTrackableShip(long targetEntityId, out TargetTracker existingTracker)
    {
        TrackableShip trackableShip;

        var keyExists = EntityIdToTrackableShip.TryGetValue(targetEntityId, out trackableShip);
        existingTracker = keyExists ? trackableShip.Tracker : null;
        return keyExists && !trackableShip.Defunct;
    }
}
