using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;


namespace ProgressControl
{
    [ApiVersion(2, 1)]
    public partial class PControl : TerrariaPlugin
    {
        //xxx.foreach的return只会让他从=>{}的括号脱离，还是会接着循环，而foreach(var)则能从外部断开，彻底拜托外部函数
        //重置色EA00FF
        //重启色FF9000
        //NPC控制色28FFB8
        //指令色00A8FF
        //金表17，铂金表709，秒表3099，杀怪计数器3095，食人魔勋章3868，计划书903
        public override string Author => "z枳";
        public override string Description => "计划书";
        public override string Name => "ProgressControl";
        public override Version Version => new Version(1, 0, 0, 2);

        public static Config config = new Config();

        /// <summary>
        /// 用来防止一开服就重置/启，用来缓冲的时间间隔，单位分钟
        /// </summary>
        private static int AvoidTime => 5;
        //新权限
        internal readonly static string p_npc = "pco.npc";
        internal readonly static string p_com = "pco.com";
        internal readonly static string p_reload = "pco.reload";
        internal readonly static string p_reset = "pco.reset";
        internal readonly static string p_admin = "pco.admin";//p_admin最大，包括上面所有
        internal readonly static string tips1 = "配置<你提供用于重置的地图名称>可以写多个名字，如果该配置有填入的话在下次重置时系统会按照你提供的名称寻找地图(路径参考配置<地图存放目录_不填时默认原目录_注意请使用除号分隔目录>)，" +
            "若找不到则按名字生成一个地图，每次重置成功都会从该配置里删掉对应的名字，代表已使用过。" +
            "配置<你提供用于重置的地图名称>为空时则启用配置<自动重置后的地图名称>，只按名称生成新地图(不再调用已有的同名地图了)。" +
            "当存在同名地图则会后缀数字编号+1以区分，当<自动重置后的地图名称>为空时默认生成World，同样看情况后缀数字编号。请不要填入.wld后缀名。若你输入的名字带有空格，生成的地图文件名中的空格会被_代替(泰拉的特色)";

