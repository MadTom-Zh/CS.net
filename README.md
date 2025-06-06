# 如题，这个库里放的都是以.net为框架的程序，基本上都是WPF；

# ReleaseNExamples

更新事件 2024-08-09

如果不想自己编译，可以从这里下载编译好的程序；

部分附带样本数据，大家可以参考使用；

# AppNTools 应用程序和工具

**有些程序用到的类库在 LibsMadTomDev中可以找到；

### GeneralSudoku

更新时间 2025-05-12

数独小软件，可以生成随机数独，自己玩，或者寻解；

**关于随机，还有一个动作是翻滚，将整个阵列顺时针翻滚0-3次，从而提供更多随机可能；但我没有做进去，以后有机会再加（因为这个软件貌似没什么用，可能以后也不会再更新了吧）；

### BiliDmTTS 2

更新时间 2024-08-09

自用bilibili直播弹幕工具，只需要在内置浏览器输入自己直播间的地址，并登录（只需第一次运行时设定，不登陆也可以，但观众名只给第一个字），就可自动获取观众发上来的弹幕和礼物，并自动朗读；

其中还有多种过滤功能，用于阻挡你的黑粉，当然各种弹幕、消息和礼物都可以单独控制是否朗读出来；

**朗读需要的语音包，需自己单独安装，win10自带的音质不是很行；

**已经弃用bililive_dm-master，作者在release里说了，从2024年1月开始，通信协议已经变更，但程序还停留在几年前，应该是没和b站谈拢；    

**投喂辣条的消息目前还算作系统消息（没来得及改），所以可能一次重复头尾辣条好几遍，等我有时间了再改吧；

### CraftingCalculator

更新时间 2024-11-14

通用配方计算器，可计算原料和机器配比数量，可出工序流图；

其中的数据需要自行录入，包括物品、通道和配方，其中使用相同机器，但设置了不同配件的配方，会属于两个不同的配方；

自动计算功能，选取一个目标产品，搜索并列出所有可能的流程，选取其中一个，自动绘制流程图；

手动计算功能，方便查询配方、手动配置流水线（配方较多时，自动计算会因为组合数量过于庞大而中止，暂时没什么好的解决办法）；

全面完善了功能，包括框选、多节点移动、手动连线、关联计算等等，不过说明书还在编写中；

### SemiAutoSerialCom

更新时间 2024-08-12

自制的USB串口通信客户端，目前主要用于调试pico，包括一个自定义的通信数据翻译组件；

使用时，打开连接后，窗口放着不用动，pico随便关闭还是开启，如果有数据，这里会自动捕获；

演示视频： https://www.bilibili.com/video/BV1Lz421m7sV

**Pico官方推荐的是PuTTY，但我个人感觉不怎么好用；

### MouseSpeeder

更新时间 2024-08-12

通过快捷键，快速设定光标移动速度，可语音播报；

### MultiExplorers2

更新时间 2024-08-12

集成多窗口文件浏览器，可相互拖拽复制粘贴，窗口布局可到多个预设并快速调用；文件传输上，对忙碌的设备只进行一个作业调度（不并行多个读写），提高读写效率；

** 目前，这个工具我已经不想再调了（简单的基本使用，还是很方便的），因为调试的时候啥问题没有，实际使用的时候，不是复制文件莫名报错，就是处理冲突文件又没任何效果，可调试的时候这些问题又无法复现；还有个最大的问题，他只能识别一个网络共享位置，多一个都不认，也查不到解决方法，真的棘手；

### QuickQR

更新时间 2024-08-12

快速识别、生成二维码；识别方法包括，屏幕截图、获取剪贴板图片、选择图片文件、摄像头截图（如使用平板电脑时）；

### QuickView

更新时间 2024-08-12

用于快速浏览和分类图片、视频，通过上下方向键（默认），可将文件快速移动到文件夹1、0中（默认）；其中视频播放为单个循环，可设定音频音量和播放速度；

**有些图片或视频文件会造成程序崩溃，偶尔发生，有机会在开发机上再遇到了我再进一步调试；

### ShutdownAfterFileFinish

更新时间 2024-08-12

原本是为Adobe Encoder设计的编码视频文件完成后关闭计算机；

原理是监视文件夹下的文件，如果文件锁定、尺寸变化，以及如果存在临时文件等情况，则继续等待，当这些情况均排除后，则执行bat关机脚本；

所以，在监视文件的基础上，执行什么脚本，可以自由选择；

