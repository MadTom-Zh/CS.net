# 如题，这个库里放的都是以.net为框架的程序，基本上都是WPF；

# AppNTools 存放可以直接运行的程序

**有些程序用到的类库在 LibsMadTomDev中可以找到；

### BiliDmTTS 2

更新时间 2024-06-27

自用bilibili直播弹幕工具，只需要输入直播间号（只需第一次运行时设定），就可自动获取观众发上来的弹幕和礼物，并自动朗读；

其中还有多种过滤功能，用于阻挡你的黑粉，当然各种弹幕、消息和礼物都可以单独控制是否朗读出来；

**朗读需要的语音包，需自己单独安装，win10自带的音质不是很行；

**代码bililive_dm-master（好像）是b站官方程序员做的，很久以前的了，如果需要新代码，请在github中自行搜索呀；    

### CraftingCalculator

更新时间 2024-07-19

通用配方计算器，可出工序流图；

其中的数据需要自行录入，包括物品、通道和配方，其中使用相同机器，但设置了不同配件的配方，会属于两个不同的配方；

目前我正在用这个工具玩Fortresscraft，回头如果发视频的话，我会把视频地址和使用工具的时间点发上来；

# LibsMadTomDev 存放运行程序需要的类库等等

## Common

### IconHelper

### IOUtilities

### Logger

### MadTomDevVariables

### MillisecondTimer

### MouseNKeyboardHelper

### SimpleStringHelper

### TextFileDecodeHelper

### ViewHistory

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