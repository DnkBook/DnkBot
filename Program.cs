// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AnyAscii;
using GammaLibrary;
using GammaLibrary.Extensions;
using PininSharp;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.Posts.Message;

var qq = new HttpApiClient();
Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");

qq.ApiAddress = "http://192.168.0.234:20000/";
var listener = new Sisters.WudiLib.WebSocket.CqHttpWebSocketEvent("ws://192.168.0.234:20001/");
listener.ApiClient = qq;
//
// var qqSelf = new HttpApiClient();
// qqSelf.ApiAddress = "http://192.168.0.234:20000/";
// var listenerSelf = new Sisters.WudiLib.WebSocket.CqHttpWebSocketEvent("ws://192.168.0.234:20001/");
// listenerSelf.ApiClient = qqSelf;
var pinyin = PinIn.CreateDefault().GetCharacter('呕').Pinyins();

var hc = new HttpClient();
listener.MessageEvent += (api, message) =>
{

    try
    {
        if (message.MessageType is Message.GroupType && message.Source.UserId is 1538757052 or 920059839 or 775942303)
        {
            var msg = (GroupMessage)message;
            foreach (var section in msg.Content.Sections)
            {
                if (section.Data.TryGetValue("file_unique", out var x) && Config.Instance.MarkedImageIds.Contains(x))
                {
                    SendDnkFun(msg.GroupId, message.Source.UserId, true);
                    return;
                }
            }
            var text = message.Content.Text;
            
            if (text.Contains("呕")
                || text.Contains("哎")
                || text.Contains("白") && text.Contains("毛")
                || text.Contains("蓝") && text.Contains("瞳")
                || text.Contains("二次元")
                || text.Contains("口") && text.Contains("区")
                || text.Contains("O") && text.Contains("区")
                || text.Contains("0") && text.Contains("区")
                || text.Contains("〇") && text.Contains("区")
                || text.Contains("好想")
                || text.Contains("绷")
                || text.Contains("藕")
                || text.Contains("欧")
                || text.ToLower().Contains("omc")
               )
            {
                SendDnkFun(msg.GroupId, message.Source.UserId, true);
                return;
            }

            try
            {
                if (PinIn.CreateDefault().GetCharacter(message.Content.Text.Trim().Last()).Pinyins().Any(x => x.ToString().StartsWith("ou")))
                {
                    SendDnkFun(msg.GroupId, message.Source.UserId, true);
                    return;
                }
            }
            catch (Exception e)
            {
                
            }

            try
            {
                var msg1 = message.Content.Text;
                var lower = BuildNonChinese(msg1).Transliterate().ToLower();
                if (lower.Contains("ou"))
                {
                    SendDnkFun(msg.GroupId, message.Source.UserId, true);
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (text.Any() && Random.Shared.NextDouble() < 0.03)
            {
                SendDnkFun(msg.GroupId, message.Source.UserId);
            }
        }

        if (message.MessageType is Message.GroupType && message.Source.UserId is 775942303)
        {
            var msg = (GroupMessage)message;
            if (msg.Content.Text == "mark")
            {
                var msgId = msg.Content.Sections[0].Data["id"];
                var originalMsg = hc.GetStringAsync("http://192.168.0.234:10000/get_msg?message_id=" + msgId).Result;
                var json = JsonDocument.Parse(originalMsg);
                var element = json.RootElement.GetProperty("data").GetProperty("message").EnumerateArray()
                    .First(x => x.GetProperty("type").GetString() == "image").GetProperty("data");
                var s = element.GetProperty("file_unique").GetString();
                Config.Instance.MarkedImageIds.Add(s);
                Config.Save();
                SendDnkFun(msg.GroupId, message.Source.UserId);
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }

};
listener.StartListen(CancellationToken.None);
//listenerSelf.StartListen(CancellationToken.None);
Thread.CurrentThread.Join();

void SendDnkFun(long group, long sourceUserId, bool mute = false)
{
    var files = Directory.GetFiles("../../../dnkFuns/dnkBook", "*.*", SearchOption.AllDirectories).Where(x =>
        Path.GetFileName(x).EndsWith(".png") || Path.GetFileName(x).EndsWith(".jpg"));
    if (Random.Shared.NextDouble() < 0.7)
    {
        SendImage(group,files.PickOne(), mute, sourceUserId);
    }
}

void SendImage(long group, string path, bool mute, long sourceUserId)
{
    qq.SendGroupMessageAsync(group, SendingMessage.LocalImage(path, true)).Wait();
    mute = false;
    if (mute)
    {
        Config.Instance.MuteCount++;
        Config.Save();
        var max = 60 * 3;
        var pow = (int)Math.Min(Math.Pow(Config.Instance.MuteCount, 2), max);
        //qqSelf.BanGroupMember(group, sourceUserId, pow);
        //qqSelf.SendGroupMessageAsync(group, pow == max ? $"精致睡眠: {(int)(Math.Pow(Config.Instance.MuteCount, 2))}s (已为最大值 3 min)" :$"精致睡眠: {pow}s");
    }
}

string BuildNonChinese(string s)
{
    var ca = s.ToCharArray();
    var sb = new StringBuilder();
    foreach (var c in ca)
    {
        if (!IsChinese(c)) sb.Append(c);
    }

    return sb.ToString();
}

bool IsChinese(char c)
{
    return cjkCharRegex.IsMatch(c.ToString());
}

[ConfigurationPath("dnk.json")]
public class Config : Configuration<Config>
{
    public List<string> MarkedImageIds = new List<string>();
    public int MuteCount = -1;
}