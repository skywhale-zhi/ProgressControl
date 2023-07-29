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
        public override Version Version => new Version(1, 0, 0, 1);

        public static Config config = new Config();

        /// <summary>
        /// 用来防止一开服就重置/启，用来缓冲的时间间隔，单位分钟
        /// </summary>
        private static int AvoidTime => 5;

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

        #region 线程
        private Thread thread_auto = new Thread(() =>
        {
            while (!Netplay.Disconnect)
            {
                try
                {
                    Thread.Sleep(1000);//每1秒检查一次是否需要重置和重启
                    Timer++;
                }
                catch { }
                //刚开服的 AvoidTime 分钟内不要自动重置和重启，手动指令存在则不用执行
                if (config.是否启用自动重置世界 && Timer >= 60 * AvoidTime && !countdownReset.enable)
                {
                    TimeSpan span = config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now;
                    int time = (int)span.TotalSeconds;
                    if (time >= 5 * 3600) //大于[5小时时，一小时一次广播
                    {
                        if (time % 3600 == 0)
                        {
                            TSPlayer.All.SendInfoMessage($"世界将于{HoursToM(span.TotalHours, "EA00FF")}后重置");
                            Console.WriteLine($"世界将于{HoursToM(span.TotalHours)}后重置");
                        }
                    }
                    else if (time >= 3600)// [1h ~ 5h)，30m一次广播
                    {
                        if (time % 1800 == 0)
                        {
                            TSPlayer.All.SendInfoMessage($"世界将于{HoursToM(span.TotalHours, "EA00FF")}后重置");
                            Console.WriteLine($"世界将于{HoursToM(span.TotalHours)}后重置");
                        }
                    }
                    else if (time >= 600)// [10m ~ 60m)，10m一次
                    {
                        if (time % 600 == 0)
                        {
                            TSPlayer.All.SendInfoMessage($"世界将于{HoursToM(span.TotalHours, "EA00FF")}后重置");
                            Console.WriteLine($"世界将于{HoursToM(span.TotalHours)}后重置");
                        }
                    }
                    else if (time >= 60)//[60s ~ 10m), 1m一次
                    {
                        if (time % 60 == 0)
                        {
                            TSPlayer.All.SendInfoMessage($"世界将于{HoursToM(span.TotalHours, "EA00FF")}后重置");
                            Console.WriteLine($"世界将于{HoursToM(span.TotalHours)}后重置");
                        }
                    }
                    else if (time >= 0)//[0 , 60)
                    {
                        TSPlayer.All.SendInfoMessage($"世界将于 [c/EA00FF:{span.Seconds}] 秒后重置");
                        Console.WriteLine($"世界将于 {span.Seconds} 秒后重置");
                    }
                    else
                    {
                        ResetGame();
                        break;
                    }
                }
                //刚开服的 AvoidTime 分钟内不要自动重置和重启，手动指令存在时不用执行
                if (config.是否启用自动重启服务器 && Timer >= 60 * AvoidTime && !countdownRestart.enable)
                {
                    TimeSpan span = config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now;
                    int time = (int)span.TotalSeconds;
                    if (time >= 3600 * 5)//[5h, +oo)
                    {
                        if (time % 3600 == 0)
                        {
                            TSPlayer.All.SendInfoMessage($"服务器将于{HoursToM(span.TotalHours, "FF9000")}后重启");
                            Console.WriteLine($"服务器将于{HoursToM(span.TotalHours)}后重启");
                        }
                    }
                    else if (time >= 3600)//[1h, 5h)
                    {
                        if (time % 1800 == 0)
                        {
                            TSPlayer.All.SendInfoMessage($"服务器将于{HoursToM(span.TotalHours, "FF9000")}后重启");
                            Console.WriteLine($"服务器将于{HoursToM(span.TotalHours)}后重启");
                        }
                    }
                    else if (time >= 600)//[10m, 1h)
                    {
                        if (time % 600 == 0)
                        {
                            TSPlayer.All.SendInfoMessage($"服务器将于{HoursToM(span.TotalHours, "FF9000")}后重启");
                            Console.WriteLine($"服务器将于{HoursToM(span.TotalHours)}后重启");
                        }
                    }
                    else if (time >= 60)//[1m, 10m)
                    {
                        if (time % 60 == 0)
                        {
                            TSPlayer.All.SendInfoMessage($"服务器将于{HoursToM(span.TotalHours, "FF9000")}后重启");
                            Console.WriteLine($"服务器将于{HoursToM(span.TotalHours)}后重启");
                        }
                    }
                    else if (time >= 0)//[0s, 1m)
                    {
                        TSPlayer.All.SendInfoMessage($"世界将于 [c/FF9000:{span.Seconds}] 秒后重启");
                        Console.WriteLine($"世界将于 {span.Seconds} 秒后重启");
                    }
                    else
                    {
                        RestartGame();
                        break;
                    }
                }

                if (config.是否启用自动执行指令 && !countdownCom.enable)
                {
                    if ((DateTime.Now - config.上次自动执行指令的日期).TotalHours >= config.多少小时后开始自动执行指令)
                    {
                        AutoCommands();
                    }
                }
            }
        });

        private Thread thread_reset = new Thread(() =>
        {
            while (countdownReset.time >= 0 && !Netplay.Disconnect && countdownReset.enable)
            {
                if (countdownReset.time >= 3600 * 5)//5h ~ 无穷，每隔1h发送广播
                {
                    if (countdownReset.time % 3600 == 0)
                    {
                        TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600, "EA00FF")}后重置");
                        Console.WriteLine($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600)}后重置");
                    }
                }
                else if (countdownReset.time >= 3600)//1h ~ 5h，每隔30m发送
                {
                    if (countdownReset.time % 1800 == 0)
                    {
                        TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600, "EA00FF")}后重置");
                        Console.WriteLine($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600)}后重置");
                    }
                }
                else if (countdownReset.time >= 600)// 10m ~ 1h，10m一次
                {
                    if (countdownReset.time % 600 == 0)
                    {
                        TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600, "EA00FF")}后重置");
                        Console.WriteLine($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600)}后重置");
                    }
                }
                else if (countdownReset.time >= 60)//1m ~ 10m，1m一次
                {
                    if (countdownReset.time % 60 == 0)
                    {
                        TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600, "EA00FF")}后重置");
                        Console.WriteLine($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600)}秒后重置");
                    }
                }
                else if (countdownReset.time >= 20)//20s ~ 60s，5s一次
                {
                    if (countdownReset.time % 5 == 0)
                    {
                        TSPlayer.All.SendInfoMessage($"服务器将在 [c/EA00FF:{countdownReset.time}] 秒后重置");
                        Console.WriteLine($"服务器将在 {countdownReset.time} 秒后重置");
                    }
                }
                else if (countdownReset.time >= 1)//1s ~ 20s ，一秒一次
                {
                    TSPlayer.All.SendInfoMessage($"服务器将在 [c/EA00FF:{countdownReset.time}] 秒后重置");
                    Console.WriteLine($"服务器将在 {countdownReset.time} 秒后重置");
                }
                else
                {
                    ResetGame();
                }
                countdownReset.time--;
                Thread.Sleep(1000);
            }
        });

        private Thread thread_reload = new Thread(() =>
        {
            while (countdownRestart.time >= 0 && !Netplay.Disconnect && countdownRestart.enable)
            {
                if (countdownRestart.time >= 3600 * 5)//大于5小时时
                {
                    if (countdownRestart.time % 3600 == 0)//每隔一小时发送自动化广播
                    {
                        TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600, "FF9000")}后重启");
                        Console.WriteLine($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600)}后重启");
                    }
                }
                else if (countdownRestart.time >= 3600)//大于1h小于5h
                {
                    if (countdownRestart.time % 1800 == 0)//每隔30m发送自动化广播
                    {
                        TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600, "FF9000")}后重启");
                        Console.WriteLine($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600)}后重启");
                    }
                }
                else if (countdownRestart.time >= 600)//10m ~ 1h
                {
                    if (countdownRestart.time % 600 == 0)//每隔十分钟发一次广播
                    {
                        TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600, "FF9000")}后重启");
                        Console.WriteLine($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600)}后重启");
                    }
                }
                else if (countdownRestart.time >= 60)//1m ~ 10m
                {
                    if (countdownRestart.time % 60 == 0)//每一分钟发一次广播
                    {
                        TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600, "FF9000")}后重启");
                        Console.WriteLine($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600)}后重启");
                    }
                }
                else if (countdownRestart.time >= 20)//20s ~ 60s
                {
                    if (countdownRestart.time % 5 == 0)//5秒发一次广播
                    {
                        TSPlayer.All.SendInfoMessage($"服务器将在 [c/FF9000:{countdownRestart.time}] 秒后重启");
                        Console.WriteLine($"服务器将在 {countdownRestart.time} 秒后重启");
                    }
                }
                else if (countdownRestart.time >= 1)//0~20秒内，每秒发一次
                {
                    TSPlayer.All.SendInfoMessage($"服务器将在 [c/FF9000:{countdownRestart.time}] 秒后重启");
                    Console.WriteLine($"服务器将在 {countdownRestart.time} 秒后重启");
                }
                else
                {
                    RestartGame();
                }
                countdownRestart.time--;
                Thread.Sleep(1000);
            }
        });
        #endregion

        public PControl(Main game) : base(game) { }

        public override void Initialize()
        {
            Timer = 0L;
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
                config.SaveConfigFile();
            }
            if ((DateTime.Now - config.上次重启服务器的日期).TotalHours >= config.多少小时后开始自动重启服务器 && config.是否启用自动重启服务器)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("自动重启已过期，现已将上次重启日期设定为现在，如果你不希望开启自动重启可以关闭，详情看ProgressControl.json配置文件（ProgressControl插件）");
                Console.ForegroundColor = ConsoleColor.Gray;
                config.上次重启服务器的日期 = DateTime.Now;
                config.SaveConfigFile();
            }
            if ((DateTime.Now - config.上次自动执行指令的日期).TotalHours >= config.多少小时后开始自动执行指令 && config.是否启用自动执行指令)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("自动执行指令已过期，现已将上次执行指令的日期设定为现在，如果你不希望开启自动执行指令可以关闭，详情看ProgressControl.json配置文件（ProgressControl插件）");
                Console.ForegroundColor = ConsoleColor.Gray;
                config.上次自动执行指令的日期 = DateTime.Now;
                config.SaveConfigFile();
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

            Commands.ChatCommands.Add(new Command("", Test, "t")
            {
                HelpText = "输入 /t"
            });

            thread_auto.Start();
        }

        private void Test(CommandArgs args)
        {
            Console.WriteLine(Main.worldName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    thread_auto.Interrupt();
                    thread_reload.Interrupt();
                    thread_reset.Interrupt();
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
                e.Player.SendInfoMessage($"自动重置世界倒计时过短，已关闭自动重置，请至少于开服后 {AvoidTime} 分钟内不要重置，避免产生错误");
                config.是否启用自动重置世界 = false;
            }
            if ((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours <= AvoidTime * 1.0 / 60.0 && config.是否启用自动重启服务器)
            {
                e.Player.SendInfoMessage($"自动重启服务器倒计时过短，已关闭自动重启，请至少于上次重启后 {AvoidTime} 分钟内不要重启，避免产生错误");
                config.是否启用自动重启服务器 = false;
            }
            if ((config.上次自动执行指令的日期.AddHours(config.多少小时后开始自动执行指令) - DateTime.Now).TotalHours <= AvoidTime * 1.0 / 60.0 && config.是否启用自动执行指令)
            {
                e.Player.SendInfoMessage($"自动执行指令倒计时过短，可能即将开始自动执行指令");
            }
            config.SaveConfigFile();
        }
    }
}