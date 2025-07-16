using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscordRPC;
using DiscordRPC.Message;
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

        private Timer triggerTimer;
        private bool isWoditorConnected = false;
        private Timestamps timestampsAtConneciton;
        private DiscordRpcClient client;

        public override void OnInitializePlugin()
        {
            triggerTimer = new Timer(Trigger, null, 0, 100);
        }

        public override void OnFinalizePlugin()
        {
            triggerTimer.Dispose();
            Disconnect();
        }

        private void Trigger(object state)
        {
            if (Host.Environment.IsWoditorConnected)
            {
                if (!isWoditorConnected)
                {
                    isWoditorConnected = true;
                    timestampsAtConneciton = Timestamps.Now;
                    Connect();
                }
            }
            else
            {
                if (isWoditorConnected)
                {
                    isWoditorConnected = false;
                    Disconnect();
                }
            }
        }

        private void Connect()
        {
            client = new DiscordRpcClient("1346144187271221279");
            client.OnPresenceUpdate += OnPresenceUpdate;
            client.OnConnectionFailed += OnConnectionFailed;
            client.OnReady += OnReady;
            client.Initialize();
        }

        private void Disconnect()
        {
            client?.Dispose();
        }

        private void OnPresenceUpdate(object sender, PresenceMessage args)
        {
            Debug.WriteLine($"{args.Type}: {args.Name} {args.ApplicationID}");
        }

        private async void OnConnectionFailed(object sender, ConnectionFailedMessage args)
        {
            // Discordクライアントが起動していなかったか終了した場合は、5秒後に再試行
            Debug.WriteLine($"{args.Type}: {args.FailedPipe}");

            client.Dispose(); // 重複実行されないためにも、まず破棄
            await Task.Delay(5000);

            if (Host.Environment.IsWoditorConnected)
            {
                Connect();
            }
        }

        private void OnReady(object sender, ReadyMessage args)
        {
            // Discordクライアントへ接続後に、Presenceをセット
            Debug.WriteLine($"{args.Type}: {args.User} {args.Version}");

            bool isPro = Host.Environment.IsWoditorProEdition;
            client.SetPresence(new RichPresence()
            {
                Details = GetGameName(),
                Assets = new Assets()
                {
                    LargeImageKey = isPro ? "editor_pro" : "editor",
                    LargeImageText = isPro ? "WOLF RPG Editor Pro" : ""
                },
                Timestamps = timestampsAtConneciton
            });
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
    }
}
