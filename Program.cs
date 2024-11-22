// See https://aka.ms/new-console-template for more information

using Sisters.WudiLib;

Console.WriteLine("Hello, World!");
var group = 954738468;
var qq = new HttpApiClient();
qq.ApiAddress = "http://192.168.0.56:5700/";

SendMessage("喵");

void SendMessage(string s)
{
    qq.SendGroupMessageAsync(group, s).Wait();
}