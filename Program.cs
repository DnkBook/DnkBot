// See https://aka.ms/new-console-template for more information

using cqhttp.Cyan.Clients;
using cqhttp.Cyan.Enums;
using GammaLibrary.Extensions;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.Posts.Message;

var group = 954738468;
var qq = new HttpApiClient();
qq.ApiAddress = "http://192.168.0.234:10000/";
var listener = new Sisters.WudiLib.WebSocket.CqHttpWebSocketEvent("ws://192.168.0.234:10001/");

listener.ApiClient = qq;
listener.MessageEvent += (api, message) =>
{
    if (message.MessageType is Message.GroupType && message.Source.UserId is 1538757052 or 920059839)
    {

        var text = message.Content.Text;
        if (text.Contains("呕")
            || text.Contains("哎")
            || text.Contains("白") && text.Contains("毛")
            || text.Contains("蓝") && text.Contains("瞳")
            || text.Contains("二次元")
            || text.Contains("口") && text.Contains("区")
            || text.Contains("好想")
            || text.Contains("绷")
            || text.ToLower().Contains("omc")
            )
        {
            SendDnkFun();
        }

    }

};
await listener.StartListen(CancellationToken.None);

Thread.CurrentThread.Join();

void SendDnkFun()
{
    var files = Directory.GetFiles("../../../dnkFuns/dnkBook", "*.*", SearchOption.AllDirectories).Where(x =>
        Path.GetFileName(x).EndsWith(".png") || Path.GetFileName(x).EndsWith(".jpg"));
    SendImage(files.PickOne());
}
void SendMessage(string s)
{
    qq.SendGroupMessageAsync(group, s).Wait();
}

void SendImage(string path)
{
    qq.SendGroupMessageAsync(group, SendingMessage.LocalImage(path, true)).Wait();
}