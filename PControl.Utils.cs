﻿using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Reflection;
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
                    if (x != null)
                    {
                        if (x.IsLoggedIn)
                            x.SaveServerCharacter();
                        x.Disconnect("服务器正在重置");
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

            /*
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
            */

            config.开服日期 = DateTime.Now;
            config.上次重启服务器的日期 = DateTime.Now;
            config.上次自动执行指令的日期 = DateTime.Now;

            //如果 你有提供预备名字 且 这个预备名字有对应地图，则直接调用
            if (config.你提供用于重置的地图名称.Count > 0 && ExistWorldNamePlus(config.你提供用于重置的地图名称.First(), out string worldname))
            {
                TShock.Config.Settings.ServerPassword = config.自动重置后的服务器密码;
                TShock.Config.Settings.MaxSlots = config.自动重置后的最多在线人数;
                Config.SaveTConfig();
                //成功后删掉预备名字
                config.你提供用于重置的地图名称.Remove(config.你提供用于重置的地图名称.First());
                config.SaveConfigFile();
                //这里worldname必须加.wld后缀
                Process.Start(Path.Combine(Environment.CurrentDirectory, "Tshock.Server.exe"),
                    $"-lang 7 -world {config.path()}/{worldname}.wld -maxplayers {TShock.Config.Settings.MaxSlots} -port {config.自动重置后的端口} -c");
            }
            //否则生成一个
            else
            {
                if (config.你提供用于重置的地图名称.Count > 0)
                {
                    worldname = config.AddNumberFile(CorrectFileName(config.你提供用于重置的地图名称.First()));
                    config.你提供用于重置的地图名称.Remove(config.你提供用于重置的地图名称.First());
                }
                else
                {
                    worldname = config.AddNumberFile(CorrectFileName(config.自动重置后的地图名称));
                }
                //将密码和最多在线人数写入配置文件中。
                //???: 启动参数里端口的修改有效，密码的修改无效，人数的修改仅重启第一次有效。密码和人数都会强制参考config.json的内容，为了修改成功，只能先写入config.json了
                TShock.Config.Settings.ServerPassword = config.自动重置后的服务器密码;//密码这个东西恒参考config.json，在启动参数里改无效
                TShock.Config.Settings.MaxSlots = config.自动重置后的最多在线人数;//端口这个东西恒参考启动参数，我就不改config.json里的了（我知道这一行不是端口，但我就是要把注释写在这）
                Config.SaveTConfig();
                config.SaveConfigFile();//需要保存下，保存对开服时间等的修改
                //-autocreate可以不用加.wld后缀
                Process.Start(Path.Combine(Environment.CurrentDirectory, "Tshock.Server.exe"),
                    $"-lang 7 -autocreate {config.自动重置后的地图大小_小1_中2_大3} -seed {config.自动重置后的地图种子} -world {config.path()}/{worldname} -difficulty {config.自动重置后的地图难度_普通0_专家1_大师2_旅途3} -maxplayers {config.自动重置后的最多在线人数} -port {config.自动重置后的端口} -c");
            }

            try//关闭serverlog
            {
                PropertyInfo? property = ServerApi.LogWriter.GetType().GetProperty("DefaultLogWriter", BindingFlags.Instance | BindingFlags.NonPublic);
                ServerLogWriter? serverLogWriter = (property != null) ? (ServerLogWriter?)property.GetValue(ServerApi.LogWriter) : null;
                serverLogWriter?.Dispose();
            }
            catch { }
            //关闭restapi,只有你开启了才会起效
            try
            {
                TShock.RestApi.Dispose();
            }
            catch { }
            TShock.ShuttingDown = true;
            Netplay.Disconnect = true;
            Environment.Exit(0); //暴力关服处理，我知道这样做不合理，但是tshock自带的关服函数强制保存一遍地图，鬼知道为什么save改成false还会保存，无法实现自动删图
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
        /// 重启游戏
        /// </summary>
        private static void RestartGame()
        {
            config.上次重启服务器的日期 = DateTime.Now;
            config.SaveConfigFile();
            config.自动重启前执行的指令_不需要加斜杠.ForEach(x => { Commands.HandleCommand(TSPlayer.Server, "/" + x.Trim('/', '.')); });
            TShock.Players.ForEach(x =>
            {
                if (x != null)
                {
                    if (x.IsLoggedIn)
                        x.SaveServerCharacter();
                    x.Disconnect("服务器正在重启");
                }
            });
            TShock.Utils.SaveWorld();
            //将密码和最多在线人数写入配置文件中。
            //bug: 启动参数里端口的修改有效，密码的修改无效，人数的修改仅重启第一次有效。密码和人数都会强制参考config.json的内容，为了修改成功，只能先写入config.json了
            TShock.Config.Settings.ServerPassword = config.自动重启后的服务器密码;//密码这个东西恒参考config.json，在启动参数里改无效
            TShock.Config.Settings.MaxSlots = config.自动重启后的最多在线人数;//端口这个东西恒参考启动参数，我就不改config.json里的了（我知道这一行不是端口）
            Config.SaveTConfig();
            TShock.Log.Info("服务器正在重启——来自插件：计划书ProgressControl");
            try//关闭日志占用
            {
                PropertyInfo? property = ServerApi.LogWriter.GetType().GetProperty("DefaultLogWriter", BindingFlags.Instance | BindingFlags.NonPublic);
                ServerLogWriter? serverLogWriter = (property != null) ? (ServerLogWriter?)property.GetValue(ServerApi.LogWriter) : null;
                serverLogWriter?.Dispose();
            }
            catch { }
            try//关闭restapi,只有你开启了才会起效
            {
                TShock.RestApi.Dispose();
            }
            catch { }
            //Main.worldPathName是有.wld后缀的
            Process.Start(Path.Combine(Environment.CurrentDirectory, "Tshock.Server.exe"),
                $"-lang 7 -world {Main.worldPathName} -maxplayers {config.自动重启后的最多在线人数} -port {config.自动重启后的端口} -c");
            Netplay.Disconnect = true;
            TShock.ShuttingDown = true;
            Environment.Exit(0);
        }


        /// <summary>
        /// 自动执行指令
        /// </summary>
        private static void ActiveCommands()
        {
            config.上次自动执行指令的日期 = DateTime.Now;
            if (config.多少小时后开始自动执行指令 < 0)
                config.多少小时后开始自动执行指令 = 0;
            config.SaveConfigFile();

            config.自动执行的指令_不需要加斜杠.ForEach(x =>
            {
                if (x.Contains("pco com", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("请不要对自动执行的指令进行套娃！[" + "/" + x + "]");
                    TShock.Log.Warn("请不要对自动执行的指令进行套娃！[" + "/" + x + "]");
                    TSPlayer.All.SendErrorMessage("请不要对自动执行的指令进行套娃！[" + "/" + x + "]");
                    return;
                }
                try
                {
                    Commands.HandleCommand(TSPlayer.Server, "/" + x.Trim('/', '.'));
                }
                catch { }
            });
            try
            {
                if (config.执行指令时是否发广播_解决指令执行频繁刷屏的问题)
                {
                    TShock.Log.Info("服务器执行指令成功——来自插件：计划书ProgressControl");
                    Console.WriteLine("服务器自动执行指令成功");
                    TSPlayer.All.SendInfoMessage("服务器自动执行指令成功");
                }
            }
            catch { }
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
        /// 随机返回一个颜色，网上抄的
        /// </summary>
        /// <returns></returns>
        private static Color TextColor()
        {
            int Hue, Saturation, Lightness;
            Hue = Main.rand.Next(0, 360);
            Saturation = Main.rand.Next(80, 245);
            Lightness = Main.rand.Next(180, 255);

            double num4 = 0.0;
            double num5 = 0.0;
            double num6 = 0.0;
            double num = Hue % 360.0;
            double num2 = Saturation / 255.0;
            double num3 = Lightness / 255.0;
            if (num2 == 0.0)
            {
                num4 = num3;
                num5 = num3;
                num6 = num3;
            }
            else
            {
                double d = num / 60.0;
                int num11 = (int)Math.Floor(d);
                double num10 = d - num11;
                double num7 = num3 * (1.0 - num2);
                double num8 = num3 * (1.0 - (num2 * num10));
                double num9 = num3 * (1.0 - (num2 * (1.0 - num10)));
                switch (num11)
                {
                    case 0:
                        num4 = num3;
                        num5 = num9;
                        num6 = num7;
                        break;
                    case 1:
                        num4 = num8;
                        num5 = num3;
                        num6 = num7;
                        break;
                    case 2:
                        num4 = num7;
                        num5 = num3;
                        num6 = num9;
                        break;
                    case 3:
                        num4 = num7;
                        num5 = num8;
                        num6 = num3;
                        break;
                    case 4:
                        num4 = num9;
                        num5 = num7;
                        num6 = num3;
                        break;
                    case 5:
                        num4 = num3;
                        num5 = num7;
                        num6 = num8;
                        break;
                }
            }
            return new Color((int)(num4 * 255.0), (int)(num5 * 255.0), (int)(num6 * 255.0));
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
            if (hours >= 0)
            {
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
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(color))
                    mess += $" [c/{color}:0] 秒";
                else
                    mess += $" 0 秒";
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
        /// 完全纠正名字： 删除文件名中的非法字符，对null和空字符的默认World化，修剪空格前后缀，移除.wld后缀
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string CorrectFileName(string? name)
        {
            //对null和空字符的默认World化
            if (string.IsNullOrWhiteSpace(name))
                return "World";
            //删除文件名中的非法字符
            for (int i = 0; i < name.Length; ++i)
            {
                bool flag = name[i] == '\\' || name[i] == '/' || name[i] == ':' || name[i] == '*' || name[i] == '?' || name[i] == '"' || name[i] == '<' || name[i] == '>' || name[i] == '|';
                if (flag)
                {
                    name = name.Remove(i, 1);
                    i--;
                }
            }
            //修剪空格前后缀
            while (name.StartsWith(' ') || name.EndsWith(' '))
            {
                name = name.Trim();
            }
            //移除.wld后缀
            if (name.EndsWith(".wld", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Remove(name.Length - 4, 4);//name = name.Remove(name.LastIndexOf('.'), 4);
            }
            //再修剪下
            while (name.StartsWith(' ') || name.EndsWith(' '))
            {
                name = name.Trim();
            }
            return name;
        }


        /// <summary>
        /// 完全纠正一个指令上的格式错误：移除前缀"/","."，移除前后空格，缩短中间空格直1格，对null返回""
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string CorrectCommand(string? c)
        {
            if (string.IsNullOrWhiteSpace(c))
                return "";
            while (c.StartsWith('/') || c.StartsWith('.') || c.StartsWith(' ') || c.EndsWith(' ') || c.Contains("  "))
            {
                c = c.TrimStart('/', '.');
                c = c.Trim();
                c = c.Replace("  ", " ");
            }
            return c;
        }


        /// <summary>
        /// 泰拉在生成一个带空格的名字时会在文件名上用_代替空格，地图内部名称依然有空格，也就是说，泰拉本身生成的地图名称不会有空格。
        /// 功能：查找文件夹内是否有把空格换成_后相同的文件 或 名字直接相同的,即 ("a bc" == "a bc" || "a bc" == "a_bc") "a bc"为形参
        /// </summary>
        /// <param name="name"> 不含.wld的名称 </param>
        /// <param name="worldname"> 返回目录中确定"相同"的名字，若无返回 "" </param>
        /// <returns></returns>
        private static bool ExistWorldNamePlus(string name, out string worldname)
        {
            name = CorrectFileName(name);
            //如果名字完全一样，直接返回（我就当用户提供了带空格的并视为相同）
            if (File.Exists(config.path() + "/" + name + ".wld"))
            {
                worldname = name;
                return true;
            }
            //把提供的带空格的名字中的空格换成_，若存在也视为有
            if (File.Exists(config.path() + "/" + name.Replace(' ', '_') + ".wld"))
            {
                worldname = name.Replace(' ', '_');
                return true;
            }
            worldname = "";
            return false;
        }


        /// <summary>
        /// 用name或者net_id搜索有多少种npc,返回npc的net_id和name，注意是netid不是id
        /// </summary>
        /// <param name="nameOrID"></param>
        /// <returns></returns>
        private static Dictionary<int, string> FindNPCNameAndIDByNetid(string nameOrID)
        {
            Dictionary<int, string> npcs = new Dictionary<int, string>();
            if (string.IsNullOrWhiteSpace(nameOrID))
                return npcs;
            if(int.TryParse(nameOrID, out int netID))
            {
                for(int i = -65; i <= NPCID.Count; i++)
                {
                    if(i == netID)
                    {
                        npcs.Add(i, Lang.GetNPCNameValue(i));
                    }
                }
                return npcs;
            }
            else
            {
                for (int i = -65; i <= NPCID.Count; i++)
                {
                    if (Lang.GetNPCNameValue(i).Contains(nameOrID))
                    {
                        npcs.Add(i, Lang.GetNPCNameValue(i));
                    }
                }
                return npcs;
            }
        }


        /// <summary>
        /// 获取这个玩家能使用的所有指令
        /// </summary>
        /// <returns></returns>
        private static List<string> getAllComCannotRun(TSPlayer player)
        {
            List<string> list = new List<string>();
            Commands.ChatCommands.ForEach(x =>
            {
                if (!(x.CanRun(player) && (x.Name != "setup" || TShock.SetupToken != 0)))
                {
                    list.AddRange(x.Names);
                }
            });
            return list;
        }
    }
}
