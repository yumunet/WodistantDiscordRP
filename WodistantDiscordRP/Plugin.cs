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

        public override MenuBarInfo ManuBarInfomation
        {
            get
            {
                return null;
            }
        }

        public override IKeyboardAction[] KeyboardActions => new IKeyboardAction[]
        {
        };
    }
}
