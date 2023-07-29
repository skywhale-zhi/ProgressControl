# ProgressControl
## 一个自动化的泰拉瑞亚插件，基于tshock

## 功能介绍

### 1.能够自动重置地图
可设置重置地图的大小、模式、种子、名称、地图存放目录、等功能

### 2.能自动重启地图
可按照当前的端口，地图数据来重启泰拉瑞亚服务器

### 3.能自动执行指令
自动执行你要设定的指令，支持原版和其他插件的

### 4.自动解锁Boss来控制进度
按时间来对boss进行封禁，时间未到不可打boss，boss会自动消失

## 指令
- 权限1：`pco.use`
- 指令：`/pco help`
- 功能：查看这个插件下的所有帮助指令
- 指令：`/pco view`
- 功能：查看当前服务器上的所有自动化计划（简易版）
-
- 权限2：`pco.admin`
- 指令：`/pco now`
- 功能：来将开服日期、上次重启日期和上次自动执行指令日期调整到现在
- 指令：`/pco autonpc`
- 功能：自动控制NPC和Boss进度计划启用，再次使用则关闭
- 指令：`/pco os autoboss [±num]`
- 功能：来将自动控制Boss的解锁时刻推迟或提前num小时，num可为小数
- 指令：`/pco npc add [id/name] [num]`
- 功能：来添加或更新一个NPC的封禁限制
- 指令：`/pco npc del [id/name]`
- 功能：来删除一个NPC的封禁限制
- 指令：`/pco autocom`
- 功能：自动执行指令计划启用，再次使用关闭
- 指令：`/pco os autocom [±num]`
- 功能：将自动执行指令的时间推迟或提前num小时，num可为小数
- 指令：`/pco com [±num]`
- 功能：手动执行指令计划启用，在num秒后开始执行，若num不填则立刻执行，num小于0则关闭当前存在的手动计划，其优先级大于自动执行指令
- 指令：`/pco com或autocom add [xxxx]`
- 功能：来添加一个自动执行的指令
- 指令：`/pco com或autocom del [xxxx]`
- 功能：来删除一个自动执行的指令
- 指令：`/supco help`
- 功能：来查看高级自动控制指令
-
- 权限3：`pco.superadmin`
- 指令：`/supco ar`
- 功能：自动重置世界计划启用，再次使用关闭
- 指令：`/supco os ar [±num]`
- 功能：将自动重置世界的时间推迟或提前num小时，num可为小数
- 指令：`/supco ad`
- 功能：自动重启服务器计划启用，再次使用关闭
- 指令：`/supco os ad [±num]`
- 功能：将自动重启服务器的时间推迟或提前num小时，num可为小数
- 指令：`/supco ar help`
- 功能：来设置重置的相关参数
- 指令：`/supco ad help`
- 功能：来设置重启的相关参数
- 指令：`/supco ar name [string]`
- 功能：来设置下次重置地图时的地图名字
- 指令：`/supco ar size [小1/中2/大3 (只能填数字)]`
- 功能：来设置下次重置时地图的大小
- 指令：`/supco ar mode [普通0/专家1/大师2/旅途3(只能填数字)]`
- 功能：来设置下次重置地图时的模式
- 指令：`/supco ar seed [string]`
- 功能：来设置下次重置地图时的地图种子
- 指令：`/supco ar maxplayers [num]`
- 功能：来设置下次重置地图时的最多在线玩家
- 指令：`/supco ar resetplayers [0/1]`
- 功能：来设置下次重置地图时是否清理玩家数据，0代表不清理
- 指令：`/supco ar port [num]`
- 功能：来设置下次重置地图时的端口
- 指令：`/supco ar password [string]`
- 功能：来设置下次重置地图时的密码
- 指令：`/supco ar addname [string]`
- 功能：来添加你自己提供用来重置的地图的名称
- 指令：`/supco ar delname [string]`
- 功能：来删除你自己提供用来重置的地图的名称
- 指令：`/supco ar listname`
- 功能：来列出你提供的所有地图名称
- 指令：`/supco ad maxplayers [num]`
- 功能：来设置下次重启地图时的最多在线玩家
- 指令：`/supco ad maxplayers [num]`
- 功能：来设置下次重启地图时的端口
- 指令：`/supco ad password [string]`
- 功能：来设置下次重启地图时的密码
- 指令：`/supco view`
- 功能：来查看当前服务器的自动化计划
- 指令：`/supco reset [±num]`
- 功能：手动重置世界计划启用，在num秒后开始重置，若num不填则立刻重置，num小于0则关闭当前存在的手动计划，其优先级大于自动重置
- 指令：`/supco reload [±num]`
- 功能：手动重启服务器计划启用，在num秒后开始重启，若num不填则立刻重启，num小于0则关闭当前存在的手动计划，其优先级大于自动重启
- 指令：`/supco stop [r/d/c/all或不填]`
- 功能：来分别终止reset/reload/com的全部手动计划，填all或不填时终止全部，自动计划不受影响

