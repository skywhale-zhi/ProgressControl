using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;


namespace ProgressControl
{
    [ApiVersion(2, 1)]
    public partial class PControl : TerrariaPlugin
    {
        //重置色EA00FF
        //重启色FF9000
        //Boss倒计时色28FFB8
        //指令色00A8FF
        //金表17，铂金表709，秒表3099，杀怪计数器3095，食人魔勋章3868，计划书903
        public override string Author => "z枳";
        public override string Description => "计划书";
        public override string Name => "ProgressControl";
        public override Version Version => new Version(1, 0, 0, 0);

        public static Config config = new Config();

        /// <summary>
        /// 用来防止一开服就重置/启，用来缓冲的时间间隔，单位分钟
        /// </summary>
        private static int AvoidTime => 6;

        /// <summary>
        /// 手动计时器的状态类
        /// </summary>
        private class CountDown
        {
            /// <summary>
            /// 是否启用了手动计时器
            /// </summary>
            public bool enable = false;
            /// <summary>
            /// 单位秒
            /// </summary>
            public int time = -1;
        }

        /// <summary>
        /// 手动重启的类型
        /// </summary>
        private static CountDown countdownRestart = new CountDown();
        /// <summary>
        /// 手动重置的类型
        /// </summary>
        private static CountDown countdownReset = new CountDown();
        /// <summary>
        /// 手动指令计划的类型
        /// </summary>
        private static CountDown countdownCom = new CountDown();

        private static long Timer = 0L;

        private Thread thread = new Thread(() =>
        {
            try
            {   //刚开服的 AvoidTime 分钟内不要自动重置和重启
                Thread.Sleep(1000 * 60 * AvoidTime);
                Timer = 60L * AvoidTime;
            }
            catch { }
            while (!Netplay.Disconnect)
            {
                try
                {
                    Thread.Sleep(1000);//每1秒检查一次是否需要重置和重启
                    Timer++;
                }
                catch { }
                if (config.是否启用自动重置世界)
                {
                    TimeSpan span = config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now;

                    if (Timer % 3600L == 0L && !countdownReset.enable)
                        TSPlayer.All.SendInfoMessage($"世界将于{HoursToM(span.TotalHours, "EA00FF")}后重置");

                    if (span.TotalSeconds > 0 && span.TotalSeconds <= 60 && !countdownReset.enable)
                    {
                        TSPlayer.All.SendInfoMessage($"世界将于 [c/EA00FF:{span.Seconds}] 秒后重置");
                    }
                    if ((DateTime.Now - config.开服日期).TotalHours >= config.多少小时后开始自动重置世界 && !countdownReset.enable)
                    {
                        ResetGame();
                        break;
                    }
                }
                if (config.是否启用自动重启服务器)
                {
                    TimeSpan span = config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now;

                    if (Timer % 3600L == 0L && !countdownRestart.enable)
                        TSPlayer.All.SendInfoMessage($"服务器将于{HoursToM(span.TotalHours, "FF9000")}后重启");

                    if (span.TotalSeconds > 0 && span.TotalSeconds <= 60 && !countdownRestart.enable)
                    {
                        TSPlayer.All.SendInfoMessage($"服务器将于 [c/FF9000:{span.Seconds}] 秒后重启");
                    }
                    if ((DateTime.Now - config.上次重启服务器的日期).TotalHours >= config.多少小时后开始自动重启服务器 && !countdownRestart.enable)
                    {
                        RestartGame();
                        break;
                    }
                }
                if (config.是否启用自动执行指令)
                {
                    if ((DateTime.Now - config.上次自动执行指令的日期).TotalHours >= config.多少小时后开始自动执行指令 && !countdownCom.enable)
                    {
                        AutoCommands();
                    }
                }
            }
        });

        public PControl(Main game) : base(game) { }


        public override void Initialize()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            config = Config.LoadConfigFile();
            //重置时间在现在之前，那么取消重置
            if ((DateTime.Now - config.开服日期).TotalHours >= config.多少小时后开始自动重置世界 && config.是否启用自动重置世界)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("自动重置已过期，现已关闭自动重置并将开服日期设定为现在，详情看ProgressControl.json配置文件（ProgressControl插件）");
                Console.ForegroundColor = ConsoleColor.Gray;
                config.开服日期 = DateTime.Now;
                config.是否启用自动重置世界 = false;
                config.多少小时后开始自动重置世界 = -1;
                Config.SaveConfigFile(config);
            }
            if ((DateTime.Now - config.上次重启服务器的日期).TotalHours >= config.多少小时后开始自动重启服务器 && config.是否启用自动重启服务器)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("自动重启已过期，现已将上次重启日期设定为现在，如果你不希望开启自动重启可以关闭，详情看ProgressControl.json配置文件（ProgressControl插件）");
                Console.ForegroundColor = ConsoleColor.Gray;
                config.上次重启服务器的日期 = DateTime.Now;
                Config.SaveConfigFile(config);
            }
            if ((DateTime.Now - config.上次自动执行指令的日期).TotalHours >= config.多少小时后开始自动执行指令 && config.是否启用自动执行指令)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("自动执行指令已过期，现已将上次执行指令的日期设定为现在，如果你不希望开启自动执行指令可以关闭，详情看ProgressControl.json配置文件（ProgressControl插件）");
                Console.ForegroundColor = ConsoleColor.Gray;
                config.上次自动执行指令的日期 = DateTime.Now;
                Config.SaveConfigFile(config);
            }
            ServerApi.Hooks.NpcAIUpdate.Register(this, NPCAIUpdate);
            ServerApi.Hooks.NpcStrike.Register(this, NPCStrike);
            GeneralHooks.ReloadEvent += OnReload;

            Commands.ChatCommands.Add(new Command("pco.use", PCO, "pco")
            {
                HelpText = "输入 /pco help 来获取该插件的帮助"
            });
            Commands.ChatCommands.Add(new Command("pco.superadmin", SuperPCO, "supco")
            {
                HelpText = "输入 /supco help 来获取该插件的帮助"
            });

            thread.Start();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    thread.Interrupt();
                }
                catch { }
                ServerApi.Hooks.NpcAIUpdate.Deregister(this, NPCAIUpdate);
                ServerApi.Hooks.NpcStrike.Deregister(this, NPCStrike);
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }

        private void OnReload(ReloadEventArgs e)
        {
            config = Config.LoadConfigFile();
            if ((config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now).TotalHours <= AvoidTime * 1.0 / 60.0 && config.是否启用自动重置世界)
            {
                e.Player.SendInfoMessage($"自动重置世界倒计时过短，已关闭自动重置，请至少于开服后 {AvoidTime - 1} 分钟内不要重置，避免产生错误");
                config.是否启用自动重置世界 = false;
            }
            if ((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours <= AvoidTime * 1.0 / 60.0 && config.是否启用自动重启服务器)
            {
                e.Player.SendInfoMessage($"自动重启服务器倒计时过短，已关闭自动重启，请至少于上次重启后 {AvoidTime - 1} 分钟内不要重启，避免产生错误");
                config.是否启用自动重启服务器 = false;
            }
            if ((config.上次自动执行指令的日期.AddHours(config.多少小时后开始自动执行指令) - DateTime.Now).TotalHours <= AvoidTime * 1.0 / 60.0 && config.是否启用自动执行指令)
            {
                e.Player.SendInfoMessage($"自动执行指令倒计时过短，可能即将开始自动执行指令");
            }
            Config.SaveConfigFile(config);
        }
    }
}