**偶尔，AE明明已经完成了视频编码，临时文件也都清除了，但为什么关机后有时会发现编码的视频无法播放呢？虽然是偶发情况，但到现在没弄明白是怎么回事；

### SimpleTaskScheduler2

更新时间 2024-08-12

可以设定多种时间方案，到点后执行脚本；内置一个消息框发生器，可生成并弹出置顶的消息框；

如果意外关机，下次启动后，会自动提醒未执行的任务；

### Gunbook

更新时间 2024-08-13

播放器，用于查看内置图片；

**当时是用来快速查看各分类中的图片的，为解锁游戏world of guns里的一个记忆成就；当然现在游戏内容扩充了很多，且下载一个模型最少要半小时，非常煎熬，所以很久没玩了；

### MLW

更新时间 2024-08-13

播放器，游戏My lovely wife全部故事、剧情走向、结局，选择节点可查看最简选择线路；

**英文部分是完整的，但中文部分只做了前两章，因为工作量实在有点大，实在熬不动了；

**上传文件时发现有几个库文件还是复制的，也可能是做了订制，忘了；

**已知问题我卸载发布区了，如果大家有对程序修改的期望，欢迎给我留言，或者在b站私信我；

### SoE Helpe

更新时间 2024-08-13

自动拼图小工具，原针对游戏Sigils of Elohim，如果有类似的游戏，也可以适用；

算法上就是遍历所有可能的排列组合，最难的那一关也就1分钟解决（15年前的老电脑）；

### MineClearanceHelper2

更新时间 2024-08-20

前两天心血来潮，把区域预测功能做了，当遇到连续未知区域时，则使用预测法判断区域中能确定的地方，当然，如果之前的问号还是问号，那就只能赌了，因为确实无法确定；

### DroplitzHelper 2013 0607

更新时间 2024-08-21

游戏辅助工具，游戏名Droplitz，用于快速计算通路；数字1、2、3、4分别用于设定堵头、双通、三通、四通；f5和f6计算最长路径和最短路径；

** 有个bug，按钮都应该取消可聚焦，否则空格键会强制激活按钮；

### FlowerzHelper 2013 0906

更新时间 2024-08-21

游戏辅助工具，游戏名Flowerz，类似于三消，通过设定棋盘，计算最佳消除路径；

### MM4MapHelper 2023 0107

更新时间 2024-08-21

地图辅助工具，不一定仅用于MM4，设定图片后，在图片坐标的基础上设定比例尺；使用时，输入坐标未知，则自动在地图上标出，且光标所在坐标对应的标记点会涂红；

### MouseClickAnalysis 2021 0809

更新时间 2024-08-21

用于分析鼠标点击，侦测两次点击之间的时间间隔，进而分析微动是否出现接触不良；

** 我的极限双击间隔时90ms，而有问题的微动，双击间隔会小于10ms，所以…… 另外，如果换不了光学微动，那我个人推荐樱桃的纯金微动，非常非常地耐用；

### ShowMeYourUSB 2023 1120

更新时间 2024-08-21

虽然你关闭了u口的自动播放功能，但还是总有人拿usb试探你的电脑？来运行这个监控工具吧，他敢插，就记录他u盘里的文件列表，甚至可以直接全盘拷贝；

### SimpleClock 2023 1120

更新时间 2024-08-21

一个很简单的时钟小软件，大界面，适合放在副屏；

### YLGY_CSDemo 2022 0926

更新时间 2024-08-21

游戏演示，模拟羊了个羊，基本功能已经实现；原本打算加上动画效果和地图编辑器，但现成的“广告播放器”已经做的很完善了，所以感觉没有必要继续了；

### 批量更新字幕编码为UTF8 2023 1120

更新时间 2024-08-21

如题，原本就是用来修正电影字幕的（不知道为什么，我电脑不识别GB编码的文件，读出来是乱码）；

# LibsMadTomDev 存放运行程序需要的类库等等

## Common

### IconHelper

更新时间 2024-08-09

可用于获取文件对应程序图标，系统图标，和Shell32图标（可配合SystemResources工具快速获取想要图标的索引号）；

内置缓存，对重复获取相同后缀文件的图标时，不会额外占用内存；

### IOUtilities

更新时间 2024-08-09

IOUtilities，包含各种文件复制、移动操作的包装，包括自己处理异常的方法，以及调用操作系统的方法（可选择是否显示复制进度窗口）；

TransferManager，系统性的处理文件复制、移动作业，当遇到情况后，可统一重新安排处理方法（主要给MultiExplorer2用的）；

Network，还有列出有网络共享主机和其中共享文件夹的方法；

