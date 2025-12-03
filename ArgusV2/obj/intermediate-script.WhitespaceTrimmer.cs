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
public static Program A;public static B C;public static Random D;public E F=new E(10);private static Queue<double>G=new
Queue<double>();int H;public
 Program
(){try{var I=DateTime.UtcNow;F.J("Beginning script setup",K.L);A=this;C=new B(this);D=new Random();M(
"Setup debug and RNG",K.C);N.O(Me);Program.M("Creating this ship as controllable ship",K.L);P.Q(Me.CubeGrid,GridTerminalSystem);Program.M(
"Creating commands",K.L);R.O();Runtime.UpdateFrequency=UpdateFrequency.Update1;var S=DateTime.UtcNow-I;M(
$"Setup completed in {S.TotalMilliseconds:F1} ms",K.T);}catch(Exception U){Echo("Crashed: "+U);}Echo(F.ToString());}public void
 Main
(string V,UpdateType W){try{if((W&UpdateType.Update1)!=0)X();if((W&(UpdateType.Trigger|UpdateType.Terminal))!=0)Y(V);}
catch(Exception U){Echo(U.ToString());Runtime.UpdateFrequency=UpdateFrequency.None;}}void X(){using(C.Z(a=>{G.Enqueue(a.
TotalMilliseconds);})){b.c();P.d(H);P.e(H++);}if(G.Count>600)G.Dequeue();f(G.Average());F.g();f(F);}void Y(string V){Action h;if(R.i(V,
out h)){h();}else{}}public static void f(object j){A.Echo(j.ToString());}public static void f(TimeSpan S,string k){double l
=S.Ticks/10.0;A.Echo($"{k}: {l} µs");}public static void M(object m,K n=K.L){A.F.J(m.ToString(),n);}}
public class B{public readonly bool o;public void r()=>p?.Invoke(q);Action<IMyProgrammableBlock>p;public void t()=>s?.
Invoke(q);Action<IMyProgrammableBlock>s;public void w(int u)=>v?.Invoke(q,u);Action<IMyProgrammableBlock,int>v;public int Ã(x
y,Color z,float ª=0.2f,float º=µ,bool?À=null)=>Á?.Invoke(q,y,z,ª,º,À??Â)??-1;Func<IMyProgrammableBlock,Vector3D,Color,
float,float,bool,int>Á;public int É(x Ä,x Å,Color z,float Ç=Æ,float º=µ,bool?À=null)=>È?.Invoke(q,Ä,Å,z,Ç,º,À??Â)??-1;Func<
IMyProgrammableBlock,Vector3D,Vector3D,Color,float,float,bool,int>È;public int Ï(BoundingBoxD Ê,Color z,Ë Í=Ë.Ì,float Ç=Æ,float º=µ,bool?À=
null)=>Î?.Invoke(q,Ê,z,(int)Í,Ç,º,À??Â)??-1;Func<IMyProgrammableBlock,BoundingBoxD,Color,int,float,float,bool,int>Î;public
int Ò(MyOrientedBoundingBoxD Ð,Color z,Ë Í=Ë.Ì,float Ç=Æ,float º=µ,bool?À=null)=>Ñ?.Invoke(q,Ð,z,(int)Í,Ç,º,À??Â)??-1;Func<
IMyProgrammableBlock,MyOrientedBoundingBoxD,Color,int,float,float,bool,int>Ñ;public int Ö(BoundingSphereD Ó,Color z,Ë Í=Ë.Ì,float Ç=Æ,int Ô=
15,float º=µ,bool?À=null)=>Õ?.Invoke(q,Ó,z,(int)Í,Ç,Ô,º,À??Â)??-1;Func<IMyProgrammableBlock,BoundingSphereD,Color,int,
float,int,float,bool,int>Õ;public int Û(MatrixD Ø,float Ù=1f,float Ç=Æ,float º=µ,bool?À=null)=>Ú?.Invoke(q,Ø,Ù,Ç,º,À??Â)??-1;
Func<IMyProgrammableBlock,MatrixD,float,float,float,bool,int>Ú;public int Þ(string Ü,x y,Color?z=null,float º=µ)=>Ý?.Invoke(
q,Ü,y,z,º)??-1;Func<IMyProgrammableBlock,string,Vector3D,Color?,float,int>Ý;public int ã(string ß,à á=à.C,float º=2)=>â?.
Invoke(q,ß,á.ToString(),º)??-1;Func<IMyProgrammableBlock,string,string,float,int>â;public void ç(string ß,string ä=null,Color?
å=null,à á=à.C)=>æ?.Invoke(q,ß,ä,å,á.ToString());Action<IMyProgrammableBlock,string,string,Color?,string>æ;public void ï(
out int u,double è,double é=0.05,ê ì=ê.ë,string í=null)=>u=î?.Invoke(q,è,é,ì.ToString(),í)??-1;Func<IMyProgrammableBlock,
double,double,string,string,int>î;public double ò(int u,double ð=1)=>ñ?.Invoke(q,u)??ð;Func<IMyProgrammableBlock,int,double>ñ;
public int ô()=>ó?.Invoke()??-1;Func<int>ó;public TimeSpan ö()=>õ?.Invoke()??TimeSpan.Zero;Func<TimeSpan>õ;public ø Z(Action<
TimeSpan>ù)=>new ø(this,ù);public struct ø:IDisposable{B ú;TimeSpan û;Action<TimeSpan>ü;public ø(B ý,Action<TimeSpan>ù){ú=ý;ü=ù;
û=ú.ö();}public void Dispose(){ü?.Invoke(ú.ö()-û);}}public enum Ë{þ,Ì,ÿ}public enum ê{Ā,ā,Ă,ă,Ą,ą,Ć,ć,Ĉ,ĉ,Ċ,ċ,Č,ë,č,Ď,ď,Đ
,đ,Ē,ē,Ĕ,ĕ,Ė,ė,Ę,ę,Ě,ě,Ĝ,ĝ,Ğ,ğ,Ġ,ġ,Ģ,ú,ģ,ü,Ĥ,ĥ,Ħ,ħ,Ĩ,A,ĩ,Ī,ī,Ĭ,ĭ,Į,į,İ,ı,û,Ĳ,ĳ,Ĵ,ĵ,Ķ,ķ,ĸ,Ĺ,ĺ,Ļ,ļ,Ľ,ľ,Ŀ,ŀ,Ł,ł,Ń,J,ń,Ņ,ņ,Ň,
ň,ŉ,Ŋ,ŋ,Ō,ō,Ŏ,ŏ,Ő,ő,Œ,œ}public enum à{C,Ŕ,ŕ,Ŗ,ŗ,Ř}const float Æ=0.02f;const float µ=-1;IMyProgrammableBlock q;bool Â;
public B(MyGridProgram ř,bool Ś=false){if(ř==null)throw new Exception("Pass `this` into the API, not null.");Â=Ś;q=ř.Me;var ś=
q.GetProperty("DebugAPI")?.As<IReadOnlyDictionary<string,Delegate>>()?.GetValue(q);if(ś!=null){Ŝ(out s,ś["RemoveAll"]);Ŝ(
out p,ś["RemoveDraw"]);Ŝ(out v,ś["Remove"]);Ŝ(out Á,ś["Point"]);Ŝ(out È,ś["Line"]);Ŝ(out Î,ś["AABB"]);Ŝ(out Ñ,ś["OBB"]);Ŝ(
out Õ,ś["Sphere"]);Ŝ(out Ú,ś["Matrix"]);Ŝ(out Ý,ś["GPS"]);Ŝ(out â,ś["HUDNotification"]);Ŝ(out æ,ś["Chat"]);Ŝ(out î,ś[
"DeclareAdjustNumber"]);Ŝ(out ñ,ś["GetAdjustNumber"]);Ŝ(out ó,ś["Tick"]);Ŝ(out õ,ś["Timestamp"]);t();o=true;}}void Ŝ<Ĳ>(out Ĳ ŝ,object Ş)=>ŝ=
(Ĳ)Ş;}public struct ţ<Ĳ>{Ĳ ş;bool Š;private readonly Func<Ĳ>š;public ţ(Func<Ĳ>Ţ){if(Ţ==null)throw new
ArgumentNullException("getter");š=Ţ;ş=default(Ĳ);Š=false;}public Ĳ Ť{get{if(!Š){ş=š();Š=true;}return ş;}}public void ť()=>Š=false;}public
static class ƚ{public class ũ{public object Ŧ;public string ŧ;public ũ(object j,string Ũ){Ŧ=j;ŧ=Ũ;}}public static string Ŭ(
object j,int Ū=-1){var ū=new StringBuilder();Ŭ(j,ū,Ū,null);Program.M("Serialized successfully",K.C);return ū.ToString();}
private static void Ŭ(object j,StringBuilder ū,int Ū,string ŭ){string Ů=new string(' ',Math.Max(Ū,0));if(j==null){if(ŭ!=null)ū.
AppendLine(Ů+ŭ+" = null");return;}ũ ů=j as ũ;if(ů!=null){bool ű=Ű(ů.Ŧ);if(ű&&ŭ!=null){ū.AppendLine(Ů+ŭ+" = "+Ų(ů.Ŧ)+"   # "+ů.ŧ);}
else{if(!string.IsNullOrEmpty(ů.ŧ))ū.AppendLine(Ů+"# "+ů.ŧ);Ŭ(ů.Ŧ,ū,Ū,ŭ);}return;}IDictionary<string,object>ų=j as
IDictionary<string,object>;if(ų!=null){if(ŭ!=null)ū.AppendLine(Ů+ŭ+" = [");foreach(var Ŵ in ų){Ŭ(Ŵ.Value,ū,Ū+2,Ŵ.Key);}if(ŭ!=null)ū
.AppendLine(Ů+"]");return;}IEnumerable<object>ŵ=j as IEnumerable<object>;if(ŵ!=null){if(ŭ!=null){ū.Append(Ů+ŭ+" = { ");
bool Ŷ=true;foreach(object ŷ in ŵ){if(!Ŷ)ū.Append(", ");ū.Append(Ų(ŷ));Ŷ=false;}ū.AppendLine(" }");}else{foreach(object ŷ in
ŵ)Ŭ(ŷ,ū,Ū,null);}return;}if(ŭ!=null){ū.AppendLine(Ů+ŭ+" = "+Ų(j));}}private static string Ų(object j){if(j==null)return
"null";if(j is string)return"\""+Ÿ((string)j)+"\"";if(j is bool)return((bool)j?"true":"false");if(j is float)return((float)j).
ToString("0.#####",System.Globalization.CultureInfo.InvariantCulture);if(j is double)return((double)j).ToString("0.##########",
System.Globalization.CultureInfo.InvariantCulture);if(j is int||j is long||j is short||j is byte)return j.ToString();return
"\""+Ÿ(j.ToString())+"\"";}private static string Ÿ(string Ź){return Ź.Replace("\\","\\\\").Replace("\"","\\\"");}private
static bool Ű(object ź){return ź is string||ź is bool||ź is int||ź is long||ź is short||ź is byte||ź is float||ź is double;}
public static object ƃ(string Ż){int ż=0;var Ž=new Dictionary<string,object>();while(ż<Ż.Length){ž(Ż,ref ż);if(ż>=Ż.Length)
break;string ŭ=ſ(Ż,ref ż);Program.f(ŭ);ƀ(Ż,ref ż,'=');object ź=Ɓ(Ż,ref ż);string Ũ=Ƃ(Ż,ref ż);if(Ũ!=null&&Ű(ź))ź=new ũ(ź,Ũ);Ž
[ŭ]=ź;}return Ž;}private static object Ɓ(string Ź,ref int ż){ž(Ź,ref ż);if(ż>=Ź.Length)return null;char Ƅ=Ź[ż];switch(Ƅ){
case'[':{ż++;var ų=new Dictionary<string,object>();while(true){ž(Ź,ref ż);if(ż>=Ź.Length)break;if(Ź[ż]==']'){ż++;break;}
string ŭ=ſ(Ź,ref ż);ƀ(Ź,ref ż,'=');object ź=Ɓ(Ź,ref ż);string Ũ=Ƃ(Ź,ref ż);if(Ũ!=null&&Ű(ź))ź=new ũ(ź,Ũ);ų[ŭ]=ź;}return ų;}
case'{':{ż++;var ŵ=new List<object>();while(true){ž(Ź,ref ż);if(ż>=Ź.Length)break;if(Ź[ż]=='}'){ż++;break;}object ź=Ɓ(Ź,ref
ż);string Ũ=Ƃ(Ź,ref ż);if(Ũ!=null&&Ű(ź))ź=new ũ(ź,Ũ);ŵ.Add(ź);ž(Ź,ref ż);if(ż<Ź.Length&&Ź[ż]==',')ż++;}return ŵ;}}string
Ɔ=ƅ(Ź,ref ż);string Ƈ=Ƃ(Ź,ref ż);object Ɖ=ƈ(Ɔ);if(Ƈ!=null&&Ű(Ɖ))Ɖ=new ũ(Ɖ,Ƈ);return Ɖ;}private static void ž(string Ź,ref
int ż){while(ż<Ź.Length){if(Ź[ż]==' '||Ź[ż]=='\t'||Ź[ż]=='\r'||Ź[ż]=='\n'){ż++;continue;}if(Ź[ż]=='#'){while(ż<Ź.Length&&Ź[
ż]!='\n')ż++;continue;}break;}}private static string ſ(string Ź,ref int ż){Ɗ(Ź,ref ż);int Ä=ż;while(ż<Ź.Length){char Ƅ=Ź[
ż];if(Ƅ=='=')break;if(Ƅ=='\n'||Ƅ=='\r')throw new Exception("Unexpected newline while reading key");ż++;}if(ż==Ä)throw new
Exception($"Empty key at index {ż}");string ŭ=Ź.Substring(Ä,ż-Ä).Trim();return ŭ;}private static string ƅ(string Ź,ref int ż){Ɗ(Ź
,ref ż);if(ż>=Ź.Length)return"";char Ƅ=Ź[ż];if(Ƅ=='"'||Ƅ=='\''){char Ƌ=Ƅ;ż++;int Ä=ż;while(ż<Ź.Length&&Ź[ż]!=Ƌ)ż++;string
ƌ=Ź.Substring(Ä,ż-Ä);if(ż<Ź.Length)ż++;return ƌ;}int ƍ=ż;while(ż<Ź.Length&&Ź[ż]!='\n'&&Ź[ż]!='\r'&&Ź[ż]!='#'&&Ź[ż]!=','&&
Ź[ż]!='}'&&Ź[ż]!=']')ż++;string Ɔ=Ź.Substring(ƍ,ż-ƍ).Trim();return Ɔ;}private static string Ƃ(string Ź,ref int ż){Ɗ(Ź,ref
ż);if(ż<Ź.Length&&Ź[ż]=='#'){ż++;int Ä=ż;while(ż<Ź.Length&&Ź[ż]!='\n'&&Ź[ż]!='\r')ż++;return Ź.Substring(Ä,ż-Ä).Trim();}
return null;}private static void Ɗ(string Ź,ref int ż){while(ż<Ź.Length&&(Ź[ż]==' '||Ź[ż]=='\t'||Ź[ż]=='\r'||Ź[ż]=='\n'))ż++;}
private static void ƀ(string Ź,ref int ż,char Ǝ){Ɗ(Ź,ref ż);if(ż>=Ź.Length||Ź[ż]!=Ǝ)throw new Exception("Expected '"+Ǝ+
"' at index "+ż);ż++;}private static object ƈ(string Ɔ){if(Ɔ.Length==0)return null;if(Ɔ=="true")return true;if(Ɔ=="false")return
false;int Ə;if(int.TryParse(Ɔ,out Ə))return Ə;double Ɛ;if(double.TryParse(Ɔ,System.Globalization.NumberStyles.Float,System.
Globalization.CultureInfo.InvariantCulture,out Ɛ))return Ɛ;return Ɔ;}public static void Ɣ(Dictionary<string,object>ų){var Ƒ=new List<
string>(ų.Keys);foreach(var ŭ in Ƒ){var ƒ=ų[ŭ];var Ƅ=ƒ as ũ;var Ɠ=ƒ as Dictionary<string,object>;var ŵ=ƒ as List<object>;if(Ƅ
!=null)ų[ŭ]=Ƅ.Ŧ;else if(Ɠ!=null)Ɣ(Ɠ);else if(ŵ!=null){for(int Ə=0;Ə<ŵ.Count;Ə++){var ƕ=ŵ[Ə];var Ɩ=ƕ as ũ;ŵ[Ə]=Ɩ!=null?Ɩ.Ŧ:
ŵ[Ə];}}}}public static Ĳ ƙ<Ĳ>(Dictionary<string,object>ų,string ŭ,ref string Ũ,Ĳ Ɨ=default(Ĳ)){Ũ="";object ź;if(ų.
TryGetValue(ŭ,out ź)){var Ƙ=ź as ũ;if(Ƙ!=null){ź=Ƙ.Ŧ;Ũ=Ƙ.ŧ;}if(ź is Ĳ)return(Ĳ)ź;try{return(Ĳ)Convert.ChangeType(ź,typeof(Ĳ));}
catch{return Ɨ;}}return Ɨ;}}class Ɲ{private readonly Dictionary<string,object>ƛ;Ɲ(Dictionary<string,object>Ɯ){ƛ=Ɯ;}public
static Ɲ Ɵ(Dictionary<string,object>ƞ,string Ü){ƚ.Ɣ(ƞ);Dictionary<string,object>ų;object j;if(!ƞ.TryGetValue(Ü,out j)||(ų=j as
Dictionary<string,object>)==null){ų=new Dictionary<string,object>();ƞ[Ü]=ų;}return new Ɲ(ų);}public void Ơ<Ĳ>(string ŭ,ref Ĳ ŝ,
string Ũ=""){var Ƙ=ƚ.ƙ(ƛ,ŭ,ref Ũ,ŝ);if(Ũ!="")ƛ[ŭ]=new ƚ.ũ(Ƙ,Ũ);else ƛ[ŭ]=Ƙ;}}public enum ƣ:byte{ơ,Ƣ,ĕ,ė,Ė,Ę,}public static
class Ƴ{private static readonly List<Ƥ>ƥ=new List<Ƥ>();private static readonly List<BoundingBoxD>Ʀ=new List<BoundingBoxD>();
private static readonly Dictionary<Ƥ,List<Ƥ>>Ƨ=new Dictionary<Ƥ,List<Ƥ>>();private static readonly List<List<Ƥ>>ƨ=new List<List
<Ƥ>>();private static int Ʃ=0;private static List<Ƥ>ƪ(){if(Ʃ>=ƨ.Count)ƨ.Add(new List<Ƥ>(8));var ŵ=ƨ[Ʃ++];ŵ.Clear();return
ŵ;}public static Dictionary<Ƥ,List<Ƥ>>ư(MyDynamicAABBTreeD ƫ){Ʃ=0;Ƨ.Clear();ƫ.GetAll(ƥ,clear:true,boxsList:Ʀ);for(int Ə=0
;Ə<ƥ.Count;Ə++){var Ƭ=ƥ[Ə];var ƭ=Ʀ[Ə];var Ʈ=ƪ();ƫ.OverlapAllBoundingBox(ref ƭ,Ʈ,0U,false);foreach(var Ư in Ʈ){if(Ƭ==Ư)
continue;if(!Ƨ.ContainsKey(Ƭ))Ƨ.Add(Ƭ,ƪ());Ƨ[Ƭ].Add(Ư);if(!Ƨ.ContainsKey(Ư))Ƨ.Add(Ư,ƪ());Ƨ[Ư].Add(Ƭ);}}return Ƨ;}public static
List<Ƥ>Ʋ(MyDynamicAABBTreeD ƫ,BoundingBoxD Ʊ){Ʃ=0;var Ʈ=ƪ();ƫ.OverlapAllBoundingBox(ref Ʊ,Ʈ,0U,false);return Ʈ;}}public
static class ƶ{public static double Ƶ(double ź,double ƴ){return(Math.Round(ź/ƴ)*ƴ);}}public static class ǲ{private static bool
ǋ(long Ʒ,long Ƹ,long ƹ,double ƺ,MatrixD ƻ,x Ƽ){double ƽ=Ʒ*ƺ;double ƾ=Ƹ*ƺ;double ƿ=ƹ*ƺ;x ǀ=ƻ.Right;x ǁ=ƻ.Up;x ǂ=ƻ.Forward;
x Ǆ=x.ǃ(ǀ);x ǅ=x.ǃ(ǁ);x ǆ=x.ǃ(ǂ);const double Ǉ=1e-8;double ǈ=ƽ*Ǆ.Ķ+ƾ*ǅ.Ķ+ƿ*ǆ.Ķ;if(ǈ>Ƽ.Ķ+Ǉ)return false;double ǉ=ƽ*Ǆ.ķ+ƾ*
ǅ.ķ+ƿ*ǆ.ķ;if(ǉ>Ƽ.ķ+Ǉ)return false;double Ǌ=ƽ*Ǆ.ĸ+ƾ*ǅ.ĸ+ƿ*ǆ.ĸ;if(Ǌ>Ƽ.ĸ+Ǉ)return false;return true;}private static bool Ǥ(x
ǀ,x ǁ,x ǂ,x Ƽ,out double ƽ,out double ƾ,out double ƿ){var ǌ=Math.Abs(ǀ.Ķ);var Ǎ=Math.Abs(ǁ.Ķ);var ǎ=Math.Abs(ǂ.Ķ);var Ǐ=
Math.Abs(ǀ.ķ);var ǐ=Math.Abs(ǁ.ķ);var Ǒ=Math.Abs(ǂ.ķ);var ǒ=Math.Abs(ǀ.ĸ);var Ǔ=Math.Abs(ǁ.ĸ);var ǔ=Math.Abs(ǂ.ĸ);double Ǖ=Ƽ
.Ķ,ǖ=Ƽ.ķ,Ǘ=Ƽ.ĸ;double ǘ=ǌ*(ǐ*ǔ-Ǒ*Ǔ)-Ǎ*(Ǐ*ǔ-Ǒ*ǒ)+ǎ*(Ǐ*Ǔ-ǐ*ǒ);const double Ǚ=1e-12;if(Math.Abs(ǘ)<Ǚ){ƽ=ƾ=ƿ=0.0;return false
;}double ǚ=(ǐ*ǔ-Ǒ*Ǔ)/ǘ;double Ǜ=-(Ǎ*ǔ-ǎ*Ǔ)/ǘ;double ǜ=(Ǎ*Ǒ-ǎ*ǐ)/ǘ;double ǝ=-(Ǐ*ǔ-Ǒ*ǒ)/ǘ;double Ǟ=(ǌ*ǔ-ǎ*ǒ)/ǘ;double ǟ=-(ǌ
*Ǒ-ǎ*Ǐ)/ǘ;double Ǡ=(Ǐ*Ǔ-ǐ*ǒ)/ǘ;double ǡ=-(ǌ*Ǔ-Ǎ*ǒ)/ǘ;double Ǣ=(ǌ*ǐ-Ǎ*Ǐ)/ǘ;ƽ=ǚ*Ǖ+Ǜ*ǖ+ǜ*Ǘ;ƾ=ǝ*Ǖ+Ǟ*ǖ+ǟ*Ǘ;ƿ=Ǡ*Ǖ+ǡ*ǖ+Ǣ*Ǘ;const
double ǣ=-1e-9;if(ƽ<ǣ||ƾ<ǣ||ƿ<ǣ)return false;ƽ=Math.Max(0.0,ƽ);ƾ=Math.Max(0.0,ƾ);ƿ=Math.Max(0.0,ƿ);return true;}public static
BoundingBoxD Ǳ(BoundingBoxD ǥ,MatrixD ƻ,double ƺ){ƺ/=2;x Ƽ=ǥ.HalfExtents;x ǀ=ƻ.Right;x ǁ=ƻ.Up;x ǂ=ƻ.Forward;double Ǧ,ǧ,Ǩ;if(Ǥ(ǀ,ǁ,ǂ,
Ƽ,out Ǧ,out ǧ,out Ǩ)){long Ʒ=Math.Max(0,(long)(Ǧ/ƺ+1e-12));long Ƹ=Math.Max(0,(long)(ǧ/ƺ+1e-12));long ƹ=Math.Max(0,(long)(
Ǩ/ƺ+1e-12));bool ǩ;do{ǩ=false;if(ǋ(Ʒ+1,Ƹ,ƹ,ƺ,ƻ,Ƽ)){Ʒ++;ǩ=true;}if(ǋ(Ʒ,Ƹ+1,ƹ,ƺ,ƻ,Ƽ)){Ƹ++;ǩ=true;}if(ǋ(Ʒ,Ƹ,ƹ+1,ƺ,ƻ,Ƽ)){ƹ++;
ǩ=true;}}while(ǩ);var Ǫ=new x(Ʒ*ƺ,Ƹ*ƺ,ƹ*ƺ);return new BoundingBoxD(-Ǫ,Ǫ);}double ǫ=Ƽ.Ķ/(Math.Abs(ǀ.Ķ)+Math.Abs(ǁ.Ķ)+Math.
Abs(ǂ.Ķ));double Ǭ=Ƽ.ķ/(Math.Abs(ǀ.ķ)+Math.Abs(ǁ.ķ)+Math.Abs(ǂ.ķ));double ǭ=Ƽ.ĸ/(Math.Abs(ǀ.ĸ)+Math.Abs(ǁ.ĸ)+Math.Abs(ǂ.ĸ))
;double Ǯ=Math.Min(Math.Min(ǫ,Ǭ),ǭ);long ǯ=Math.Max(0,(long)Math.Floor(Ǯ/ƺ));var ǰ=new x(ǯ*ƺ,ǯ*ƺ,ǯ*ƺ);return new
BoundingBoxD(-ǰ,ǰ);}}public class ȁ{double ǳ;double Ǵ;public double ǵ;double Ƕ;public double Ƿ;double Ǹ;double ǹ;double Ǻ;public ȁ(
double ǻ,double Ǽ,double ǽ,double Ǿ=0,double ǿ=0,double Ȁ=60){ǵ=ǻ;Ƕ=Ǽ;Ƿ=ǽ;Ǹ=Ǿ;ǹ=ǿ;Ǻ=Ȁ;}public double Ȇ(double Ȃ,int ȃ){double
Ȅ=Math.Round(Ȃ,ȃ);ǳ=ǳ+(Ȃ/Ǻ);ǳ=(Ǹ>0&&ǳ>Ǹ?Ǹ:ǳ);ǳ=(ǹ<0&&ǳ<ǹ?ǹ:ǳ);double ȅ=(Ȅ-Ǵ)*Ǻ;Ǵ=Ȅ;return(ǵ*Ȃ)+(Ƕ*ǳ)+(Ƿ*ȅ);}public void ȇ
(){ǳ=Ǵ=0;}}public static class ɐ{public static double Ȉ=>Double.MaxValue;public const double ȉ=1e-6,Ȋ=-0.5,ȋ=1.73205,Ȍ=ȋ/
2,ȍ=1.0/3.0,Ȏ=1.0/9.0,ȏ=1.0/6.0,Ȑ=1.0/54.0;public static x ȯ(double ȑ,x Ȓ,x ȓ,x Ȕ,x ȕ,x Ȗ,x ȗ,x Ș,bool ș,x Ț=default(x),
bool ț=false){double Ȝ=0;x Ȟ=x.ȝ,ȟ=ȗ,Ƞ=Ȗ,ȡ,Ȣ;if(ȗ.ȣ()>1){Ȝ=Math.Min((x.Ȥ(ȗ)*N.ȥ.Ȧ-x.ȧ(ref Ȗ,ref ȗ)).Ȩ(),2*N.ȥ.Ȧ)/ȗ.Ȩ();Ȗ=x.ȩ
(Ȗ+ȗ*Ȝ,N.ȥ.Ȧ);Ƞ+=ȗ*Ȝ*0.5;ȗ=x.ȝ;}if(ȓ.ȣ()>1){double Ȫ=Math.Max((x.Ȥ(ȓ)*ȑ-x.ȧ(ref Ȓ,ref ȓ)).Ȩ(),0)/ȓ.Ȩ(),ȫ=(Ȓ*Ȫ+ȓ*Ȫ*Ȫ).Ȩ();
x Ȭ=Ȕ+Ȗ*Ȫ+0.5*ȗ*Ȫ*Ȫ;if(Ȭ.Ȩ()>ȫ){ȓ=x.ȝ;Ȓ=x.ȩ(Ȓ+ȓ*Ȫ,ȑ);Ȕ-=(x)x.Ȥ(Ȕ)*ȫ;}}ȡ=Ȗ-Ȓ;Ȣ=ȗ-ȓ;double Ȯ=ȭ(Ȣ.ȣ()*0.25,Ȣ.Ķ*ȡ.Ķ+Ȣ.ķ*ȡ.ķ+Ȣ
.ĸ*ȡ.ĸ,ȡ.ȣ()-Ȓ.ȣ()+Ȕ.Ķ*Ȣ.Ķ+Ȕ.ķ*Ȣ.ķ+Ȕ.ĸ*Ȣ.ĸ,2*(Ȕ.Ķ*ȡ.Ķ+Ȕ.ķ*ȡ.ķ+Ȕ.ĸ*ȡ.ĸ),Ȕ.ȣ());if(Ȯ==Ȉ||double.IsNaN(Ȯ)||Ȯ>100)Ȯ=100;if(Ȝ>
Ȯ){Ȝ=Ȯ;Ȯ=0;}else Ȯ-=Ȝ;return ș?Ȕ+Ȗ*Ȯ+Ƞ*Ȝ+0.5*ȟ*Ȝ*Ȝ+0.5*ȗ*Ȯ*Ȯ+Ȟ:ȕ+(Ȗ-Ȓ)*Ȯ+(Ƞ-Ȓ)*Ȝ+0.5*ȟ*Ȝ*Ȝ+0.5*ȗ*Ȯ*Ȯ+-0.5*Ț*(Ȯ+Ȝ)*(Ȯ+Ȝ)*
Convert.ToDouble(ț)+Ȟ;}public static double ȭ(double Ȱ,double ȱ,double Ƅ,double Ȳ,double ȳ){if(Math.Abs(Ȱ)<ȉ)Ȱ=Ȱ>=0?ȉ:-ȉ;double
ȴ=1/Ȱ;ȱ*=ȴ;Ƅ*=ȴ;Ȳ*=ȴ;ȳ*=ȴ;double ȵ=-Ƅ,ȶ=ȱ*Ȳ-4*ȳ,ȷ=-ȱ*ȱ*ȳ-Ȳ*Ȳ+4*Ƅ*ȳ,ȸ;double[]ȹ;bool Ȼ=Ⱥ(ȵ,ȶ,ȷ,out ȹ);ȸ=ȹ[0];if(Ȼ){if(Math
.Abs(ȹ[1])>Math.Abs(ȸ))ȸ=ȹ[1];if(Math.Abs(ȹ[2])>Math.Abs(ȸ))ȸ=ȹ[2];}double ȼ,Ƚ,Ⱦ,ȿ,ɀ;double Ɂ=ȸ*ȸ-4*ȳ;if(Math.Abs(Ɂ)<ȉ){ȼ
=Ƚ=ȸ*0.5;Ɂ=ȱ*ȱ-4*(Ƅ-ȸ);if(Math.Abs(Ɂ)<ȉ)Ⱦ=ȿ=ȱ*0.5;else{ɀ=Math.Sqrt(Ɂ);Ⱦ=(ȱ+ɀ)*0.5;ȿ=(ȱ-ɀ)*0.5;}}else{ɀ=Math.Sqrt(Ɂ);ȼ=(ȸ+
ɀ)*0.5;Ƚ=(ȸ-ɀ)*0.5;double ɂ=1/(ȼ-Ƚ);Ⱦ=(ȱ*ȼ-Ȳ)*ɂ;ȿ=(Ȳ-ȱ*Ƚ)*ɂ;}double Ƀ,Ʉ;Ɂ=Ⱦ*Ⱦ-4*ȼ;if(Ɂ<0)Ƀ=Ȉ;else{ɀ=Math.Sqrt(Ɂ);Ƀ=Ʌ(-Ⱦ+ɀ
,-Ⱦ-ɀ)*0.5;}Ɂ=ȿ*ȿ-4*Ƚ;if(Ɂ<0)Ʉ=Ȉ;else{ɀ=Math.Sqrt(Ɂ);Ʉ=Ʌ(-ȿ+ɀ,-ȿ-ɀ)*0.5;}return Ʌ(Ƀ,Ʉ);}private static bool Ⱥ(double Ȱ,
double ȱ,double Ƅ,out double[]ȹ){ȹ=new double[4];double Ɇ=Ȱ*Ȱ,ɇ=(Ɇ-3*ȱ)*Ȏ,Ɉ=(Ȱ*(2*Ɇ-9*ȱ)+27*Ƅ)*Ȑ,ƾ=Ɉ*Ɉ,ɉ=ɇ*ɇ*ɇ;if(ƾ<ɉ){double
Ɋ=Math.Sqrt(ɇ),ɋ=Ɉ/(Ɋ*Ɋ*Ɋ);if(ɋ<-1)ɋ=-1;else if(ɋ>1)ɋ=1;ɋ=Math.Acos(ɋ);Ȱ*=ȍ;ɇ=-2*Ɋ;double Ɍ=Math.Cos(ɋ*ȍ),ɍ=Math.Sin(ɋ*ȍ)
;ȹ[0]=ɇ*Ɍ-Ȱ;ȹ[1]=ɇ*((Ɍ*Ȋ)-(ɍ*Ȍ))-Ȱ;ȹ[2]=ɇ*((Ɍ*Ȋ)+(ɍ*Ȍ))-Ȱ;return true;}else{double Ɏ=-Math.Pow(Math.Abs(Ɉ)+Math.Sqrt(ƾ-ɉ)
,ȍ),ɏ;if(Ɉ<0)Ɏ=-Ɏ;ɏ=Ɏ==0?0:ɇ/Ɏ;Ȱ*=ȍ;ȹ[0]=Ɏ+ɏ-Ȱ;ȹ[1]=-0.5*(Ɏ+ɏ)-Ȱ;ȹ[2]=0.5*ȋ*(Ɏ-ɏ);if(Math.Abs(ȹ[2])<ȉ){ȹ[2]=ȹ[1];return
true;}return false;}}private static double Ʌ(double Ȱ,double ȱ){if(Ȱ<=0)return ȱ>0?ȱ:Ȉ;else if(ȱ<=0)return Ȱ;else return
Math.Min(Ȱ,ȱ);}}public enum K{ɑ,C,L,ɒ,ɓ,ɔ,T}public class E{Dictionary<K,string>ɖ=new Dictionary<K,string>(){{K.ɑ,
$"{ɕ(Color.Gray)}"},{K.C,$"{ɕ(Color.DarkSeaGreen)}"},{K.L,$"{ɕ(Color.White)}"},{K.ɒ,$"{ɕ(Color.Gold)}"},{K.ɓ,$"{ɕ(Color.Red)}"},{K.ɔ,
$"{ɕ(Color.DarkRed)}"},{K.T,$"{ɕ(Color.Aquamarine)}"}};private static string ɕ(Color z){return$"[color=#{z.A:X2}{z.R:X2}{z.G:X2}{z.B:X2}]";}
private static string ɗ="[/color]\n";class ɝ{internal readonly string ɘ;internal readonly double ə;internal readonly K ɚ;public
ɝ(string ɛ,double ɜ,K n){ɘ=ɛ;ə=ɜ;ɚ=n;}}private readonly List<ɝ>ɞ=new List<ɝ>();private readonly double ɟ;public E(double
ɠ){ɟ=ɠ;}public void J(string ß,K n){if(n<N.ȥ.K)return;double ɡ=(System.DateTime.UtcNow-new System.DateTime(1970,1,1)).
TotalSeconds;ɞ.Add(new ɝ(ß,ɡ,n));}public void g(){double ɡ=(System.DateTime.UtcNow-new System.DateTime(1970,1,1)).TotalSeconds;ɞ.
RemoveAll(ȳ=>ɡ-ȳ.ə>ɟ);}public List<string>ɢ(){return ɞ.Select(ȳ=>ȳ.ɘ).ToList();}public override string ToString(){string ź="";
foreach(var ɣ in ɞ){ź+=ɖ[ɣ.ɚ]+ɣ.ɘ+ɗ;}return ź;}}public class ɦ{public int ɤ;public Action ɥ;}public static class b{private
static readonly Dictionary<int,ɦ>ɧ=new Dictionary<int,ɦ>();private static readonly Dictionary<int,ɦ>ɨ=new Dictionary<int,ɦ>();
private static int ɩ=1;public static int ɬ(int a,Action ɪ=null){if(a<=0){if(ɪ!=null)ɪ();return-1;}int u=ɩ++;var ɫ=new ɦ{ɤ=a,ɥ=ɪ
};ɨ[u]=ɫ;return u;}public static int ɭ(float a,Action ɪ=null){return ɬ((int)(a*60),ɪ);}public static void ɮ(int u){ɧ.
Remove(u);ɨ.Remove(u);}public static void c(){if(ɨ.Count>0){foreach(var ɯ in ɨ){ɧ[ɯ.Key]=ɯ.Value;}ɨ.Clear();}var ɰ=new List<
int>();foreach(var ɯ in ɧ){var ɫ=ɯ.Value;ɫ.ɤ--;if(ɫ.ɤ<=0){ɫ.ɥ?.Invoke();ɰ.Add(ɯ.Key);}}foreach(var u in ɰ){ɧ.Remove(u);}}
public static bool ɱ(int u){return ɧ.ContainsKey(u)||ɨ.ContainsKey(u);}public static int ɲ(int u){ɦ ɫ;if(ɧ.TryGetValue(u,out ɫ
))return ɫ.ɤ;if(ɨ.TryGetValue(u,out ɫ))return ɫ.ɤ;return-1;}}public static class R{private static Dictionary<string,
Action>ɳ;public static void O(){ɳ=new Dictionary<string,Action>{{N.ɴ.ɵ,P.ɶ.ɷ},{N.ɴ.ɸ,P.ɶ.ɹ},{"FireAllTest",P.ɶ.ɺ.ɻ},{
"CancelAllTest",P.ɶ.ɺ.ɼ}};if(K.ɑ<=N.ȥ.K){foreach(var ɽ in ɳ){Program.M($"Command: {ɽ.Key}",K.ɑ);}}}public static bool i(string V,out
Action h){return ɳ.TryGetValue(V,out h);}}public static class N{private static ɾ ʀ=new ɾ("General Config",""){Ơ=ɿ};public
static readonly ʁ ȥ=new ʁ();public static readonly ʂ ɴ=new ʂ();public static readonly ʃ ʄ=new ʃ();public static readonly ʅ ʆ=
new ʅ();public static readonly ʇ ʈ=new ʇ();public static readonly ʉ ʊ=new ʉ();public static void O(IMyProgrammableBlock ʋ){
Program.M("Setting up config");ʌ.ʍ();ʎ.ʍ();ʋ.CustomData=ɾ.ɿ(ʋ.CustomData);Program.M("Written config to custom data",K.C);
Program.M("Commands set up",K.C);ʏ(ʋ);Program.M("Config setup done",K.L);}private static void ʏ(IMyProgrammableBlock ʋ){Program
.M("Setting up global state",K.C);ʐ.ʑ=ʈ.ʒ;Program.M($"Precision mode state is {ʐ.ʑ}",K.ɑ);}private static void ɿ(
Dictionary<string,object>j){ȥ.Ơ(j);ɴ.Ơ(j);ʄ.Ơ(j);ʆ.Ơ(j);ʈ.Ơ(j);ʊ.Ơ(j);}public class ʁ{public K K=K.ɑ;public double ʓ=2000;public
double Ȧ=104;public double ʔ=30;internal void Ơ(Dictionary<string,object>j){var ʕ=Ɲ.Ɵ(j,"General Config");ʕ.Ơ("LogLevel",ref K
);ʕ.Ơ("MaxWeaponRange",ref ʓ);ʕ.Ơ("GridSpeedLimit",ref Ȧ);ʕ.Ơ("MaxAngularVelocityRPM",ref ʔ);}}public class ʂ{public
string ɸ="Target";public string ɵ="Untarget";public string ʖ="ArgusV2";public string ʗ="TrackerGroup";internal void Ơ(
Dictionary<string,object>j){var ʕ=Ɲ.Ɵ(j,"String Config");ʕ.Ơ("ArgumentTarget",ref ɸ);ʕ.Ơ("ArgumentUnTarget",ref ɵ);ʕ.Ơ("GroupName"
,ref ʖ);ʕ.Ơ("TrackerGroupName",ref ʗ);}}public class ʃ{public double ʘ=3000;public float ʙ=40;public double ʚ=0.999999;
internal void Ơ(Dictionary<string,object>j){var ʕ=Ɲ.Ɵ(j,"Behavior Config");ʕ.Ơ("LockRange",ref ʘ);ʕ.Ơ("LockAngle",ref ʙ);ʕ.Ơ(
"MinFireDot",ref ʚ);}}public class ʅ{public string ʛ="CTC: Tracking";public string ʜ="CTC: Searching";public string ʝ="CTC: Standby"
;public int ʞ=3600;internal void Ơ(Dictionary<string,object>j){var ʕ=Ɲ.Ɵ(j,"Tracker Config");ʕ.Ơ("TrackingName",ref ʛ);ʕ.
Ơ("SearchingName",ref ʜ);ʕ.Ơ("StandbyName",ref ʝ);ʕ.Ơ("ScannedBlockMaxValidFrames",ref ʞ);}}public class ʇ{public int ʟ=
300;public double ʠ=9.81;public bool ʒ=false;public bool ʡ=false;public double ʢ=0.005;public int ʣ=300;public int ʤ=600;
public int ʥ=600;internal void Ơ(Dictionary<string,object>j){var ʕ=Ɲ.Ɵ(j,"Gravity Config");ʕ.Ơ("TimeoutFrames",ref ʟ);ʕ.Ơ(
"Acceleration",ref ʠ);ʕ.Ơ("DefaultPrecisionMode",ref ʒ);ʕ.Ơ("DisablePrecisionModeOnEnemyDetected",ref ʡ);ʕ.Ơ("Step",ref ʢ);ʕ.Ơ(
"MassBalanceFrequencyFrames",ref ʣ);ʕ.Ơ("BallBalanceFrequencyFrames",ref ʤ);ʕ.Ơ("AccelerationRecalcDelay",ref ʥ);}}public class ʉ{public double ʦ=
500;public double ʧ=0;public double ʨ=30;public double ʩ=-0.05;public double ʪ=0.05;internal void Ơ(Dictionary<string,
object>j){var ʕ=Ɲ.Ɵ(j,"PID Config");ʕ.Ơ("ProportionalGain",ref ʦ);ʕ.Ơ("IntegralGain",ref ʧ);ʕ.Ơ("DerivativeGain",ref ʨ);ʕ.Ơ(
"IntegralLowerLimit",ref ʩ);ʕ.Ơ("IntegralUpperLimit",ref ʪ);}}}public class ɾ{private static readonly Dictionary<string,ɾ>ʫ=new Dictionary<
string,ɾ>();public delegate void ʬ(Dictionary<string,object>ų);public ʬ Ơ{get;set;}public ɾ(string Ü,string Ũ){ʭ=Ü;ũ=Ũ;ʫ.Add(Ü
,this);}public string ʭ{get;}public string ũ{get;}public static string ɿ(string Ȃ){var ʮ=ƚ.ƃ(Ȃ);var ų=ʮ as Dictionary<
string,object>;if(ų==null){Program.M("Config malformed",K.ɔ);throw new Exception();}foreach(var Ŵ in ʫ){if(!ų.ContainsKey(Ŵ.
Key))ų[Ŵ.Key]=new Dictionary<string,object>();Ŵ.Value.Ơ?.Invoke((Dictionary<string,object>)ų[Ŵ.Key]);}return ƚ.Ŭ(ʮ);}}
public class ʎ{private static readonly ɾ N=new ɾ("Projectile Data",
"The main list of known projectiles. Gun Data should reference these by name."){Ơ=ɿ,};public static Dictionary<string,ʎ>ʯ=new Dictionary<string,ʎ>();public static readonly ʎ ʰ=new ʎ(0,0,0,0);static
ʎ(){ʯ.Add("Default",ʰ);ʯ.Add("LargeRailgun",new ʎ(2000,2000,2000,0));ʯ.Add("Artillery",new ʎ(500,500,2000,0));ʯ.Add(
"SmallRailgun",new ʎ(1000,1000,1400,0));ʯ.Add("Gatling",new ʎ(400,400,800,0));ʯ.Add("AssaultCannon",new ʎ(500,500,1400,0));ʯ.Add(
"Rocket",new ʎ(100,200,800,1000));}public static void ʍ(){Program.M("Projectile data loaded",K.C);}public static ʎ ʳ(string ʱ){ʎ
ʲ;return ʯ.TryGetValue(ʱ,out ʲ)?ʲ:ʰ;}public float ʴ{get;private set;}public float ʵ{get;private set;}public float ʶ{get;
private set;}public float ʠ{get;private set;}public ʎ(float ʷ,float ʸ,float ʹ,float ʺ){ʴ=ʷ;ʵ=ʸ;ʶ=ʹ;ʠ=ʺ;}private static void ɿ(
Dictionary<string,object>ƞ){var ʻ=new Dictionary<string,ʎ>(ʯ);foreach(var Ŵ in ʻ){var Ü=Ŵ.Key;var ʼ=Ŵ.Value;var ʽ=Ɲ.Ɵ(ƞ,Ü);var ʷ=ʼ
.ʴ;var ʸ=ʼ.ʵ;var ʹ=ʼ.ʶ;var ʺ=ʼ.ʠ;ʽ.Ơ("ProjectileVelocity",ref ʷ);ʽ.Ơ("MaxVelocity",ref ʸ);ʽ.Ơ("MaxRange",ref ʹ);ʽ.Ơ(
"Acceleration",ref ʺ);ʯ[Ü]=new ʎ(ʷ,ʸ,ʹ,ʺ);}foreach(var Ŵ in ƞ){string ʾ="";if(!ʯ.ContainsKey(Ŵ.Key)){var ʿ=Ŵ.Value as Dictionary<
string,object>;if(ʿ==null)continue;var ʷ=ƚ.ƙ(ʿ,"ProjectileVelocity",ref ʾ,0.0f);var ʸ=ƚ.ƙ(ʿ,"MaxVelocity",ref ʾ,0.0f);var ʹ=ƚ.
ƙ(ʿ,"MaxRange",ref ʾ,0.0f);var ʺ=ƚ.ƙ(ʿ,"Acceleration",ref ʾ,0.0f);ʯ[Ŵ.Key]=new ʎ(ʷ,ʸ,ʹ,ʺ);}}}}public class ʌ{private
static readonly ɾ N=new ɾ("Gun Data",
"The main list of known gun types and their definition names. Should reference a known projectile type."){Ơ=ɿ};public static Dictionary<string,ʌ>ʯ=new Dictionary<string,ʌ>();public static readonly ʌ ˀ=new ʌ("Default",0,0,0f,
0f);static ʌ(){ʯ.Add("Default",ˀ);ʯ.Add("LargeRailgun",new ʌ("LargeRailgun",ˁ.ˆ,ˇ.ˈ,2.0f,4.0f));ʯ.Add(
"LargeBlockLargeCalibreGun",new ʌ("Artillery",0,0,0,12));ʯ.Add("LargeMissileLauncher",new ʌ("Rocket",0,0,0,0.5f));ʯ.Add("SmallRailgun",new ʌ(
"SmallRailgun",ˁ.ˆ,ˇ.ˈ,0.5f,4.0f));ʯ.Add("SmallBlockAutocannon",new ʌ("Gatling",0,0,0.0f,0.4f));ʯ.Add("SmallBlockMediumCalibreGun",new
ʌ("AssaultCannon",0,0,0.0f,6f));ʯ.Add("MyObjectBuilder_SmallGatlingGun",new ʌ("Gatling",0,0,0.0f,0.1f));ʯ.Add(
"MyObjectBuilder_SmallMissileLauncher",new ʌ("Rocket",0,0,0.0f,1f));ʯ.Add("SmallRocketLauncherReload",ʳ("MyObjectBuilder_SmallMissileLauncher"));ʯ.Add(
"SmallGatlingGunWarfare2",ʳ("MyObjectBuilder_SmallGatlingGun"));ʯ.Add("SmallMissileLauncherWarfare2",ʳ("MyObjectBuilder_SmallMissileLauncher"));}
public static void ʍ(){Program.M("Gun data loaded",K.C);}public static ʌ ʳ(string ʱ){ʌ ʲ;return ʯ.TryGetValue(ʱ,out ʲ)?ʲ:ˀ;}
string ˉ;public ʎ ʎ{get;}public ˁ ˊ{get;}public ˇ ˋ{get;}public int ˌ{get;}public float ˍ=>ˌ/60.0f;public int ˎ{get;}public
float ˏ=>ˎ/60.0f;public ʌ(string ː,ˁ ˑ,ˇ ˠ,float ˡ,float ˢ){ˉ=ː;ʎ=ʎ.ʳ(ː);ˊ=ˑ;ˋ=ˠ;ˌ=(int)(ˡ*60);ˎ=(int)(ˢ*60);}private static
void ɿ(Dictionary<string,object>ƞ){var ʻ=new Dictionary<string,ʌ>(ʯ);foreach(var Ŵ in ʻ){var Ü=Ŵ.Key;var ʼ=Ŵ.Value;var ʽ=Ɲ.Ɵ
(ƞ,Ü);var ˣ=ʼ.ˉ;var ˑ=ʼ.ˊ;var ˠ=ʼ.ˋ;var ˡ=ʼ.ˍ;var ˢ=ʼ.ˏ;ʽ.Ơ("Projectile",ref ˣ);ʽ.Ơ("ReloadType",ref ˑ,
"0 = normal, 1 = charged");ʽ.Ơ("FireType",ref ˠ,"0 = normal, 1 = delay before firing");ʽ.Ơ("FireTime",ref ˡ);ʽ.Ơ("ReloadTime",ref ˢ);ʯ[Ü]=new ʌ(ˣ
,ˑ,ˠ,ˡ,ˢ);}foreach(var Ŵ in ƞ){if(!ʯ.ContainsKey(Ŵ.Key)){var ʿ=Ŵ.Value as Dictionary<string,object>;if(ʿ==null)continue;
var ˤ="";var ˣ=ƚ.ƙ(ʿ,"Projectile",ref ˤ,"");var ˑ=ƚ.ƙ(ʿ,"ReloadType",ref ˤ,0);var ˠ=ƚ.ƙ(ʿ,"FireType",ref ˤ,0);var ˡ=ƚ.ƙ(ʿ,
"FireTime",ref ˤ,0.0f);var ˢ=ƚ.ƙ(ʿ,"ReloadTime",ref ˤ,0.0f);ʯ[Ŵ.Key]=new ʌ(ˣ,(ˁ)ˑ,(ˇ)ˠ,ˡ,ˢ);}}}}public static class ʐ{public
static bool ʑ;}public abstract class ͳ{protected x ˬ;protected x ˮ;protected int Ͱ;public ͱ ͱ=ͱ.Ͳ;public ͳ(){Program.M(
"New ArgusShip",K.C);Ͱ=Program.D.Next()%6000;}public abstract x ʹ{get;}public abstract x Ͷ{get;}public abstract x ʠ{get;}public
abstract float ͷ{get;}public abstract string ʭ{get;}public abstract void d(int ͺ);public abstract void e(int ͺ);public x Γ(ͳ ͻ,
float ʷ){x ͼ=this.ʹ;x ͽ=this.Ͷ;x Ά=ͻ.ʹ;x Έ=ͻ.ʠ;x Ή=ͻ.Ͷ-ͽ;x Ί=Ά-ͼ;double Ź=ʷ;double Ȱ=Ή.ȣ()-Ź*Ź;double ȱ=2.0*Ί.Ό(Ή);double Ƅ=Ί
.ȣ();double ɋ;if(Math.Abs(Ȱ)<1e-6){if(Math.Abs(ȱ)<1e-6)ɋ=0;else ɋ=-Ƅ/ȱ;}else{double Ύ=ȱ*ȱ-4*Ȱ*Ƅ;if(Ύ<0)return Ά;double Ώ=
Math.Sqrt(Ύ);double ΐ=(-ȱ+Ώ)/(2*Ȱ);double Α=(-ȱ-Ώ)/(2*Ȱ);ɋ=Math.Min(ΐ,Α)>0?Math.Min(ΐ,Α):Math.Max(ΐ,Α);if(ɋ<0)ɋ=Math.Max(ΐ,Α
);}x Β=Ά+Ή*ɋ+0.5*Έ*ɋ*ɋ;return Β;}}public struct Π{public readonly x Δ;public readonly x Ε;public readonly x Ζ;public
readonly x Η;public readonly double Ό;public readonly double Θ;public MatrixD Ι;public Π(x Κ,x ȕ,x Λ,x Μ,double Ν,double Ξ,
MatrixD Ο){Δ=Κ;Ε=ȕ;Ζ=Λ;Η=Μ;Ό=Ν;Θ=Ξ;Ι=Ο;}}public class Ψ{Ρ Σ;Τ Υ;public Ψ(Ρ Φ,Τ Χ){Program.M($"Setting up FCS",K.L);Σ=Φ;Υ=Χ;}
public Π μ(){Υ.Ω();int Ϋ=Υ.Ϊ;ά ή=Υ.έ;var ΰ=Υ.ί;var β=Σ.α();var Ί=β-ΰ;var γ=Ί.Ȩ();var δ=Ί/γ;var Ν=δ.Ό(Σ.ε);var η=Υ.ζ();var ι=(η
-ΰ).θ();Program.f(ι);if(Ν>N.ʄ.ʚ&&β!=x.ȝ)Υ.κ();else Υ.λ();return new Π(ι,β,ΰ,Σ.ε,Ν,γ,Σ.Ι);}}public enum ˁ{ν,ˆ}public enum
ˇ{ν,ˈ}public enum τ{ξ,ο,π,ρ,ς,σ}public class ϔ{private static readonly MyDefinitionId υ=new MyDefinitionId(typeof(
MyObjectBuilder_GasProperties),"Electricity");IMyUserControllableGun φ;ˁ χ;ˇ ψ;MyResourceSinkComponent ω;int ϊ;int ϋ;ţ<τ>ό;bool ύ;ʌ ώ;Τ Ϗ;public ϔ(
IMyUserControllableGun ʲ,Τ ϐ){var ϑ=ʲ.BlockDefinition;Program.M($"Set up new gun {ϑ}",K.ɑ);var ϒ=ʌ.ʳ(ϑ.SubtypeIdAttribute);if(ϒ==ʌ.ˀ)ϒ=ʌ.ʳ(ϑ.
TypeIdString);ώ=ϒ;Ϗ=ϐ;φ=ʲ;ω=ʲ.Components.Get<MyResourceSinkComponent>();χ=ώ.ˊ;ψ=ώ.ˋ;ό=new ţ<τ>(ϓ);}public x ϖ=>(Vector3)(φ.Min+φ.Max
)/2*Ϗ.ϕ.ͷ;public x ϗ=>φ.GetPosition();public x ƣ{get;set;}public float Ͷ=>ώ.ʎ.ʴ;public float ʠ=>ώ.ʎ.ʠ;public float ʵ=>ώ.ʎ
.ʵ;public float ʶ=>ώ.ʎ.ʶ;public ʌ ʌ=>ώ;public τ Ϙ=>ό.Ť;public x ơ=>φ.WorldMatrix.Forward;public void d(int ͺ){ό.ť();}
public void e(int ͺ){}public bool ϛ(){if(Ϙ!=τ.π)return false;φ.ShootOnce();ϊ=b.ɬ(ώ.ˎ,ϙ);if(ψ==ˇ.ˈ){ϋ=b.ɬ(ώ.ˌ,Ϛ);}else{ϋ=b.ɬ(0,
Ϛ);}return true;}public bool ϝ(){if(Ϙ!=τ.ξ)return false;φ.Enabled=false;b.ɬ(0,Ϝ);ύ=true;return true;}public bool Ϟ(){if(Ϙ
!=τ.ξ)return false;if(b.ɲ(ϋ)>1)return false;φ.Enabled=false;b.ɬ(0,Ϝ);ύ=true;return true;}public x ϡ(x ϟ){x Ϡ=ϟ+ƣ*Ͷ;if(Ϡ.ȣ(
)>Ͷ*Ͷ)Ϡ=Ϡ.θ()*Ͷ;return Ϡ;}τ ϓ(){bool Ϣ=φ.IsFunctional;if(!Ϣ)return τ.σ;if(ψ==ˇ.ˈ&&b.ɱ(ϋ))return ύ?τ.ο:τ.ξ;switch(χ){case
ˁ.ν:if(b.ɱ(ϊ))return τ.ρ;break;case ˁ.ˆ:if(b.ɱ(ϊ))return τ.ρ;if(ω.CurrentInputByType(υ)>0.02f)return τ.ς;break;}return τ.
π;}void Ϛ(){if(ύ){ύ=false;return;}}void ϙ(){}void Ϝ(){φ.Enabled=true;}}public enum ά{ϣ,Ϥ,ξ}public enum ˋ{ϥ,Ϧ,ϧ}public
class Τ{List<ϔ>Υ=new List<ϔ>();List<ϔ>Ϩ=new List<ϔ>();x ϩ;ά Ϫ;int ϫ;ʌ ώ;public Τ(List<IMyTerminalBlock>Ϭ,Ρ ϭ){Program.M(
"Setting up gun manager",K.L);foreach(var Ϯ in Ϭ){var ʲ=Ϯ as IMyUserControllableGun;if(ʲ!=null)Υ.Add(new ϔ(ʲ,this));}if(Υ.Count<=0)Program.M(
$"No guns in group {N.ɴ.ʖ}",K.ɒ);ϕ=ϭ;}public Ρ ϕ{get;}public IMyCubeGrid ϰ=>ϕ.ϯ.CubeGrid;public int ϱ=>Υ.Count;public x ί=>ϩ;public ά έ=>Ϫ;public
int Ϊ=>ϫ;public void d(int ͺ){foreach(var ʲ in Υ)ʲ.d(ͺ);}public void Ω(){var ή=ά.ϣ;var ϲ=new Dictionary<ʌ,List<ϔ>>();var ϳ=
new Dictionary<ʌ,List<ϔ>>();var ϴ=0;var ϵ=0;foreach(var ʲ in Υ){switch(ʲ.Ϙ){case τ.π:if(!ϲ.ContainsKey(ʲ.ʌ))ϲ.Add(ʲ.ʌ,new
List<ϔ>());ϲ[ʲ.ʌ].Add(ʲ);ϴ++;break;case τ.ξ:if(!ϳ.ContainsKey(ʲ.ʌ))ϳ.Add(ʲ.ʌ,new List<ϔ>());ϳ[ʲ.ʌ].Add(ʲ);ϵ++;break;}}
Program.f(ϴ);var Ϸ=ϳ;if(ϴ>0){Ϫ=ά.Ϥ;Ϸ=ϲ;}if(ϵ>0){Ϫ=ά.ξ;Ϸ=ϳ;}var ͻ=ϕ.α();var ϸ=ϕ.ʹ;var Ϲ=(ͻ-ϸ).ȣ();foreach(var Ϻ in Ϸ){var ʿ=Ϻ.
Key;var Χ=Ϻ.Value;var ʹ=ʿ.ʎ.ʶ;if(ʹ*ʹ<Ϲ)continue;Ϩ=Χ;ώ=ʿ;break;}ϫ=Ϩ.Count;if(ϫ==0){ϩ=ϕ.ʹ;return;}x ϻ=x.ȝ;foreach(var ʲ in Ϩ)
{ϻ+=ʲ.ϖ;}ϻ/=ϫ;ϩ=x.ϼ(ϻ,ϕ.Ι);}public void e(int ͺ){foreach(var ʲ in Υ)ʲ.e(ͺ);}public void ɻ(){foreach(var ʲ in Υ)ʲ.ϛ();}
public void ɼ(){foreach(var ʲ in Υ)ʲ.ϝ();}public void κ(){foreach(var ʲ in Υ)ʲ.ϛ();}public void λ(){foreach(var ʲ in Υ)ʲ.Ϟ();}
public x ζ(){if(ώ==null)return x.ȝ;var ȑ=ώ.ʎ.ʵ;var ͻ=ϕ.α();var Ί=ͻ-ϩ;var Ͻ=Ϩ[0];if(Ͻ==null)return x.ȝ;var Ț=ϕ.Ͼ;var ț=Ț.ȣ()!=0
;return ɐ.ȯ(ȑ,Ͻ.ϡ(ϕ.Ͷ)/60,ώ.ʎ.ʠ*Ͻ.ơ,Ί,ϕ.Ͽ.ʹ,ϕ.Ͽ.Ͷ/60,ϕ.Ͽ.ʠ/60,x.ȝ,false,Ț,ț);}}public class Є{private readonly List<
IMyGyro>Ѐ;private readonly ȁ Ё;private readonly ȁ Ђ;public Є(List<IMyTerminalBlock>Ϭ){Program.M("Setting up gyro manager",K.L);
Ѐ=new List<IMyGyro>();foreach(var ȱ in Ϭ){var Ѓ=ȱ as IMyGyro;if(Ѓ!=null){Ѐ.Add(Ѓ);}}if(Ѐ.Count<=0)Program.M(
$"No gyroscopes found in group: {N.ɴ.ʖ}",K.ɒ);Ё=new ȁ(N.ʊ.ʦ,N.ʊ.ʧ,N.ʊ.ʨ,N.ʊ.ʪ,N.ʊ.ʩ);Ђ=new ȁ(N.ʊ.ʦ,N.ʊ.ʧ,N.ʊ.ʨ,N.ʊ.ʪ,N.ʊ.ʩ);}public void Г(ref Π Ѕ,double І=0){
int Ї=7;double Ј=1.0;if(Ѕ.Ό>0.9999){Ј*=0.8;Ї=4;}if(Ѕ.Ό>0.99999){Ј*=0.8;Ї=3;}if(Ѕ.Ό>0.999999){Ј*=0.8;Ї=2;}if(Ѕ.Ό>0.9999999){
Ј*=0.8;Ї=1;}double Љ;double Њ;var Ћ=І;var Ѝ=x.Ќ(Ѕ.Η,Ѕ.Δ);var Џ=x.Ў(Ѝ,MatrixD.Transpose(Ѕ.Ι));var А=Ё.Ȇ(-Џ.Ķ,Ї);var ȸ=Ђ.Ȇ(
-Џ.ķ,Ї);Љ=MathHelper.Clamp(А,-N.ȥ.ʔ,N.ȥ.ʔ);Њ=MathHelper.Clamp(ȸ,-N.ȥ.ʔ,N.ȥ.ʔ);if(Math.Abs(Њ)+Math.Abs(Љ)>N.ȥ.ʔ){var Б=N.ȥ
.ʔ/(Math.Abs(Њ)+Math.Abs(Љ));Њ*=Б;Љ*=Б;}Љ*=Ј;Њ*=Ј;В(Љ,Њ,Ћ,Ѕ.Ι);}void В(double Д,double Е,double Ж,MatrixD Ο){var З=new x(
Д,Е,Ж);var И=x.Ў(З,Ο);foreach(var Ѓ in Ѐ)if(Ѓ.IsFunctional&&Ѓ.IsWorking&&Ѓ.Enabled&&!Ѓ.Closed){var Й=x.Ў(И,MatrixD.
Transpose(Ѓ.WorldMatrix));Ѓ.Pitch=(float)Й.Ķ;Ѓ.Yaw=(float)Й.ķ;Ѓ.Roll=(float)Й.ĸ;Ѓ.GyroOverride=true;return;}}public void К(){
foreach(var Ѓ in Ѐ)if(Ѓ.IsFunctional&&Ѓ.IsWorking&&Ѓ.Enabled&&!Ѓ.Closed){Ѓ.GyroOverride=false;return;}}}public enum ͱ{Л,М,Н,Ͳ}
public class Р{public static int П(ͱ О){switch(О){case ͱ.Л:return 600;case ͱ.М:return 60;case ͱ.Н:return 10;case ͱ.Ͳ:return 1;
default:return Int32.MaxValue;}}}public class Я{private readonly List<С>Т;private readonly List<У>Ф;private readonly List<Х>Ц;
bool Ч;int Ш=0;x Щ=x.ȝ;Ρ Σ;public Я(List<IMyTerminalBlock>Ϭ,Ρ Φ){Ц=new List<Х>();Т=new List<С>();Ф=new List<У>();Σ=Φ;foreach
(var Ϯ in Ϭ){var Ъ=Ϯ as IMySpaceBall;if(Ъ!=null){var Ы=new У(Ъ,this,Σ);Ф.Add(Ы);Ц.Add(Ы);Щ+=Ы.Ь;continue;}var Э=Ϯ as
IMyArtificialMassBlock;if(Э!=null){var Ы=new С(Э,this,Σ);Т.Add(Ы);Ц.Add(Ы);Щ+=Ы.Ь;}}Ш=Program.D.Next()%(Math.Max(N.ʈ.ʣ,N.ʈ.ʤ)-1);Ю();}public
bool а{get;set;}public double б{get;private set;}public List<Х>в=>Ц;public void d(int ͺ){if((ͺ+Ш)%N.ʈ.ʣ==0)г();if((ͺ+Ш)%N.ʈ.
ʤ==0)д();}public void e(int ͺ){bool е=false;foreach(var Ϯ in Т){е|=Ϯ.ж();}foreach(var Ъ in Ф){е|=Ъ.ж();}if(е){Ю();}}
public bool и(){var з=Ч;Ч=false;return з;}void Ю(){б=0;foreach(var Ϯ in Т){б+=Ϯ.й;}foreach(var Ъ in Ф){б+=Ъ.й;}}void г(){x к=Щ
;foreach(var Ϯ in Т){x м=Ϯ.л?x.ȝ:Ϯ.Ь;if((Щ-(Ϯ.л?Ϯ.Ь:x.ȝ)+м).ȣ()<Щ.ȣ()){Ϯ.л=!Ϯ.л;Щ=Щ-(Ϯ.л?x.ȝ:Ϯ.Ь)+м;}}Ч|=к!=Щ;}void д(){x
к=Щ;Ч|=к!=Щ;}}internal class ы{Я н;private readonly List<о>п;bool р;float с;float т;int у;ţ<double>ф;ţ<double>х;public ы(
List<о>ц,ƣ ч,Я ш){п=ц;ƣ=ч;ф=new ţ<double>(()=>щ(ш.б));х=new ţ<double>(()=>ъ(ш.в));н=ш;}public ƣ ƣ{get;private set;}public
bool а{get;private set;}public double ь=>ф.Ť;public double э=>х.Ť;public double ю=>ь+э;public void d(int ͺ){if(с==0)у++;else
у=0;if(у>N.ʈ.ʟ)а=false;п.RemoveAll(Ɏ=>Ɏ.я);if(ͺ%N.ʈ.ʥ==0&&н.и()){ф.ť();х.ť();}}public void e(int ͺ){if(р!=а)foreach(var ѐ
in п)ѐ.а=а;р=а;if(т!=с)foreach(var ѐ in п)ѐ.ʠ=(float)(с*N.ʈ.ʠ);т=с;}public void ё(float ʺ){ʺ=(float)MathHelperD.Clamp(ƶ.Ƶ(
ʺ,N.ʈ.ʢ),-1,1);if(ʺ==с&&ʺ==0)return;а=true;if(ʺ==с)return;с=ʺ;}double ъ(List<Х>ђ){var ѓ=x.ȝ;foreach(var ѐ in п){if(ѐ is є
){foreach(var Э in ђ){var δ=((x)(Э.ϖ-ѐ.ϖ)).θ();var і=δ*Э.й*N.ʈ.ʠ*ѐ.ѕ;ѓ+=і;}}}return ѓ.Ό(Base6Directions.Directions[(int)ƣ
]);}double щ(double Э){double ʺ=0;foreach(var ѐ in п){var ј=ѐ as ї;if(ј!=null)ʺ+=N.ʈ.ʠ;}return ʺ*Э;}}public class ѧ{
private readonly ы љ;private readonly ы њ;private readonly ы ћ;Я н;Ρ Σ;bool ќ;public ѧ(List<IMyTerminalBlock>Ϭ,Ρ Φ){Program.M(
$"Setting up gravity drive",K.L);Σ=Φ;var ѝ=new List<о>();var ў=new List<о>();var џ=new List<о>();var Ѡ=new Dictionary<ƣ,List<о>>{{ƣ.Ė,џ},{ƣ.Ę,џ},{ƣ
.ĕ,ў},{ƣ.ė,ў},{ƣ.ơ,ѝ},{ƣ.Ƣ,ѝ}};foreach(var Ϯ in Ϭ){var ѡ=Ϯ as IMyGravityGenerator;if(ѡ!=null){var δ=(ƣ)ѡ.Orientation.Up;
var ŵ=Ѡ[δ];bool Ѣ=(int)δ%2==0;ŵ.Add(new ї(ѡ,δ,Ѣ));}var ѣ=Ϯ as IMyGravityGeneratorSphere;if(ѣ!=null){var ѥ=Σ.Ѥ;var Ѧ=
Base6Directions.Directions[(int)ѥ];var Ѣ=Ѧ.Dot((Vector3D)Σ.ʹ-ѣ.GetPosition())>0;var ŵ=Ѡ[ѥ];ŵ.Add(new є(ѣ,ѥ,Ѣ));}}if(ѝ.Count==0)Program.
M($"No Forward/backward gravity generators",K.ɒ);if(ў.Count==0)Program.M($"No Left/Right gravity generators",K.ɒ);if(џ.
Count==0)Program.M($"No Up/Down gravity generators",K.ɒ);н=new Я(Ϭ,Φ);љ=new ы(ѝ,ƣ.ơ,н);њ=new ы(ў,ƣ.ĕ,н);ћ=new ы(џ,ƣ.Ė,н);}
bool Ѩ=>љ.а||њ.а||ћ.а;public double б=>н.б;public void d(int ͺ){љ.d(ͺ);њ.d(ͺ);ћ.d(ͺ);н.d(ͺ);}public void e(int ͺ){љ.e(ͺ);њ.e
(ͺ);ћ.e(ͺ);if(Ѩ!=ќ)н.а=Ѩ;Program.f(н.а);ќ=Ѩ;н.e(ͺ);}public void Ѫ(x ѩ){љ.ё((float)ѩ.Ό(x.ơ));њ.ё((float)ѩ.Ό(x.ĕ));ћ.ё((
float)ѩ.Ό(-x.Ė));}public double ѫ(){var і=љ.ю;if(і==0)і=1;return і;}public double Ѭ(){var і=њ.ю;if(і==0)і=1;return і;}public
double ѭ(){var і=ћ.ю;if(і==0)і=1;return і;}}public class У:Х{IMySpaceBall Ѯ;Я н;Ρ Σ;public У(IMySpaceBall Ъ,Я ш,Ρ Φ){Ѯ=Ъ;н=ш;Σ
=Σ;}public bool л{get;set;}=true;public bool ѯ=>н.а;public bool ɱ=>л&&ѯ;public override x ʹ=>Ѯ.GetPosition();public
override double Ѱ=>Ѯ.VirtualMass;public override double й=>л?Ѯ.VirtualMass:0;public override Vector3I ϖ=>Ѯ.Position;public x Ь=>
Ѱ*(Ѯ.GetPosition()-Σ.ϯ.CenterOfMass);public bool ж(){var Ɉ=false;Ѯ.Enabled=ɱ;return Ɉ;}}public class С:Х{
IMyArtificialMassBlock ѱ;Я н;Ρ Σ;bool Ѳ;public С(IMyArtificialMassBlock Э,Я ш,Ρ Φ){ѱ=Э;н=ш;Σ=Φ;ѱ.Enabled=false;}public bool л{get;set;}=true;
public bool ѯ=>н.а;public bool ɱ=>л&&ѯ;public override x ʹ=>ѱ.GetPosition();public override double Ѱ=>ѱ.VirtualMass;public
override double й=>л?ѱ.VirtualMass:0;public override Vector3I ϖ=>ѱ.Position;public x Ь=>Ѱ*(ѱ.GetPosition()-Σ.ϯ.CenterOfMass);
public bool ж(){var е=false;if(Ѳ!=ɱ){ѱ.Enabled=ɱ;е=true;}Ѳ=ɱ;return е;}}public abstract class о{protected bool ѳ;public
IMyGravityGeneratorBase Ѵ{get;protected set;}public ƣ ƣ{get;protected set;}public bool а{get{return Ѵ.Enabled;}set{Ѵ.Enabled=value;}}public
float ʠ{get{return Ѵ.GravityAcceleration*ѕ;}set{Ѵ.GravityAcceleration=value*ѕ;}}public x ʹ=>Ѵ.GetPosition();public bool я=>Ѵ.
Closed;public int ѕ=>ѳ?-1:1;public Vector3I ϖ=>Ѵ.Position;}public class ї:о{public ї(IMyGravityGenerator ѵ,ƣ δ,bool Ѣ){Ѵ=ѵ;ƣ=δ
;ѳ=Ѣ;}}public class є:о{public є(IMyGravityGeneratorSphere ѣ,ƣ δ,bool Ѣ){Ѵ=ѣ;ƣ=δ;ѳ=Ѣ;}}public abstract class Х{public
abstract x ʹ{get;}public abstract double Ѱ{get;}public abstract double й{get;}public abstract Vector3I ϖ{get;}}public class ѹ{ѧ
Ѷ;ѷ Ѹ;Ρ Σ;public ѹ(List<IMyTerminalBlock>Ϭ,Ρ Φ){Program.M($"Setting up propulsion controller",K.L);Σ=Φ;Ѷ=new ѧ(Ϭ,Φ);Ѹ=new
ѷ();}public void d(int ͺ){Ѷ.d(ͺ);Ѹ.d(ͺ);}public void e(int ͺ){var Ѻ=Σ.ϯ.MoveIndicator;Matrix Ø;Σ.ϯ.Orientation.GetMatrix(
out Ø);x ѻ=x.ϼ(Ѻ,Ø);if(Σ.ϯ.DampenersOverride){var Ѽ=Σ.Ͷ;var ѽ=x.Ў(Ѽ,MatrixD.Invert(Σ.Ι));var ѿ=ѽ*x.ơ*10*Ѿ();var ҁ=ѽ*x.ĕ*10*
Ҁ();var ҋ=ѽ*x.Ė*10*Ҋ();if(ѻ.Ό(x.ơ)==0)ѻ+=ѿ;if(ѻ.Ό(x.ĕ)==0)ѻ+=ҁ;if(ѻ.Ό(x.Ė)==0)ѻ+=ҋ;}Ѷ.Ѫ(ѻ);Ѷ.e(ͺ);Ѹ.e(ͺ);}double Ѿ(){
return Σ.Х.TotalMass/Ѷ.ѫ();}double Ҁ(){return Σ.Х.TotalMass/Ѷ.Ѭ();}double Ҋ(){return Σ.Х.TotalMass/Ѷ.ѭ();}}public class ѷ{
public void d(int ͺ){}public void e(int ͺ){}}public class ҍ{public ҍ(IMyTurretControlBlock Ϯ){Ҍ=Ϯ;}public
IMyTurretControlBlock Ҍ{get;}public bool я=>Ҍ.Closed;bool Ҏ;bool ҏ=true;public bool Ґ{get;private set;}public bool ґ{get;private set;}public
bool Ғ{get;private set;}public bool ғ{get;private set;}public bool а{get{return Ҍ.Enabled;}set{Ҍ.Enabled=value;}}public
string Ҕ{get{return Ҍ.CustomName;}set{Ҍ.CustomName=value;}}public long ҕ{get;set;}public Ƥ Җ{get;set;}public x ʹ=>Ҍ.
GetPosition();public void ж(){var җ=Ҍ.HasTarget;ґ=!җ&&Ҏ;Ҏ=җ;Ґ=җ;var ҙ=Җ!=null&&Җ.Ҙ;ғ=ҙ&&ҏ;ҏ=!ҙ;Ғ=ҙ;if(!җ)ҕ=0;}public Қ Ҝ(){var қ=Ҍ.
GetTargetedEntity();ҕ=қ.EntityId;return қ;}}public class Ρ:ҝ{public readonly Τ ɺ;private readonly Є Ҟ;private readonly Ψ ҟ;private
readonly ѹ Ҡ;List<IMyLargeTurretBase>ҡ;ţ<x>Ң;ţ<MyShipMass>ѱ;public Ρ(IMyCubeGrid ң,List<IMyTerminalBlock>Ϭ,List<IMyTerminalBlock
>Ҥ):base(ң,Ҥ){Program.M("New ControllableShip : SupportingShip : ArgusShip",K.C);foreach(var Ϯ in Ϭ){var ҥ=Ϯ as
IMyShipController;if(ҥ!=null)ϯ=ҥ;}if(ϯ==null){Program.M($"WARNING: Controller not present in group: {N.ɴ.ʖ}");return;}Ҟ=new Є(Ϭ);ɺ=new Τ(
Ϭ,this);ҟ=new Ψ(this,ɺ);Ң=new ţ<x>(()=>ϯ.GetNaturalGravity());ѱ=new ţ<MyShipMass>(()=>ϯ.CalculateShipMass());Ҡ=new ѹ(Ϭ,
this);}public IMyShipController ϯ{get;private set;}public MatrixD Ι=>Ҧ.WorldMatrix;public x ε=>ϯ.WorldMatrix.Forward;public
x ҧ=>ϯ.WorldMatrix.Up;public Vector3 Ҩ=>Base6Directions.Directions[(int)ϯ.Orientation.Forward];public ƣ Ѥ=>(ƣ)ϯ.
Orientation.Forward;public Ƥ Ͽ{get;private set;}public bool Ґ=>Ͽ!=null;public x Ͼ=>Ң.Ť;public MyShipMass Х=>ѱ.Ť;public override
void d(int ͺ){base.d(ͺ);ɺ.d(ͺ);Ҡ.d(ͺ);if((ͺ+Ͱ)%Р.П(ͱ)==0){Ң.ť();ѱ.ť();}}public override void e(int ͺ){base.e(ͺ);if(Ґ){var Ѕ=
ҟ.μ();if(Ѕ.Ε==x.ȝ)Ҟ.К();else Ҟ.Г(ref Ѕ);ɺ.e(ͺ);}Ҡ.e(ͺ);}public void ɷ(){Ͽ=P.ҩ(this,N.ʄ.ʘ,N.ʄ.ʙ);if(Ͽ==null){Ҟ.К();Program
.M("Couldn't find new target",K.ɒ);}else{Program.M("Got new target",K.L);}}public void ɹ(){Ͽ=null;Ҟ.К();}public x α(){
return Ͽ?.ʹ??x.ȝ;}}public class ҝ:ͳ{private readonly List<ҍ>Ҫ;protected readonly IMyCubeGrid Ҧ;public ҝ(IMyCubeGrid ң,List<
IMyTerminalBlock>Ҥ){Program.M("New SupportingShip : ArgusShip",K.C);IMyUserControllableGun ʲ=null;IMyMotorStator ҫ=null;Ҫ=new List<ҍ>();
foreach(var Ϯ in Ҥ){var ҥ=Ϯ as IMyTurretControlBlock;if(ҥ!=null){Ҫ.Add(new ҍ(ҥ));continue;}ʲ=ʲ??Ϯ as IMyUserControllableGun;ҫ=ҫ
??Ϯ as IMyMotorStator;}if(ʲ!=null&&ҫ!=null){Program.M("Setting up trackers",K.C);foreach(var Ҭ in Ҫ){Program.M(
$"Set up tracker: {Ҭ.Ҕ}");var Ϯ=Ҭ.Ҍ;Ϯ.ClearTools();Ϯ.AddTool(ʲ);Ϯ.AzimuthRotor=null;Ϯ.ElevationRotor=ҫ;Ϯ.AIEnabled=true;Ϯ.CustomName=N.ʆ.ʜ;}if(Ҫ
.Count<=0)Program.M("No target trackers in group",K.ɒ);}else Program.M(
$"Gun/rotor not present in group: {N.ɴ.ʗ}, cannot setup trackers",K.ɒ);Ҧ=ң;}public override x ʹ=>Ҧ.GetPosition();public override x Ͷ=>ˮ;public override x ʠ=>(ˮ-ˬ)*60;public override
float ͷ=>Ҧ.GridSize;public override string ʭ=>Ҧ.CustomName;public override string ToString()=>ʭ;public override void d(int ͺ)
{ˬ=ˮ;ˮ=Ҧ.LinearVelocity;}public override void e(int ͺ){for(int Ə=Ҫ.Count-1;Ə>=0;Ə--){var Ҭ=Ҫ[Ə];if(Ҭ.я){Ҫ.RemoveAt(Ə);
continue;}Ҭ.ж();if(!Ҭ.Ґ||Ҭ.Ғ){if(Ҭ.ґ&&!Ҭ.а){Ҭ.а=true;Ҭ.Ҕ=N.ʆ.ʜ;if(Ҭ.Җ!=null){Ҭ.Җ.ҭ=true;Ҭ.Җ.ʆ=null;}Ҭ.Җ=null;}else if(Ҭ.ғ){Ҭ.а=
true;Ҭ.Ҕ=N.ʆ.ʜ;}continue;}if(Ҭ.ҕ!=0)continue;Қ ͻ=Ҭ.Ҝ();ҍ Ү;if(P.ү(ͻ.Ұ,out Ү)){if(Ү==Ҭ&&Ҭ.а)Ҭ.а=false;Ү.Җ.ұ(ͻ,null,Ҭ);
continue;}var ҳ=P.Ҳ(Ҭ,ͻ.Ұ,ͻ);Ҭ.Җ=ҳ;Ҭ.Ҕ=N.ʆ.ʛ;Ҭ.а=false;Ҭ.ҕ=ͻ.Ұ;}}}enum ҹ{Ҵ,ҵ,Ҷ,ҷ,Ҹ}class Ҿ{int Һ;private readonly int һ;public ҹ
Ҽ;public Ҿ(int ɫ,ҹ ҽ){Һ=ɫ;һ=ɫ;Ҽ=ҽ;}public void ҿ(){Һ=һ;}public bool Ӏ(){Һ--;return Һ<=0;}}public class Ƥ:ͳ{Dictionary<
Vector3I,Ҿ>Ӂ=new Dictionary<Vector3I,Ҿ>();Dictionary<Vector3I,Ҿ>ӂ=new Dictionary<Vector3I,Ҿ>();public ҝ Ӄ;public Қ L;public long
Ұ;bool ӄ=true;BoundingBoxD Ӆ;x ӆ;x Ӈ;private readonly float ӈ=1f;public Ƥ(ҍ Ҭ,long Ӊ,Қ è){ʆ=Ҭ;Ұ=Ӊ;ͱ=ͱ.М;switch(è.Ҽ){case
MyDetectedEntityType.SmallGrid:ӈ=0.5f;break;case MyDetectedEntityType.LargeGrid:ӈ=2.5f;break;}L=è;ӊ=L.Ӌ;ӊ.Translation=L.ʹ;}public override x
ʹ=>L.ʹ;public override x Ͷ=>ˮ;public override x ʠ=>(ˮ-ˬ)*60;public override float ͷ=>ӈ;public override string ʭ=>
$"Trackable ship {Ұ}";public bool ҭ{get;set;}=false;public override string ToString()=>ʭ;public ҍ ʆ{get;set;}public bool Ҙ{get;set;}public
BoundingBoxD Ӎ=>L.ӌ;public x ӏ=>ӎ.Extents;public x Ӑ=>ӎ.HalfExtents;public int ӑ{get;set;}=0;public BoundingBoxD ӎ{get{if(ӄ)Ӓ();
return Ӆ;}}public x ӓ{get{if(ӄ)Ӓ();return ӆ;}}x Ӕ;MatrixD ӊ;public x ӕ(){var Ƙ=Ӕ;Ӕ=x.ȝ;return Ƙ;}void Ӓ(){Ӆ=ǲ.Ǳ(Ӎ,L.Ӌ,ӈ);ӄ=
false;var Ӗ=Ӑ;var ɏ=ӈ/2;ӆ=new x(ɏ-Ӗ.Ķ%ӈ,ɏ-Ӗ.ķ%ӈ,ɏ-Ӗ.ĸ%ӈ);}public override void d(int ͺ){if(ҭ)return;if((ͺ+Ͱ)%Р.П(ͱ)!=0)return
;L=ʆ.Ҝ();if(ʆ.я||L.Ұ!=Ұ)ҭ=true;ˬ=ˮ;ˮ=(Vector3D)L.Ͷ;Ӕ=ʹ-Ӈ;if(ͱ==ͱ.Ͳ&&(Ӕ*60-ˮ).ȣ()>10000){ӄ=true;Vector3I Ί=(Vector3I)(x.ϼ(
Ӕ-(ˮ/60),MatrixD.Invert(ӊ))*2);ӗ(Ί);}ӊ=L.Ӌ;ӊ.Translation+=L.ʹ;Ӈ=ʹ;}public override void e(int ͺ){ӂ.Clear();foreach(var Ŵ
in Ӂ){var Ә=x.ϼ((x)Ŵ.Key*ӈ+ӓ,ӊ);if(Ŵ.Value.Ӏ())continue;ӂ[Ŵ.Key]=Ŵ.Value;Program.C.Ã(Ә,Color.White,0.2f,0.016f,true);}var
Ƙ=Ӂ;Ӂ=ӂ;ӂ=Ƙ;var ә=new MyOrientedBoundingBoxD(ӎ,L.Ӌ);ә.Center=Ӎ.Center;}public void ұ(Қ қ,IMyLargeTurretBase Ӛ=null,ҍ ҥ=
null){if(қ.Ұ!=L.Ұ||қ.ӛ==null)return;var Ӝ=(x)қ.ӛ;var ӝ=Ӝ-ʹ;var Ӟ=x.Ў(ӝ,MatrixD.Transpose(ӊ));Ӟ-=ӓ;var ӟ=new Vector3I((int)
Math.Round(Ӟ.Ķ/ӈ),(int)Math.Round(Ӟ.ķ/ӈ),(int)Math.Round(Ӟ.ĸ/ӈ));var ҽ=ҹ.Ҵ;var Ӡ=Ӛ!=null?Ӛ.GetTargetingGroup():ҥ!=null?ҥ.Ҍ.
GetTargetingGroup():"";switch(Ӡ){case"Weapons":ҽ=ҹ.ҵ;break;case"Propulsion":ҽ=ҹ.Ҷ;break;case"Power Systems":ҽ=ҹ.ҷ;break;}if(Ӂ.ContainsKey
(ӟ)){var ʼ=Ӂ[ӟ];if(ҽ!=ʼ.Ҽ)ʼ.Ҽ=ҹ.Ҹ;ʼ.ҿ();return;}else{Ӂ.Add(ӟ,new Ҿ(N.ʆ.ʞ,ҽ));}}void ӗ(Vector3I Ί){var ӡ=new Dictionary<
Vector3I,Ҿ>();foreach(var Ϯ in Ӂ){ӡ.Add(Ϯ.Key+Ί,Ϯ.Value);}Ӂ=ӡ;}}public static class P{public static readonly List<ͳ>Ӣ=new List<ͳ
>();private static List<Ƥ>ӣ=new List<Ƥ>();public static Dictionary<long,Ƥ>Ӥ=new Dictionary<long,Ƥ>();private static
IEnumerator<Ƥ>ӥ;private static MyDynamicAABBTreeD Ӧ=new MyDynamicAABBTreeD();static P(){}public static Ρ ɶ{get;set;}public static
void d(int ͺ){ӧ();for(var Ө=Ӣ.Count-1;Ө>=0;Ө--){var Φ=Ӣ[Ө];Φ.d(ͺ);}foreach(var Φ in Ӣ){var ө=Φ as Ƥ;if(ө==null)continue;var
Ʊ=ө.Ӎ;if(ө.ӑ!=0){var Ί=ө.ӕ();Ӧ.MoveProxy(ө.ӑ,ref Ʊ,Ί);}else{ө.ӑ=Ӧ.AddProxy(ref Ʊ,ө,0U);}}foreach(var ɯ in Ƴ.ư(Ӧ)){var Ӫ=ɯ
.Key;var ӫ=ɯ.Value;var Ӭ=Ӫ.Ӎ.Size.LengthSquared();foreach(var ӭ in ӫ){var Ӯ=ӭ.Ӎ.Size.LengthSquared();if(Ӯ>Ӭ){Ӫ.Ҙ=true;
return;}}Ӫ.Ҙ=false;}}private static void ӧ(){if(!ӥ.MoveNext()){ӥ=ӯ().GetEnumerator();ӥ.MoveNext();}}public static void e(int ͺ
){for(var Ө=Ӣ.Count-1;Ө>=0;Ө--){var Φ=Ӣ[Ө];Φ.e(ͺ);}}private static IEnumerable<Ƥ>ӯ(){var Ӱ=N.ȥ.ʓ*N.ȥ.ʓ;for(var Ө=Ӣ.Count-
1;Ө>=0;Ө--){if(Ө>=Ӣ.Count)continue;var Φ=Ӣ[Ө];var ҳ=Φ as Ƥ;if(ҳ==null)continue;var ΰ=ҳ.ʹ;var ӱ=ɶ.ʹ;var Ӳ=(ΰ-ӱ).ȣ();if(Ӳ>Ӱ
)ҳ.ͱ=ͱ.М;else ҳ.ͱ=ͱ.Ͳ;yield return ҳ;}}public static List<Ƥ>ӵ(ҝ Φ,double Ξ){ӣ.Clear();foreach(var ӳ in Ӣ){var Ӵ=ӳ as Ƥ;if
(Ӵ==null)continue;if((Ӵ.ʹ-Φ.ʹ).ȣ()<Ξ*Ξ)ӣ.Add(Ӵ);}return ӣ;}public static Ƥ ҩ(Ρ Φ,double Ξ,float Ӷ){var ӷ=ӵ(Φ,Ξ);if(ӷ.
Count<1)return null;var ѥ=Φ.ε;double Ӹ=Math.Cos(Ӷ*Math.PI/180.0);double ӹ=double.MaxValue;Ƥ Ӻ=null;foreach(var ӻ in ӷ){var Ӽ=
(ӻ.ʹ-Φ.ʹ);var Ù=Ӽ.Ȩ();var Ν=(Ӽ/Ù).Ό(ѥ);Ν=MathHelperD.Clamp(Ν,-1.0,1.0);if(Ν<Ӹ)continue;var ӽ=(1-Ν)*Ù;if(ӽ<ӹ){Ӻ=ӻ;ӹ=ӽ;}}
return Ӻ;}public static void Q(IMyCubeGrid ң,IMyGridTerminalSystem Ӿ){var Ϻ=Ӿ.GetBlockGroupWithName(N.ɴ.ʖ);Program.M(
$"Getting group : {N.ɴ.ʖ}",K.ɑ);var ӿ=Ӿ.GetBlockGroupWithName(N.ɴ.ʗ);Program.M($"Getting group : {N.ɴ.ʗ}",K.ɑ);var Ϭ=new List<IMyTerminalBlock>();
var Ҥ=new List<IMyTerminalBlock>();if(Ϻ!=null){Ϻ.GetBlocks(Ϭ);Program.M($"Got group: {N.ɴ.ʖ}",K.C);}else Program.M(
$"Group not present: {N.ɴ.ʖ}",K.ɒ);if(ӿ!=null){ӿ.GetBlocks(Ҥ);Program.M($"Got group: {N.ɴ.ʗ}",K.C);}else Program.M($"Group not present: {N.ɴ.ʗ}",K.ɒ)
;var Φ=new Ρ(ң,Ϭ,Ҥ);Ӣ.Add(Φ);ɶ=Φ;ӥ=ӯ().GetEnumerator();}public static Ƥ Ҳ(ҍ Ҭ,long Ӊ,Қ è){Ƥ ҳ;if(Ӥ.TryGetValue(Ӊ,out ҳ)){
Program.M("Restoring defunct ship"+Ӊ,K.C);if(!ҳ.ҭ)return null;ҳ.ʆ=Ҭ;ҳ.ҭ=false;return ҳ;}Program.M("Creating new ship "+Ӊ,K.C);ҳ
=new Ƥ(Ҭ,Ӊ,è);Ӣ.Add(ҳ);Ӥ.Add(Ӊ,ҳ);return ҳ;}public static void Ԁ(Ƥ ҳ){Ӣ.Remove(ҳ);Ӥ.Remove(ҳ.Ұ);Ӧ.RemoveProxy(ҳ.ӑ);}
public static bool ү(long ԁ,out ҍ Ү){Ƥ ҳ;var Ԃ=Ӥ.TryGetValue(ԁ,out ҳ);Ү=Ԃ?ҳ.ʆ:null;return Ԃ&&!ҳ.ҭ;}}public struct Қ{
MyDetectedEntityInfo ԃ;Қ(MyDetectedEntityInfo қ){ԃ=қ;}public static implicit operator Қ(MyDetectedEntityInfo қ)=>new Қ(қ);public long Ұ=>ԃ.
EntityId;public MyDetectedEntityType Ҽ=>ԃ.Type;public Vector3D?ӛ=>ԃ.HitPosition;public MatrixD Ӌ=>ԃ.Orientation;public Vector3 Ͷ
=>ԃ.Velocity;public BoundingBoxD ӌ=>ԃ.BoundingBox;public Vector3D ʹ=>ӌ.Center;}public struct x{Vector3D ş;public x(double
А,double ȸ,double Ԅ){ş=new Vector3D(А,ȸ,Ԅ);}x(Vector3D ź){ş=ź;}public static implicit operator x(Vector3D ź)=>new x(ź);
public static implicit operator x(Vector3 ź)=>(Vector3D)ź;public static implicit operator Vector3D(x ź)=>ź.ş;public static
explicit operator Vector3I(x ź)=>(Vector3I)ź.ş;public static explicit operator x(Vector3I ź)=>(Vector3D)ź;public static x
operator-(x ź)=>-ź.ş;public static bool operator==(x ԅ,x Ԇ)=>ԅ.ş==Ԇ.ş;public static bool operator!=(x ԅ,x Ԇ)=>ԅ.ş!=Ԇ.ş;public
static x operator+(x ԅ,x Ԇ)=>ԅ.ş+Ԇ.ş;public static x operator-(x ԅ,x Ԇ)=>ԅ.ş-Ԇ.ş;public static x operator*(x ԅ,x Ԇ)=>ԅ.ş*Ԇ.ş;
public static x operator*(x ź,double ԇ)=>ź.ş*ԇ;public static x operator*(double ԇ,x ź)=>ź.ş*ԇ;public static x operator/(x ԅ,x
Ԇ)=>ԅ.ş/Ԇ.ş;public static x operator/(x ź,double Ԉ)=>ź.ş/Ԉ;public double Ķ=>ş.X;public double ķ=>ş.Y;public double ĸ=>ş.Z
;public static x ȝ=>Vector3D.Zero;public static Vector3 ơ=>Vector3D.Forward;public static Vector3 ĕ=>Vector3D.Left;public
static x Ė=>Vector3D.Up;public double Ȩ()=>ş.Length();public double Ό(x ԉ)=>ş.Dot(ԉ.ş);public double ȣ()=>ş.LengthSquared();
public static x ϼ(x Ԋ,MatrixD ԋ)=>Vector3D.Transform(Ԋ,ԋ);public static x Ў(x Ԋ,MatrixD ԋ)=>Vector3D.TransformNormal(Ԋ,ԋ);
public static x ǃ(x ǀ)=>Vector3D.Abs(ǀ);public static x Ȥ(x ƒ)=>Vector3D.Normalize(ƒ.ş);public static x ȩ(x Ԋ,double ª)=>
Vector3D.ClampToSphere(Ԋ.ş,ª);public static x ȧ(ref x Ԍ,ref x ԍ)=>Vector3D.ProjectOnVector(ref Ԍ.ş,ref ԍ.ş);public static x Ќ(x
Ԏ,x ԏ)=>Vector3D.Cross(Ԏ,ԏ);public x θ()=>ş.Normalized();public bool Ԑ(x ԉ)=>ş==ԉ.ş;}