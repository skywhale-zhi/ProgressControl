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
                        "输入 /supco ar   自动重置世界计划启用，再次使用关闭\n" +
                        "输入 /supco os ar [±num]   将自动重置世界的时间推迟或提前num小时，num可为小数\n" +
                        "输入 /supco ad   自动重启服务器计划启用，再次使用关闭\n" +
                        "输入 /supco os ad [±num]   将自动重启服务器的时间推迟或提前num小时，num可为小数\n" +

                        "输入 /supco ar help   来设置重置的相关参数\n" +
                        "输入 /supco ad help   来设置重启的相关参数\n" +

                        "输入 /supco view   来查看当前服务器的自动化计划\n" +

                        "输入 /supco reset [±num]   手动重置世界计划启用，在num秒后开始重置，若num不填则立刻重置，num小于0则关闭当前存在的手动计划，其优先级大于自动重置\n" +
                        "输入 /supco reload [±num]   手动重启服务器计划启用，在num秒后开始重启，若num不填则立刻重启，num小于0则关闭当前存在的手动计划，其优先级大于自动重启\n" +
                        "输入 /supco stop [r/d/c/all或不填]  来分别终止reset/reload/com的全部手动计划，填all或不填时终止全部，自动计划不受影响", TextColor());
                }
                else if (args.Parameters[0].Equals("autoreset", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("ar", StringComparison.OrdinalIgnoreCase))
                {
                    if (config.是否启用自动重置世界)
                    {
                        if (!args.Player.IsLoggedIn)
                            args.Player.SendSuccessMessage("自动重置计划已关闭");
                        TSPlayer.All.SendSuccessMessage("自动重置计划已关闭");
                        config.是否启用自动重置世界 = !config.是否启用自动重置世界;
                        config.SaveConfigFile();
                    }
                    else
                    {
                        if (config.开服日期.AddHours(config.多少小时后开始自动重置世界) <= DateTime.Now.AddMinutes(AvoidTime))
                        {
                            args.Player.SendInfoMessage($"自动重置世界倒计时过短，已关闭自动重置，请至少于开服后 {AvoidTime} 分钟内不要重置，避免产生错误");
                        }
                        else
                        {
                            if (!args.Player.IsLoggedIn)
                                args.Player.SendSuccessMessage($"自动重置计划已启用，将在从现在起{HoursToM((config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now).TotalHours)}后自动重置");
                            TSPlayer.All.SendSuccessMessage($"自动重置计划已启用，将在从现在起{HoursToM((config.开服日期.AddHours(config.多少小时后开始自动重置世界) - DateTime.Now).TotalHours, "EA00FF")}后自动重置");
                            config.是否启用自动重置世界 = !config.是否启用自动重置世界;
                            config.SaveConfigFile();
                        }
                    }
                }
                else if (args.Parameters[0].Equals("autoreload", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("ad", StringComparison.OrdinalIgnoreCase))
                {
                    if (config.是否启用自动重启服务器)
                    {
                        if (!args.Player.IsLoggedIn)
                            args.Player.SendSuccessMessage("自动重启计划已关闭");
                        TSPlayer.All.SendSuccessMessage("自动重启计划已关闭");
                        config.是否启用自动重启服务器 = !config.是否启用自动重启服务器;
                        config.SaveConfigFile();
                    }
                    else
                    {
                        if (config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) <= DateTime.Now.AddMinutes(AvoidTime))
                        {
                            args.Player.SendInfoMessage($"自动重启服务器倒计时过短，已关闭自动重启，请至少于上次重启后 {AvoidTime} 分钟内不要重启，避免产生错误");
                        }
                        else
                        {
                            if (!args.Player.IsLoggedIn)
                                args.Player.SendSuccessMessage($"自动重启计划已启用，将在从现在起{HoursToM((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours)}后自动重启");
                            TSPlayer.All.SendSuccessMessage($"自动重启计划已启用，将在从现在起{HoursToM((config.上次重启服务器的日期.AddHours(config.多少小时后开始自动重启服务器) - DateTime.Now).TotalHours, "FF9000")}后自动重启");
                            config.是否启用自动重启服务器 = !config.是否启用自动重启服务器;
                            config.SaveConfigFile();
                        }
                    }
                }
                else if (args.Parameters[0].Equals("view", StringComparison.OrdinalIgnoreCase))
                {
                    //boss进度数据输出
                    string mess = "";
                    if (config.是否自动控制NPC进度)
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
                        mess += args.Player.IsLoggedIn ? "" + "[i:3868]已锁NPC倒计时：\n" : "@已锁NPC倒计时：\n";
                        int count = 0;
                        //把排好序的数据输出
                        foreach (var v in sortpairs)
                        {
                            if (v.Value >= lv)
                            {
                                count++;
                                if (args.Player.IsLoggedIn)
                                    mess += $"[{v.Key}{HoursToM(v.Value - lv, "28FFB8")}] ";
                                else
                                    mess += $"[{v.Key}{HoursToM(v.Value - lv)}] ";
                                if (count == 4)
                                {
                                    mess += "\n";
                                    count = 0;
                                }
                            }
                        }
                        mess = mess.Trim('\n');
                        //对npc封禁进行整理*******************************************************************
                        Dictionary<int, double> keyValuePairsnpc = new Dictionary<int, double>();
                        Dictionary<int, double> sortpairsnpc = new Dictionary<int, double>();
                        foreach (var v in config.NPC封禁时长距开服日期_ID和单位小时)
                        {
                            keyValuePairsnpc.Add(v.Key, v.Value);
                        }
                        //排序
                        while (keyValuePairsnpc.Count > 0)
                        {
                            double min = double.MaxValue;
                            int key = -1;
                            foreach (var v in keyValuePairsnpc)
                            {
                                if (v.Value < min)
                                {
                                    key = v.Key;
                                    min = v.Value;
                                }
                            }
                            if (key != -1)
                            {
                                sortpairsnpc.Add(key, min);
                                keyValuePairsnpc.Remove(key);
                            }
                        }
                        count = 0;
                        mess += "\n";
                        //把排好序的数据输出
                        foreach (var v in sortpairsnpc)
                        {
                            if (v.Value >= lv)
                            {
                                count++;
                                if (args.Player.IsLoggedIn)
                                    mess += $"[({v.Key}){Lang.GetNPCNameValue(v.Key)}{HoursToM(v.Value - lv, "28FFB8")}] ";
                                else
                                    mess += $"[({v.Key}){Lang.GetNPCNameValue(v.Key)}{HoursToM(v.Value - lv)}] ";
                                if (count == 4)
                                {
                                    mess += "\n";
                                    count = 0;
                                }
                            }
                        }
                        if (mess == "[i:3868]已锁NPC倒计时：\n" || mess == "@已锁NPC倒计时：\n")
                        {
                            mess = args.Player.IsLoggedIn ? "" + "[i:3868]已锁NPC倒计时：无\n" : "@已锁NPC倒计时：无\n";
                        }
                    }
                    else
                        mess += args.Player.IsLoggedIn ? "[i:3868]" + MtoM("没有任何NPC进度控制计划", "28FFB8") : "@没有任何NPC进度控制计划";
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


                    //自动生成地图还是挑选备用地图
                    string clock1_mess;
                    if (config.你提供用于重置的地图名称.Count > 0 && File.Exists(config.path() + "/" + config.你提供用于重置的地图名称.First() + ".wld"))
                    {
                        clock1_mess = $"重置后使用提供的地图：{config.你提供用于重置的地图名称.First()}，重置后的最多在线人数：{config.自动重置的最多在线人数}，重置后的端口：{config.自动重置的端口}，是否重置玩家数据：{config.自动重置是否重置玩家数据}，重置后的服务器密码：以配置文件config.json的为准";
                    }
                    else
                    {
                        string temp;
                        if (config.你提供用于重置的地图名称.Count > 0)
                            temp = config.AddNumberFile(CorrectFileName(config.你提供用于重置的地图名称.First()));
                        else
                            temp = config.AddNumberFile(CorrectFileName(config.自动重置的地图名称));
                        clock1_mess =
                        $"重置后的名称：{temp}，重置后的地图大小：{size}，重置后世界的模式：{mode}\n" +
                        $"重置后的种子：{(string.IsNullOrWhiteSpace(config.自动重置的地图种子) ? "随机" : config.自动重置的地图种子)}，重置后的最多在线人数：{config.自动重置的最多在线人数}，重置后的端口：{config.自动重置的端口}\n" +
                        $"重置后的服务器密码：以配置文件config.json的为准，是否自动重置玩家数据：{config.自动重置是否重置玩家数据}";
                    }

                    args.Player.SendInfoMessage(
                        $"{mess}\n" +
                        $"{clock1}\n{clock1_mess}\n" +
                        $"{clock2}\n" +
                        $"重启后的名称：{(string.IsNullOrWhiteSpace(Main.worldName) ? "未设置" : Main.worldName)}，重启后的最多在线人数：{config.自动重启后的最多在线人数}，重启后的端口：{config.自动重启后的端口}\n" +
                        $"重启后的服务器密码：以配置文件config.json的为准\n" +
                        $"{clock3}\n" +
                        $"要执行的指令：{(string.IsNullOrWhiteSpace(mess3) ? "无" : mess3)}");
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
                    config.SaveConfigFile();
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
                if (args.Parameters[0].Equals("ar", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Parameters[1].Equals("help", StringComparison.OrdinalIgnoreCase))
                    {
                        args.Player.SendMessage(
                            "输入 /supco ar name [string]   来设置自动重置地图的地图名字\n" +
                            "输入 /supco ar size [小1/中2/大3(只能填数字)]   来设置下次重置时地图的大小\n" +
                            "输入 /supco ar mode [普通0/专家1/大师2/旅途3(只能填数字)]   来设置下次重置地图时的模式\n" +
                            "输入 /supco ar seed [string]   来设置下次重置地图时的地图种子\n" +
                            "输入 /supco ar maxplayers [num]   来设置下次重置地图时的最多在线玩家\n" +
                            "输入 /supco ar resetplayers [0/1]   来设置下次重置地图时是否清理玩家数据，0代表不清理\n" +
                            "输入 /supco ar port [num]   来设置下次重置地图时的端口\n" +
                            "输入 /supco ar password [string]   来设置下次重置地图时的密码\n" +
                            "输入 /supco ar addname [string]   来添加你自己提供用来重置的地图的名称\n" +
                            "输入 /supco ar delname [string]   来删除你自己提供用来重置的地图的名称\n" +
                            "输入 /supco ar listname   来列出你提供的所有地图名称"
                            , TextColor());
                    }
                    else if (args.Parameters[1].Equals("listname", StringComparison.OrdinalIgnoreCase))
                    {
                        string text = "";
                        if (config.你提供用于重置的地图名称.Count == 0)
                        {
                            text += "你没有提供任何名字，服务器将按<自动重置的地图名称>生成地图";
                        }
                        else
                        {
                            int count = 1;
                            foreach (var v in config.你提供用于重置的地图名称)
                            {
                                if (File.Exists(config.path() + "/" + v + ".wld"))
                                {
                                    text += $"{v} 地图存在，第 {count} 次重置地图将自动调用\n";
                                }
                                else
                                {
                                    string name = config.AddNumberFile(CorrectFileName(v));
                                    text += $"{name} 地图不存在，第 {count} 次重置地图将自动生成\n";
                                }
                                count++;
                            }
                        }
                        text = text.Trim('\n');
                        args.Player.SendInfoMessage(text);
                    }
                    else
                        args.Player.SendInfoMessage("输入 /supco ar help 来获取该插件的帮助");
                }
                else if (args.Parameters[0].Equals("ad", StringComparison.OrdinalIgnoreCase) && args.Parameters[1].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    args.Player.SendMessage(
                        "输入 /supco ad maxplayers [num]   来设置下次重启地图时的最多在线玩家\n" +
                        "输入 /supco ad maxplayers [num]   来设置下次重启地图时的端口\n" +
                        "输入 /supco ad password [string]   来设置下次重启地图时的密码"
                        , TextColor());
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
                                thread_reset.Start();
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
                                thread_reload.Start();
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
                if (args.Parameters[0].Equals("ar", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Parameters[1].Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        string worldname = CorrectFileName(args.Parameters[2]);
                        if (args.Parameters[2] != worldname)
                            args.Player.SendErrorMessage("你输入的地图名称带有非法字符，已将非法字符过滤，改为：" + worldname);
                        config.自动重置的地图名称 = worldname;
                        config.SaveConfigFile();
                        args.Player.SendSuccessMessage("修改成功，自动重置的地图名称修改为：" + worldname);
                    }
                    else if (args.Parameters[1].Equals("size", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(args.Parameters[2], out int num) && (num == 1 || num == 2 || num == 3))
                        {
                            if (num == 1)
                                args.Player.SendSuccessMessage("下次重置地图的大小成功修改为：小");
                            else if (num == 2)
                                args.Player.SendSuccessMessage("下次重置地图的大小成功修改为：中");
                            else
                                args.Player.SendSuccessMessage("下次重置地图的大小成功修改为：大");
                            config.自动重置的地图大小_小1_中2_大3 = num;
                            config.SaveConfigFile();
                        }
                        else
                            args.Player.SendInfoMessage("请输入数字1,2或3");
                    }
                    else if (args.Parameters[1].Equals("mode", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(args.Parameters[2], out int num) && (num == 0 || num == 1 || num == 2 || num == 3))
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
                            config.SaveConfigFile();
                        }
                        else
                            args.Player.SendInfoMessage("请输入数字0,1,2或3");
                    }
                    else if (args.Parameters[1].Equals("seed", StringComparison.OrdinalIgnoreCase))
                    {
                        args.Player.SendSuccessMessage("下次重置地图的种子成功修改为：" + args.Parameters[2]);
                        config.自动重置的地图种子 = args.Parameters[2];
                        config.SaveConfigFile();
                    }
                    else if (args.Parameters[1].Equals("maxplayers", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(args.Parameters[2], out int num) && num > 0 && num < 200)
                        {
                            args.Player.SendSuccessMessage("下次重置地图的玩家上限成功修改为：" + num);
                            config.自动重置的最多在线人数 = num;
                            config.SaveConfigFile();
                        }
                        else
                            args.Player.SendInfoMessage("请输入整数，不要输入负数，数字不要过大");
                    }
                    else if (args.Parameters[1].Equals("resetplayers", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(args.Parameters[2], out int num) && (num == 0 || num == 1))
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
                            config.SaveConfigFile();
                        }
                        else
                            args.Player.SendInfoMessage("请输入数字0或1");
                    }
                    else if (args.Parameters[1].Equals("port", StringComparison.OrdinalIgnoreCase))
                    {
                        config.自动重置的端口 = args.Parameters[2];
                        config.SaveConfigFile();
                        args.Player.SendSuccessMessage("下次重置地图时的端口为：" + args.Parameters[2]);
                    }
                    else if (args.Parameters[1].Equals("password", StringComparison.OrdinalIgnoreCase))
                    {
                        config.自动重置的密码 = args.Parameters[2];
                        config.SaveConfigFile();
                        args.Player.SendSuccessMessage("下次重置服务器的密码修改成功");
                    }
                    else if (args.Parameters[1].Equals("addname", StringComparison.OrdinalIgnoreCase))
                    {
                        string worldname = CorrectFileName(args.Parameters[2]);
                        if (args.Parameters[2] != worldname)
                            args.Player.SendErrorMessage("你输入的地图名称带有非法字符，已将非法字符过滤，改为：" + worldname);
                        if (config.你提供用于重置的地图名称.Add(worldname))
                        {
                            if (File.Exists(config.path() + "/" + worldname + ".wld"))
                                args.Player.SendSuccessMessage($"{worldname} 添加成功，该名称的地图存在，重置时将直接读取");
                            else
                                args.Player.SendSuccessMessage($"{worldname} 添加成功，该名称的地图不存在，重置将生成一个");
                            config.SaveConfigFile();
                        }
                        else
                            args.Player.SendInfoMessage("添加失败，该名称已存在");
                    }
                    else if (args.Parameters[1].Equals("delname", StringComparison.OrdinalIgnoreCase))
                    {
                        string worldname = CorrectFileName(args.Parameters[2]);
                        if (args.Parameters[2] != worldname)
                            args.Player.SendErrorMessage("你输入的地图名称带有非法字符，已将非法字符过滤，改为：" + worldname);
                        if (config.你提供用于重置的地图名称.Remove(worldname))
                        {
                            args.Player.SendSuccessMessage($"删除成功");
                            config.SaveConfigFile();
                        }
                        else
                            args.Player.SendInfoMessage("删除失败，该名称可能不存在");
                    }
                    else
                        args.Player.SendInfoMessage("输入 /supco ar help 来获取该插件的帮助");
                }
                else if (args.Parameters[0].Equals("ad", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Parameters[1].Equals("maxplayers", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(args.Parameters[2], out int num) && num > 0 && num < 200)
                        {
                            args.Player.SendSuccessMessage("下次重启地图的玩家上限成功修改为：" + num);
                            config.自动重启后的最多在线人数 = num;
                            config.SaveConfigFile();
                        }
                        else
                            args.Player.SendInfoMessage("请输入整数，不要输入负数，数字不要过大");
                    }
                    else if (args.Parameters[1].Equals("port", StringComparison.OrdinalIgnoreCase))
                    {
                        config.自动重启后的端口 = args.Parameters[2];
                        config.SaveConfigFile();
                        args.Player.SendSuccessMessage("下次重启地图时的端口为：" + args.Parameters[2]);
                    }
                    else if (args.Parameters[1].Equals("password", StringComparison.OrdinalIgnoreCase))
                    {
                        config.自动重启后的密码 = args.Parameters[2];
                        config.SaveConfigFile();
                        args.Player.SendSuccessMessage("下次重置服务器的密码修改成功");
                    }
                    else
                        args.Player.SendInfoMessage("输入 /supco ad help 来获取该插件的帮助");
                }
                else if (args.Parameters[0].Equals("offset", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("os", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Parameters[1].Equals("autoreset", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("ar", StringComparison.OrdinalIgnoreCase))
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
                                config.SaveConfigFile();
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
                    else if (args.Parameters[1].Equals("autoreload", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("ad", StringComparison.OrdinalIgnoreCase))
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
                                config.SaveConfigFile();
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
            {
                TShock.Players.ForEach(x =>
                {
                    if (x != null && x.IsLoggedIn)
                    {
                        x.SaveServerCharacter();
                        x.Kick("服务器正在重置", true);
                    }
                });
            }
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
            TShock.Log.Info("服务器正在重置——来自插件：计划书ProgressControl");
           

            config.开服日期 = DateTime.Now;
            config.上次重启服务器的日期 = DateTime.Now;
            config.上次自动执行指令的日期 = DateTime.Now;

            //如果 你有提供预备名字 且 这个预备名字有对应地图，则直接调用
            if (config.你提供用于重置的地图名称.Count > 0 && File.Exists(config.path() + "/" + config.你提供用于重置的地图名称.First() + ".wld"))
            {
                string worldname = config.你提供用于重置的地图名称.First();
                TShock.Config.Settings.ServerPassword = config.自动重置的密码;
                TShock.Config.Settings.MaxSlots = config.自动重置的最多在线人数;
                Config.SaveTConfig();
                //成功后删掉预备名字
                config.你提供用于重置的地图名称.Remove(worldname);
                config.SaveConfigFile();
                Process.Start(Path.Combine(Environment.CurrentDirectory, "Tshock.Server.exe"),
                    $"-lang 7 -world {config.path()}/{worldname}.wld -maxplayers {TShock.Config.Settings.MaxSlots} -port {config.自动重置的端口} -c");
            }
            //否则生成一个
            else
            {
                string worldname;
                if (config.你提供用于重置的地图名称.Count > 0)
                {
                    worldname = config.AddNumberFile(CorrectFileName(config.你提供用于重置的地图名称.First()));
                    config.你提供用于重置的地图名称.Remove(config.你提供用于重置的地图名称.First());
                    config.SaveConfigFile();
                }
                else
                {
                    worldname = config.AddNumberFile(CorrectFileName(config.自动重置的地图名称));
                }
                //将密码和最多在线人数写入配置文件中。
                //???: 启动参数里端口的修改有效，密码的修改无效，人数的修改仅重启第一次有效。密码和人数都会强制参考config.json的内容，为了修改成功，只能先写入config.json了
                TShock.Config.Settings.ServerPassword = config.自动重置的密码;//密码这个东西恒参考config.json，在启动参数里改无效
                TShock.Config.Settings.MaxSlots = config.自动重置的最多在线人数;//端口这个东西恒参考启动参数，我就不改config.json里的了（我知道这一行不是端口，但我就是要把注释写在这）
                Config.SaveTConfig();
                Process.Start(Path.Combine(Environment.CurrentDirectory, "Tshock.Server.exe"),
                    $"-lang 7 -autocreate {config.自动重置的地图大小_小1_中2_大3} -seed {config.自动重置的地图种子} -world {config.path()}/{worldname} -difficulty {config.自动重置的地图难度_普通0_专家1_大师2_旅途3} -maxplayers {config.自动重置的最多在线人数} -port {config.自动重置的端口} -c");
            }
            Console.Clear();
            Netplay.Disconnect = true;
            Environment.Exit(0); //暴力关服处理，我知道这样做不合理，但是tshock自带的关服函数会失败
        }


        /// <summary>
        /// 重启游戏
        /// </summary>
        private static void RestartGame()
        {
            config.上次重启服务器的日期 = DateTime.Now;
            config.SaveConfigFile();
            config.自动重启前执行的指令_不需要加斜杠.ForEach(x => { Commands.HandleCommand(TSPlayer.Server, "/" + x.Trim('/', '.')); });
            TShock.Utils.SaveWorld();
            TShock.Players.ForEach(x =>
            {
                if (x != null && x.IsLoggedIn)
                {
                    x.SaveServerCharacter();
                    x.Kick("服务器需要重启", true);
                }
            });
            //将密码和最多在线人数写入配置文件中。
            //bug: 启动参数里端口的修改有效，密码的修改无效，人数的修改仅重启第一次有效。密码和人数都会强制参考config.json的内容，为了修改成功，只能先写入config.json了
            TShock.Config.Settings.ServerPassword = config.自动重启后的密码;//密码这个东西恒参考config.json，在启动参数里改无效
            TShock.Config.Settings.MaxSlots = config.自动重启后的最多在线人数;//端口这个东西恒参考启动参数，我就不改config.json里的了（我知道这一行不是端口）
            Config.SaveTConfig();
            TShock.Log.Info("服务器正在重启——来自插件：计划书ProgressControl");
            Console.Clear();
            Process.Start(Path.Combine(Environment.CurrentDirectory, "Tshock.Server.exe"),
                $"-lang 7 -world {Main.worldPathName} -maxplayers {config.自动重启后的最多在线人数} -port {config.自动重启后的端口} -c");
            Netplay.Disconnect = true;
            Environment.Exit(0); //暴力关服处理，我知道这样做不合理，但是tshock自带的关服函数会失败
        }


        /// <summary>
        /// 自动执行指令
        /// </summary>
        private static void AutoCommands()
        {
            config.上次自动执行指令的日期 = DateTime.Now;
            config.SaveConfigFile();

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
            try
            {
                TShock.Log.Info("服务器自动执行指令成功");
                if (config.多少小时后开始自动执行指令 * 3600 >= 60)//如果指令执行的频率过高，那就不用向玩家发送了，免得堆积信息
                    TSPlayer.All.SendSuccessMessage("服务器自动执行指令成功");
                else
                    Console.WriteLine("指令执行频率过高，将不会对游戏内玩家发送广播");
                Console.WriteLine("服务器自动执行指令成功");
            }
            catch { }
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

                        "输入 /pco autonpc   自动控制NPC和Boss进度计划启用，再次使用则关闭\n" +
                        "输入 /pco os autoboss [±num]   来将自动控制Boss(不包含额外设置的NPC)的解锁时刻推迟或提前num小时，num可为小数\n" +
                        "输入 /pco npc add [id/name] [num]   来添加或更新一个NPC的封禁限制\n" +
                        "输入 /pco npc del [id/name]   来删除一个NPC的封禁限制\n" +

                        "输入 /pco autocom   自动执行指令计划启用，再次使用关闭\n" +
                        "输入 /pco os autocom [±num]   将自动执行指令的时间推迟或提前num小时，num可为小数\n" +
                        "输入 /pco com [±num]   手动执行指令计划启用，在num秒后开始执行，若num不填则立刻执行，num小于0则关闭当前存在的手动计划，其优先级大于自动执行指令\n" +
                        "输入 /pco com add [string]   来添加一个要执行的指令，包含自动和手动计划\n" +
                        "输入 /pco com del [string]   来删除一个要执行的指令，包含自动和手动计划\n" +
                        "输入 /supco help   来查看高级自动控制指令", TextColor());
                }
                else if (args.Parameters[0].Equals("view", StringComparison.OrdinalIgnoreCase))
                {
                    string othermess = "";
                    //boss进度数据输出
                    if (config.是否自动控制NPC进度)
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
                        //npc的************************************************************************************
                        Dictionary<int, double> keyValuePairsnpc = new Dictionary<int, double>();
                        Dictionary<int, double> sortpairsnpc = new Dictionary<int, double>();
                        foreach (var v in config.NPC封禁时长距开服日期_ID和单位小时)
                        {
                            keyValuePairsnpc.Add(v.Key, v.Value);
                        }
                        //排序
                        while (keyValuePairsnpc.Count > 0)
                        {
                            double min = double.MaxValue;
                            int key = -1;
                            foreach (var v in keyValuePairsnpc)
                            {
                                if (v.Value < min)
                                {
                                    key = v.Key;
                                    min = v.Value;
                                }
                            }
                            if (key != -1)
                            {
                                sortpairsnpc.Add(key, min);
                                keyValuePairsnpc.Remove(key);
                            }
                        }
                        //把排好序的数据输出
                        foreach (var v in sortpairsnpc)
                        {
                            if (v.Value >= lv)
                            {
                                if (args.Player.IsLoggedIn)
                                    mess += $"\n[{v.Key}]{Lang.GetNPCNameValue(v.Key)} 还剩{HoursToM(v.Value - lv, "28FFB8")}解锁";
                                else
                                    mess += $"\n[{v.Key}]{Lang.GetNPCNameValue(v.Key)} 还剩{HoursToM(v.Value - lv)}解锁";
                            }
                        }
                        args.Player.SendMessage(mess, TextColor());
                    }
                    else
                        othermess += args.Player.IsLoggedIn ? "[i:3868]" + MtoM("NPC进度控制未开启", "28FFB8") + "\n" : "NPC进度控制未开启\n";

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
                    if (!args.Player.HasPermission("pco.admin") && !args.Player.HasPermission("pco.superadmin"))
                    {
                        args.Player.SendInfoMessage("权限不足！[pco.admin]");
                        return;
                    }
                    config.开服日期 = DateTime.Now;
                    config.上次重启服务器的日期 = DateTime.Now;
                    config.上次自动执行指令的日期 = DateTime.Now;
                    config.SaveConfigFile();
                    args.Player.SendSuccessMessage("定位成功，所有已开启的自动计划将从现在开始计时");
                }
                else if (args.Parameters[0].Equals("autonpc", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("an", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin") && !args.Player.HasPermission("pco.superadmin"))
                    {
                        args.Player.SendInfoMessage("权限不足！[pco.admin]");
                        return;
                    }
                    if (config.是否自动控制NPC进度)
                        args.Player.SendSuccessMessage("已取消Boss的封禁限制计划");
                    else
                        args.Player.SendSuccessMessage("已开启Boss的封禁限制计划");
                    config.是否自动控制NPC进度 = !config.是否自动控制NPC进度;
                    config.SaveConfigFile();
                }
                else if (args.Parameters[0].Equals("autocom", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("ac", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin") && !args.Player.HasPermission("pco.superadmin"))
                    {
                        args.Player.SendInfoMessage("权限不足！[pco.admin]");
                        return;
                    }
                    if (!args.Player.HasPermission("pco.superadmin"))
                    {
                        config.自动执行的指令_不需要加斜杠.ForEach(x =>
                        {
                            if (x.Equals("supco", StringComparison.OrdinalIgnoreCase))
                            {
                                args.Player.SendInfoMessage("权限不足[pco.superadmin]，不能调整含有supco系列指令的自动计划");
                                return;
                            }
                        });
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
                    config.SaveConfigFile();
                }
                else if (args.Parameters[0].Equals("com", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin") && !args.Player.HasPermission("pco.superadmin"))
                    {
                        args.Player.SendInfoMessage("权限不足！[pco.admin]");
                    }
                    if (!args.Player.HasPermission("pco.superadmin"))
                    {
                        config.自动执行的指令_不需要加斜杠.ForEach(x =>
                        {
                            if (x.Equals("supco", StringComparison.OrdinalIgnoreCase))
                            {
                                args.Player.SendInfoMessage("权限不足[pco.superadmin]，不能发起含有supco系列指令的手动计划");
                                return;
                            }
                        });
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
                    if (!args.Player.HasPermission("pco.admin") && !args.Player.HasPermission("pco.superadmin"))
                    {
                        args.Player.SendInfoMessage("权限不足！[pco.admin]");
                        return;
                    }
                    if (!args.Player.HasPermission("pco.superadmin"))
                    {
                        config.自动执行的指令_不需要加斜杠.ForEach(x =>
                        {
                            if (x.Equals("supco", StringComparison.OrdinalIgnoreCase))
                            {
                                args.Player.SendInfoMessage("权限不足[pco.superadmin]，不能发起含有supco系列指令的手动计划");
                                return;
                            }
                        });
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
                if (args.Parameters[0].Equals("offset", StringComparison.OrdinalIgnoreCase) || args.Parameters[0].Equals("os", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin") && !args.Player.HasPermission("pco.superadmin"))
                    {
                        args.Player.SendInfoMessage("权限不足！[pco.admin]");
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
                            config.SaveConfigFile();
                            string st;
                            if (addtime > 0)
                                st = "推迟" + (args.Player.IsLoggedIn ? HoursToM(addtime, "28FFB8") : HoursToM(addtime));
                            else if (addtime < 0)
                                st = "提前" + (args.Player.IsLoggedIn ? HoursToM(-1 * addtime, "28FFB8") : HoursToM(-1 * addtime));
                            else
                                st = "正常";
                            if (!config.是否自动控制NPC进度)
                                args.Player.SendErrorMessage("警告，未开启自动控制Boss进度计划，你的修改不会有任何效果");
                            else
                                TSPlayer.All.SendSuccessMessage($"定位成功，Boss将{st}解锁");
                            if (!args.Player.IsLoggedIn)
                                args.Player.SendSuccessMessage($"定位成功，Boss将{st}解锁");
                        }
                        else if (args.Parameters[1].Equals("autocom", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!args.Player.HasPermission("pco.superadmin"))
                            {
                                config.自动执行的指令_不需要加斜杠.ForEach(x =>
                                {
                                    if (x.Equals("supco", StringComparison.OrdinalIgnoreCase))
                                    {
                                        args.Player.SendInfoMessage("权限不足[pco.superadmin]，不能对含有supco系列指令的自动计划进行时间修改");
                                        return;
                                    }
                                });
                            }
                            config.多少小时后开始自动执行指令 += addtime;
                            config.SaveConfigFile();
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
                else if (args.Parameters[0].Equals("com", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin") && !args.Player.HasPermission("pco.superadmin"))
                    {
                        args.Player.SendInfoMessage("权限不足！[pco.admin]");
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
                        if (mess.Contains("supco", StringComparison.OrdinalIgnoreCase) && !args.Player.HasPermission("pco.superadmin"))
                        {
                            args.Player.SendInfoMessage("权限不足[pco.superadmin]，不能添加supco系列指令");
                        }
                        else
                        {
                            if (config.自动执行的指令_不需要加斜杠.Add(mess))
                            {
                                args.Player.SendSuccessMessage($"已将指令 /{mess} 添加成功！");
                                config.SaveConfigFile();
                            }
                            else
                                args.Player.SendErrorMessage("添加失败，请检查是否已添加过");
                        }
                    }
                    else if (args.Parameters[1].Equals("del", StringComparison.OrdinalIgnoreCase))
                    {
                        string mess = "";
                        for (int i = 2; i < args.Parameters.Count; i++)
                        {
                            mess += args.Parameters[i].Trim('/', '.') + " ";
                        }
                        mess = mess.Trim(' ');
                        if (mess.Contains("supco", StringComparison.OrdinalIgnoreCase) && !args.Player.HasPermission("pco.superadmin"))
                        {
                            args.Player.SendInfoMessage("权限不足[pco.superadmin]，不能删除supco系列指令");
                        }
                        else
                        {
                            if (config.自动执行的指令_不需要加斜杠.Remove(mess))
                            {
                                args.Player.SendSuccessMessage($"已将指令 /{mess} 删除成功！");
                                config.SaveConfigFile();
                            }
                            else
                                args.Player.SendErrorMessage("删除失败，请检查是否存在该指令");
                        }
                    }
                    else
                        args.Player.SendInfoMessage("输入 /pco help 来获取该插件的帮助");
                }
                else if (args.Parameters[0].Equals("npc", StringComparison.OrdinalIgnoreCase))
                {
                    if (!args.Player.HasPermission("pco.admin") && !args.Player.HasPermission("pco.superadmin"))
                    {
                        args.Player.SendInfoMessage("权限不足！[pco.admin]");
                        return;
                    }
                    if (args.Parameters[1].Equals("add", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Parameters.Count != 4)
                        {
                            args.Player.SendInfoMessage("输入 /pco help 来获取该插件的帮助");
                            return;
                        }
                        List<NPC> npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[2]);
                        if (npcs.Count > 1)
                        {
                            int cout = 0;
                            string text = "目标过多，你想查找的是？\n";
                            foreach (var v in npcs)
                            {
                                cout++;
                                if (cout == 10)
                                {
                                    text += $"{v.FullName}:{v.netID}, \n";
                                    cout = 0;
                                }
                                else
                                    text += $"{v.FullName}:{v.netID}, ";
                            }
                            text = text.Trim(' ', ',', '\n');
                            text = text.Trim(' ', ',', '\n');
                            text = text.Trim(' ', ',', '\n');
                            args.Player.SendInfoMessage(text);
                        }
                        else if (npcs.Count == 1)
                        {
                            if (double.TryParse(args.Parameters[3], out double num))
                            {
                                if (config.NPC封禁时长距开服日期_ID和单位小时.TryGetValue(npcs[0].netID, out double temp))
                                {
                                    config.NPC封禁时长距开服日期_ID和单位小时[npcs[0].netID] = num;
                                }
                                else
                                {
                                    config.NPC封禁时长距开服日期_ID和单位小时.Add(npcs[0].netID, num);
                                }
                                config.SaveConfigFile();
                                double d = (config.开服日期.AddHours(num) - DateTime.Now).TotalHours;
                                if (d > 0)
                                    args.Player.SendSuccessMessage($"NPC:{npcs[0].FullName} 已更新成功，将在从现在起{HoursToM(d, "28FFB8")}后解锁");
                                else
                                    args.Player.SendSuccessMessage($"NPC:{npcs[0].FullName} 已更新成功，且目前已解锁");
                            }
                            else
                                args.Player.SendInfoMessage("num应为小数");
                        }
                        else
                            args.Player.SendInfoMessage("未找到该生物");
                    }
                    else if (args.Parameters[1].Equals("del", StringComparison.OrdinalIgnoreCase))
                    {
                        List<NPC> npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[2]);
                        if (npcs.Count == 1)
                        {
                            if (config.NPC封禁时长距开服日期_ID和单位小时.Remove(npcs[0].netID))
                            {
                                config.SaveConfigFile();
                                args.Player.SendSuccessMessage($"NPC:{npcs[0].FullName} 已删除成功");
                            }
                            else
                            {
                                args.Player.SendSuccessMessage($"NPC:{npcs[0].FullName} 未在计划中找到，删除失败");
                            }
                        }
                        else if (npcs.Count > 1)
                        {
                            int cout = 0;
                            string text = "目标过多，你想查找的是？\n";
                            foreach (var v in npcs)
                            {
                                cout++;
                                if (cout == 10)
                                {
                                    text += $"{v.FullName}:{v.netID}, \n";
                                    cout = 0;
                                }
                                else
                                    text += $"{v.FullName}:{v.netID}, ";
                            }
                            text = text.Trim(' ', ',', '\n');
                            text = text.Trim(' ', ',', '\n');
                            text = text.Trim(' ', ',', '\n');
                            args.Player.SendInfoMessage(text);
                        }
                        else
                            args.Player.SendInfoMessage("未找到该生物");
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
            if (!config.是否自动控制NPC进度 || args == null || args.Npc == null || !args.Npc.active || Main.time % 2 != 0)
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

            if ((int)Main.time % 5 == 0 && config.NPC封禁时长距开服日期_ID和单位小时.TryGetValue(args.Npc.netID, out double num))
            {
                TimeSpan span = config.开服日期.AddHours(num) - DateTime.Now;
                if (span.TotalHours >= 0)
                {
                    args.Npc.active = false;
                    args.Npc.type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", args.Npc.whoAmI);
                    //TSPlayer.All.SendInfoMessage($"{args.Npc.FullName} 未到解锁时间，还剩{HoursToM(span.TotalHours, "28FFB8")}");
                }
            }
        }


        /// <summary>
        /// npc受击的
        /// </summary>
        /// <param name="args"></param>
        private void NPCStrike(NpcStrikeEventArgs args)
        {
            if (!config.是否自动控制NPC进度 || args == null || args.Npc == null || !args.Npc.active)
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
        private void Function(NPC npc, string bossname, bool enableBC = true)
        {
            double jiange = (DateTime.Now - config.开服日期).TotalHours;
            if (jiange < config.Boss封禁时长距开服日期_单位小时[bossname])
            {
                npc.active = false;
                npc.type = 0;
                TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npc.whoAmI);
                if (enableBC)
                    TSPlayer.All.SendInfoMessage($"{npc.FullName} 未到解锁时间，还剩{HoursToM(config.Boss封禁时长距开服日期_单位小时[bossname] - jiange, "28FFB8")}");
            }
        }


        /// <summary>
        /// 随机返回一个颜色
        /// </summary>
        /// <returns></returns>
        private static Color TextColor()
        {
            int r, g, b;
            r = Main.rand.Next(100, 255);
            g = Main.rand.Next(100, 255);
            b = Main.rand.Next(100, 255);
            return new Color(r, g, b);
        }


        /// <summary>
        /// 将 xxh 转化为 { xx.xxx 时 xx 分 xx 秒 }，数字用彩色强调，颜色不填时只返回纯文本
        /// </summary>
        /// <param name="h"></param>
        /// <param name="color"> 修改数字的颜色,不填时默认原色，泰拉游戏中的颜色 [c/十六进制:文本]，只用填十六进制即可 </param>
        /// <returns></returns>
        private static string HoursToM(double hours, string color = "")
        {
            string mess = "";
            int h, s, m; //时分秒
            h = (int)hours;
            m = (int)(hours % 1 * 60);//分
            s = (int)(hours % 1 * 60 % 1 * 60);//秒
            if (!string.IsNullOrWhiteSpace(color))
            {
                if (h > 0)
                    mess += $" [c/{color}:{h}] 时";
                if (m > 0)
                    mess += $" [c/{color}:{m}] 分";
                if (s > 0 || h == 0 && m == 0 && s == 0)
                    mess += $" [c/{color}:{s}] 秒";
            }
            else
            {
                if (h > 0)
                    mess += $" {h} 时";
                if (m > 0)
                    mess += $" {m} 分";
                if (s > 0 || h == 0 && m == 0 && s == 0)
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
        private static string MtoM(string m, string color = "")
        {
            if (color == "")
                return m;
            else
                return $"[c/{color}:{m}]";
        }


        /// <summary>
        /// 删除文件名中的非法字符
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string CorrectFileName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "World";
            for (int i = 0; i < name.Length; ++i)
            {
                bool flag = name[i] == '\\' || name[i] == '/' || name[i] == ':' || name[i] == '*' || name[i] == '?' || name[i] == '"' || name[i] == '<' || name[i] == '>' || name[i] == '|';
                if (flag)
                {
                    name = name.Remove(i, 1);
                    i--;
                }
            }
            return name;
        }
    }
}
