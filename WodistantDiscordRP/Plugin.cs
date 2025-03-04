using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscordRPC;
using Windows.Win32;
using Windows.Win32.Foundation;
using Wodistant.PluginLibrary.Hook.Keyboard;
using Wodistant.PluginLibrary.MenuBar;

namespace WodistantDiscordRP
{
    public class Plugin : Wodistant.PluginLibrary.PluginBase
    {
        public override string Name => "Discord Rich Presence";

        public override string Author => "yumu";

        public override string Version => "1.0.0";

        public override string Description => "Discordクライアントと連携して、アクティビティステータス（Rich Presence）を表示させます。";

        public override MenuBarInfo ManuBarInfomation => null;

        public override IKeyboardAction[] KeyboardActions => new IKeyboardAction[] { };

        private bool isConnected = false;

        private DiscordRpcClient client;

        public override void OnInitializePlugin()
        {
            Task.Run(() => Observer());
        }

        public override void OnFinalizePlugin()
        {
            client?.Dispose();
        }

        private void Observer()
        {
            while (true)
            {
                if (Host.Environment.IsWoditorConnected)
                {
                    if (!isConnected)
                    {
                        isConnected = true;
                        OnConnected();
                    }
                }
                else
                {
                    if (isConnected)
                    {
                        isConnected = false;
                        OnUnconnected();
                    }
                }
                Thread.Sleep(1);
            }
        }

        private void OnConnected()
        {
            bool isPro = IsWoditorPro();
            client = new DiscordRpcClient("1346144187271221279");
            client.Initialize();
            client.SetPresence(new RichPresence()
            {
                Details = GetGameName(),
                Assets = new Assets()
                {
                    LargeImageKey = isPro ? "editor_pro" : "editor",
                    LargeImageText = "WOLF RPG Editor" + (isPro ? " PRO" : "")
                },
                Timestamps = Timestamps.Now
            });
        }

        private void OnUnconnected()
        {
            client.Dispose();
        }

        private string GetGameName()
        {
            string gameDatPath = Host.Environment.ConnectedWoditorDirectory + "\\Data\\BasicData\\Game.dat";
            try
            {
                using var stream = new FileStream(gameDatPath, FileMode.Open, FileAccess.Read);
                using var reader = new BinaryReader(stream);

                const int headerSize = 10;
                stream.Seek(headerSize, SeekOrigin.Begin);
                int firstSize = reader.ReadInt32();
                stream.Seek(firstSize + 4, SeekOrigin.Current);

                int size = reader.ReadInt32();
                byte[] bytes = reader.ReadBytes(size - 1); // 末尾のnull文字は含めない
                string name = Encoding.GetEncoding("UTF-8").GetString(bytes);
                return name;
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine(e);
            }
            catch (DirectoryNotFoundException e)
            {
                Debug.WriteLine(e);
            }
            catch (IOException e)
            {
                Debug.WriteLine(e);
            }
            return null;
        }

        private bool IsWoditorPro()
        {
            var hMainWnd = (HWND)Host.MapEditor.GetMapEditorWindowHandle();
            int length = PInvoke.GetWindowTextLength(hMainWnd) + 1;
            string windowTitle;
            unsafe
            {
                fixed (char* chars = new char[length])
                {
                    PInvoke.GetWindowText(hMainWnd, chars, length);
                    windowTitle = new string(chars);
                }
            }
            return windowTitle.Contains("WOLF RPGエディター PRO");
        }
    }
}
