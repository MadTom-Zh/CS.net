using System.Composition;

namespace MadTomDev.App.Translators
{
    [Export(typeof(ITranslator))]
    [MetaData
    (
        Name = "PicoQATester_zh",
        Version = 202312181546,
        UserLanguage = "简体中文",
        MachineLanguage = "PicoQATester"
    )]
    public class PicoQATester_zh : ITranslator
    {
        public string Name => this.GetType().Name;

        public string ToMachine(string userMessage, out bool seccess)
        {
            string umLower = userMessage.Trim();
            while (umLower.Contains("  "))
            {
                umLower = umLower.Replace("  ", " ");
            }
            umLower = umLower.ToLower();

            seccess = true;
            string calCmd;
            if (umLower.StartsWith("嗨"))
            {
                if (!a_isHi_orHello)
                {
                    a_isHi_orHello = true;
                    a_isHi_orHello_count = 0;
                }
                return "a";
            }
            else if (umLower.StartsWith("你好"))
            {
                if (a_isHi_orHello)
                {
                    a_isHi_orHello = false;
                    a_isHi_orHello_count = 0;
                }
                return "a";
            }

            else if (umLower.StartsWith("你是谁"))
                return "b";

            else if (umLower.StartsWith("算"))
            {
                calCmd = GetCalCmd(umLower.Substring(1));
                seccess = calCmd.Length > 1;
                return calCmd;
            }
            else if (umLower.StartsWith("计算"))
            {
                calCmd = GetCalCmd(umLower.Substring(2));
                seccess = calCmd.Length > 1;
                return calCmd;
            }

            else if (umLower.StartsWith("帮助"))
                return "h";
            else if (umLower.StartsWith("手册"))
                return "h";

            seccess = false;
            return userMessage;

            string GetCalCmd(string v)
            {
                v = v.Replace(" ", "");
                string par = null;
                if (v.Contains('+'))
                    par = GetCalCmdPars(v, '+');
                if (v.Contains('-'))
                    par = GetCalCmdPars(v, '-');
                if (v.Contains('*'))
                    par = GetCalCmdPars(v, '*');
                if (v.Contains('/'))
                    par = GetCalCmdPars(v, '/');
                if (v.Contains('%'))
                    par = GetCalCmdPars(v, '%');

                return 'c' + par;
            }
            string GetCalCmdPars(string v, char op)
            {
                if (v.Contains(op))
                {
                    string[] pars = v.Split(op);
                    if (pars.Length >= 2)
                    {
                        return op + pars[0] + "," + pars[1];
                    }
                }
                return null;
            }
        }

        private bool a_isHi_orHello;
        private int a_isHi_orHello_count = 0;
        public string ToUser(string machineMessage, out bool seccess)
        {
            if (machineMessage != null)
            {
                machineMessage = machineMessage.Trim();
                while (machineMessage.EndsWith("\r")) machineMessage = machineMessage.Substring(0, machineMessage.Length - 1);
                while (machineMessage.EndsWith("\n")) machineMessage = machineMessage.Substring(0, machineMessage.Length - 1);
                if (machineMessage.Length > 0)
                {
                    seccess = true;
                    char c = machineMessage[0];
                    switch (c)
                    {
                        case 'a':
                            {
                                bool hOrL;
                                if (a_isHi_orHello)
                                    hOrL = (a_isHi_orHello_count % 2) == 0;
                                else
                                    hOrL = (a_isHi_orHello_count % 2) != 0;
                                ++a_isHi_orHello_count;

                                if (hOrL)
                                    return "嗨！";
                                else
                                    return "你好！";
                            }
                        case 'b':
                            return "我是 " + machineMessage.Substring(1);
                        case 'c':
                            if (machineMessage.Length > 1)
                                return "结果：" + machineMessage.Substring(1);
                            else
                                return "太难了，我不会！";
                        default:
                        case 'h':
                            return
@"可用语句如下：
嗨，你好;
你是谁？
算 [a+b] (a，b为数字运算符还支持 - * / %)
计算 [a+b] (同上)";
                        case 'z':
                            return "未知命令！";
                    }
                }
            }

            seccess = false;
            return machineMessage;
        }
    }
}