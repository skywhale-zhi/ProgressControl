using Microsoft.Xna.Framework;
using System.Diagnostics;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;

namespace ProgressControl
{
    public partial class PControl : TerrariaPlugin
    {
        /// <summary>
        /// 重置地图的指令，超管
        /// </summary>
        /// <param name="args"></param>
        private void SuperPCO(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendInfoMessage("输入 /supco help 来获取该插件的帮助");
            }
            else if (args.Parameters.Count == 1)
            {
                if (args.Parameters[0].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    args.Player.SendMessage(
                        "输入 /supco autoreset   自动重置世界计划启用，再次使用关闭\n" +
                        "输入 /supco offset autoreset [±num]   将自动重置世界的时间推迟或提前num小时，num可为小数\n" +
                        "输入 /supco autoreload   自动重启服务器计划启用，再次使用关闭\n" +
                        "输入 /supco offset autoreload [±num]   将自动重启服务器的时间推迟或提前num小时，num可为小数\n" +

                        "输入 /supco name [string]   来设置下次重置地图时的地图名字\n" +
                        "输入 /supco size [小1/中2/大3 (只能填数字)]   来设置下次重置时地图的大小\n" +
                        "输入 /supco mode [普通0/专家1/大师2/旅途3(只能填数字)]   来设置下次重置地图时的模式\n" +
                        "输入 /supco seed [string]   来设置下次重置地图时的地图种子\n" +
                        "输入 /supco maxplayers [num]   来设置下次重置地图时的最多在线玩家\n" +
                        "输入 /supco resetplayers [0/1]   来设置下次重置地图时是否清理玩家数据，0代表不清理\n" +
                        "输入 /supco port [num]   来设置下次重置地图时的端口\n" +

                        "输入 /supco view   来查看当前服务器的自动化计划\n" +

                        "输入 /supco reset [±num]   手动重置世界计划启用，在num秒后开始重置，若num不填则立刻重置，num小于0则关闭当前存在的手动计划，其优先级大于自动重置\n" +
                        "输入 /supco reload [±num]   手动重启服务器计划启用，在num秒后开始重启，若num不填则立刻重启，num小于0则关闭当前存在的手动计划，其优先级大于自动重启\n" +
                        "输入 /supco stop [r/d/c/all或不填]  来分别终止reset/reload/com的全部手动计划，填all或不填时终止全部，自动计划不受影响", TextColor());
                }
                else if (args.Parameters[0].Equals("autoreset", StringComparison.OrdinalIgnoreCase))
                {
                    if (config.是否启用自动重置世界)
                    {
                        if (!args.Player.IsLoggedIn)
                            args.Player.SendSuccessMessage("自动重置计划已关闭");
                        TSPlayer.All.SendSuccessMessage("自动重置计划已关闭");
                        config.是否启用自动重置世界 = !config.是否启用自动重置世界;
                        Config.SaveConfigFile(config);
                    }
                    else
                    {
                        if (config.开服日期.AddHours(config.多少小时后开始自动重置世界) <= DateTime.Now.AddMinutes(AvoidTime))
                        {
                            args.Player.SendInfoMessage($"自动重置世界倒计时过短，已关闭自动重置，请至少于开服后 {AvoidTime - 1} 分钟内不要重置，避免产生错误");
                        }
                        else
                        {
                            if (!args.Player.IsLoggedIn)
                                args.Player.SendSuccessMessage($"自动重置计划已启用，将在从现在起{HoursToM((config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now).TotalHours)}后自动重置");
                            TSPlayer.All.SendSuccessMessage($"自动重置计划已启用，将在从现在起{HoursToM((config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now).TotalHours, "EA00FF")}后自动重置");
                            config.是否启用自动重置世界 = !config.是否启用自动重置世界;
                            Config.SaveConfigFile(config);
                        }
                    }
                }
                else if (args.Parameters[0].Equals("autoreload", StringComparison.OrdinalIgnoreCase))
                {
                    if (config.是否启用自动重启服务器)
                    {
                        if (!args.Player.IsLoggedIn)
                            args.Player.SendSuccessMessage("自动重启计划已关闭");
                        TSPlayer.All.SendSuccessMessage("自动重启计划已关闭");
                        config.是否启用自动重启服务器 = !config.是否启用自动重启服务器;
                        Config.SaveConfigFile(config);
                    }
                    else
                    {
                        if (config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) <= DateTime.Now.AddMinutes(AvoidTime))
                        {
                            args.Player.SendInfoMessage($"自动重启服务器倒计时过短，已关闭自动重启，请至少于上次重启后 {AvoidTime - 1} 分钟内不要重启，避免产生错误");
                        }
                        else
                        {
                            if (!args.Player.IsLoggedIn)
                                args.Player.SendSuccessMessage($"自动重启计划已启用，将在从现在起{HoursToM((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours)}后自动重启");
                            TSPlayer.All.SendSuccessMessage($"自动重启计划已启用，将在从现在起{HoursToM((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours, "FF9000")}后自动重启");
                            config.是否启用自动重启服务器 = !config.是否启用自动重启服务器;
                            Config.SaveConfigFile(config);
                        }
                    }
                }
                else if (args.Parameters[0].Equals("view", StringComparison.OrdinalIgnoreCase))
                {
                    //boss进度数据输出
                    string mess = "";
                    if (config.是否自动控制Boss进度)
                    {
                        double lv = (DateTime.Now - config.开服日期).TotalHours;
                        Dictionary<string, double> keyValuePairs = new Dictionary<string, double>();
                        Dictionary<string, double> sortpairs = new Dictionary<string, double>();
                        foreach (var v in config.Boss封禁时长距开服日期_单位小时)
                        {
                            keyValuePairs.Add(v.Key, v.Value);
                        }
                        //排序
                        while (keyValuePairs.Count > 0)
                        {
                            double min = double.MaxValue;
                            string key = "";
                            foreach (var v in keyValuePairs)
                            {
                                if (v.Value < min)
                                {
                                    key = v.Key;
                                    min = v.Value;
                                }
                            }
                            if (key != "")
                            {
                                sortpairs.Add(key, min);
                                keyValuePairs.Remove(key);
                            }
                        }
                        mess += args.Player.IsLoggedIn ? "" + "[i:3868]已锁Boss倒计时：\n" : "@已锁Boss倒计时：\n";
                        int count = 0;
                        //把排好序的数据输出
                        foreach (var v in sortpairs)
                        {
                            if (v.Value >= lv)
                            {
                                count++;
                                if (args.Player.IsLoggedIn)
                                    mess += $"[{v.Key}{HoursToM(v.Value - lv, "28FFB8")}]  ";
                                else
                                    mess += $"[{v.Key}{HoursToM(v.Value - lv)}]  ";
                                if (count == 6)
                                {
                                    mess += "\n";
                                    count = 0;
                                }
                            }
                        }
                    }
                    else
                        mess += args.Player.IsLoggedIn ? "[i:3868]" + MtoM("没有任何Boss进度控制计划", "28FFB8") : "@没有任何Boss进度控制计划";
                    mess = mess.Trim('\n');
                    //3个自动化设置的信息
                    string clock1, clock2, clock3;
                    if (!config.是否启用自动重置世界 && !countdownReset.enable)
                        clock1 = args.Player.IsLoggedIn ? "[i:3099]" + MtoM("没有任何重置计划", "EA00FF") : "@没有任何重置计划";
                    else if (config.是否启用自动重置世界 && !countdownReset.enable)
                        clock1 = $"{(args.Player.IsLoggedIn ? "[i:3099]" : "@")}世界将在{HoursToM((config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now).TotalHours, (args.Player.IsLoggedIn ? "EA00FF" : ""))}后开始自动重置";
                    else
                        clock1 = $"{(args.Player.IsLoggedIn ? "[i:3099]" : "@")}世界将在{HoursToM(countdownReset.time * 1.0 / 3600, (args.Player.IsLoggedIn ? "EA00FF" : ""))}后开始手动重置";

                    if (!config.是否启用自动重启服务器 && !countdownRestart.enable)
                        clock2 = args.Player.IsLoggedIn ? "[i:17]" + MtoM("没有任何重启计划", "FF9000") : "@没有任何重启计划";
                    else if (config.是否启用自动重启服务器 && !countdownRestart.enable)
                        clock2 = $"{(args.Player.IsLoggedIn ? "[i:17]" : "@")}服务器将在{HoursToM((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours, (args.Player.IsLoggedIn ? "FF9000" : ""))}后开始自动重启";
                    else
                        clock2 = $"{(args.Player.IsLoggedIn ? "[i:17]" : "@")}服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600, (args.Player.IsLoggedIn ? "FF9000" : ""))}后开始手动重启";

                    if (!config.是否启用自动执行指令 && !countdownCom.enable)
                        clock3 = args.Player.IsLoggedIn ? "[i:903]" + MtoM("没有任何指令计划", "00A8FF") : "@没有任何指令计划";
                    else if (config.是否启用自动执行指令 && !countdownCom.enable)
                        clock3 = $"{(args.Player.IsLoggedIn ? "[i:903]" : "@")}服务器将在{HoursToM((config.上次自动执行指令的日期.AddHours(config.多少小时后开始自动执行指令) - DateTime.Now).TotalHours, (args.Player.IsLoggedIn ? "00A8FF" : ""))}后开始自动执行指令";
                    else
                        clock3 = $"{(args.Player.IsLoggedIn ? "[i:903]" : "@")}服务器将在{HoursToM(countdownCom.time * 1.0 / 3600, (args.Player.IsLoggedIn ? "00A8FF" : ""))}后开始手动执行指令";
                    string mess3 = "";
                    foreach (string v in config.自动执行的指令_不需要加斜杠)
                    {
                        if (!string.IsNullOrWhiteSpace(v))
                            mess3 += "/" + v + ", ";
                    }
                    mess3 = mess3.Trim();
                    mess3 = mess3.Trim(',');
                    string size;
                    if (config.自动重置的地图大小_小1_中2_大3 == 1)
                        size = "小";
                    else if (config.自动重置的地图大小_小1_中2_大3 == 2)
                        size = "中";
                    else if (config.自动重置的地图大小_小1_中2_大3 == 3)
                        size = "大";
                    else
                        size = "错误，请检查数据填写是否有误";
                    string mode;
                    if (config.自动重置的地图难度_普通0_专家1_大师2_旅途3 == 0)
                        mode = "普通";
                    else if (config.自动重置的地图难度_普通0_专家1_大师2_旅途3 == 1)
                        mode = "专家";
                    else if (config.自动重置的地图难度_普通0_专家1_大师2_旅途3 == 2)
                        mode = "大师";
                    else if (config.自动重置的地图难度_普通0_专家1_大师2_旅途3 == 3)
                        mode = "旅途";
                    else
                        mode = "错误，请检查数据填写是否有误";

                    args.Player.SendInfoMessage(
                        $"{mess}\n{clock1}\n" +
                        $"重置后的名称：{(string.IsNullOrWhiteSpace(config.自动重置的地图名称) ? "未设置，默认为World" : config.自动重置的地图名称)}，重置后的地图大小：{size}，重置后世界的模式：{mode}\n" +
                        $"重置后的种子：{(string.IsNullOrWhiteSpace(config.自动重置的地图种子) ? "随机" : config.自动重置的地图种子)}，重置后的自动重置的最多在线人数：{config.自动重置的最多在线人数}，重置后的端口：{config.自动重置的端口}\n" +
                        $"是否自动重置是否重置玩家数据：{config.自动重置是否重置玩家数据}\n" +
                        $"{clock2}\n" +
                        $"重启后的名称：{(string.IsNullOrWhiteSpace(Main.worldName) ? "未设置" : Main.worldName)}，重启后的自动重置的最多在线人数：{TShock.Config.Settings.MaxSlots}，重启后的端口：{Netplay.ListenPort}\n" +
                        $"重启后的服务器密码：以配置文件config.json的为准\n" +
                        $"{clock3}\n" +
                        $"要执行的指令：{mess3}");
                }
                else if (args.Parameters[0].Equals("reset", StringComparison.OrdinalIgnoreCase))
                {
                    ResetGame();
                }
                else if (args.Parameters[0].Equals("reload", StringComparison.OrdinalIgnoreCase))
                {
                    RestartGame();
                }
                else if (args.Parameters[0].Equals("seed", StringComparison.OrdinalIgnoreCase))
                {
                    config.自动重置的地图种子 = "";
                    Config.SaveConfigFile(config);
                    args.Player.SendInfoMessage("已将地图种子设为随机");
                }
                else if (args.Parameters[0].Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    countdownReset.enable = false;
                    countdownRestart.enable = false;
                    countdownCom.enable = false;
                    countdownReset.time = -1;
                    countdownRestart.time = -1;
                    countdownCom.time = -1;
                    if (!args.Player.IsLoggedIn)
                        args.Player.SendSuccessMessage("已将全部手动计划关闭");
                    TSPlayer.All.SendSuccessMessage("已将全部手动计划关闭");
                }
                else
                    args.Player.SendInfoMessage("输入 /supco help 来获取该插件的帮助");
            }
            else if (args.Parameters.Count == 2)
            {
                if (args.Parameters[0].Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    string worldname = new string(args.Parameters[1]);
                    //移除不合法的字符
                    for (int i = 0; i < worldname.Length; ++i)
                    {
                        bool flag = worldname[i] == '\\' || worldname[i] == '/' || worldname[i] == ':' || worldname[i] == '*' || worldname[i] == '?' || worldname[i] == '"' || worldname[i] == '<' || worldname[i] == '>' || worldname[i] == '|';
                        if (flag)
                        {
                            worldname = worldname.Remove(i, 1);
                            i--;
                        }
                    }
                    config.自动重置的地图名称 = worldname;
                    Config.SaveConfigFile(config);
                    args.Player.SendSuccessMessage($"下次重置的地图名称：{worldname}");
                }
                else if (args.Parameters[0].Equals("size", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(args.Parameters[1], out int num) && (num == 1 || num == 2 || num == 3))
                    {
                        if (num == 1)
                            args.Player.SendSuccessMessage("下次重置地图的大小成功修改为：小");
                        else if (num == 2)
                            args.Player.SendSuccessMessage("下次重置地图的大小成功修改为：中");
                        else
                            args.Player.SendSuccessMessage("下次重置地图的大小成功修改为：大");
                        config.自动重置的地图大小_小1_中2_大3 = num;
                        Config.SaveConfigFile(config);
                    }
                    else
                        args.Player.SendInfoMessage("请输入数字1,2或3");
                }
                else if (args.Parameters[0].Equals("mode", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(args.Parameters[1], out int num) && (num == 0 || num == 1 || num == 2 || num == 3))
                    {
                        if (num == 0)
                            args.Player.SendSuccessMessage("下次重置地图的模式成功修改为：普通");
                        else if (num == 1)
                            args.Player.SendSuccessMessage("下次重置地图的模式成功修改为：专家");
                        else if (num == 2)
                            args.Player.SendSuccessMessage("下次重置地图的模式成功修改为：大师");
                        else
                            args.Player.SendSuccessMessage("下次重置地图的模式成功修改为：旅途");
                        config.自动重置的地图难度_普通0_专家1_大师2_旅途3 = num;
                        Config.SaveConfigFile(config);
                    }
                    else
                        args.Player.SendInfoMessage("请输入数字0,1,2或3");
                }
                else if (args.Parameters[0].Equals("seed", StringComparison.OrdinalIgnoreCase))
                {
                    args.Player.SendSuccessMessage("下次重置地图的种子成功修改为：" + args.Parameters[1]);
                    config.自动重置的地图种子 = args.Parameters[1];
                    Config.SaveConfigFile(config);
                }
                else if (args.Parameters[0].Equals("maxplayers", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(args.Parameters[1], out int num) && num > 0 && num < 200)
                    {
                        args.Player.SendSuccessMessage("下次重置地图的玩家上限成功修改为：" + num);
                        config.自动重置的最多在线人数 = num;
                        Config.SaveConfigFile(config);
                    }
                    else
                        args.Player.SendInfoMessage("请输入整数，不要输入负数，数字不要过大");
                }
                else if (args.Parameters[0].Equals("resetplayers", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(args.Parameters[1], out int num) && (num == 0 || num == 1))
                    {
                        if (num == 0)
                        {
                            args.Player.SendSuccessMessage("下次重置地图不会自动重置是否重置玩家数据");
                            config.自动重置是否重置玩家数据 = false;
                        }
                        else
                        {
                            args.Player.SendSuccessMessage("下次重置地图会自动重置是否重置玩家数据");
                            config.自动重置是否重置玩家数据 = true;
                        }
                        Config.SaveConfigFile(config);
                    }
                    else
                        args.Player.SendInfoMessage("请输入数字0或1");
                }
                else if (args.Parameters[0].Equals("port", StringComparison.OrdinalIgnoreCase))
                {
                    config.自动重置的端口 = args.Parameters[1];
                    Config.SaveConfigFile(config);
                    args.Player.SendSuccessMessage("下次重置地图时的端口为：" + args.Parameters[1]);
                }
                else if (args.Parameters[0].Equals("reset", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(args.Parameters[1], out int num))
                    {
                        if (num >= 0)
                        {
                            if (config.是否启用自动重置世界 && config.开服日期.AddHours(config.多少小时后开始自动重置世界) < DateTime.Now.AddSeconds(num))
                            {
                                args.Player.SendErrorMessage("警告：手动重置计划倒计时时间太长，已超过自动重置计划时间，如果突然使用stop指令中断，自动重置可能立刻开始生效，不建议这么做");
                            }
                            if (countdownRestart.enable && countdownRestart.time < num)
                            {
                                args.Player.SendErrorMessage("警告：当前存在手动重启计划，且时间小于你输入的数值，你的手动重置计划将不会生效！");
                            }
                            if (!countdownRestart.enable && config.是否启用自动重启服务器 && config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) < DateTime.Now.AddSeconds(num))
                            {
                                args.Player.SendErrorMessage("警告：当前存在自动重启计划，且时间小于你输入的数值，你的手动重置计划将不会生效！");
                            }

                            if (countdownReset.enable)
                            {
                                countdownReset.time = num;
                                TSPlayer.All.SendInfoMessage($"手动重置计划已重新开始，服务器将在{HoursToM(num * 1.0 / 3600, "EA00FF")}后重置");
                                if (!args.Player.IsLoggedIn)
                                    args.Player.SendInfoMessage($"手动重置计划已重新开始，服务器将在{HoursToM(num * 1.0 / 3600)}后重置");
                            }
                            else
                            {
                                TSPlayer.All.SendInfoMessage($"手动重置计划开启，服务器将在{HoursToM(num * 1.0 / 3600, "EA00FF")}后重置");
                                if (!args.Player.IsLoggedIn)
                                    args.Player.SendInfoMessage($"手动重置计划开启，服务器将在{HoursToM(num * 1.0 / 3600)}后重置");
                                countdownReset.enable = true;
                                countdownReset.time = num;
                                Thread thread = new Thread(() =>
                                {
                                    while (countdownReset.time >= 0 && !Netplay.Disconnect && countdownReset.enable)
                                    {
                                        if (countdownReset.time >= 3600)
                                        {
                                            if (countdownReset.time % 3600 == 0)
                                            {
                                                TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600, "EA00FF")}后重置");
                                                Console.WriteLine($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600)}后重置");
                                            }
                                        }
                                        else if (countdownReset.time >= 600)
                                        {
                                            if (countdownReset.time % 600 == 0)
                                            {
                                                TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600, "EA00FF")}后重置");
                                                Console.WriteLine($"服务器将在{HoursToM(countdownReset.time * 1.0 / 3600)}后重置");
                                            }
                                        }
                                        else if (countdownReset.time >= 60)
                                        {
                                            if (countdownReset.time % 60 == 0)
                                            {
                                                TSPlayer.All.SendInfoMessage($"服务器将在 [c/EA00FF:{countdownReset.time}] 秒后重置");
                                                Console.WriteLine($"服务器将在 {countdownReset.time} 秒后重置");
                                            }
                                        }
                                        else if (countdownReset.time >= 20)
                                        {
                                            if (countdownReset.time % 5 == 0)
                                            {
                                                TSPlayer.All.SendInfoMessage($"服务器将在 [c/EA00FF:{countdownReset.time}] 秒后重置");
                                                Console.WriteLine($"服务器将在 {countdownReset.time} 秒后重置");
                                            }
                                        }
                                        else if (countdownReset.time >= 1)
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
                                thread.Start();
                            }
                        }
                        else
                        {
                            if (countdownReset.enable)
                            {
                                countdownReset.enable = false;
                                countdownReset.time = -1;
                                if (!args.Player.IsLoggedIn)
                                {
                                    args.Player.SendSuccessMessage("手动重置计划已关闭");
                                }
                                TSPlayer.All.SendSuccessMessage("手动重置计划已关闭");
                            }
                            else
                            {
                                if (!args.Player.IsLoggedIn)
                                {
                                    args.Player.SendSuccessMessage("未开启手动重置计划");
                                }
                                TSPlayer.All.SendSuccessMessage("未开启手动重置计划");
                            }
                        }
                    }
                    else
                        args.Player.SendInfoMessage("请输入整数");
                }
                else if (args.Parameters[0].Equals("reload", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(args.Parameters[1], out int num))
                    {
                        if (num >= 0)
                        {
                            if (config.是否启用自动重启服务器 && config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) < DateTime.Now.AddSeconds(num))
                            {
                                args.Player.SendErrorMessage("警告：手动重启计划倒计时时间太长，已超过自动重启计划时间，如果突然使用stop指令中断，自动重启可能立刻开始生效，不建议这么做");
                            }
                            if (countdownReset.enable && countdownReset.time < num)
                            {
                                args.Player.SendErrorMessage("警告：当前存在手动重置计划，且时间小于你输入的数值，你的手动重启计划将不会生效！");
                            }
                            if (!countdownReset.enable && config.是否启用自动重置世界 && config.开服日期.AddHours(config.多少小时后开始自动重置世界) < DateTime.Now.AddSeconds(num))
                            {
                                args.Player.SendErrorMessage("警告：当前存在自动重置计划，且时间小于你输入的数值，你的手动重启计划将不会生效！");
                            }

                            if (countdownRestart.enable)
                            {
                                countdownRestart.time = num;
                                TSPlayer.All.SendInfoMessage($"手动重启计划已重新开始，服务器将在{HoursToM(num * 1.0 / 3600, "FF9000")}后重启");
                                if (!args.Player.IsLoggedIn)
                                    args.Player.SendInfoMessage($"手动重启计划已重新开始，服务器将在{HoursToM(num * 1.0 / 3600)}后重启");
                            }
                            else
                            {
                                TSPlayer.All.SendInfoMessage($"手动重启计划开启，服务器将在{HoursToM(num * 1.0 / 3600, "FF9000")}后重启");
                                if (!args.Player.IsLoggedIn)
                                    args.Player.SendInfoMessage($"手动重启计划开启，服务器将在{HoursToM(num * 1.0 / 3600)}后重启");
                                countdownRestart.enable = true;
                                countdownRestart.time = num;
                                Thread thread = new Thread(() =>
                                {
                                    while (countdownRestart.time >= 0 && !Netplay.Disconnect && countdownRestart.enable)
                                    {
                                        if (countdownRestart.time >= 3600)//大于一小时时
                                        {
                                            if (countdownRestart.time % 3600 == 0)//每隔一小时发送自动化广播
                                            {
                                                TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600, "FF9000")}后重启");
                                                Console.WriteLine($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600)}后重启");
                                            }
                                        }
                                        else if (countdownRestart.time >= 600)//大于十分钟时
                                        {
                                            if (countdownRestart.time % 600 == 0)//每隔十分钟发一次广播
                                            {
                                                TSPlayer.All.SendInfoMessage($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600, "FF9000")}后重启");
                                                Console.WriteLine($"服务器将在{HoursToM(countdownRestart.time * 1.0 / 3600)}后重启");
                                            }
                                        }
                                        else if (countdownRestart.time >= 60)//大于1分钟时
                                        {
                                            if (countdownRestart.time % 60 == 0)//每一分钟发一次广播
                                            {
                                                TSPlayer.All.SendInfoMessage($"服务器将在 [c/FF9000:{countdownRestart.time}] 秒后重启");
                                                Console.WriteLine($"服务器将在 {countdownRestart.time} 秒后重启");
                                            }
                                        }
                                        else if (countdownRestart.time >= 20)//在20秒到60秒
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
                                thread.Start();
                            }
                        }
                        else
                        {
                            if (countdownRestart.enable)
                            {
                                countdownRestart.enable = false;
                                countdownRestart.time = -1;
                                if (!args.Player.IsLoggedIn)
                                    args.Player.SendSuccessMessage("手动重启计划已关闭");
                                TSPlayer.All.SendSuccessMessage("手动重启计划已关闭");
                            }
                            else
                            {
                                if (!args.Player.IsLoggedIn)
                                    args.Player.SendSuccessMessage("未开启手动重启计划");
                                TSPlayer.All.SendSuccessMessage("未开启手动重启计划");
                            }
                        }
                    }
                    else
                        args.Player.SendInfoMessage("请输入整数");
                }
                else if (args.Parameters[0].Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Parameters[1].Equals("t", StringComparison.OrdinalIgnoreCase))
                    {
                        if (countdownReset.enable)
                        {
                            countdownReset.enable = false;
                            countdownReset.time = -1;
                            if (!args.Player.IsLoggedIn)
                                args.Player.SendSuccessMessage("手动重置计划已关闭");
                            TSPlayer.All.SendSuccessMessage("手动重置计划已关闭");
                        }
                        else
                        {
                            countdownReset.time = -1;
                            args.Player.SendInfoMessage("手动重置计划未开启");
                        }
                    }
                    else if (args.Parameters[1].Equals("d", StringComparison.OrdinalIgnoreCase))
                    {
                        if (countdownRestart.enable)
                        {
                            countdownRestart.enable = false;
                            countdownRestart.time = -1;
                            if (!args.Player.IsLoggedIn)
                                args.Player.SendSuccessMessage("手动重启计划已关闭");
                            TSPlayer.All.SendSuccessMessage("手动重启计划已关闭");
                        }
                        else
                        {
                            countdownRestart.time = -1;
                            args.Player.SendInfoMessage("手动重启计划未开启");
                        }
                    }
                    else if (args.Parameters[1].Equals("c", StringComparison.OrdinalIgnoreCase))
                    {
                        if (countdownCom.enable)
                        {
                            countdownCom.enable = false;
                            countdownCom.time = -1;
                            if (!args.Player.IsLoggedIn)
                                args.Player.SendSuccessMessage("手动执行指令计划已关闭");
                            TSPlayer.All.SendSuccessMessage("手动执行指令计划已关闭");
                        }
                        else
                        {
                            countdownCom.time = -1;
                            args.Player.SendInfoMessage("手动执行指令计划未开启");
                        }
                    }
                    else if (args.Parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                    {
                        countdownReset.enable = false;
                        countdownReset.time = -1;
                        countdownRestart.enable = false;
                        countdownRestart.time = -1;
                        countdownCom.enable = false;
                        countdownCom.time = -1;
                        if (!args.Player.IsLoggedIn)
                            args.Player.SendSuccessMessage("已将全部手动计划关闭");
                        TSPlayer.All.SendSuccessMessage("已将全部手动计划关闭");
                    }
                    else
                        args.Player.SendInfoMessage("输入 /supco help 来获取该插件的帮助");
                }
                else
                    args.Player.SendInfoMessage("输入 /supco help 来获取该插件的帮助");
            }
            else if (args.Parameters.Count == 3)
            {
                if (args.Parameters[0].Equals("offset", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Parameters[1].Equals("autoreset", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!config.是否启用自动重置世界)
                        {
                            args.Player.SendErrorMessage("警告，并未开启自动重置计划，你的修改不会有任何效果");
                        }
                        if (double.TryParse(args.Parameters[2], out double num))
                        {
                            if (config.开服日期.AddHours(config.多少小时后开始自动重置世界 + num) < DateTime.Now.AddMinutes(AvoidTime))
                            {
                                double temp = (DateTime.Now.AddMinutes(AvoidTime) - config.开服日期.AddHours(config.多少小时后开始自动重置世界)).TotalHours;
                                args.Player.SendInfoMessage($"重置世界倒计时过短，需{(temp > 0 ? "小" : "大")}于 {temp:0.00} 来避免立刻重置，修改失败，若要立刻重置，请使用 /supco reset 指令");
                            }
                            else
                            {
                                config.多少小时后开始自动重置世界 += num;
                                Config.SaveConfigFile(config);
                                if (args.Player.IsLoggedIn)
                                    args.Player.SendSuccessMessage($"时间已修改为 {config.多少小时后开始自动重置世界:0.00} 即从现在起{HoursToM((config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now).TotalHours, "EA00FF")}后开始重置");
                                else
                                    args.Player.SendSuccessMessage($"时间已修改为 {config.多少小时后开始自动重置世界:0.00} 即从现在起{HoursToM((config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now).TotalHours)}后开始重置");
                                if (config.是否启用自动重置世界)
                                    TSPlayer.All.SendInfoMessage("自动重置计划已修改");
                            }
                        }
                        else
                            args.Player.SendInfoMessage("请输入正负整数小数");
                    }
                    else if (args.Parameters[1].Equals("autoreload", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!config.是否启用自动重启服务器)
                        {
                            args.Player.SendErrorMessage("警告，并未开启自动重启计划，你的修改不会有任何效果");
                        }
                        if (double.TryParse(args.Parameters[2], out double num))
                        {
                            if (config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器 + num) < DateTime.Now.AddMinutes(AvoidTime))
                            {
                                double temp = (DateTime.Now.AddMinutes(AvoidTime) - config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器)).TotalHours;
                                args.Player.SendInfoMessage($"重启服务器倒计时过短，需{(temp > 0 ? "小" : "大")}于 {temp:0.00} 来避免立刻重启，修改失败，若要立刻重启，请使用 /supco reload 指令");
                            }
                            else
                            {
                                config.多少小时后开始自动重启服务器 += num;
                                Config.SaveConfigFile(config);
                                if (args.Player.IsLoggedIn)
                                    args.Player.SendSuccessMessage($"时间已修改为 {config.多少小时后开始自动重启服务器:0.00} 即从现在起{HoursToM((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours, "FF9000")}后开始重启");
                                else
                                    args.Player.SendSuccessMessage($"时间已修改为 {config.多少小时后开始自动重启服务器:0.00} 即从现在起{HoursToM((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours)}后开始重启");
                                if (config.是否启用自动重启服务器)
                                    TSPlayer.All.SendInfoMessage("自动重启计划已修改");
                            }
                        }
                        else
                            args.Player.SendInfoMessage("请输入正负整数小数");
                    }
                    else
                        args.Player.SendInfoMessage("输入 /supco help 来获取该插件的帮助");
                }
                else
                    args.Player.SendInfoMessage("输入 /supco help 来获取该插件的帮助");
            }
            else
                args.Player.SendInfoMessage("输入 /supco help 来获取该插件的帮助");
        }

        /*
        /// <summary>
        /// 在服务器关闭后执行指令
        /// </summary>
        /// <param name="args"></param>
        private void RunResetCmd(string[] args)
        {
            if (Terraria.Utils.ParseArguements(args).TryGetValue("cmd", out string? text) && text != null && text.Equals("run", StringComparison.OrdinalIgnoreCase))
            {
                config.重置后执行的指令.ForEach(x =>
                {
                    Commands.HandleCommand(TSPlayer.Server, x);
                    for(int i = 0;i < 100; i++)
                    {
                        Console.WriteLine(i);
                    }
                });
            }
        }
         */

        /// <summary>
        /// 重置函数
        /// </summary>
        private static void ResetGame()
        {
            //在服务器关闭前执行指令
            config.自动重置前执行的指令_不需要加斜杠.ForEach(x => { Commands.HandleCommand(TSPlayer.Server, "/" + x.Trim('/', '.')); });
            config.自动重置前删除哪些数据库表?.ForEach(x =>
            {
                try
                {
                    TShock.DB.Query("DROP TABLE " + x);
                }
                catch { }
            });
            if (!config.自动重置是否重置玩家数据)
                TShock.Players.ForEach(x => { if (x != null && x.IsLoggedIn) x.SaveServerCharacter(); });
            if (config.自动重置是否重置玩家数据)
            {
                try
                {
                    TShock.DB.Query("delete from tsCharacter");
                }
                catch { }
            }
            if (!config.自动重置前是否删除地图)
                TShock.Utils.SaveWorld();
            if (config.自动重置前是否删除地图 && File.Exists(Main.worldPathName))
            {
                File.Delete(Main.worldPathName);
                if (File.Exists(Main.worldPathName + ".bak"))
                    File.Delete(Main.worldPathName + ".bak");
                if (File.Exists(Main.worldPathName + ".bak2"))
                    File.Delete(Main.worldPathName + ".bak2");
                if (File.Exists(Main.worldPathName + ".crash.bak"))
                    File.Delete(Main.worldPathName + ".crash.bak");
                if (File.Exists(Main.worldPathName + ".crash.bak2"))
                    File.Delete(Main.worldPathName + ".crash.bak2");
            }
            if (config.自动重置前是否删除日志)
            {
                string name = TShock.Log.FileName;
                TShock.Log.Dispose();
                new DirectoryInfo(TShock.Config.Settings.LogPath).GetFiles().ForEach(x =>
                {
                    if (name.Contains(x.Name))
                    {
                        x.Delete();
                    }
                });
            }
            /*
            try
            {
                //TShock.RestApi.Stop();
            }
            catch { }
            try
            {
                PropertyInfo? property = ServerApi.LogWriter.GetType().GetProperty("DefaultLogWriter", BindingFlags.Instance | BindingFlags.NonPublic);
                ServerLogWriter? serverLogWriter = (property != null) ? (ServerLogWriter?)property.GetValue(ServerApi.LogWriter) : null;
                serverLogWriter?.Dispose();
            }
            catch { }
            */

            config.开服日期 = DateTime.Now;
            config.上次重启服务器的日期 = DateTime.Now;
            config.上次自动执行指令的日期 = DateTime.Now;
            Config.SaveConfigFile(config);
            Console.Clear();
            string path = "";
            if (string.IsNullOrWhiteSpace(config.地图存放目录_不填时默认原目录_注意请使用除号分隔目录))
                path = Main.WorldPath;
            else
                path = config.地图存放目录_不填时默认原目录_注意请使用除号分隔目录;

            int count = 1;
            string worldname = "";
            if (string.IsNullOrWhiteSpace(config.自动重置的地图名称))
            {
                config.自动重置的地图名称 = "World";
            }
            while (true)
            {
                if (File.Exists(path + "/" + config.自动重置的地图名称 + (count == 1 ? "" : count) + ".wld"))
                {
                    count++;
                }
                else
                {
                    worldname = config.自动重置的地图名称 + (count == 1 ? "" : count);
                    break;
                }
            }
            Process.Start(Path.Combine(Environment.CurrentDirectory, "Tshock.Server.exe"),
                $"-lang 7 -autocreate {config.自动重置的地图大小_小1_中2_大3} -seed {config.自动重置的地图种子} -world {path}/{worldname} -difficulty {config.自动重置的地图难度_普通0_专家1_大师2_旅途3} -maxplayers {config.自动重置的最多在线人数} -port {config.自动重置的端口} -c");
            Netplay.Disconnect = true;
            Environment.Exit(0); //暴力关服处理
        }

        /// <summary>
        /// 重启游戏
        /// </summary>
        private static void RestartGame()
        {
            config.上次重启服务器的日期 = DateTime.Now;
            Config.SaveConfigFile(config);
            config.自动重启前执行的指令_不需要加斜杠.ForEach(x => { Commands.HandleCommand(TSPlayer.Server, "/" + x.Trim('/', '.')); });
            TShock.Utils.SaveWorld();
            TShock.Players.ForEach(x => { if (x != null && x.IsLoggedIn) x.SaveServerCharacter(); });
            TShock.Log.Dispose();
            /*
            try
            {
                TShock.RestApi.Stop();
            }
            catch { }
            */
            Console.Clear();
            Process.Start(Path.Combine(Environment.CurrentDirectory, "Tshock.Server.exe"),
                $"-lang 7 -world {Main.worldPathName} -maxplayers {TShock.Config.Settings.MaxSlots} -port {Netplay.ListenPort} -c");
            Netplay.Disconnect = true;
            Environment.Exit(0); //暴力关服处理
        }

        private static void AutoCommands()
        {
            config.上次自动执行指令的日期 = DateTime.Now;
            Config.SaveConfigFile(config);

            config.自动执行的指令_不需要加斜杠.ForEach(x =>
            {
                if (x.Contains("pco com", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("请不要对自动执行的指令进行套娃！" + "/" + x);
                    TShock.Log.Warn("请不要对自动执行的指令进行套娃！" + "/" + x);
                    TSPlayer.All.SendErrorMessage("请不要对自动执行的指令进行套娃！" + "/" + x);
                    return;
                }
                Commands.HandleCommand(TSPlayer.Server, "/" + x.Trim('/', '.'));
            });
            TSPlayer.All.SendSuccessMessage("服务器自动执行指令成功");
            Console.WriteLine("服务器自动执行指令成功");
        }


        /// <summary>
        /// 普通控制boss进度的指令
        /// </summary>
        /// <param name="args"></param>
        private void PCO(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendInfoMessage("输入 /pco help 来获取该插件的帮助");
            }
            else if (args.Parameters.Count == 1)
            {
                if (args.Parameters[0].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    args.Player.SendMessage(
                        "输入 /pco help   来获取该插件的帮助\n" +
                        "输入 /pco view   来查看当前服务器的自动化计划\n" +
                        "输入 /pco now   来将开服日期、上次重启日期和上次自动执行指令日期调整到现在\n" +

                        "输入 /pco autoboss   自动控制Boss进度计划启用，再次使用则关闭\n" +
                        "输入 /pco offset autoboss [±num]   来将自动控制Boss的解锁时刻推迟或提前num小时，num可为小数\n" +
                        "输入 /pco autocom   自动执行指令计划启用，再次使用关闭\n" +
                        "输入 /pco offset autocom [±num]   将自动执行指令的时间推迟或提前num小时，num可为小数\n" +
                        "输入 /pco com [±num]   手动执行指令计划启用，在num秒后开始执行，若num不填则立刻执行，num小于0则关闭当前存在的手动计划，其优先级大于自动执行指令\n" +
                        "输入 /pco com或autocom add [string]   来添加一个自动执行的指令\n" +
                        "输入 /pco com或autocom del [string]   来删除一个自动执行的指令\n" +
                        "输入 /supco help   来查看高级自动控制指令", TextColor());
                }
                else if (args.Parameters[0].Equals("view", StringComparison.OrdinalIgnoreCase))
                {
                    string othermess = "";
                    //boss进度数据输出
                    if (config.是否自动控制Boss进度)
                    {
                        double lv = (DateTime.Now - config.开服日期).TotalHours;
                        Dictionary<string, double> keyValuePairs = new Dictionary<string, double>();
                        Dictionary<string, double> sortpairs = new Dictionary<string, double>();
                        foreach (var v in config.Boss封禁时长距开服日期_单位小时)
                        {
                            keyValuePairs.Add(v.Key, v.Value);
                        }
                        //排序
                        while (keyValuePairs.Count > 0)
                        {
                            double min = double.MaxValue;
                            string key = "";
                            foreach (var v in keyValuePairs)
                            {
                                if (v.Value < min)
                                {
                                    key = v.Key;
                                    min = v.Value;
                                }
                            }
                            if (key != "")
                            {
                                sortpairs.Add(key, min);
                                keyValuePairs.Remove(key);
                            }
                        }
                        string mess = args.Player.IsLoggedIn ? "[i:3868]已解锁Boss：\n" : "@已解锁Boss：\n";
                        int count = 0;
                        //把排好序的数据输出
                        foreach (var v in sortpairs)
                        {
                            if (v.Value < lv)
                            {
                                mess += $"{v.Key}  ";
                                count++;
                                if (count == 8)
                                {
                                    mess += "\n";
                                    count = 0;
                                }
                            }
                            else
                            {
                                if (args.Player.IsLoggedIn)
                                    mess += $"{(count != 0 ? "\n" : "")}{v.Key} 还剩{HoursToM(v.Value - lv, "28FFB8")}解锁";
                                else
                                    mess += $"{(count != 0 ? "\n" : "")}{v.Key} 还剩{HoursToM(v.Value - lv)}解锁";
                                count = 1;
                            }
                        }
                        args.Player.SendMessage(mess, TextColor());
                    }
                    else
                        othermess += args.Player.IsLoggedIn ? "[i:3868]" + MtoM("Boss进度控制未开启", "28FFB8") + "\n" : "Boss进度控制未开启\n";

                    //自动重置数据输出
                    if (config.是否启用自动重置世界 && !countdownReset.enable)
                    {
                        if (args.Player.IsLoggedIn)
                            args.Player.SendInfoMessage($"[i:3099]世界将于{HoursToM((config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now).TotalHours, "EA00FF")}后重置");
                        else
                            args.Player.SendInfoMessage($"@世界将于{HoursToM((config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now).TotalHours)}后重置");
                    }
                    else if (countdownReset.enable)
                    {
                        if (args.Player.IsLoggedIn)
                            args.Player.SendInfoMessage($"[i:709]世界将于{HoursToM(countdownReset.time * 1.0 / 3600, "EA00FF")}后重置");
                        else
                            args.Player.SendInfoMessage($"@世界将于{HoursToM(countdownReset.time * 1.0 / 3600)}后重置");
                    }
                    else
                        othermess += args.Player.IsLoggedIn ? "[i:3099]" + MtoM("世界自动重置未开启", "EA00FF") + "\n" : "世界自动重置未开启\n";

                    //自动重启数据输出
                    if (config.是否启用自动重启服务器 && !countdownRestart.enable)
                    {
                        if (args.Player.IsLoggedIn)
                            args.Player.SendInfoMessage($"[i:17]服务器将于{HoursToM((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours, "FF9000")}后重启");
                        else
                            args.Player.SendInfoMessage($"@服务器将于{HoursToM((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours)}后重启");
                    }
                    else if (countdownRestart.enable)
                    {
                        if (args.Player.IsLoggedIn)
                            args.Player.SendInfoMessage($"[i:17]服务器将于{HoursToM(countdownRestart.time * 1.0 / 3600, "FF9000")}后重启");
                        else
                            args.Player.SendInfoMessage($"@服务器将于{HoursToM(countdownRestart.time * 1.0 / 3600)}后重启");
                    }
                    else
                        othermess += args.Player.IsLoggedIn ? "[i:17]" + MtoM("服务器自动重启未开启", "FF9000") + "\n" : "服务器自动重启未开启\n";

                    //自动执行指令输出
                    if (config.是否启用自动执行指令 && !countdownCom.enable)
                    {
                        if (args.Player.IsLoggedIn)
                            args.Player.SendInfoMessage($"[i:903]服务器将于{HoursToM((config.上次自动执行指令的日期.AddHours(config.多少小时后开始自动执行指令) - DateTime.Now).TotalHours, "00A8FF")}后执行指令");
                        else
                            args.Player.SendInfoMessage($"@服务器将于{HoursToM((config.上次自动执行指令的日期.AddHours(config.多少小时后开始自动执行指令) - DateTime.Now).TotalHours)}后执行指令");
                    }
                    else if (countdownCom.enable)
                    {
                        if (args.Player.IsLoggedIn)
                            args.Player.SendInfoMessage($"[i:903]服务器将于{HoursToM(countdownCom.time * 1.0 / 3600, "00A8FF")}后执行指令");
                        else
                            args.Player.SendInfoMessage($"@服务器将于{HoursToM(countdownCom.time * 1.0 / 3600)}后执行指令");
                    }
                    else
                        othermess += args.Player.IsLoggedIn ? "[i:903]" + MtoM("服务器自动执行指令未开启", "00A8FF") : "服务器自动执行指令未开启";

                    if (!string.IsNullOrWhiteSpace(othermess))
                        args.Player.SendInfoMessage(othermess);
                }
                else if (args.Parameters[0].Equals("now", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin"))
                    {
                        args.Player.SendInfoMessage("权限不足！");
                        return;
                    }
                    config.开服日期 = DateTime.Now;
                    config.上次重启服务器的日期 = DateTime.Now;
                    config.上次自动执行指令的日期 = DateTime.Now;
                    Config.SaveConfigFile(config);
                    args.Player.SendSuccessMessage("定位成功，所有已开启的自动计划将从现在开始计时");
                }
                else if (args.Parameters[0].Equals("autoboss", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin"))
                    {
                        args.Player.SendInfoMessage("权限不足！");
                        return;
                    }
                    if (config.是否自动控制Boss进度)
                        args.Player.SendSuccessMessage("已取消Boss的封禁限制计划");
                    else
                        args.Player.SendSuccessMessage("已开启Boss的封禁限制计划");
                    config.是否自动控制Boss进度 = !config.是否自动控制Boss进度;
                    Config.SaveConfigFile(config);
                }
                else if (args.Parameters[0].Equals("autocom", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin"))
                    {
                        args.Player.SendInfoMessage("权限不足！");
                        return;
                    }
                    if (config.是否启用自动执行指令)
                    {
                        if (!args.Player.IsLoggedIn)
                            args.Player.SendSuccessMessage("已取消自动执行指令的计划");
                        TSPlayer.All.SendSuccessMessage("已取消自动执行指令的计划");
                    }
                    else
                    {
                        if (config.多少小时后开始自动执行指令 <= 0)
                        {
                            args.Player.SendInfoMessage("自动执行指令倒计时必须大于0");
                            return;
                        }
                        if (config.上次自动执行指令的日期.AddHours(config.多少小时后开始自动执行指令) <= DateTime.Now.AddMinutes(AvoidTime))
                        {
                            args.Player.SendInfoMessage("自动执行指令倒计时过短，可能即将开始自动执行指令");
                        }

                        if (!args.Player.IsLoggedIn)
                            args.Player.SendSuccessMessage($"自动执行指令计划已启用，将在从现在起{HoursToM((config.上次自动执行指令的日期.AddHours(config.多少小时后开始自动执行指令) - DateTime.Now).TotalHours)}后自动执行指令");
                        TSPlayer.All.SendSuccessMessage($"自动执行指令计划已启用，将在从现在起{HoursToM((config.上次自动执行指令的日期.AddHours(config.多少小时后开始自动执行指令) - DateTime.Now).TotalHours, "00A8FF")}后自动执行指令");
                    }
                    config.是否启用自动执行指令 = !config.是否启用自动执行指令;
                    Config.SaveConfigFile(config);
                }
                else if (args.Parameters[0].Equals("com", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin"))
                    {
                        args.Player.SendInfoMessage("权限不足！");
                        return;
                    }
                    AutoCommands();
                }
                else
                    args.Player.SendInfoMessage("输入 /pco help 来获取该插件的帮助");
            }
            else if (args.Parameters.Count == 2)
            {
                if (args.Parameters[0].Equals("com", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin"))
                    {
                        args.Player.SendInfoMessage("权限不足！");
                        return;
                    }
                    if (int.TryParse(args.Parameters[1], out int num))
                    {
                        if (num >= 0)
                        {
                            if (countdownRestart.enable && countdownRestart.time < num)
                            {
                                args.Player.SendErrorMessage("警告：当前存在手动重启计划，且时间小于你输入的数值，你的手动执行指令计划将不会生效！");
                            }
                            if (!countdownRestart.enable && config.是否启用自动重启服务器 && config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) < DateTime.Now.AddSeconds(num))
                            {
                                args.Player.SendErrorMessage("警告：当前存在自动重启计划，且时间小于你输入的数值，你的手动执行指令计划将不会生效！");
                            }
                            if (countdownReset.enable && countdownReset.time < num)
                            {
                                args.Player.SendErrorMessage("警告：当前存在手动重置计划，且时间小于你输入的数值，你的手动执行指令计划将不会生效！");
                            }
                            if (!countdownReset.enable && config.是否启用自动重置世界 && config.开服日期.AddHours(config.多少小时后开始自动重置世界) < DateTime.Now.AddSeconds(num))
                            {
                                args.Player.SendErrorMessage("警告：当前存在自动重置计划，且时间小于你输入的数值，你的手动执行指令计划将不会生效！");
                            }

                            if (countdownCom.enable)
                            {
                                countdownCom.time = num;
                                TSPlayer.All.SendInfoMessage($"手动执行指令计划已重新开始，服务器将在{HoursToM(num * 1.0 / 3600, "00A8FF")}后执行指令");
                                if (!args.Player.IsLoggedIn)
                                    args.Player.SendInfoMessage($"手动执行指令计划已重新开始，服务器将在{HoursToM(num * 1.0 / 3600)}后执行指令");
                            }
                            else
                            {
                                TSPlayer.All.SendInfoMessage($"手动执行指令计划开启，服务器将在{HoursToM(num * 1.0 / 3600, "00A8FF")}后执行指令");
                                if (!args.Player.IsLoggedIn)
                                    args.Player.SendInfoMessage($"手动执行指令计划开启，服务器将在{HoursToM(num * 1.0 / 3600)}后执行指令");
                                countdownCom.enable = true;
                                countdownCom.time = num;
                                Thread thread = new Thread(() =>
                                {
                                    while (countdownCom.time >= 0 && !Netplay.Disconnect && countdownCom.enable)
                                    {
                                        if (countdownCom.time <= 0)
                                        {
                                            AutoCommands();
                                            countdownCom.enable = false;
                                        }
                                        countdownCom.time--;
                                        Thread.Sleep(1000);
                                    }
                                });
                                thread.Start();
                            }
                        }
                        else
                        {
                            if (countdownCom.enable)
                            {
                                countdownCom.enable = false;
                                countdownCom.time = -1;
                                if (!args.Player.IsLoggedIn)
                                    args.Player.SendSuccessMessage("手动执行指令计划已关闭");
                                TSPlayer.All.SendSuccessMessage("手动执行指令计划已关闭");
                            }
                            else
                            {
                                if (!args.Player.IsLoggedIn)
                                    args.Player.SendSuccessMessage("未开启手动执行指令计划");
                                TSPlayer.All.SendSuccessMessage("未开启手动执行指令计划");
                            }
                        }
                    }
                    else
                        args.Player.SendInfoMessage("请输入整数");
                }
                else
                    args.Player.SendInfoMessage("输入 /pco help 来获取该插件的帮助");
            }
            else if (args.Parameters.Count >= 3)
            {
                if (args.Parameters[0].Equals("offset", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin"))
                    {
                        args.Player.SendInfoMessage("权限不足！");
                        return;
                    }
                    if (double.TryParse(args.Parameters[2], out double addtime))
                    {
                        if (args.Parameters[1].Equals("autoboss", StringComparison.OrdinalIgnoreCase))
                        {
                            var keys = config.Boss封禁时长距开服日期_单位小时.Keys.ToArray();
                            foreach (var x in keys)
                            {
                                try
                                {
                                    config.Boss封禁时长距开服日期_单位小时[x] += addtime;
                                }
                                catch { }
                            }
                            Config.SaveConfigFile(config);
                            string st;
                            if (addtime > 0)
                                st = "推迟" + (args.Player.IsLoggedIn ? HoursToM(addtime, "28FFB8") : HoursToM(addtime));
                            else if (addtime < 0)
                                st = "提前" + (args.Player.IsLoggedIn ? HoursToM(-1 * addtime, "28FFB8") : HoursToM(-1 * addtime));
                            else
                                st = "正常";
                            if (!config.是否自动控制Boss进度)
                                args.Player.SendErrorMessage("警告，未开启自动控制Boss进度计划，你的修改不会有任何效果");
                            else
                                TSPlayer.All.SendSuccessMessage($"定位成功，Boss将{st}解锁");
                            if (!args.Player.IsLoggedIn)
                                args.Player.SendSuccessMessage($"定位成功，Boss将{st}解锁");
                        }
                        else if (args.Parameters[1].Equals("autocom", StringComparison.OrdinalIgnoreCase))
                        {
                            config.多少小时后开始自动执行指令 += addtime;
                            Config.SaveConfigFile(config);
                            string st;
                            if (addtime > 0)
                                st = "推迟" + (args.Player.IsLoggedIn ? HoursToM(addtime, "00A8FF") : HoursToM(addtime));
                            else if (addtime < 0)
                                st = "提前" + (args.Player.IsLoggedIn ? HoursToM(-1 * addtime, "00A8FF") : HoursToM(-1 * addtime));
                            else
                                st = "时间不变";
                            double h = (config.上次自动执行指令的日期.AddHours(config.多少小时后开始自动执行指令) - DateTime.Now).TotalHours;
                            if (!config.是否启用自动执行指令)
                                args.Player.SendErrorMessage("警告，未开启自动执行指令的计划，你的修改不会有任何效果");
                            else
                                TSPlayer.All.SendInfoMessage("自动执行指令计划已修改");
                            if (!args.Player.IsLoggedIn)
                                args.Player.SendSuccessMessage($"定位成功，下次自动执行指令将{st}，" + (h > 0 ? $"即从现在起{HoursToM(h)}后" : "你提前的太多了，指令将立刻执行"));
                            else
                                args.Player.SendSuccessMessage($"定位成功，下次自动执行指令将{st}，" + (h > 0 ? $"即从现在起{HoursToM(h, "00A8FF")}后" : "你提前的太多了，指令将立刻执行"));
                        }
                        else
                            args.Player.SendInfoMessage("输入 /pco help 来获取该插件的帮助");
                    }
                    else
                        args.Player.SendInfoMessage("请输入正负整数或小数！");
                }
                else if (args.Parameters[0].Equals("com", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("autocom", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin"))
                    {
                        args.Player.SendInfoMessage("权限不足！");
                        return;
                    }
                    if (args.Parameters[1].Equals("add", StringComparison.OrdinalIgnoreCase))
                    {
                        string mess = "";
                        for (int i = 2; i < args.Parameters.Count; i++)
                        {
                            mess += args.Parameters[i].Trim('/', '.') + " ";
                        }
                        mess = mess.Trim(' ');
                        if (config.自动执行的指令_不需要加斜杠.Add(mess))
                        {
                            args.Player.SendSuccessMessage($"已将指令 /{mess} 添加成功！");
                            Config.SaveConfigFile(config);
                        }
                        else
                            args.Player.SendErrorMessage("添加失败，请检查是否已添加过");
                    }
                    else if (args.Parameters[1].Equals("del", StringComparison.OrdinalIgnoreCase))
                    {
                        string mess = "";
                        for (int i = 2; i < args.Parameters.Count; i++)
                        {
                            mess += args.Parameters[i].Trim('/', '.') + " ";
                        }
                        mess = mess.Trim(' ');
                        if (config.自动执行的指令_不需要加斜杠.Remove(mess))
                        {
                            args.Player.SendSuccessMessage($"已将指令 /{mess} 删除成功！");
                            Config.SaveConfigFile(config);
                        }
                        else
                            args.Player.SendErrorMessage("删除失败，请检查是否存在该指令");
                    }
                    else
                        args.Player.SendInfoMessage("输入 /pco help 来获取该插件的帮助");
                }
                else
                    args.Player.SendInfoMessage("输入 /pco help 来获取该插件的帮助");
            }
            else
                args.Player.SendInfoMessage("输入 /pco help 来获取该插件的帮助");
        }


        /// <summary>
        /// npc更新的
        /// </summary>
        /// <param name="args"></param>
        private void NPCAIUpdate(NpcAiUpdateEventArgs args)
        {
            if (!config.是否自动控制Boss进度 || args == null || args.Npc == null || !args.Npc.active || Main.time % 2 != 0)
                return;

            switch (args.Npc.netID)
            {
                case NPCID.KingSlime:
                    Function(args.Npc, "史莱姆王");
                    break;
                case NPCID.EyeofCthulhu:
                    Function(args.Npc, "克苏鲁之眼");
                    break;
                case NPCID.EaterofWorldsHead:
                    Function(args.Npc, "世界吞噬者");
                    break;
                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsTail:
                    Function(args.Npc, "世界吞噬者", false);
                    break;
                case NPCID.BrainofCthulhu:
                    Function(args.Npc, "克苏鲁之脑");
                    break;
                case NPCID.Creeper:
                    Function(args.Npc, "克苏鲁之脑", false);
                    break;
                case NPCID.QueenBee:
                    Function(args.Npc, "蜂后");
                    break;
                case NPCID.Deerclops:
                    Function(args.Npc, "巨鹿");
                    break;
                case NPCID.SkeletronHead:
                    Function(args.Npc, "骷髅王");
                    break;
                case NPCID.SkeletronHand:
                    Function(args.Npc, "骷髅王", false);
                    break;
                case NPCID.WallofFlesh:
                    Function(args.Npc, "血肉墙");
                    break;
                case NPCID.WallofFleshEye:
                case NPCID.TheHungry:
                case NPCID.TheHungryII:
                    Function(args.Npc, "血肉墙", false);
                    break;
                case NPCID.QueenSlimeBoss:
                    Function(args.Npc, "史莱姆皇后");
                    break;
                case 125://双子
                case 126:
                    Function(args.Npc, "双子魔眼");
                    break;
                case NPCID.TheDestroyer:
                    Function(args.Npc, "毁灭者");
                    break;
                case NPCID.TheDestroyerBody:
                case NPCID.TheDestroyerTail:
                case NPCID.Probe:
                    Function(args.Npc, "毁灭者", false);
                    break;
                case 127://机械骷髅王
                    Function(args.Npc, "机械骷髅王");
                    break;
                case 128:
                case 129:
                case 130:
                case 131:
                    Function(args.Npc, "机械骷髅王", false);
                    break;
                case NPCID.Plantera:
                    Function(args.Npc, "世纪之花");
                    break;
                case NPCID.PlanterasTentacle:
                    Function(args.Npc, "世纪之花", false);
                    break;
                case NPCID.GolemFistLeft:
                case NPCID.GolemFistRight:
                    Function(args.Npc, "石巨人", false);
                    break;
                case NPCID.Golem:
                case NPCID.GolemHead:
                    Function(args.Npc, "石巨人");
                    break;
                case NPCID.DukeFishron:
                    Function(args.Npc, "猪龙鱼公爵");
                    break;
                case NPCID.HallowBoss:
                    Function(args.Npc, "光之女皇");
                    break;
                case NPCID.CultistBoss:
                    Function(args.Npc, "拜月教教徒");
                    break;
                case NPCID.LunarTowerSolar:
                case NPCID.LunarTowerVortex:
                case NPCID.LunarTowerStardust:
                case NPCID.LunarTowerNebula:
                    Function(args.Npc, "四柱");
                    break;
                case NPCID.MoonLordCore:
                    Function(args.Npc, "月亮领主");
                    break;
                case NPCID.MoonLordHead:
                case NPCID.MoonLordHand:
                case NPCID.MoonLordLeechBlob:
                    Function(args.Npc, "月亮领主", false);
                    break;
                default: break;
            }
        }

        /// <summary>
        /// npc受击的
        /// </summary>
        /// <param name="args"></param>
        private void NPCStrike(NpcStrikeEventArgs args)
        {
            if (!config.是否自动控制Boss进度 || args == null || args.Npc == null || !args.Npc.active)
                return;

            switch (args.Npc.netID)
            {
                case NPCID.KingSlime:
                    Function(args.Npc, "史莱姆王", false);
                    break;
                case NPCID.EyeofCthulhu:
                    Function(args.Npc, "克苏鲁之眼", false);
                    break;
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsTail:
                    Function(args.Npc, "世界吞噬者", false);
                    break;
                case NPCID.BrainofCthulhu:
                case NPCID.Creeper:
                    Function(args.Npc, "克苏鲁之脑", false);
                    break;
                case NPCID.QueenBee:
                    Function(args.Npc, "蜂后", false);
                    break;
                case NPCID.Deerclops:
                    Function(args.Npc, "巨鹿", false);
                    break;
                case NPCID.SkeletronHead:
                case NPCID.SkeletronHand:
                    Function(args.Npc, "骷髅王", false);
                    break;
                case NPCID.WallofFlesh:
                case NPCID.WallofFleshEye:
                case NPCID.TheHungry:
                case NPCID.TheHungryII:
                    Function(args.Npc, "血肉墙", false);
                    break;
                case NPCID.QueenSlimeBoss:
                    Function(args.Npc, "史莱姆皇后", false);
                    break;
                case 125://双子
                case 126:
                    Function(args.Npc, "双子魔眼", false);
                    break;
                case NPCID.TheDestroyer:
                case NPCID.TheDestroyerBody:
                case NPCID.TheDestroyerTail:
                case NPCID.Probe:
                    Function(args.Npc, "毁灭者", false);
                    break;
                case 127://机械骷髅王
                case 128:
                case 129:
                case 130:
                case 131:
                    Function(args.Npc, "机械骷髅王", false);
                    break;
                case NPCID.Plantera:
                case NPCID.PlanterasTentacle:
                    Function(args.Npc, "世纪之花", false);
                    break;
                case NPCID.GolemFistLeft:
                case NPCID.GolemFistRight:
                case NPCID.Golem:
                case NPCID.GolemHead:
                    Function(args.Npc, "石巨人", false);
                    break;
                case NPCID.DukeFishron:
                    Function(args.Npc, "猪龙鱼公爵", false);
                    break;
                case NPCID.HallowBoss:
                    Function(args.Npc, "光之女皇", false);
                    break;
                case NPCID.CultistBoss:
                    Function(args.Npc, "拜月教教徒", false);
                    break;
                case NPCID.LunarTowerSolar:
                case NPCID.LunarTowerVortex:
                case NPCID.LunarTowerStardust:
                case NPCID.LunarTowerNebula:
                    Function(args.Npc, "四柱", false);
                    break;
                case NPCID.MoonLordCore:
                case NPCID.MoonLordHead:
                case NPCID.MoonLordHand:
                case NPCID.MoonLordLeechBlob:
                    Function(args.Npc, "月亮领主", false);
                    break;
                default: break;
            }
        }


        /// <summary>
        /// 小功能函数
        /// </summary>
        /// <param name="bossname"></param>
        /// <param name="enableBC">是否发出广播</param>
        void Function(NPC npc, string bossname, bool enableBC = true)
        {
            DateTime dt = DateTime.Now;
            double jiange = (dt - config.开服日期).TotalHours;
            if (jiange < config.Boss封禁时长距开服日期_单位小时[bossname])
            {
                npc.active = false;
                TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npc.whoAmI);
                if (enableBC)
                    TSPlayer.All.SendInfoMessage($"{npc.FullName} 未到解锁时间，还剩{HoursToM(config.Boss封禁时长距开服日期_单位小时[bossname] - jiange, "28FFB8")}");
            }
        }


        public static Color TextColor()
        {
            int r, g, b;
            r = Main.rand.Next(100, 255);
            g = Main.rand.Next(100, 255);
            b = Main.rand.Next(100, 255);
            return new Color(r, g, b);
        }

        /// <summary>
        /// 将 xxh 转化为 { xx.xxx 时 xx 分钟 xx 秒 }，数字用彩色强调，颜色不填时只返回纯文本
        /// </summary>
        /// <param name="h"></param>
        /// <param name="color"> 修改数字的颜色,不填时默认原色，泰拉游戏中的颜色 [c/十六进制:文本]，只用填十六进制即可 </param>
        /// <returns></returns>
        public static string HoursToM(double h, string color = "")
        {
            string mess = "";
            int newH, s, m; //时分秒
            newH = (int)h;
            m = (int)(h % 1 * 60);//分
            s = (int)(h % 1 * 60 % 1 * 60);//秒
            if (!string.IsNullOrWhiteSpace(color))
            {
                if (newH > 0)
                    mess += $" [c/{color}:{newH}] 时";
                if (m > 0)
                    mess += $" [c/{color}:{m}] 分";
                mess += $" [c/{color}:{s}] 秒";
            }
            else
            {
                if (newH > 0)
                    mess += $" {newH} 时";
                if (m > 0)
                    mess += $" {m} 分";
                mess += $" {s} 秒";
            }
            return mess;
        }

        /// <summary>
        /// 将 xxx 转化为 [c/十六进制:xxx] 带彩色的那种，颜色不填时只返回纯文本
        /// </summary>
        /// <param name="m"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string MtoM(string m, string color = "")
        {
            if (color == "")
                return m;
            else
                return $"[c/{color}:{m}]";
        }
    }
}
