// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using GammaLibrary;
using GammaLibrary.Extensions;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.Posts.Message;

var qq = new HttpApiClient();
qq.ApiAddress = "http://192.168.0.234:10000/";
var listener = new Sisters.WudiLib.WebSocket.CqHttpWebSocketEvent("ws://192.168.0.234:10001/");
var hc = new HttpClient();
listener.ApiClient = qq;
listener.MessageEvent += (api, message) =>
{

    try
    {
        if (message.MessageType is Message.GroupType && message.Source.UserId is 1538757052 or 920059839)
        {
            var msg = (GroupMessage)message;
            foreach (var section in msg.Content.Sections)
            {
                if (section.Data.TryGetValue("file_unique", out var x) && Config.Instance.MarkedImageIds.Contains(x))
                {
                    SendDnkFun(msg.GroupId);
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
                || text.ToLower().Contains("omc")
               )
            {
                SendDnkFun(msg.GroupId);
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
                SendDnkFun(msg.GroupId);
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }

};
await listener.StartListen(CancellationToken.None);

Thread.CurrentThread.Join();

void SendDnkFun(long group)
{
    var files = Directory.GetFiles("../../../dnkFuns/dnkBook", "*.*", SearchOption.AllDirectories).Where(x =>
        Path.GetFileName(x).EndsWith(".png") || Path.GetFileName(x).EndsWith(".jpg"));
    SendImage(group,files.PickOne());
}

void SendImage(long group, string path)
{
    qq.SendGroupMessageAsync(group, SendingMessage.LocalImage(path, true)).Wait();
}


[ConfigurationPath("dnk.json")]
public class Config : Configuration<Config>
{
    public List<string> MarkedImageIds = new List<string>();
}