FileNameSorter，排序文件名，按照文件前缀数字的大小来排序（而不是将数字字符单纯当作字符）；

FileFilterHelper，筛选文件名，可以按照多个筛选条件来层层筛选；

IOInfoShadow，将文件名称、大小、属性等信息保存在结构体中，一遍后续处理（不锁文件，尺寸还比较小）；

### Logger

更新时间 2024-08-09

如题，开发者必不可少的工具，可静态快速写日志，或生成实例后，按各种设定要求来写日志；

### MadTomDevVariables

更新时间 2024-08-09

个人用变量库，方便获取日志文件夹位置、配置文件夹位置等信息；

### MillisecondTimer

更新时间 2024-08-09

以1ms毫秒计时器为基础，较为精确的生产各种间隔的tick事件；

**相比直接用1000ms计时器，再也不会出现自制时钟一次跳2秒的情况了；

### MouseNKeyboardHelper

更新时间 2024-08-09

按情况获取拖拽效果，快速拖拽文件，检查拖拽文件，键盘按键监听，获取系统鼠标双击时间间隔；

### SimpleStringHelper

更新时间 2024-08-09

CSVHelper，用于快速读取和写入CSV表格文件；

SimpleStringHelper，以?和*匹配查询， 检查重复词，消除多余空格，编码换行符，检查是否全角字符，表格样式，单位转换，转换为拼音等；

SimpleValueHelper，尝试将字符串转换为各种类型的数据类型；

Extensions，string的静态方法集合，转换16进制，转换2进制，转换颜色代码，转换文件属性，小写数字转大写，点字符串互转，尺寸字符串互转，颜色字符串互转；

### TextFileDecodeHelper

更新时间 2024-08-09

文本文件本身并不会像图片文件那样，打上字符集的标记，所以到底用了什么编码，就需要自行判断了（所以我写的代码，凡写文本文件的，统一UTF-8）；

使用这个检测工具，判断最符合的编码和可信度，除中文，还包括日文编码和韩文编码，通常可信度超过95%的都正确；

### ViewHistory

更新时间 2024-08-09

保存浏览记录用，可设置不重复记录，例如之前访问过C:,那么当历史记录中存在这一条时，再访问C:那么就不会记录这次记录（避免重复）；

常用操作，如获取某条历史（移动指针到读取位置），获取之前的记录（指针之前），或获取之后的记录（指针之后），清空记录等；

## Data

### SQLiteHelper

更新时间 2024-08-07

将官方的System.Data.SQLite做了简单封装，包括增删改，常用的编辑功能，以及事务；如果没有能满足需求的，也可以手动输入SQL，然后Execute()；

支持设置密码，当然如果设置了密码，就不方便用3方工具查看和编辑了（我用的免费工具，不支持加密，收费版不知道了）；

### SettingXML

更新时间 2024-08-07

结构化保存数据，每个XML节点可以包含若干不重名的属性，还能包含若干重名或不重名的XML子节点；

如果需要保存复杂字符串，如存在特殊负号的，建议保存到CDATA节点中；

### SettingsLanguage

更新时间 2024-08-07

实时切换语言，太香了；

首先，将你的UI.Text动态绑定到ResourceDictionary；然后，执行TrySetLang()来修改ResourceDictionary，因为控件的Text是动态绑定的，所以会跟着变为新的字符串；

### SettingsTxt

更新时间 2024-08-07

一个简单的key-value存取库，读写txt文件，可以方便的从外部查看和编辑；

灵感来源于java的一个免费类库，写cfg文件的，功能几乎一模一样；

### SuperErrorCorrectionStream

更新时间 2024-08-07

中断的项目；

原设计是将1维校验数据扩展到2维、3维，如2维数据表增加右侧校验数据和底部校验数据；此时单个包的尺寸也会成平方和立方增长，适合大尺寸数据的容错存储和传输；

因为通过CPU来执行校验计算效率太低，能达到直接拷贝数据速度的1/10就相当不错了，但我又没有考虑过使用硬件实现，如使用FPGA，所以就这么放弃了；

### WPSHelper

更新时间 2024-08-07

通过程序来启动、搜索、选择、编辑、保存、关闭WPS文档；

你敢信，这个库我就是从我自己写的MSWordHelper的代码复制粘贴的，几乎没有改动，这让我不得不怀疑，WPS的内核是不是就是Word；

### EncryptedZip

更新时间 2024-07-19

工程里直接引用了CS.fx中EncryptedZip的cs源文件（.net standard），链接形式的，所以这里就不上传文件了，只有个项目文件而已；