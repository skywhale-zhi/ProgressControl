using Newtonsoft.Json;
using Terraria;
using TShockAPI;


namespace ProgressControl
{
    public class Config
    {
        public static string configPath = Path.Combine(TShock.SavePath, "ProgressControl.json");

        /// <summary>
        /// 从文件中导出
        /// </summary>
        /// <returns></returns>
        public static Config LoadConfigFile()
        {
            if (!Directory.Exists(TShock.SavePath))
            {
                Directory.CreateDirectory(TShock.SavePath);
            }
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new Config(), Formatting.Indented));
            }
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="config"></param>
        public void SaveConfigFile()
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        /// <summary>
        /// 将原版的tshock的config.json从内存中写入文件一次
        /// </summary>
        public static void SaveTConfig()
        {
            TShock.Config.Write(Path.Combine(TShock.SavePath, "config.json"));
        }

        public Config()
        {
            开服日期 = DateTime.Now;
            是否启用自动重置世界 = false;
            多少小时后开始自动重置世界 = -1;
            自动重置是否重置玩家数据 = true;
            自动重置前执行的指令_不需要加斜杠 = new HashSet<string>();
            自动重置前删除哪些数据库表 = new HashSet<string>();
            自动重置前是否删除地图 = false;
            自动重置后的地图大小_小1_中2_大3 = 3;
            自动重置后的地图难度_普通0_专家1_大师2_旅途3 = 2;
            自动重置后的地图种子 = "";
            自动重置后的地图名称 = "";
            你提供用于重置的地图名称= new HashSet<string>();
            上面两条的注释 = PControl.tips1;
            自动重置后的最多在线人数 = 16;
            自动重置后的端口 = "7777";
            自动重置后的服务器密码 = "";
            地图存放目录_不填时默认原目录_注意请使用除号分隔目录 = "";


            上次重启服务器的日期 = DateTime.Now;
            是否启用自动重启服务器 = false;
            多少小时后开始自动重启服务器 = -1;
            自动重启后的最多在线人数 = 16;
            自动重启后的端口 = "7777";
            自动重启后的服务器密码 = "";
            自动重启前执行的指令_不需要加斜杠 = new HashSet<string>();


            是否自动控制NPC进度 = false;
            Boss封禁时长距开服日期_单位小时 = new Dictionary<string, double>()
            {
                {"史莱姆王", 0},//第一天
                {"克苏鲁之眼", 5 },
                {"世界吞噬者" ,24},//第2天
                {"克苏鲁之脑" ,24},
                {"蜂后" ,48},//3
                {"巨鹿" ,48},
                {"骷髅王" ,49},
                {"血肉墙" ,72},//4
                {"史莱姆皇后" ,73},
                {"双子魔眼" ,96},//5
                {"毁灭者" ,97},
                {"机械骷髅王" ,98},
                {"世纪之花" ,120},//6
                {"猪龙鱼公爵" ,144},
                {"光之女皇" ,145},
                {"石巨人" ,146},//7
                {"拜月教教徒" ,168},//8
                {"四柱" ,168},
                {"月亮领主" ,174}//8+1/4天
            };
            NPC封禁时长距开服日期_ID和单位小时 = new Dictionary<int, double> { };


            上次自动执行指令的日期 = DateTime.Now;
            是否启用自动执行指令 = false;
            多少小时后开始自动执行指令 = -1;
            自动执行的指令_不需要加斜杠 = new HashSet<string> { };
            执行指令时是否发广播_解决指令执行频繁刷屏的问题 = true;
            越权检查 = true;
        }

        //重置计划
        public DateTime 开服日期;
        public bool 是否启用自动重置世界;
        public double 多少小时后开始自动重置世界;
        public bool 自动重置是否重置玩家数据;
        public HashSet<string> 自动重置前执行的指令_不需要加斜杠;
        public HashSet<string> 自动重置前删除哪些数据库表;
        public bool 自动重置前是否删除地图;
        public int 自动重置后的地图大小_小1_中2_大3;
        public int 自动重置后的地图难度_普通0_专家1_大师2_旅途3;
        public string 自动重置后的地图种子;
        public string 自动重置后的地图名称;
        public HashSet<string> 你提供用于重置的地图名称;
        public string 上面两条的注释;
        public int 自动重置后的最多在线人数;
        public string 自动重置后的端口;
        public string 自动重置后的服务器密码;
        public string 地图存放目录_不填时默认原目录_注意请使用除号分隔目录;

        //重启计划
        public DateTime 上次重启服务器的日期;
        public bool 是否启用自动重启服务器;
        public double 多少小时后开始自动重启服务器;
        public int 自动重启后的最多在线人数;
        public string 自动重启后的端口;
        public string 自动重启后的服务器密码;
        public HashSet<string> 自动重启前执行的指令_不需要加斜杠;

        //Boss进度控制计划
        public bool 是否自动控制NPC进度;
        public Dictionary<string, double> Boss封禁时长距开服日期_单位小时;
        public Dictionary<int, double> NPC封禁时长距开服日期_ID和单位小时;

        //指令使用计划
        public DateTime 上次自动执行指令的日期;
        public bool 是否启用自动执行指令;
        public double 多少小时后开始自动执行指令;
        public HashSet<string> 自动执行的指令_不需要加斜杠;
        public bool 执行指令时是否发广播_解决指令执行频繁刷屏的问题;
        public bool 越权检查;


        /// <summary>
        /// 地图的文件夹目录
        /// </summary>
        /// <returns></returns>
        public string path()
        {
            if (string.IsNullOrWhiteSpace(地图存放目录_不填时默认原目录_注意请使用除号分隔目录))
                return Main.WorldPath;
            else
                return 地图存放目录_不填时默认原目录_注意请使用除号分隔目录;
        }


        /// <summary>
        /// 为防止地图的文件夹目录里对即将要创造的地图名称重名，进行后缀加数字
        /// </summary>
        /// <param name="name">要创造的地图的名字</param>
        /// <param name="willDelete">将要删掉的地图的名字，删掉发生在创造地图前</param>
        /// <returns></returns>
        public string AddNumberFile(string? name, string willDelete = "")
        {   //尝试给重复地图编号，其实原版有自动编号的，但是自动编号的地图名称和地图数据的内部名称不一致，你自己手动开服就分不清了
            int count = 1;
            if (string.IsNullOrWhiteSpace(name))
                name = "World";
            while (true)
            {
                if (File.Exists(path() + "/" + name + (count == 1 ? "" : count) + ".wld") && name != willDelete)
                    count++;
                else if (name != willDelete)
                    return name + (count == 1 ? "" : count);
                else
                    return willDelete;
            }
        }
    }
}