## 详述
- 插件里的有自动计划和手动计划两种计划，自动计划就是依赖配置文件中，每隔一段时间执行一次。有：自动重启计划，自动重置计划，自动控制Boss解锁时间计划，自动执行指令的计划 4 个，除非你在配置文件中把他们都设置成false，否则将一直执行下去。
- 手动计划就是用户使用指令产生一个，该计划只生效一次，如用于临时发起重启像72小时后重启一下，然后在重启完成后计划删除（原自动计划不受影响）。有：手动重置计划，手动重启计划，手动执行指令计划 3 个。**手动计划会覆盖自动计划**
- 配置文件中所有的自动计划都是默认关闭的，比如你想要服务器每隔24小时重置一份地图，那么你就可以在自动计划里设置`"多少小时后开始自动重置世界": 24.0`，那么服务器将会每隔24小时重置地图（记得启用`"是否启用自动重置世界": true`，都在配置文件中找）
- 计划执行的时间不是你改文件的时间也不是你用指令的时间，每个计划都有一个日期用来参照，**重置计划对应`开服日期`，重启计划对应`上次重启服务器的日期`，boss限制计划对应`开服日期`，自动执行指令计划对应`上次自动执行指令的日期`**，这些计划的时间按照这些日期来计算
- 手动计划是为了**临时**计划一个事情。只会生效一次，且计划按照你用指令的时间开始计算，会覆盖掉自动计划。比如你原计划24小时自动重启，但因为某种原因你需要临时修改一次时间，你可以直接用手动计划使用指令`/supco reload [秒数]`发起临时的重启计划，这个计划会让之前设定的自动计划无效，在这个计划执行后，手动计划消失，继续执行自动计划。
- 你可以用指令直接修改每种计划的时间，但我不太建议（因为需要你大致算一下），你用`offset系列指令`改相当于改配置文件，改过后均按这样来执行。offset的功能是推迟或提前一段时间，不是直接改时间，请注意计算
- `/pco now`指令用于将所有的参照日期都改成现在，用于刚开服时使用，**当游戏自动重置的时候也会默认执行一次这个操作**
- 手动计划的单位是秒，你仔细看指令介绍就知道了，**你不必牢记所有指令，只需要记得`/pco help`即可，所有别的指令都在这个指令使用后告诉你**
- 控制boss进度的原理是，这个boss一旦出现就直接清理

## 配置文件 ProgressControl.json
```
{
  "开服日期": "2023-07-20T06:04:24.9417951+08:00",  //没什么好说的，就是这个时间你开服了
  "是否启用自动重置世界": false,   //重置世界的自动计划
  "多少小时后开始自动重置世界": 48.0,  //单位小时，小时，所有指令里只有手动计划的指令单位是秒，其他都是小时
  "自动重置是否重置玩家数据": true,   //删除了tsCharacter表
  "自动重置前执行的指令_不需要加斜杠": [ //这里是两个例子，在你重置前执行一些你想要的指令，你不需要加斜杠/或点 . (你加了也不一定有事)，支持其他插件的指令，默认使用者的权限为最高，部分指令会使用失败比如/god这种对象为具体的玩家的
    "playing",
    "help"
  ],
  "自动重置前删除哪些数据库表": [],  //为其他插件的数据做出一些操作
  "自动重置前是否删除日志": false, //只删掉要重置掉的世界的日志，其他的不动
  "自动重置前是否删除地图": true,
  "自动重置的地图大小_小1_中2_大3": 2, //只能填这三个数字，下同
  "自动重置的地图难度_普通0_专家1_大师2_旅途3": 2, 
  "自动重置的地图种子": "", //如果你什么都不填，默认随机
  "自动重置的地图名称": "", //如果你什么都不填，默认World，请不要填非法字符，默认插件会帮你把非法字符过滤掉
  "自动重置的最多在线人数": 16,
  "自动重置的端口": "7777",
  "地图存放目录_不填时默认原目录_注意请使用除号分隔目录": "", //除号是指 / 不是 \
  "上次重启服务器的日期": "2023-07-21T19:37:36.3303958+08:00",
  "是否启用自动重启服务器": false, //重启世界的自动计划
  "多少小时后开始自动重启服务器": -1.0,
  "自动重启的参数设置": "这是一条提示信息。自动重启的端口会沿用重启前的；最大玩家数目、密码将按照config.json来配置",//就是提示信息，改它没用
  "自动重启前执行的指令_不需要加斜杠": [],
  "是否自动控制NPC进度": false,  //控制boss进度的自动计划
  "Boss封禁时长距开服日期_单位小时": { //不要改这些boss的名字，会出错，目前不支持添加其他生物
    "史莱姆王": 0.0,
    "克苏鲁之眼": 5.0,
    "世界吞噬者": 24.0,
    "克苏鲁之脑": 24.0,
    "蜂后": 48.0,
    "巨鹿": 48.0,
    "骷髅王": 49.0,
    "血肉墙": 72.0,
    "史莱姆皇后": 73.0,
    "双子魔眼": 96.0,
    "毁灭者": 97.0,
    "机械骷髅王": 98.0,
    "世纪之花": 120.0,
    "石巨人": 144.0,
    "猪龙鱼公爵": 145.0,
    "光之女皇": 146.0,
    "拜月教教徒": 168.0,
    "四柱": 168.0,
    "月亮领主": 174.0
  },
  "上次自动执行指令的日期": "2023-07-21T19:38:41.7904607+08:00", //自动执行指令的事件
  "是否启用自动执行指令": false,
  "多少小时后开始自动执行指令": 4.0,
  "自动执行的指令_不需要加斜杠": [ //同样这里是两个例子
    "playing",
    "help"
  ]
}
```