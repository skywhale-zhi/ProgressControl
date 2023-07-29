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
            自动重置的地图大小_小1_中2_大3 = 3;
            自动重置的地图难度_普通0_专家1_大师2_旅途3 = 2;
            自动重置的地图种子 = "";
            自动重置的地图名称 = "";
            你提供用于重置的地图名称= new HashSet<string>();
            上面两条的注释 = "这是一条注释修改它无效。如果你提供了用于重置的地图名称，在下次重置时系统会按照你提供的名称寻找地图(路径参考<地图存放目录_不填时默认原目录_注意请使用除号分隔目录>参数)，若找不到则生成该名字的地图，每次重置成功都会消耗掉对应的提供的名字，当提供的名字为空时则启用<自动重置的地图名称>参数，只生成新地图(只会生成不再调用已有的了)，当存在同名地图则会后缀数字编号，当<自动重置的地图名称>为空时默认生成World，同样看情况后缀数字编号，请不要填入.wld后缀名";
            自动重置的最多在线人数 = 16;
            自动重置的端口 = "7777";
            自动重置的密码 = "";
            地图存放目录_不填时默认原目录_注意请使用除号分隔目录 = "";

            上次重启服务器的日期 = DateTime.Now;
            是否启用自动重启服务器 = false;
            多少小时后开始自动重启服务器 = -1;
            自动重启后的最多在线人数 = 16;
            自动重启后的端口 = "7777";
            自动重启后的密码 = "";
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
                {"石巨人" ,144},//7
                {"猪龙鱼公爵" ,145},
                {"光之女皇" ,146},
                {"拜月教教徒" ,168},//8
                {"四柱" ,168},
                {"月亮领主" ,174}//8+1/4天
            };
            NPC封禁时长距开服日期_ID和单位小时 = new Dictionary<int, double> { };
            上次自动执行指令的日期 = DateTime.Now;
            是否启用自动执行指令 = false;
            多少小时后开始自动执行指令 = -1;
            自动执行的指令_不需要加斜杠 = new HashSet<string> { };
        }

        //重置计划
        public DateTime 开服日期;
        public bool 是否启用自动重置世界;
        public double 多少小时后开始自动重置世界;
        public bool 自动重置是否重置玩家数据;
        public HashSet<string> 自动重置前执行的指令_不需要加斜杠;
        public HashSet<string> 自动重置前删除哪些数据库表;
        public bool 自动重置前是否删除地图;
        public int 自动重置的地图大小_小1_中2_大3;
        public int 自动重置的地图难度_普通0_专家1_大师2_旅途3;
        public string 自动重置的地图种子;
        public string 自动重置的地图名称;
        public HashSet<string> 你提供用于重置的地图名称;
        public string 上面两条的注释;
        public int 自动重置的最多在线人数;
        public string 自动重置的端口;
        public string 自动重置的密码;
        public string 地图存放目录_不填时默认原目录_注意请使用除号分隔目录;

        //重启计划
        public DateTime 上次重启服务器的日期;
        public bool 是否启用自动重启服务器;
        public double 多少小时后开始自动重启服务器;
        public int 自动重启后的最多在线人数;
        public string 自动重启后的端口;
        public string 自动重启后的密码;
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
        /// 在地图的文件夹目录里对即将要创造的地图名称进行后缀加数字
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string AddNumberFile(string? name)
        {   //尝试给重复地图编号，其实原版有自动编号的，但是自动编号的地图名称和地图数据的内部名称不一致，你自己手动开服就分不清了
            int count = 1;
            if (string.IsNullOrWhiteSpace(name))
                name = "World";
            while (true)
            {
                if (File.Exists(path() + "/" + name + (count == 1 ? "" : count) + ".wld"))
                    count++;
                else
                    return name + (count == 1 ? "" : count);
            }
        }
    }
}