        /// <summary>
        /// 手动计划的状态类
        /// </summary>
        private class CountDown
        {
            /// <summary>
            /// 是否启用了手动计划
            /// </summary>
            public bool enable = false;
            /// <summary>
            /// 手动计划的计时器。单位秒
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
        /// <summary>
        /// 自动计划线程，包括自动重置，自动重启，自动执行指令
        /// </summary>
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
                //刚开服的 1 分钟里不要自动执行指令
                if (config.是否启用自动执行指令 && Timer >= 60 && !countdownCom.enable)
                {
                    if ((DateTime.Now - config.上次自动执行指令的日期).TotalHours >= config.多少小时后开始自动执行指令)
                    {
                        ActiveCommands();
                    }
                }
            }
        });
        /// <summary>
        /// 手动重置线程
        /// </summary>
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
                    break;
                }
                countdownReset.time--;
                Thread.Sleep(1000);
            }
        });
        /// <summary>
        /// 手动重启线程
        /// </summary>
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
                    break;
                }
                countdownRestart.time--;
                Thread.Sleep(1000);
            }
        });
        /// <summary>
        /// 手动指令线程
        /// </summary>
        private Thread thread_com = new Thread(() =>
        {
            while (countdownCom.time >= 0 && !Netplay.Disconnect && countdownCom.enable)
            {
                if (countdownCom.time <= 0)
                {
                    ActiveCommands();
                    countdownCom.enable = false;
                }
                countdownCom.time--;
                Thread.Sleep(1000);
            }
        });
        #endregion

        public PControl(Main game) : base(game) { }

        public override void Initialize()
        {
            Timer = 0L;
            config = Config.LoadConfigFile();
            ServerApi.Hooks.NpcAIUpdate.Register(this, NPCAIUpdate);
            ServerApi.Hooks.NpcStrike.Register(this, NPCStrike);
            ServerApi.Hooks.GamePostInitialize.Register(this, PostInit);
            GeneralHooks.ReloadEvent += OnReload;
            Commands.ChatCommands.Add(new Command("", PCO, "pco")
            {
                HelpText = "输入 /pco help 来获取该插件的帮助"
            });
            /*
            Commands.ChatCommands.Add(new Command("", Text, "t")
            {
                HelpText = "输入 /t"
            });
            */
        }
        /*
        private void Text(CommandArgs args)
        {
            args.Player.SendInfoMessage("1");
            config.自动执行的指令_不需要加斜杠.ForEach(x =>
            {
                args.Player.SendInfoMessage("3");
                return;
            });
            foreach(var cmd in config.自动执行的指令_不需要加斜杠)
            {
                args.Player.SendInfoMessage("2");
                return;
            }
            args.Player.SendInfoMessage("4");

        }
        */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {   //我觉得完全没必要
                try
                {
                    thread_auto.Interrupt();
                    thread_reload.Interrupt();
                    thread_reset.Interrupt();
                }
                catch { }
                ServerApi.Hooks.NpcAIUpdate.Deregister(this, NPCAIUpdate);
                ServerApi.Hooks.NpcStrike.Deregister(this, NPCStrike);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, PostInit);
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }


        private void OnReload(ReloadEventArgs e)
        {
            config = Config.LoadConfigFile();
            //对自动重置的加载判断下
            if ((config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now).TotalHours <= AvoidTime * 1.0 / 60.0 && config.是否启用自动重置世界)
            {
                e.Player.SendInfoMessage($"自动重置世界倒计时过短，已关闭自动重置，请至少于开服后 {AvoidTime} 分钟内不要重置，避免产生错误");
                config.是否启用自动重置世界 = false;
            }
            //对重启的判断下
            if ((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours <= AvoidTime * 1.0 / 60.0 && config.是否启用自动重启服务器)
            {
                e.Player.SendInfoMessage($"自动重启服务器倒计时过短，已关闭自动重启，请至少于上次重启后 {AvoidTime} 分钟内不要重启，避免产生错误");
                config.是否启用自动重启服务器 = false;
            }
            //对自动执行指令的警告下
            if ((config.上次自动执行指令的日期.AddHours(config.多少小时后开始自动执行指令) - DateTime.Now).TotalSeconds <= 60.0 && config.是否启用自动执行指令)
            {
                e.Player.SendInfoMessage($"自动执行指令倒计时过短，可能即将开始自动执行指令");
            }
            //对自动指令的情况过滤下
            HashSet<string> com_temp = new HashSet<string>();
            config.自动执行的指令_不需要加斜杠.ForEach(x =>
            {
                string t = CorrectCommand(x);
                if(t != x && !string.IsNullOrWhiteSpace(t))
                {
                    e.Player.SendInfoMessage($"你在配置文件中提供自动执行的指令：[/{x}] 含有多余斜杠和空格，已转化为等价指令：[/{t}]");
                }
                if (!string.IsNullOrWhiteSpace(t))
                    com_temp.Add(t);
                else
                    e.Player.SendInfoMessage($"你在配置文件中提供了一个空白指令，已删除");
            });
            config.自动执行的指令_不需要加斜杠 = com_temp;
            //对预设重置地图名字的修正
            HashSet<string> world_name = new HashSet<string>();
            config.你提供用于重置的地图名称.ForEach(x =>
            {
                string tempname = CorrectFileName(x);
                world_name.Add(tempname);
                if (tempname != x)
                    e.Player.SendInfoMessage($"你在配置文件中提供用于重置的地图名称：[{x}] 含有一些不规则的字符或不必要的后缀，已过滤：[{tempname}]");
            });
            config.你提供用于重置的地图名称 = world_name;
            //对预设重置地图名字和这次开服的名字相同警告下
            string world_name_temp = config.你提供用于重置的地图名称.Count > 0 ? config.你提供用于重置的地图名称.First() : "";
            if (world_name_temp == Main.worldName && (config.是否启用自动重置世界 || countdownReset.enable) && !config.自动重置前是否删除地图)
                e.Player.SendInfoMessage("警告：你当前在配置文件中提供的第一个用于重置的地图名称和当前地图名称相同，这会导致下次重置直接调用本次地图");
            //注释不要变
            config.上面两条的注释 = tips1;
            config.SaveConfigFile();
        }
    }
}