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

### SettingXML

### SettingsLanguage

### SettingsTxt

### SuperErrorCorrectionStream

### WPSHelper

### EncryptedZip

更新时间 2024-07-19

工程里直接引用了CS.fx中EncryptedZip的cs源文件（.net standard），链接形式的，所以这里就不上传文件了，只有个项目文件而已；