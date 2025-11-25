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
public static Program A;public static B C;public static Random D;public E F=new E(10);int G;public
 Program(){try{var H=
DateTime.UtcNow;F.I("Beginning script setup",J.K);A=this;C=new B(this);D=new Random();L("Setup debug and RNG",J.C);M.N(Me);
Program.L("Creating this ship as controllable ship",J.K);O.P(Me.CubeGrid,GridTerminalSystem);Runtime.UpdateFrequency=
UpdateFrequency.Update1;var Q=DateTime.UtcNow-H;L($"Setup completed in {Q.TotalMilliseconds:F1} ms",J.R);}catch(Exception S){Echo(
"Crashed: "+S);}Echo(F.ToString());}public void
 Main
(string T,UpdateType U){if((U&UpdateType.Update1)!=0)V();if((U&(UpdateType.Trigger|UpdateType.Terminal))!=0)W(T);}void V(
){X.Y();O.Z(G);O.a(G++);F.b();c(F);}void W(string T){Action d;if(M.e.TryGetValue(T,out d)){d();}else{}}public static void
c(object f){A.Echo(f.ToString());}public static void c(TimeSpan Q,string g){double h=Q.Ticks/10.0;A.Echo($"{g}: {h} µs");
}public static void L(object i,J j=J.K){A.F.I(i.ToString(),j);}}
public class B{public readonly bool k;public void n()=>l?.Invoke(m);Action<IMyProgrammableBlock>l;public void p()=>o?.
Invoke(m);Action<IMyProgrammableBlock>o;public void s(int q)=>r?.Invoke(m,q);Action<IMyProgrammableBlock,int>r;public int µ(
Vector3D t,Color u,float v=0.2f,float x=w,bool?y=null)=>z?.Invoke(m,t,u,v,x,y??ª)??-1;Func<IMyProgrammableBlock,Vector3D,Color,
float,float,bool,int>z;public int Ä(Vector3D º,Vector3D À,Color u,float Â=Á,float x=w,bool?y=null)=>Ã?.Invoke(m,º,À,u,Â,x,y??
ª)??-1;Func<IMyProgrammableBlock,Vector3D,Vector3D,Color,float,float,bool,int>Ã;public int Ê(BoundingBoxD Å,Color u,Æ È=Æ
.Ç,float Â=Á,float x=w,bool?y=null)=>É?.Invoke(m,Å,u,(int)È,Â,x,y??ª)??-1;Func<IMyProgrammableBlock,BoundingBoxD,Color,
int,float,float,bool,int>É;public int Í(MyOrientedBoundingBoxD Ë,Color u,Æ È=Æ.Ç,float Â=Á,float x=w,bool?y=null)=>Ì?.
Invoke(m,Ë,u,(int)È,Â,x,y??ª)??-1;Func<IMyProgrammableBlock,MyOrientedBoundingBoxD,Color,int,float,float,bool,int>Ì;public int
Ñ(BoundingSphereD Î,Color u,Æ È=Æ.Ç,float Â=Á,int Ï=15,float x=w,bool?y=null)=>Ð?.Invoke(m,Î,u,(int)È,Â,Ï,x,y??ª)??-1;
Func<IMyProgrammableBlock,BoundingSphereD,Color,int,float,int,float,bool,int>Ð;public int Õ(MatrixD Ò,float Ó=1f,float Â=Á,
float x=w,bool?y=null)=>Ô?.Invoke(m,Ò,Ó,Â,x,y??ª)??-1;Func<IMyProgrammableBlock,MatrixD,float,float,float,bool,int>Ô;public
int Ù(string Ö,Vector3D t,Color?u=null,float x=w)=>Ø?.Invoke(m,Ö,t,u,x)??-1;Func<IMyProgrammableBlock,string,Vector3D,Color
?,float,int>Ø;public int Þ(string Ú,Û Ü=Û.C,float x=2)=>Ý?.Invoke(m,Ú,Ü.ToString(),x)??-1;Func<IMyProgrammableBlock,
string,string,float,int>Ý;public void â(string Ú,string ß=null,Color?à=null,Û Ü=Û.C)=>á?.Invoke(m,Ú,ß,à,Ü.ToString());Action<
IMyProgrammableBlock,string,string,Color?,string>á;public void ê(out int q,double ã,double ä=0.05,å ç=å.æ,string è=null)=>q=é?.Invoke(m,ã,ä,
ç.ToString(),è)??-1;Func<IMyProgrammableBlock,double,double,string,string,int>é;public double í(int q,double ë=1)=>ì?.
Invoke(m,q)??ë;Func<IMyProgrammableBlock,int,double>ì;public int ï()=>î?.Invoke()??-1;Func<int>î;public TimeSpan ñ()=>ð?.
Invoke()??TimeSpan.Zero;Func<TimeSpan>ð;public ò ô(Action<TimeSpan>ó)=>new ò(this,ó);public struct ò:IDisposable{B õ;TimeSpan
ö;Action<TimeSpan>ø;public ò(B ù,Action<TimeSpan>ó){õ=ù;ø=ó;ö=õ.ñ();}public void Dispose(){ø?.Invoke(õ.ñ()-ö);}}public
enum Æ{ú,Ç,û}public enum å{ü,ý,þ,ÿ,Ā,ā,Ă,ă,Ą,ą,Ć,ć,Ĉ,æ,ĉ,Ċ,ċ,Č,č,Ď,ď,Đ,đ,Ē,ē,Ĕ,ĕ,Ė,ė,Ę,ę,Ě,ě,Ĝ,ĝ,Ğ,õ,ğ,ø,Ġ,ġ,Ģ,ģ,Ĥ,A,ĥ,Ħ,ħ,Ĩ
,ĩ,Ī,ī,Ĭ,ĭ,ö,Į,į,İ,ı,Ĳ,ĳ,Ĵ,ĵ,Ķ,ķ,ĸ,Ĺ,ĺ,Ļ,ļ,Ľ,ľ,Ŀ,I,ŀ,Ł,ł,Ń,ń,Ņ,ņ,Ň,ň,ŉ,Ŋ,ŋ,Ō,ō,Ŏ,ŏ}public enum Û{C,Ő,ő,Œ,œ,Ŕ}const float
Á=0.02f;const float w=-1;IMyProgrammableBlock m;bool ª;public B(MyGridProgram ŕ,bool Ŗ=false){if(ŕ==null)throw new
Exception("Pass `this` into the API, not null.");ª=Ŗ;m=ŕ.Me;var ŗ=m.GetProperty("DebugAPI")?.As<IReadOnlyDictionary<string,
Delegate>>()?.GetValue(m);if(ŗ!=null){Ř(out o,ŗ["RemoveAll"]);Ř(out l,ŗ["RemoveDraw"]);Ř(out r,ŗ["Remove"]);Ř(out z,ŗ["Point"]);
Ř(out Ã,ŗ["Line"]);Ř(out É,ŗ["AABB"]);Ř(out Ì,ŗ["OBB"]);Ř(out Ð,ŗ["Sphere"]);Ř(out Ô,ŗ["Matrix"]);Ř(out Ø,ŗ["GPS"]);Ř(out
Ý,ŗ["HUDNotification"]);Ř(out á,ŗ["Chat"]);Ř(out é,ŗ["DeclareAdjustNumber"]);Ř(out ì,ŗ["GetAdjustNumber"]);Ř(out î,ŗ[
"Tick"]);Ř(out ð,ŗ["Timestamp"]);p();k=true;}}void Ř<Į>(out Į ř,object Ś)=>ř=(Į)Ś;}public static class Ǝ{public class Ş{public
object ś;public string Ŝ;public Ş(object f,string ŝ){ś=f;Ŝ=ŝ;}}public static string š(object f,int ş=-1){var Š=new
StringBuilder();š(f,Š,ş,null);Program.L("Serialized successfully",J.C);return Š.ToString();}private static void š(object f,
StringBuilder Š,int ş,string Ţ){string ţ=new string(' ',Math.Max(ş,0));if(f==null){if(Ţ!=null)Š.AppendLine(ţ+Ţ+" = null");return;}Ş Ť
=f as Ş;if(Ť!=null){bool Ŧ=ť(Ť.ś);if(Ŧ&&Ţ!=null){Š.AppendLine(ţ+Ţ+" = "+ŧ(Ť.ś)+"   # "+Ť.Ŝ);}else{if(!string.
IsNullOrEmpty(Ť.Ŝ))Š.AppendLine(ţ+"# "+Ť.Ŝ);š(Ť.ś,Š,ş,Ţ);}return;}IDictionary<string,object>Ũ=f as IDictionary<string,object>;if(Ũ!=
null){if(Ţ!=null)Š.AppendLine(ţ+Ţ+" = [");foreach(var ũ in Ũ){š(ũ.Value,Š,ş+2,ũ.Key);}if(Ţ!=null)Š.AppendLine(ţ+"]");return;
}IEnumerable<object>Ū=f as IEnumerable<object>;if(Ū!=null){if(Ţ!=null){Š.Append(ţ+Ţ+" = { ");bool ū=true;foreach(object Ŭ
in Ū){if(!ū)Š.Append(", ");Š.Append(ŧ(Ŭ));ū=false;}Š.AppendLine(" }");}else{foreach(object Ŭ in Ū)š(Ŭ,Š,ş,null);}return;}
if(Ţ!=null){Š.AppendLine(ţ+Ţ+" = "+ŧ(f));}}private static string ŧ(object f){if(f==null)return"null";if(f is string)return
"\""+ŭ((string)f)+"\"";if(f is bool)return((bool)f?"true":"false");if(f is float)return((float)f).ToString("0.#####",System.
Globalization.CultureInfo.InvariantCulture);if(f is double)return((double)f).ToString("0.##########",System.Globalization.CultureInfo
.InvariantCulture);if(f is int||f is long||f is short||f is byte)return f.ToString();return"\""+ŭ(f.ToString())+"\"";}
private static string ŭ(string Ů){return Ů.Replace("\\","\\\\").Replace("\"","\\\"");}private static bool ť(object ů){return ů
is string||ů is bool||ů is int||ů is long||ů is short||ů is byte||ů is float||ů is double;}public static object Ÿ(string Ű
){int ű=0;var Ų=new Dictionary<string,object>();while(ű<Ű.Length){ų(Ű,ref ű);if(ű>=Ű.Length)break;string Ţ=Ŵ(Ű,ref ű);
Program.c(Ţ);ŵ(Ű,ref ű,'=');object ů=Ŷ(Ű,ref ű);string ŝ=ŷ(Ű,ref ű);if(ŝ!=null&&ť(ů))ů=new Ş(ů,ŝ);Ų[Ţ]=ů;}return Ų;}private
static object Ŷ(string Ů,ref int ű){ų(Ů,ref ű);if(ű>=Ů.Length)return null;char Ź=Ů[ű];switch(Ź){case'[':{ű++;var Ũ=new
Dictionary<string,object>();while(true){ų(Ů,ref ű);if(ű>=Ů.Length)break;if(Ů[ű]==']'){ű++;break;}string Ţ=Ŵ(Ů,ref ű);ŵ(Ů,ref ű,'='
);object ů=Ŷ(Ů,ref ű);string ŝ=ŷ(Ů,ref ű);if(ŝ!=null&&ť(ů))ů=new Ş(ů,ŝ);Ũ[Ţ]=ů;}return Ũ;}case'{':{ű++;var Ū=new List<
object>();while(true){ų(Ů,ref ű);if(ű>=Ů.Length)break;if(Ů[ű]=='}'){ű++;break;}object ů=Ŷ(Ů,ref ű);string ŝ=ŷ(Ů,ref ű);if(ŝ!=
null&&ť(ů))ů=new Ş(ů,ŝ);Ū.Add(ů);ų(Ů,ref ű);if(ű<Ů.Length&&Ů[ű]==',')ű++;}return Ū;}}string Ż=ź(Ů,ref ű);string ż=ŷ(Ů,ref ű)
;object ž=Ž(Ż);if(ż!=null&&ť(ž))ž=new Ş(ž,ż);return ž;}private static void ų(string Ů,ref int ű){while(ű<Ů.Length){if(Ů[ű
]==' '||Ů[ű]=='\t'||Ů[ű]=='\r'||Ů[ű]=='\n'){ű++;continue;}if(Ů[ű]=='#'){while(ű<Ů.Length&&Ů[ű]!='\n')ű++;continue;}break;
}}private static string Ŵ(string Ů,ref int ű){ſ(Ů,ref ű);int º=ű;while(ű<Ů.Length){char Ź=Ů[ű];if(Ź=='=')break;if(Ź=='\n'
||Ź=='\r')throw new Exception("Unexpected newline while reading key");ű++;}if(ű==º)throw new Exception(
$"Empty key at index {ű}");string Ţ=Ů.Substring(º,ű-º).Trim();return Ţ;}private static string ź(string Ů,ref int ű){ſ(Ů,ref ű);if(ű>=Ů.Length)
return"";char Ź=Ů[ű];if(Ź=='"'||Ź=='\''){char ƀ=Ź;ű++;int º=ű;while(ű<Ů.Length&&Ů[ű]!=ƀ)ű++;string Ɓ=Ů.Substring(º,ű-º);if(ű<Ů
.Length)ű++;return Ɓ;}int Ƃ=ű;while(ű<Ů.Length&&Ů[ű]!='\n'&&Ů[ű]!='\r'&&Ů[ű]!='#'&&Ů[ű]!=','&&Ů[ű]!='}'&&Ů[ű]!=']')ű++;
string Ż=Ů.Substring(Ƃ,ű-Ƃ).Trim();return Ż;}private static string ŷ(string Ů,ref int ű){ſ(Ů,ref ű);if(ű<Ů.Length&&Ů[ű]=='#'){
ű++;int º=ű;while(ű<Ů.Length&&Ů[ű]!='\n'&&Ů[ű]!='\r')ű++;return Ů.Substring(º,ű-º).Trim();}return null;}private static
void ſ(string Ů,ref int ű){while(ű<Ů.Length&&(Ů[ű]==' '||Ů[ű]=='\t'||Ů[ű]=='\r'||Ů[ű]=='\n'))ű++;}private static void ŵ(
string Ů,ref int ű,char ƃ){ſ(Ů,ref ű);if(ű>=Ů.Length||Ů[ű]!=ƃ)throw new Exception("Expected '"+ƃ+"' at index "+ű);ű++;}private
static object Ž(string Ż){if(Ż.Length==0)return null;if(Ż=="true")return true;if(Ż=="false")return false;int Ƅ;if(int.TryParse
(Ż,out Ƅ))return Ƅ;double ƅ;if(double.TryParse(Ż,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo
.InvariantCulture,out ƅ))return ƅ;return Ż;}public static void Ɖ(Dictionary<string,object>Ũ){var Ɔ=new List<string>(Ũ.
Keys);foreach(var Ţ in Ɔ){var Ƈ=Ũ[Ţ];var Ź=Ƈ as Ş;var ƈ=Ƈ as Dictionary<string,object>;var Ū=Ƈ as List<object>;if(Ź!=null)Ũ[
Ţ]=Ź.ś;else if(ƈ!=null)Ɖ(ƈ);else if(Ū!=null){for(int Ƅ=0;Ƅ<Ū.Count;Ƅ++){var Ɗ=Ū[Ƅ];var Ƌ=Ɗ as Ş;Ū[Ƅ]=Ƌ!=null?Ƌ.ś:Ū[Ƅ];}}}
}public static Į ƍ<Į>(Dictionary<string,object>Ũ,string Ţ,Į ƌ=default(Į)){object ů;if(Ũ.TryGetValue(Ţ,out ů)){if(ů is Į)
return(Į)ů;try{return(Į)Convert.ChangeType(ů,typeof(Į));}catch{return ƌ;}}return ƌ;}}public enum Ƒ:byte{Ə,Ɛ,đ,ē,Ē,Ĕ,}public
static class ơ{private static readonly List<ƒ>Ɠ=new List<ƒ>();private static readonly List<BoundingBoxD>Ɣ=new List<
BoundingBoxD>();private static readonly Dictionary<ƒ,List<ƒ>>ƕ=new Dictionary<ƒ,List<ƒ>>();private static readonly List<List<ƒ>>Ɩ=
new List<List<ƒ>>();private static int Ɨ=0;private static List<ƒ>Ƙ(){if(Ɨ>=Ɩ.Count)Ɩ.Add(new List<ƒ>(8));var Ū=Ɩ[Ɨ++];Ū.
Clear();return Ū;}public static Dictionary<ƒ,List<ƒ>>ƞ(MyDynamicAABBTreeD ƙ){Ɨ=0;ƕ.Clear();ƙ.GetAll(Ɠ,clear:true,boxsList:Ɣ);
for(int Ƅ=0;Ƅ<Ɠ.Count;Ƅ++){var ƚ=Ɠ[Ƅ];var ƛ=Ɣ[Ƅ];var Ɯ=Ƙ();ƙ.OverlapAllBoundingBox(ref ƛ,Ɯ,0U,false);foreach(var Ɲ in Ɯ){if
(ƚ==Ɲ)continue;if(!ƕ.ContainsKey(ƚ))ƕ.Add(ƚ,Ƙ());ƕ[ƚ].Add(Ɲ);if(!ƕ.ContainsKey(Ɲ))ƕ.Add(Ɲ,Ƙ());ƕ[Ɲ].Add(ƚ);}}return ƕ;}
public static List<ƒ>Ơ(MyDynamicAABBTreeD ƙ,BoundingBoxD Ɵ){Ɨ=0;var Ɯ=Ƙ();ƙ.OverlapAllBoundingBox(ref Ɵ,Ɯ,0U,false);return Ɯ;}
}public static class ǜ{private static bool Ƶ(long Ƣ,long ƣ,long Ƥ,double ƥ,MatrixD Ʀ,Vector3D Ƨ){double ƨ=Ƣ*ƥ;double Ʃ=ƣ*
ƥ;double ƪ=Ƥ*ƥ;Vector3D ƫ=Ʀ.Right;Vector3D Ƭ=Ʀ.Up;Vector3D ƭ=Ʀ.Forward;Vector3D Ʈ=Vector3D.Abs(ƫ);Vector3D Ư=Vector3D.Abs
(Ƭ);Vector3D ư=Vector3D.Abs(ƭ);const double Ʊ=1e-8;double Ʋ=ƨ*Ʈ.X+Ʃ*Ư.X+ƪ*ư.X;if(Ʋ>Ƨ.X+Ʊ)return false;double Ƴ=ƨ*Ʈ.Y+Ʃ*Ư.
Y+ƪ*ư.Y;if(Ƴ>Ƨ.Y+Ʊ)return false;double ƴ=ƨ*Ʈ.Z+Ʃ*Ư.Z+ƪ*ư.Z;if(ƴ>Ƨ.Z+Ʊ)return false;return true;}private static bool ǎ(
Vector3D ƫ,Vector3D Ƭ,Vector3D ƭ,Vector3D Ƨ,out double ƨ,out double Ʃ,out double ƪ){var ƶ=Math.Abs(ƫ.X);var Ʒ=Math.Abs(Ƭ.X);var
Ƹ=Math.Abs(ƭ.X);var ƹ=Math.Abs(ƫ.Y);var ƺ=Math.Abs(Ƭ.Y);var ƻ=Math.Abs(ƭ.Y);var Ƽ=Math.Abs(ƫ.Z);var ƽ=Math.Abs(Ƭ.Z);var ƾ
=Math.Abs(ƭ.Z);double ƿ=Ƨ.X,ǀ=Ƨ.Y,ǁ=Ƨ.Z;double ǂ=ƶ*(ƺ*ƾ-ƻ*ƽ)-Ʒ*(ƹ*ƾ-ƻ*Ƽ)+Ƹ*(ƹ*ƽ-ƺ*Ƽ);const double ǃ=1e-12;if(Math.Abs(ǂ)<
ǃ){ƨ=Ʃ=ƪ=0.0;return false;}double Ǆ=(ƺ*ƾ-ƻ*ƽ)/ǂ;double ǅ=-(Ʒ*ƾ-Ƹ*ƽ)/ǂ;double ǆ=(Ʒ*ƻ-Ƹ*ƺ)/ǂ;double Ǉ=-(ƹ*ƾ-ƻ*Ƽ)/ǂ;double ǈ
=(ƶ*ƾ-Ƹ*Ƽ)/ǂ;double ǉ=-(ƶ*ƻ-Ƹ*ƹ)/ǂ;double Ǌ=(ƹ*ƽ-ƺ*Ƽ)/ǂ;double ǋ=-(ƶ*ƽ-Ʒ*Ƽ)/ǂ;double ǌ=(ƶ*ƺ-Ʒ*ƹ)/ǂ;ƨ=Ǆ*ƿ+ǅ*ǀ+ǆ*ǁ;Ʃ=Ǉ*ƿ+ǈ*
ǀ+ǉ*ǁ;ƪ=Ǌ*ƿ+ǋ*ǀ+ǌ*ǁ;const double Ǎ=-1e-9;if(ƨ<Ǎ||Ʃ<Ǎ||ƪ<Ǎ)return false;ƨ=Math.Max(0.0,ƨ);Ʃ=Math.Max(0.0,Ʃ);ƪ=Math.Max(0.0
,ƪ);return true;}public static BoundingBoxD Ǜ(BoundingBoxD Ǐ,MatrixD Ʀ,double ƥ){ƥ/=2;Vector3D Ƨ=Ǐ.HalfExtents;Vector3D ƫ
=Ʀ.Right;Vector3D Ƭ=Ʀ.Up;Vector3D ƭ=Ʀ.Forward;double ǐ,Ǒ,ǒ;if(ǎ(ƫ,Ƭ,ƭ,Ƨ,out ǐ,out Ǒ,out ǒ)){long Ƣ=Math.Max(0,(long)(ǐ/ƥ+
1e-12));long ƣ=Math.Max(0,(long)(Ǒ/ƥ+1e-12));long Ƥ=Math.Max(0,(long)(ǒ/ƥ+1e-12));bool Ǔ;do{Ǔ=false;if(Ƶ(Ƣ+1,ƣ,Ƥ,ƥ,Ʀ,Ƨ)){Ƣ++;
Ǔ=true;}if(Ƶ(Ƣ,ƣ+1,Ƥ,ƥ,Ʀ,Ƨ)){ƣ++;Ǔ=true;}if(Ƶ(Ƣ,ƣ,Ƥ+1,ƥ,Ʀ,Ƨ)){Ƥ++;Ǔ=true;}}while(Ǔ);var ǔ=new Vector3D(Ƣ*ƥ,ƣ*ƥ,Ƥ*ƥ);
return new BoundingBoxD(-ǔ,ǔ);}double Ǖ=Ƨ.X/(Math.Abs(ƫ.X)+Math.Abs(Ƭ.X)+Math.Abs(ƭ.X));double ǖ=Ƨ.Y/(Math.Abs(ƫ.Y)+Math.Abs(Ƭ
.Y)+Math.Abs(ƭ.Y));double Ǘ=Ƨ.Z/(Math.Abs(ƫ.Z)+Math.Abs(Ƭ.Z)+Math.Abs(ƭ.Z));double ǘ=Math.Min(Math.Min(Ǖ,ǖ),Ǘ);long Ǚ=
Math.Max(0,(long)Math.Floor(ǘ/ƥ));var ǚ=new Vector3D(Ǚ*ƥ,Ǚ*ƥ,Ǚ*ƥ);return new BoundingBoxD(-ǚ,ǚ);}}public class ǫ{double ǝ;
double Ǟ;public double ǟ;double Ǡ;public double ǡ;double Ǣ;double ǣ;double Ǥ;public ǫ(double ǥ,double Ǧ,double ǧ,double Ǩ=0,
double ǩ=0,double Ǫ=60){ǟ=ǥ;Ǡ=Ǧ;ǡ=ǧ;Ǣ=Ǩ;ǣ=ǩ;Ǥ=Ǫ;}public double ǰ(double Ǭ,int ǭ){double Ǯ=Math.Round(Ǭ,ǭ);ǝ=ǝ+(Ǭ/Ǥ);ǝ=(Ǣ>0&&ǝ>
Ǣ?Ǣ:ǝ);ǝ=(ǣ<0&&ǝ<ǣ?ǣ:ǝ);double ǯ=(Ǯ-Ǟ)*Ǥ;Ǟ=Ǯ;return(ǟ*Ǭ)+(Ǡ*ǝ)+(ǡ*ǯ);}public void Ǳ(){ǝ=Ǟ=0;}}public static class ȳ{
private static double ǲ=104;public static double ǳ=>Double.MaxValue;public const double Ǵ=1e-6,ǵ=-0.5,Ƕ=1.73205,Ƿ=Ƕ/2,Ǹ=1.0/3.0
,ǹ=1.0/9.0,Ǻ=1.0/6.0,ǻ=1.0/54.0;public static Vector3D Ȓ(double Ǽ,Vector3D ǽ,Vector3D Ǿ,Vector3D ǿ,Vector3D Ȁ,Vector3D ȁ,
Vector3D Ȃ,Vector3D ȃ,bool Ȅ,Vector3D ȅ=default(Vector3D),bool Ȇ=false){double ȇ=0;Vector3D Ȉ=Vector3D.Zero,ȉ=Ȃ,Ȋ=ȁ,ȋ,Ȍ;if(Ȃ.
LengthSquared()>1){ȇ=Math.Min((Vector3D.Normalize(Ȃ)*ǲ-Vector3D.ProjectOnVector(ref ȁ,ref Ȃ)).Length(),2*ǲ)/Ȃ.Length();ȁ=Vector3D.
ClampToSphere(ȁ+Ȃ*ȇ,ǲ);Ȋ+=Ȃ*ȇ*0.5;Ȃ=Vector3D.Zero;}if(Ǿ.LengthSquared()>1){double ȍ=Math.Max((Vector3D.Normalize(Ǿ)*Ǽ-Vector3D.
ProjectOnVector(ref ǽ,ref Ǿ)).Length(),0)/Ǿ.Length(),Ȏ=(ǽ*ȍ+Ǿ*ȍ*ȍ).Length();Vector3D ȏ=ǿ+ȁ*ȍ+0.5*Ȃ*ȍ*ȍ;if(ȏ.Length()>Ȏ){Ǿ=Vector3D.Zero
;ǽ=Vector3D.ClampToSphere(ǽ+Ǿ*ȍ,Ǽ);ǿ-=Vector3D.Normalize(ǿ)*Ȏ;}}ȋ=ȁ-ǽ;Ȍ=Ȃ-Ǿ;double ȑ=Ȑ(Ȍ.LengthSquared()*0.25,Ȍ.X*ȋ.X+Ȍ.Y
*ȋ.Y+Ȍ.Z*ȋ.Z,ȋ.LengthSquared()-ǽ.LengthSquared()+ǿ.X*Ȍ.X+ǿ.Y*Ȍ.Y+ǿ.Z*Ȍ.Z,2*(ǿ.X*ȋ.X+ǿ.Y*ȋ.Y+ǿ.Z*ȋ.Z),ǿ.LengthSquared());
if(ȑ==ǳ||double.IsNaN(ȑ)||ȑ>100)ȑ=100;if(ȇ>ȑ){ȇ=ȑ;ȑ=0;}else ȑ-=ȇ;return Ȅ?ǿ+ȁ*ȑ+Ȋ*ȇ+0.5*ȉ*ȇ*ȇ+0.5*Ȃ*ȑ*ȑ+Ȉ:Ȁ+(ȁ-ǽ)*ȑ+(Ȋ-ǽ)*
ȇ+0.5*ȉ*ȇ*ȇ+0.5*Ȃ*ȑ*ȑ+-0.5*ȅ*(ȑ+ȇ)*(ȑ+ȇ)*Convert.ToDouble(Ȇ)+Ȉ;}public static double Ȑ(double ȓ,double Ȕ,double Ź,double
ȕ,double Ȗ){if(Math.Abs(ȓ)<Ǵ)ȓ=ȓ>=0?Ǵ:-Ǵ;double ȗ=1/ȓ;Ȕ*=ȗ;Ź*=ȗ;ȕ*=ȗ;Ȗ*=ȗ;double Ș=-Ź,ș=Ȕ*ȕ-4*Ȗ,Ț=-Ȕ*Ȕ*Ȗ-ȕ*ȕ+4*Ź*Ȗ,ț;
double[]Ȝ;bool Ȟ=ȝ(Ș,ș,Ț,out Ȝ);ț=Ȝ[0];if(Ȟ){if(Math.Abs(Ȝ[1])>Math.Abs(ț))ț=Ȝ[1];if(Math.Abs(Ȝ[2])>Math.Abs(ț))ț=Ȝ[2];}double
ȟ,Ƞ,ȡ,Ȣ,ȣ;double Ȥ=ț*ț-4*Ȗ;if(Math.Abs(Ȥ)<Ǵ){ȟ=Ƞ=ț*0.5;Ȥ=Ȕ*Ȕ-4*(Ź-ț);if(Math.Abs(Ȥ)<Ǵ)ȡ=Ȣ=Ȕ*0.5;else{ȣ=Math.Sqrt(Ȥ);ȡ=(Ȕ+
ȣ)*0.5;Ȣ=(Ȕ-ȣ)*0.5;}}else{ȣ=Math.Sqrt(Ȥ);ȟ=(ț+ȣ)*0.5;Ƞ=(ț-ȣ)*0.5;double ȥ=1/(ȟ-Ƞ);ȡ=(Ȕ*ȟ-ȕ)*ȥ;Ȣ=(ȕ-Ȕ*Ƞ)*ȥ;}double Ȧ,ȧ;Ȥ=ȡ
*ȡ-4*ȟ;if(Ȥ<0)Ȧ=ǳ;else{ȣ=Math.Sqrt(Ȥ);Ȧ=Ȩ(-ȡ+ȣ,-ȡ-ȣ)*0.5;}Ȥ=Ȣ*Ȣ-4*Ƞ;if(Ȥ<0)ȧ=ǳ;else{ȣ=Math.Sqrt(Ȥ);ȧ=Ȩ(-Ȣ+ȣ,-Ȣ-ȣ)*0.5;}
return Ȩ(Ȧ,ȧ);}private static bool ȝ(double ȓ,double Ȕ,double Ź,out double[]Ȝ){Ȝ=new double[4];double ȩ=ȓ*ȓ,Ȫ=(ȩ-3*Ȕ)*ǹ,ȫ=(ȓ*(
2*ȩ-9*Ȕ)+27*Ź)*ǻ,Ʃ=ȫ*ȫ,Ȭ=Ȫ*Ȫ*Ȫ;if(Ʃ<Ȭ){double ȭ=Math.Sqrt(Ȫ),Ȯ=ȫ/(ȭ*ȭ*ȭ);if(Ȯ<-1)Ȯ=-1;else if(Ȯ>1)Ȯ=1;Ȯ=Math.Acos(Ȯ);ȓ*=Ǹ
;Ȫ=-2*ȭ;double ȯ=Math.Cos(Ȯ*Ǹ),Ȱ=Math.Sin(Ȯ*Ǹ);Ȝ[0]=Ȫ*ȯ-ȓ;Ȝ[1]=Ȫ*((ȯ*ǵ)-(Ȱ*Ƿ))-ȓ;Ȝ[2]=Ȫ*((ȯ*ǵ)+(Ȱ*Ƿ))-ȓ;return true;}else
{double ȱ=-Math.Pow(Math.Abs(ȫ)+Math.Sqrt(Ʃ-Ȭ),Ǹ),Ȳ;if(ȫ<0)ȱ=-ȱ;Ȳ=ȱ==0?0:Ȫ/ȱ;ȓ*=Ǹ;Ȝ[0]=ȱ+Ȳ-ȓ;Ȝ[1]=-0.5*(ȱ+Ȳ)-ȓ;Ȝ[2]=0.5*Ƕ
*(ȱ-Ȳ);if(Math.Abs(Ȝ[2])<Ǵ){Ȝ[2]=Ȝ[1];return true;}return false;}}private static double Ȩ(double ȓ,double Ȕ){if(ȓ<=0)
return Ȕ>0?Ȕ:ǳ;else if(Ȕ<=0)return ȓ;else return Math.Min(ȓ,Ȕ);}}public enum J{ȴ,C,K,ȵ,ȶ,ȷ,R}public class E{Dictionary<J,
string>ȹ=new Dictionary<J,string>(){{J.ȴ,$"{ȸ(Color.Gray)}"},{J.C,$"{ȸ(Color.DarkSeaGreen)}"},{J.K,$"{ȸ(Color.White)}"},{J.ȵ,
$"{ȸ(Color.Gold)}"},{J.ȶ,$"{ȸ(Color.Red)}"},{J.ȷ,$"{ȸ(Color.DarkRed)}"},{J.R,$"{ȸ(Color.Aquamarine)}"}};private static string ȸ(Color u){
return$"[color=#{u.A:X2}{u.R:X2}{u.G:X2}{u.B:X2}]";}private static string Ⱥ="[/color]\n";class ɀ{internal readonly string Ȼ;
internal readonly double ȼ;internal readonly J Ƚ;public ɀ(string Ⱦ,double ȿ,J j){Ȼ=Ⱦ;ȼ=ȿ;Ƚ=j;}}private readonly List<ɀ>Ɂ=new
List<ɀ>();private readonly double ɂ;public E(double Ƀ){ɂ=Ƀ;}public void I(string Ú,J j){if(j<M.J)return;double Ʉ=(System.
DateTime.UtcNow-new System.DateTime(1970,1,1)).TotalSeconds;Ɂ.Add(new ɀ(Ú,Ʉ,j));}public void b(){double Ʉ=(System.DateTime.
UtcNow-new System.DateTime(1970,1,1)).TotalSeconds;Ɂ.RemoveAll(Ȗ=>Ʉ-Ȗ.ȼ>ɂ);}public List<string>Ʌ(){return Ɂ.Select(Ȗ=>Ȗ.Ȼ).
ToList();}public override string ToString(){string ů="";foreach(var Ɇ in Ɂ){ů+=ȹ[Ɇ.Ƚ]+Ɇ.Ȼ+Ⱥ;}return ů;}}public class ɉ{public
int ɇ;public Action Ɉ;}public static class X{private static readonly Dictionary<int,ɉ>Ɋ=new Dictionary<int,ɉ>();private
static readonly Dictionary<int,ɉ>ɋ=new Dictionary<int,ɉ>();private static int Ɍ=1;public static int ɐ(int ɍ,Action Ɏ=null){if(
ɍ<=0){if(Ɏ!=null)Ɏ();return-1;}int q=Ɍ++;var ɏ=new ɉ{ɇ=ɍ,Ɉ=Ɏ};ɋ[q]=ɏ;return q;}public static int ɑ(float ɍ,Action Ɏ=null)
{return ɐ((int)(ɍ*60),Ɏ);}public static void ɒ(int q){Ɋ.Remove(q);ɋ.Remove(q);}public static void Y(){if(ɋ.Count>0){
foreach(var ɓ in ɋ){Ɋ[ɓ.Key]=ɓ.Value;}ɋ.Clear();}var ɔ=new List<int>();foreach(var ɓ in Ɋ){var ɏ=ɓ.Value;ɏ.ɇ--;if(ɏ.ɇ<=0){ɏ.Ɉ?.
Invoke();ɔ.Add(ɓ.Key);}}foreach(var q in ɔ){Ɋ.Remove(q);}}public static bool ɕ(int q){return Ɋ.ContainsKey(q)||ɋ.ContainsKey(q
);}public static int ɖ(int q){ɉ ɏ;if(Ɋ.TryGetValue(q,out ɏ))return ɏ.ɇ;if(ɋ.TryGetValue(q,out ɏ))return ɏ.ɇ;return-1;}}
public static class M{private static ɗ ɜ=new ɗ("General Config",""){ɘ=ə,ɚ=ɛ};public static Dictionary<string,Action>e;public
static J J=J.ȴ;public static string ɝ="Target";public static string ɞ="Untarget";public static string ɟ="ArgusV2";public
static string ɠ="TrackerGroup";public static double ɡ=2000;public static double ɢ=3000;public static float ɣ=40;public static
double ɤ=0.999999;public static string ɥ="CTC: Tracking";public static string ɦ="CTC: Searching";public static string ɧ=
"CTC: Standby";public static int ɨ=3600;public static double ɩ=500;public static double ɪ=0;public static double ɫ=30;public static
double ɬ=-0.05;public static double ɭ=0.05;public static double ɮ=30;public static int ɯ=300;public static double ɰ=9.81;
public static bool ɱ=false;public static bool ɲ=false;public static void N(IMyProgrammableBlock ɳ){Program.L(
"Setting up config");ɴ.ɵ();ɶ.ɵ();if(ɳ.CustomData.Length>0)ɗ.ɷ(ɳ.CustomData);ɳ.CustomData=ɗ.ɸ();Program.L("Written config to custom data",J.
C);e=new Dictionary<string,Action>{{ɝ,()=>O.ɹ.ɺ()},{ɞ,()=>O.ɹ.ɻ()},{"FireAllTest",()=>O.ɹ.ɼ.ɽ()},{"CancelAllTest",()=>O.ɹ
.ɼ.ɾ()}};if(J.ȴ<=J){foreach(var ɿ in e){Program.L($"Command: {ɿ.Key}",J.ȴ);}}Program.L("Commands set up",J.C);ʀ(ɳ);
Program.L("Config setup done",J.K);}public static void ʀ(IMyProgrammableBlock ɳ){Program.L("Setting up global state",J.C);ʁ.ʂ=ɱ
;Program.L($"Precision mode state is {ʁ.ʂ}",J.ȴ);}static M(){}private static Dictionary<string,object>ə(){return new
Dictionary<string,object>{["String Config"]=new Dictionary<string,object>{["ArgumentTarget"]=ɝ,["ArgumentUnTarget"]=ɞ,["GroupName"
]=ɟ,["TrackerGroupName"]=ɠ},["Behavior Config"]=new Dictionary<string,object>{["MaxWeaponRange"]=ɡ,["LockRange"]=ɢ,[
"LockAngle"]=ɣ,["MinFireDot"]=ɤ,},["Tracker Config"]=new Dictionary<string,object>{["TrackingName"]=ɥ,["SearchingName"]=ɦ,[
"StandbyName"]=ɧ,["ScannedBlockMaxValidFrames"]=ɨ},["PID Config"]=new Dictionary<string,object>{["ProportionalGain"]=ɩ,[
"IntegralGain"]=ɪ,["DerivativeGain"]=ɫ,["IntegralLowerLimit"]=ɬ,["IntegralUpperLimit"]=ɭ,["MaxAngularVelocityRPM"]=ɮ}};}private static
void ɛ(Dictionary<string,object>f){Ǝ.Ɖ(f);var ʃ=f.ContainsKey("String Config")?f["String Config"]as Dictionary<string,object
>:null;if(ʃ!=null){ɝ=Ǝ.ƍ(ʃ,"ArgumentTarget",ɝ);ɞ=Ǝ.ƍ(ʃ,"ArgumentUnTarget",ɞ);ɟ=Ǝ.ƍ(ʃ,"GroupName",ɟ);ɠ=Ǝ.ƍ(ʃ,
"TrackerGroupName",ɠ);}var ʄ=f.ContainsKey("Behavior Config")?f["Behavior Config"]as Dictionary<string,object>:null;if(ʄ!=null){ɡ=Ǝ.ƍ(ʄ,
"MaxWeaponRange",ɡ);ɢ=Ǝ.ƍ(ʄ,"LockRange",ɢ);ɣ=Ǝ.ƍ(ʄ,"LockAngle",ɣ);ɤ=Ǝ.ƍ(ʄ,"MinFireDot",ɤ);}var ʅ=f.ContainsKey("Tracker Config")?f[
"Tracker Config"]as Dictionary<string,object>:null;if(ʅ!=null){ɥ=Ǝ.ƍ(ʅ,"TrackingName",ɥ);ɦ=Ǝ.ƍ(ʅ,"SearchingName",ɦ);ɧ=Ǝ.ƍ(ʅ,
"StandbyName",ɧ);ɨ=Ǝ.ƍ(ʅ,"ScannedBlockMaxValidFrames",ɨ);}var ʆ=f.ContainsKey("PID Config")?f["PID Config"]as Dictionary<string,
object>:null;if(ʆ!=null){ɩ=Ǝ.ƍ(ʆ,"ProportionalGain",ɩ);ɪ=Ǝ.ƍ(ʆ,"IntegralGain",ɪ);ɫ=Ǝ.ƍ(ʆ,"DerivativeGain",ɫ);ɬ=Ǝ.ƍ(ʆ,
"IntegralLowerLimit",ɬ);ɭ=Ǝ.ƍ(ʆ,"IntegralUpperLimit",ɭ);ɮ=Ǝ.ƍ(ʆ,"MaxAngularVelocityRPM",ɮ);}}}public class ɗ{private static readonly
Dictionary<string,ɗ>ʇ=new Dictionary<string,ɗ>();public static string ɸ(){Program.L("Writing config",J.K);var ʈ=new Dictionary<
string,object>();foreach(var ũ in ʇ){Program.L($"Collecting config: {ũ.Key}",J.C);ʈ.Add(ũ.Key,ũ.Value.ɘ());}return Ǝ.š(ʈ);}
public static void ɷ(string ʈ){Program.L("Reading config from custom data",J.K);var ʉ=Ǝ.Ÿ(ʈ);Program.L(
"DeltaWing Object Notation: Parsed successfully",J.C);var Ũ=ʉ as Dictionary<string,object>;if(Ũ==null){Program.L("Config malformed",J.ȷ);throw new Exception();}foreach(
var ũ in Ũ){ɗ ʊ;if(!ʇ.TryGetValue(ũ.Key,out ʊ))continue;var ʋ=ũ.Value as Dictionary<string,object>;if(ʋ!=null){Program.L(
"Config set: "+ũ.Key,J.C);ʊ.ɚ(ʋ);}}}public Func<Dictionary<string,object>>ɘ{get;set;}public Action<Dictionary<string,object>>ɚ{get;set
;}public ɗ(string Ö,string ŝ){ʌ=Ö;Ş=ŝ;ʇ.Add(Ö,this);}public string ʌ{get;}public string Ş{get;}public Dictionary<string,
object>ə()=>ɘ?.Invoke();public void ɛ(Dictionary<string,object>ů)=>ɚ?.Invoke(ů);}public class ɶ{private static readonly ɗ M=
new ɗ("Projectile Data","The main list of known projectiles. Gun Data should reference these by name."){ɘ=ə,ɚ=ɛ};public
static Dictionary<string,ɶ>ʍ=new Dictionary<string,ɶ>();public static readonly ɶ ʎ=new ɶ(0,0,0,0);static ɶ(){ʍ.Add("Default",ʎ
);ʍ.Add("LargeRailgun",new ɶ(2000,2000,2000,0));ʍ.Add("Artillery",new ɶ(500,500,2000,0));ʍ.Add("SmallRailgun",new ɶ(1000,
1000,1400,0));ʍ.Add("Gatling",new ɶ(400,400,800,0));ʍ.Add("AssaultCannon",new ɶ(500,500,1400,0));ʍ.Add("Rocket",new ɶ(100,
200,800,1000));}public static void ɵ(){Program.L("Projectile data loaded",J.C);}public static ɶ ɘ(string ʏ){ɶ ʐ;return ʍ.
TryGetValue(ʏ,out ʐ)?ʐ:ʎ;}public float ʑ{get;private set;}public float ʒ{get;private set;}public float ʓ{get;private set;}public
float ʔ{get;private set;}public ɶ(float ʕ,float ʖ,float ʗ,float ʘ){ʑ=ʕ;ʒ=ʖ;ʓ=ʗ;ʔ=ʘ;}private static Dictionary<string,object>ə
(){var ʙ=new Dictionary<string,object>();foreach(var ũ in ʍ){var Ö=ũ.Key;var ʚ=ũ.Value;ʙ[Ö]=new Dictionary<string,object>
{["ProjectileVelocity"]=ʚ.ʑ,["MaxVelocity"]=ʚ.ʒ,["MaxRange"]=ʚ.ʓ,["Acceleration"]=ʚ.ʔ};}return ʙ;}private static void ɛ(
Dictionary<string,object>ʈ){Ǝ.Ɖ(ʈ);foreach(var ũ in ʈ){var ʛ=(Dictionary<string,object>)ũ.Value;var ʜ=ʍ[ũ.Key]??ʎ;var ʕ=Ǝ.ƍ(ʛ,
"ProjectileVelocity",ʜ.ʑ);var ʖ=Ǝ.ƍ(ʛ,"MaxVelocity",ʜ.ʒ);var ʗ=Ǝ.ƍ(ʛ,"MaxRange",ʜ.ʓ);var ʘ=Ǝ.ƍ(ʛ,"Acceleration",ʜ.ʔ);var ʝ=new ɶ(ʕ,ʖ,ʗ,ʘ);ʍ[
ũ.Key]=ʝ;}}}public class ɴ{private static readonly ɗ M=new ɗ("Gun Data",
"The main list of known gun types and their definition names. Should reference a known projectile type."){ɘ=ə,ɚ=ɛ};public static Dictionary<string,ɴ>ʍ=new Dictionary<string,ɴ>();public static readonly ɴ ʞ=new ɴ("Default",0,0
,0f,0f);static ɴ(){ʍ.Add("Default",ʞ);ʍ.Add("LargeRailgun",new ɴ("LargeRailgun",ʟ.ʠ,ʡ.ʢ,2.0f,4.0f));ʍ.Add(
"LargeBlockLargeCalibreGun",new ɴ("Artillery",0,0,0,12));ʍ.Add("LargeMissileLauncher",new ɴ("Rocket",0,0,0,0.5f));ʍ.Add("SmallRailgun",new ɴ(
"SmallRailgun",ʟ.ʠ,ʡ.ʢ,0.5f,4.0f));ʍ.Add("SmallBlockAutocannon",new ɴ("Gatling",0,0,0.0f,0.4f));ʍ.Add("SmallBlockMediumCalibreGun",new
ɴ("AssaultCannon",0,0,0.0f,6f));ʍ.Add("MyObjectBuilder_SmallGatlingGun",new ɴ("Gatling",0,0,0.0f,0.1f));ʍ.Add(
"MyObjectBuilder_SmallMissileLauncher",new ɴ("Rocket",0,0,0.0f,1f));ʍ.Add("SmallRocketLauncherReload",ɘ("MyObjectBuilder_SmallMissileLauncher"));ʍ.Add(
"SmallGatlingGunWarfare2",ɘ("MyObjectBuilder_SmallGatlingGun"));ʍ.Add("SmallMissileLauncherWarfare2",ɘ("MyObjectBuilder_SmallMissileLauncher"));}
public static void ɵ(){Program.L("Gun data loaded",J.C);}public static ɴ ɘ(string ʏ){ɴ ʐ;return ʍ.TryGetValue(ʏ,out ʐ)?ʐ:ʞ;}
string ʣ;public ɶ ɶ{get;}public ʟ ʤ{get;}public ʡ ʥ{get;}public int ʦ{get;}public float ʧ=>ʦ/60.0f;public int ʨ{get;}public
float ʩ=>ʨ/60.0f;public ɴ(string ʪ,ʟ ʫ,ʡ ʬ,float ʭ,float ʮ){ʣ=ʪ;ɶ=ɶ.ɘ(ʪ);ʤ=ʫ;ʥ=ʬ;ʦ=(int)(ʭ*60);ʨ=(int)(ʮ*60);}private static
Dictionary<string,object>ə(){var ʙ=new Dictionary<string,object>();foreach(var ũ in ʍ){var Ö=ũ.Key;var ʚ=ũ.Value;var ʝ=new
Dictionary<string,object>();ʝ["Projectile"]=ʚ.ʣ;ʝ["ReloadType"]=new Ǝ.Ş((int)ʚ.ʤ,"0 = normal, 1 = charged");ʝ["FireType"]=new Ǝ.Ş(
(int)ʚ.ʥ,"0 = normal, 1 = delay before firing");ʝ["FireTime"]=ʚ.ʧ;ʝ["ReloadTime"]=ʚ.ʩ;ʙ[Ö]=ʝ;}return ʙ;}private static
void ɛ(Dictionary<string,object>ʈ){Ǝ.Ɖ(ʈ);foreach(var ũ in ʈ){var ʛ=(Dictionary<string,object>)ũ.Value;var ʜ=ʍ[ũ.Key]??ʞ;var
ʯ=Ǝ.ƍ(ʛ,"Projectile",ʜ.ʣ);var ʰ=Ǝ.ƍ(ʛ,"ReloadType",ʜ.ʤ);var ʬ=Ǝ.ƍ(ʛ,"FireType",ʜ.ʥ);var ʭ=Ǝ.ƍ(ʛ,"FireTime",ʜ.ʧ);var ʮ=Ǝ.ƍ
(ʛ,"ReloadTime",ʜ.ʩ);var ʝ=new ɴ(ʯ,ʰ,ʬ,ʭ,ʮ);ʍ[ũ.Key]=ʝ;}}}public static class ʁ{public static bool ʂ;}public abstract
class ʷ{protected Vector3D ʱ;protected Vector3D ʲ;protected int ʳ;public ʴ ʶ=ʴ.ʵ;public ʷ(){Program.L("New ArgusShip",J.C);ʳ=
Program.D.Next()%600;}public abstract Vector3D ʸ{get;}public abstract Vector3D ʹ{get;}public abstract Vector3D ʔ{get;}public
abstract float ʺ{get;}public abstract string ʌ{get;}public abstract void Z(int ʻ);public abstract void a(int ʻ);public Vector3D
ˌ(ʷ ʼ,float ʕ){Vector3D ʽ=this.ʸ;Vector3D ʾ=this.ʹ;Vector3D ʿ=ʼ.ʸ;Vector3D ˀ=ʼ.ʔ;Vector3D ˁ=ʼ.ʹ-ʾ;Vector3D ˆ=ʿ-ʽ;double Ů
=ʕ;double ȓ=ˁ.LengthSquared()-Ů*Ů;double Ȕ=2.0*ˆ.Dot(ˁ);double Ź=ˆ.LengthSquared();double Ȯ;if(Math.Abs(ȓ)<1e-6){if(Math.
Abs(Ȕ)<1e-6)Ȯ=0;else Ȯ=-Ź/Ȕ;}else{double ˇ=Ȕ*Ȕ-4*ȓ*Ź;if(ˇ<0)return ʿ;double ˈ=Math.Sqrt(ˇ);double ˉ=(-Ȕ+ˈ)/(2*ȓ);double ˊ=(
-Ȕ-ˈ)/(2*ȓ);Ȯ=Math.Min(ˉ,ˊ)>0?Math.Min(ˉ,ˊ):Math.Max(ˉ,ˊ);if(Ȯ<0)Ȯ=Math.Max(ˉ,ˊ);}Vector3D ˋ=ʿ+ˁ*Ȯ+0.5*ˀ*Ȯ*Ȯ;return ˋ;}}
public struct ͱ{public readonly Vector3D ˍ;public readonly Vector3D ˎ;public readonly Vector3D ˏ;public readonly Vector3D ː;
public readonly double ˑ;public readonly double ˠ;public MatrixD ˡ;public ͱ(Vector3D ˢ,Vector3D Ȁ,Vector3D ˣ,Vector3D ˤ,double
ˬ,double ˮ,MatrixD Ͱ){ˍ=ˢ;ˎ=Ȁ;ˏ=ˣ;ː=ˤ;ˑ=ˬ;ˠ=ˮ;ˡ=Ͱ;}}public class ͻ{Ͳ ͳ;ʹ Ͷ;public ͻ(Ͳ ͷ,ʹ ͺ){Program.L($"Setting up FCS",
J.K);ͳ=ͷ;Ͷ=ͺ;}public ͱ Θ(){Ͷ.ͼ();int Ά=Ͷ.ͽ;Έ Ί=Ͷ.Ή;var Ύ=Ͷ.Ό;var ΐ=ͳ.Ώ();var ˆ=ΐ-Ύ;var Α=ˆ.Length();var Β=ˆ/Α;var ˬ=Β.Dot
(ͳ.Ə);var Δ=Ͷ.Γ();var Ε=(Δ-Ύ).Normalized();Program.c(Ε);if(ˬ>M.ɤ&&ΐ!=Vector3D.Zero)Ͷ.Ζ();else Ͷ.Η();return new ͱ(Ε,ΐ,Ύ,ͳ.
Ə,ˬ,Α,ͳ.ˡ);}}public enum ʟ{Ι,ʠ}public enum ʡ{Ι,ʢ}public enum Π{Κ,Λ,Μ,Ν,Ξ,Ο}public class ΰ{private static readonly
MyDefinitionId Ρ=new MyDefinitionId(typeof(MyObjectBuilder_GasProperties),"Electricity");IMyUserControllableGun Σ;ʟ Τ;ʡ Υ;
MyResourceSinkComponent Φ;int Χ;int Ψ;Π Ω;bool Ϊ;bool Ϋ;ɴ ά;ʹ έ;public ΰ(IMyUserControllableGun ʐ,ʹ ή){var ί=ʐ.BlockDefinition;Program.L(
$"Set up new gun {ί}",J.K);var ʝ=ɴ.ɘ(ί.SubtypeIdAttribute);if(ʝ==ɴ.ʞ)ʝ=ɴ.ɘ(ί.TypeIdString);ά=ʝ;έ=ή;Σ=ʐ;Φ=ʐ.Components.Get<
MyResourceSinkComponent>();Τ=ά.ʤ;Υ=ά.ʥ;}public Vector3D β=>(Vector3)(Σ.Min+Σ.Max)/2*έ.α.ʺ;public Vector3D γ=>Σ.GetPosition();public Vector3D Ƒ{
get;set;}public float ʹ=>ά.ɶ.ʑ;public float ʔ=>ά.ɶ.ʔ;public float ʒ=>ά.ɶ.ʒ;public float ʓ=>ά.ɶ.ʓ;public ɴ ɴ=>ά;public Π ε{
get{if(!Ϊ){Ω=δ();Ϊ=true;}return Ω;}}public Vector3D Ə=>Σ.WorldMatrix.Forward;public void Z(int ʻ){Ϊ=false;}public void a(
int ʻ){}public bool θ(){if(ε!=Π.Μ)return false;Σ.ShootOnce();Χ=X.ɐ(ά.ʨ,ζ);if(Υ==ʡ.ʢ){Ψ=X.ɐ(ά.ʦ,η);}else{Ψ=X.ɐ(0,η);}return
true;}public bool κ(){if(ε!=Π.Κ)return false;Σ.Enabled=false;X.ɐ(0,ι);Ϋ=true;return true;}public bool λ(){if(ε!=Π.Κ)return
false;if(X.ɖ(Ψ)>1)return false;Σ.Enabled=false;X.ɐ(0,ι);Ϋ=true;return true;}public Vector3D ξ(Vector3D μ){Vector3D ν=μ+Ƒ*ʹ;if
(ν.LengthSquared()>ʹ*ʹ)ν=ν.Normalized()*ʹ;return ν;}Π δ(){bool ο=Σ.IsFunctional;if(!ο)return Π.Ο;if(Υ==ʡ.ʢ&&X.ɕ(Ψ))return
Ϋ?Π.Λ:Π.Κ;switch(Τ){case ʟ.Ι:if(X.ɕ(Χ))return Π.Ν;break;case ʟ.ʠ:if(X.ɕ(Χ))return Π.Ν;if(Φ.CurrentInputByType(Ρ)>0.02f)
return Π.Ξ;break;}return Π.Μ;}void η(){if(Ϋ){Ϋ=false;return;}}void ζ(){}void ι(){Σ.Enabled=true;}}public enum Έ{π,ρ,Κ}public
enum ʥ{ς,σ,τ}public class ʹ{List<ΰ>Ͷ=new List<ΰ>();List<ΰ>υ=new List<ΰ>();Vector3D φ;Έ χ;int ψ;ɴ ά;public ʹ(List<
IMyTerminalBlock>ω,Ͳ ϊ){Program.L("Setting up gun manager",J.K);foreach(var ϋ in ω){var ʐ=ϋ as IMyUserControllableGun;if(ʐ!=null)Ͷ.Add(
new ΰ(ʐ,this));}if(Ͷ.Count<=0)Program.L($"No guns in group {M.ɟ}",J.ȵ);α=ϊ;}public Ͳ α{get;}public IMyCubeGrid ύ=>α.ό.
CubeGrid;public int ώ=>Ͷ.Count;public Vector3D Ό=>φ;public Έ Ή=>χ;public int ͽ=>ψ;public void Z(int ʻ){foreach(var ʐ in Ͷ)ʐ.Z(ʻ)
;}public void ͼ(){var Ί=Έ.π;var Ϗ=new Dictionary<ɴ,List<ΰ>>();var ϐ=new Dictionary<ɴ,List<ΰ>>();var ϑ=0;var ϒ=0;foreach(
var ʐ in Ͷ){switch(ʐ.ε){case Π.Μ:if(!Ϗ.ContainsKey(ʐ.ɴ))Ϗ.Add(ʐ.ɴ,new List<ΰ>());Ϗ[ʐ.ɴ].Add(ʐ);ϑ++;break;case Π.Κ:if(!ϐ.
ContainsKey(ʐ.ɴ))ϐ.Add(ʐ.ɴ,new List<ΰ>());ϐ[ʐ.ɴ].Add(ʐ);ϒ++;break;}}Program.c(ϑ);var ϓ=ϐ;if(ϑ>0){χ=Έ.ρ;ϓ=Ϗ;}if(ϒ>0){χ=Έ.Κ;ϓ=ϐ;}var
ʼ=α.Ώ();var ϔ=α.ʸ;var ϕ=(ʼ-ϔ).LengthSquared();foreach(var ϖ in ϓ){var ʛ=ϖ.Key;var ͺ=ϖ.Value;var ʗ=ʛ.ɶ.ʓ;if(ʗ*ʗ<ϕ)continue
;υ=ͺ;ά=ʛ;break;}ψ=υ.Count;if(ψ==0){φ=α.ʸ;return;}Vector3D ϗ=Vector3D.Zero;foreach(var ʐ in υ){ϗ+=ʐ.β;}ϗ/=ψ;φ=Vector3D.
Transform(ϗ,α.ˡ);}public void a(int ʻ){foreach(var ʐ in Ͷ)ʐ.a(ʻ);}public void ɽ(){foreach(var ʐ in Ͷ)ʐ.θ();}public void ɾ(){
foreach(var ʐ in Ͷ)ʐ.κ();}public void Ζ(){foreach(var ʐ in Ͷ)ʐ.θ();}public void Η(){foreach(var ʐ in Ͷ)ʐ.λ();}public Vector3D Γ
(){if(ά==null)return Vector3D.Zero;var Ǽ=ά.ɶ.ʒ;var ʼ=α.Ώ();var ˆ=ʼ-φ;var Ϙ=υ[0];if(Ϙ==null)return Vector3D.Zero;var ȅ=α.ϙ
;var Ȇ=ȅ.LengthSquared()!=0;return ȳ.Ȓ(Ǽ,Ϙ.ξ(α.ʹ)/60,ά.ɶ.ʔ*Ϙ.Ə,ˆ,α.Ϛ.ʸ,α.Ϛ.ʹ/60,α.Ϛ.ʔ/60,Vector3D.Zero,false,ȅ,Ȇ);}}
public class ϟ{private readonly List<IMyGyro>ϛ;private readonly ǫ Ϝ;private readonly ǫ ϝ;public ϟ(List<IMyTerminalBlock>ω){
Program.L("Setting up gyro manager",J.K);ϛ=new List<IMyGyro>();foreach(var Ȕ in ω){var Ϟ=Ȕ as IMyGyro;if(Ϟ!=null){ϛ.Add(Ϟ);}}if
(ϛ.Count<=0)Program.L($"No gyroscopes found in group: {M.ɟ}",J.ȵ);Ϝ=new ǫ(M.ɩ,M.ɪ,M.ɫ,M.ɭ,M.ɬ);ϝ=new ǫ(M.ɩ,M.ɪ,M.ɫ,M.ɭ,M.
ɬ);}public void Ϭ(ref ͱ Ϡ,double ϡ=0){int Ϣ=7;double ϣ=1.0;if(Ϡ.ˑ>0.9999){ϣ*=0.8;Ϣ=4;}if(Ϡ.ˑ>0.99999){ϣ*=0.8;Ϣ=3;}if(Ϡ.ˑ>
0.999999){ϣ*=0.8;Ϣ=2;}if(Ϡ.ˑ>0.9999999){ϣ*=0.8;Ϣ=1;}double Ϥ;double ϥ;var Ϧ=ϡ;var ϧ=Vector3D.Cross(Ϡ.ː,Ϡ.ˍ);var Ϩ=Vector3D.
TransformNormal(ϧ,MatrixD.Transpose(Ϡ.ˡ));var ϩ=Ϝ.ǰ(-Ϩ.X,Ϣ);var ț=ϝ.ǰ(-Ϩ.Y,Ϣ);Ϥ=MathHelper.Clamp(ϩ,-M.ɮ,M.ɮ);ϥ=MathHelper.Clamp(ț,-M.ɮ,
M.ɮ);if(Math.Abs(ϥ)+Math.Abs(Ϥ)>M.ɮ){var Ϫ=M.ɮ/(Math.Abs(ϥ)+Math.Abs(Ϥ));ϥ*=Ϫ;Ϥ*=Ϫ;}Ϥ*=ϣ;ϥ*=ϣ;ϫ(Ϥ,ϥ,Ϧ,Ϡ.ˡ);}void ϫ(double
ϭ,double Ϯ,double ϯ,MatrixD Ͱ){var ϰ=new Vector3D(ϭ,Ϯ,ϯ);var ϱ=Vector3D.TransformNormal(ϰ,Ͱ);foreach(var Ϟ in ϛ)if(Ϟ.
IsFunctional&&Ϟ.IsWorking&&Ϟ.Enabled&&!Ϟ.Closed){var ϲ=Vector3D.TransformNormal(ϱ,MatrixD.Transpose(Ϟ.WorldMatrix));Ϟ.Pitch=(float)ϲ
.X;Ϟ.Yaw=(float)ϲ.Y;Ϟ.Roll=(float)ϲ.Z;Ϟ.GyroOverride=true;return;}}public void ϳ(){foreach(var Ϟ in ϛ)if(Ϟ.IsFunctional&&
Ϟ.IsWorking&&Ϟ.Enabled&&!Ϟ.Closed){Ϟ.GyroOverride=false;return;}}}internal class ϸ{List<ϴ>ω;List<ϵ>Ϸ;}internal class Є{
List<Ϲ>Ϻ;bool ϻ;float ϼ;int Ͻ;public Є(List<Ϲ>Ͼ,Ƒ Ͽ){Ϻ=Ͼ;Ѐ=0;Ƒ=Ͽ;foreach(var Ё in Ͼ){var Ѓ=Ё as Ђ;if(Ѓ!=null)Ѐ+=M.ɰ;}}public
void Z(int ʻ){if(ϼ==0)Ͻ++;else Ͻ=0;if(Ͻ>M.ɯ)Ѕ=false;}public void a(int ʻ){if(ϻ!=Ѕ)foreach(var Ё in Ϻ)Ё.Ѕ=Ѕ;ϻ=Ѕ;}public Ƒ Ƒ{
get;private set;}public bool Ѕ{get;set;}public double Ѐ{get;private set;}public void І(float ʘ){Ѕ=true;if(ʘ==ϼ)return;ϼ=ʘ;
foreach(var Ё in Ϻ){Ё.ʔ=ʘ*(float)M.ɰ;}}}public class Ж{private readonly Є Ї;private readonly Є Ј;private readonly Є Љ;ϸ Њ;Ͳ ͳ;
public Ж(List<IMyTerminalBlock>ω,Ͳ ͷ){Program.L($"Setting up gravity drive",J.K);ͳ=ͷ;var Ћ=new List<Ϲ>();var Ќ=new List<Ϲ>();
var Ѝ=new List<Ϲ>();var Ў=new Dictionary<Ƒ,List<Ϲ>>{{Ƒ.Ē,Ѝ},{Ƒ.Ĕ,Ѝ},{Ƒ.đ,Ќ},{Ƒ.ē,Ќ},{Ƒ.Ə,Ћ},{Ƒ.Ɛ,Ћ}};foreach(var ϋ in ω){
var Џ=ϋ as IMyGravityGenerator;if(Џ!=null){var Β=(Ƒ)Џ.Orientation.Up;var Ū=Ў[Β];bool А=(int)Β%2==0;Ū.Add(new Ђ(Џ,Β,А));}var
Б=ϋ as IMyGravityGeneratorSphere;if(Б!=null){var Г=ͳ.В;var Д=Base6Directions.Directions[(int)Г];var А=Д.Dot(ͳ.ʸ-Б.
GetPosition())<0;var Ū=Ў[Г];Ū.Add(new Е(Б,Г,А));}}if(Ћ.Count==0)Program.L($"No Forward/backward gravity generators",J.ȵ);if(Ќ.Count
==0)Program.L($"No Left/Right gravity generators",J.ȵ);if(Ѝ.Count==0)Program.L($"No Up/Down gravity generators",J.ȵ);Ї=new
Є(Ћ,Ƒ.Ə);Ј=new Є(Ќ,Ƒ.đ);Љ=new Є(Ѝ,Ƒ.Ē);}public void Z(int ʻ){Ї.Z(ʻ);Ј.Z(ʻ);Љ.Z(ʻ);}public void a(int ʻ){Ї.a(ʻ);Ј.a(ʻ);Љ.a
(ʻ);}public void И(Vector3 З){Ї.І(З.Dot(Vector3D.Forward));Ј.І(З.Dot(Vector3D.Left));Љ.І(З.Dot(Vector3D.Up));}}public
class ϵ:Њ{IMySpaceBall Й;public override double К=>Й.VirtualMass;public override bool Л{get;set;}public override double М=>Л?
Й.VirtualMass:0;}public class ϴ:Њ{public override bool Л{get;set;}public override double К{get;}public override double М{
get;}}public abstract class Ϲ{protected bool Н;public IMyGravityGeneratorBase О{get;protected set;}public Ƒ Ƒ{get;protected
set;}public bool Ѕ{get{return О.Enabled;}set{О.Enabled=value;}}public float ʔ{get{return О.GravityAcceleration*(Н?-1:1);}
set{О.GravityAcceleration=value*(Н?-1:1);}}}public class Ђ:Ϲ{public Ђ(IMyGravityGenerator П,Ƒ Β,bool А){О=П;Ƒ=Β;Н=А;}}
public class Е:Ϲ{public Е(IMyGravityGeneratorSphere Б,Ƒ Β,bool А){О=Б;Ƒ=Β;Н=А;}}public abstract class Њ{public abstract bool Л
{get;set;}public abstract double К{get;}public abstract double М{get;}}public class У{Ж Р;С Т;Ͳ ͳ;public У(List<
IMyTerminalBlock>ω,Ͳ ͷ){Program.L($"Setting up propulsion controller",J.K);ͳ=ͷ;Р=new Ж(ω,ͷ);Т=new С();}public void Z(int ʻ){Р.Z(ʻ);Т.Z(ʻ
);}public void a(int ʻ){var Ф=ͳ.ό.MoveIndicator;Matrix Ò;ͳ.ό.Orientation.GetMatrix(out Ò);var Х=Vector3.Transform(Ф,Ò);if
(ͳ.ό.DampenersOverride){var Ц=ͳ.ʹ;var Ч=Vector3D.TransformNormal(Ц,MatrixD.Invert(ͳ.ˡ));var Ш=Ч*Vector3D.Forward;var Щ=Ч*
Vector3D.Left;var Ъ=Ч*Vector3D.Down;if(Х.Dot(Vector3D.Forward)==0)Х+=Ш;if(Х.Dot(Vector3D.Left)==0)Х+=Щ;if(Х.Dot(Vector3D.Down)==
0)Х+=Ъ;}Р.И(Х);Р.a(ʻ);Т.a(ʻ);}}public class С{public void Z(int ʻ){}public void a(int ʻ){}}public enum ʴ{Ы,Ь,Э,ʵ}public
class а{public static int Я(ʴ Ю){switch(Ю){case ʴ.Ы:return 600;case ʴ.Ь:return 60;case ʴ.Э:return 10;case ʴ.ʵ:return 1;
default:return Int32.MaxValue;}}}public class в{public в(IMyTurretControlBlock ϋ){б=ϋ;}public IMyTurretControlBlock б{get;}
public bool г=>б.Closed;bool д;bool е=true;public bool ж{get;private set;}public bool з{get;private set;}public bool и{get;
private set;}public bool й{get;private set;}public bool Ѕ{get{return б.Enabled;}set{б.Enabled=value;}}public string к{get{
return б.CustomName;}set{б.CustomName=value;}}public long л{get;set;}public ƒ м{get;set;}public Vector3D ʸ=>б.GetPosition();
public void р(){var н=б.HasTarget;з=!н&&д;д=н;ж=н;var п=м!=null&&м.о;й=п&&е;е=!п;и=п;if(!н)л=0;}public MyDetectedEntityInfo т(
){var с=б.GetTargetedEntity();л=с.EntityId;return с;}}public class Ͳ:у{ϟ ф;public ʹ ɼ;ͻ х;List<IMyLargeTurretBase>ц;У ч;ƒ
ш;bool щ;Vector3D ъ;bool ы=false;public Ͳ(IMyCubeGrid ь,List<IMyTerminalBlock>ω,List<IMyTerminalBlock>э):base(ь,э){
Program.L("New ControllableShip : SupportingShip : ArgusShip",J.C);ф=new ϟ(ω);ɼ=new ʹ(ω,this);х=new ͻ(this,ɼ);foreach(var ϋ in
ω){var ю=ϋ as IMyShipController;if(ю!=null)ό=ю;}if(ό==null)Program.L($"WARNING: Controller not present in group: {M.ɟ}");
ч=new У(ω,this);}public IMyShipController ό{get;set;}public Vector3D Ə=>ό.WorldMatrix.Forward;public Vector3 я=>
Base6Directions.Directions[(int)ό.Orientation.Forward];public Ƒ В=>(Ƒ)ό.Orientation.Forward;public Vector3D Ē=>ό.WorldMatrix.Up;public
MatrixD ˡ=>ѐ.WorldMatrix;public ƒ Ϛ=>ш;public Vector3D ϙ{get{if(!ы){ъ=ό.GetNaturalGravity();ы=true;}return ъ;}}public override
void Z(int ʻ){base.Z(ʻ);ɼ.Z(ʻ);ч.Z(ʻ);}public override void a(int ʻ){base.a(ʻ);if(щ){ы=false;var Ϡ=х.Θ();if(Ϡ.ˎ==Vector3D.
Zero)ф.ϳ();else ф.Ϭ(ref Ϡ);ɼ.a(ʻ);}ч.a(ʻ);}public void ɻ(){ш=null;щ=false;ф.ϳ();}public void ɺ(){ш=O.ё(this,M.ɢ,M.ɣ);щ=true;
if(ш==null){щ=false;ф.ϳ();Program.L("Couldn't find new target",J.ȵ);}else{Program.L("Got new target",J.K);}}public
Vector3D Ώ(){return ш?.ʸ??Vector3D.Zero;}}public class у:ʷ{private readonly List<в>ђ;protected readonly IMyCubeGrid ѐ;public у(
IMyCubeGrid ь,List<IMyTerminalBlock>э){Program.L("New SupportingShip : ArgusShip",J.C);IMyUserControllableGun ʐ=null;IMyMotorStator
ѓ=null;ђ=new List<в>();foreach(var ϋ in э){var ю=ϋ as IMyTurretControlBlock;if(ю!=null){ђ.Add(new в(ю));continue;}ʐ=ʐ??ϋ
as IMyUserControllableGun;ѓ=ѓ??ϋ as IMyMotorStator;}if(ʐ!=null&&ѓ!=null){Program.L("Setting up trackers",J.C);foreach(var
є in ђ){Program.L($"Set up tracker: {є.к}");var ϋ=є.б;ϋ.ClearTools();ϋ.AddTool(ʐ);ϋ.AzimuthRotor=null;ϋ.ElevationRotor=ѓ;
ϋ.AIEnabled=true;ϋ.CustomName=M.ɦ;}if(ђ.Count<=0)Program.L("No target trackers in group",J.ȵ);}else Program.L(
$"Gun/rotor not present in group: {M.ɠ}, cannot setup trackers",J.ȵ);ѐ=ь;}public override Vector3D ʸ=>ѐ.GetPosition();public override Vector3D ʹ=>ʲ;public override Vector3D ʔ=>(ʲ-ʱ)*
60;public override float ʺ=>ѐ.GridSize;public override string ʌ=>ѐ.CustomName;public override string ToString()=>ʌ;public
override void Z(int ʻ){ʱ=ʲ;ʲ=ѐ.LinearVelocity;}public override void a(int ʻ){for(int Ƅ=ђ.Count-1;Ƅ>=0;Ƅ--){var є=ђ[Ƅ];if(є.г){ђ.
RemoveAt(Ƅ);continue;}є.р();if(!є.ж||є.и){if(є.з&&!є.Ѕ){є.Ѕ=true;є.к=M.ɦ;if(є.м!=null){є.м.ѕ=true;є.м.і=null;}є.м=null;}else if(
є.й){є.Ѕ=true;є.к=M.ɦ;}continue;}if(є.л!=0)continue;var ʼ=є.т();в ї;if(O.ј(ʼ.EntityId,out ї)){if(ї==є&&є.Ѕ)є.Ѕ=false;ї.м.
љ(ʼ,null,є);continue;}var ћ=O.њ(є,ʼ.EntityId,ʼ);є.м=ћ;є.к=M.ɥ;є.Ѕ=false;є.л=ʼ.EntityId;}}}enum ѡ{ќ,ѝ,ў,џ,Ѡ}class Ѧ{int Ѣ;
private readonly int ѣ;public ѡ Ѥ;public Ѧ(int ɏ,ѡ ѥ){Ѣ=ɏ;ѣ=ɏ;Ѥ=ѥ;}public void ѧ(){Ѣ=ѣ;}public bool Ѩ(){Ѣ--;return Ѣ<=0;}}
public class ƒ:ʷ{Dictionary<Vector3I,Ѧ>ѩ=new Dictionary<Vector3I,Ѧ>();Dictionary<Vector3I,Ѧ>Ѫ=new Dictionary<Vector3I,Ѧ>();
public у ѫ;public MyDetectedEntityInfo K;public long Ѭ;bool ѭ=true;BoundingBoxD Ѯ;Vector3D ѯ;Vector3D Ѱ;private readonly float
ѱ=1f;public ƒ(в є,long Ѳ,MyDetectedEntityInfo ã){і=є;Ѭ=Ѳ;ʶ=ʴ.Ь;switch(ã.Type){case MyDetectedEntityType.SmallGrid:ѱ=0.5f;
break;case MyDetectedEntityType.LargeGrid:ѱ=2.5f;break;}K=ã;ѳ=K.Orientation;ѳ.Translation=K.Position;}public override
Vector3D ʸ=>K.Position;public override Vector3D ʹ=>ʲ;public override Vector3D ʔ=>(ʲ-ʱ)*60;public override float ʺ=>ѱ;public
override string ʌ=>$"Trackable ship {Ѭ}";public bool ѕ{get;set;}=false;public override string ToString()=>ʌ;public в і{get;set;}
public bool о{get;set;}public BoundingBoxD Ѵ=>K.BoundingBox;public Vector3D Ѷ=>ѵ.Extents;public Vector3D ѷ=>ѵ.HalfExtents;
public int Ѹ{get;set;}=0;public BoundingBoxD ѵ{get{if(ѭ)ѹ();return Ѯ;}}public Vector3D Ѻ{get{if(ѭ)ѹ();return ѯ;}}Vector3D ѻ;
MatrixD ѳ;public Vector3D ѽ(){var Ѽ=ѻ;ѻ=Vector3D.Zero;return Ѽ;}void ѹ(){Ѯ=ǜ.Ǜ(Ѵ,K.Orientation,ѱ);ѭ=false;var Ѿ=ѷ;var Ȳ=ѱ/2;ѯ=
new Vector3D(Ȳ-Ѿ.X%ѱ,Ȳ-Ѿ.Y%ѱ,Ȳ-Ѿ.Z%ѱ);}public override void Z(int ʻ){if(ѕ)return;if((ʻ+ʳ)%а.Я(ʶ)!=0)return;K=і.т();if(і.г||
K.EntityId!=Ѭ)ѕ=true;ʱ=ʲ;ʲ=K.Velocity;ѻ=ʸ-Ѱ;if(ʶ==ʴ.ʵ&&(ѻ*60-ʲ).LengthSquared()>10000){ѭ=true;Vector3I ˆ=(Vector3I)(
Vector3D.Transform(ѻ-(ʲ/60),MatrixD.Invert(ѳ))*2);ѿ(ˆ);}ѳ=K.Orientation;ѳ.Translation+=K.Position;Ѱ=ʸ;}public override void a(
int ʻ){Ѫ.Clear();foreach(var ũ in ѩ){var Ҁ=Vector3D.Transform((Vector3D)(Vector3)ũ.Key*(double)ѱ+Ѻ,ѳ);if(ũ.Value.Ѩ())
continue;Ѫ[ũ.Key]=ũ.Value;Program.C.µ(Ҁ,Color.White,0.2f,0.016f,true);}var Ѽ=ѩ;ѩ=Ѫ;Ѫ=Ѽ;Program.C.Ê(Ѵ,Color.Green,B.Æ.Ç,0.02f,
0.016f);var ҁ=new MyOrientedBoundingBoxD(ѵ,K.Orientation);ҁ.Center=Ѵ.Center;Program.C.Í(ҁ,Color.Red,B.Æ.Ç,0.02f,0.016f);}
public void љ(MyDetectedEntityInfo с,IMyLargeTurretBase Ҋ=null,в ю=null){if(с.EntityId!=K.EntityId||с.HitPosition==null)return
;var ҋ=(Vector3D)с.HitPosition;var Ҍ=ҋ-ʸ;var ҍ=Vector3D.TransformNormal(Ҍ,MatrixD.Transpose(ѳ));ҍ-=Ѻ;var Ҏ=new Vector3I((
int)Math.Round(ҍ.X/ѱ),(int)Math.Round(ҍ.Y/ѱ),(int)Math.Round(ҍ.Z/ѱ));var ѥ=ѡ.ќ;var ҏ=Ҋ!=null?Ҋ.GetTargetingGroup():ю!=null?
ю.б.GetTargetingGroup():"";switch(ҏ){case"Weapons":ѥ=ѡ.ѝ;break;case"Propulsion":ѥ=ѡ.ў;break;case"Power Systems":ѥ=ѡ.џ;
break;}if(ѩ.ContainsKey(Ҏ)){var ʜ=ѩ[Ҏ];if(ѥ!=ʜ.Ѥ)ʜ.Ѥ=ѡ.Ѡ;ʜ.ѧ();return;}else{ѩ.Add(Ҏ,new Ѧ(M.ɨ,ѥ));}}void ѿ(Vector3I ˆ){var Ґ=
new Dictionary<Vector3I,Ѧ>();foreach(var ϋ in ѩ){Ґ.Add(ϋ.Key+ˆ,ϋ.Value);}ѩ=Ґ;}}public static class O{public static readonly
List<ʷ>ґ=new List<ʷ>();private static List<ƒ>Ғ=new List<ƒ>();public static Dictionary<long,ƒ>ғ=new Dictionary<long,ƒ>();
private static IEnumerator<ƒ>Ҕ;private static MyDynamicAABBTreeD ҕ=new MyDynamicAABBTreeD();static O(){}public static Ͳ ɹ{get;
set;}public static void Z(int ʻ){Җ();for(var җ=ґ.Count-1;җ>=0;җ--){var ͷ=ґ[җ];ͷ.Z(ʻ);}foreach(var ͷ in ґ){var Ҙ=ͷ as ƒ;if(Ҙ
==null)continue;var Ɵ=Ҙ.Ѵ;if(Ҙ.Ѹ!=0){var ˆ=Ҙ.ѽ();ҕ.MoveProxy(Ҙ.Ѹ,ref Ɵ,ˆ);}else{Ҙ.Ѹ=ҕ.AddProxy(ref Ɵ,Ҙ,0U);}}foreach(var ɓ
in ơ.ƞ(ҕ)){var ҙ=ɓ.Key;var Қ=ɓ.Value;var қ=ҙ.Ѵ.Size.LengthSquared();foreach(var Ҝ in Қ){var ҝ=Ҝ.Ѵ.Size.LengthSquared();if(
ҝ>қ){ҙ.о=true;return;}}ҙ.о=false;}}private static void Җ(){if(!Ҕ.MoveNext()){Ҕ=Ҟ().GetEnumerator();Ҕ.MoveNext();}}public
static void a(int ʻ){for(var җ=ґ.Count-1;җ>=0;җ--){var ͷ=ґ[җ];ͷ.a(ʻ);}}private static IEnumerable<ƒ>Ҟ(){var ҟ=M.ɡ*M.ɡ;for(var
җ=ґ.Count-1;җ>=0;җ--){if(җ>=ґ.Count)continue;var ͷ=ґ[җ];var ћ=ͷ as ƒ;if(ћ==null)continue;var Ύ=ћ.ʸ;var Ҡ=ɹ.ʸ;var ҡ=(Ύ-Ҡ).
LengthSquared();if(ҡ>ҟ)ћ.ʶ=ʴ.Ь;else ћ.ʶ=ʴ.ʵ;yield return ћ;}}public static List<ƒ>Ҥ(у ͷ,double ˮ){Ғ.Clear();foreach(var Ң in ґ){var ң
=Ң as ƒ;if(ң==null)continue;if((ң.ʸ-ͷ.ʸ).LengthSquared()<ˮ*ˮ)Ғ.Add(ң);}return Ғ;}public static ƒ ё(Ͳ ͷ,double ˮ,float ҥ){
var Ҧ=Ҥ(ͷ,ˮ);if(Ҧ.Count<1)return null;var Г=ͷ.Ə;double ҧ=Math.Cos(ҥ*Math.PI/180.0);double Ҩ=double.MaxValue;ƒ ҩ=null;
foreach(var Ҫ in Ҧ){var ҫ=(Ҫ.ʸ-ͷ.ʸ);var Ó=ҫ.Length();var ˬ=(ҫ/Ó).Dot(Г);ˬ=MathHelperD.Clamp(ˬ,-1.0,1.0);if(ˬ<ҧ)continue;var Ҭ=(
1-ˬ)*Ó;if(Ҭ<Ҩ){ҩ=Ҫ;Ҩ=Ҭ;}}return ҩ;}public static void P(IMyCubeGrid ь,IMyGridTerminalSystem ҭ){var ϖ=ҭ.
GetBlockGroupWithName(M.ɟ);Program.L($"Getting group : {M.ɟ}",J.ȴ);var Ү=ҭ.GetBlockGroupWithName(M.ɠ);Program.L($"Getting group : {M.ɠ}",J.ȴ)
;var ω=new List<IMyTerminalBlock>();var э=new List<IMyTerminalBlock>();if(ϖ!=null){ϖ.GetBlocks(ω);Program.L(
$"Got group: {M.ɟ}",J.C);}else Program.L($"Group not present: {M.ɟ}",J.ȵ);if(Ү!=null){Ү.GetBlocks(э);Program.L($"Got group: {M.ɠ}",J.C);}
else Program.L($"Group not present: {M.ɠ}",J.ȵ);var ͷ=new Ͳ(ь,ω,э);ґ.Add(ͷ);ɹ=ͷ;Ҕ=Ҟ().GetEnumerator();}public static ƒ њ(в є
,long Ѳ,MyDetectedEntityInfo ã){ƒ ћ;if(ғ.TryGetValue(Ѳ,out ћ)){Program.L("Restoring defunct ship"+Ѳ,J.C);if(!ћ.ѕ)return
null;ћ.і=є;ћ.ѕ=false;return ћ;}Program.L("Creating new ship "+Ѳ,J.C);ћ=new ƒ(є,Ѳ,ã);ґ.Add(ћ);ғ.Add(Ѳ,ћ);return ћ;}public
static void ү(ƒ ћ){ґ.Remove(ћ);ғ.Remove(ћ.Ѭ);ҕ.RemoveProxy(ћ.Ѹ);}public static bool ј(long Ұ,out в ї){ƒ ћ;var ұ=ғ.TryGetValue(
Ұ,out ћ);ї=ұ?ћ.і:null;return ұ&&!ћ.ѕ;}}