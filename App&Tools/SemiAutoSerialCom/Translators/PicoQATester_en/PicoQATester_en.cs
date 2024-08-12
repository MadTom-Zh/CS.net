using System.Composition;

namespace MadTomDev.App.Translators
{
    [Export(typeof(ITranslator))]
    [MetaData
    (
        Name = "PicoQATester_en",
        Version = 202312181541,
        UserLanguage = "English",
        MachineLanguage = "PicoQATester"
    )]
    public class PicoQATester_en : ITranslator
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
            if (umLower == "hi" || umLower.StartsWith("hi "))
            {
                if (!a_isHi_orHello)
                {
                    a_isHi_orHello = true;
                    a_isHi_orHello_count = 0;
                }
                return "a";
            }
            else if (umLower == "hello" || umLower.StartsWith("hello "))
            {
                if (a_isHi_orHello)
                {
                    a_isHi_orHello = false;
                    a_isHi_orHello_count = 0;
                }
                return "a";
            }

            else if (umLower == "who are you" || umLower.StartsWith("who are you?") || umLower.StartsWith("who are you "))
                return "b";

            else if (umLower.StartsWith("cal "))
            {
                calCmd = GetCalCmd(umLower.Substring(4));
                seccess = calCmd.Length > 1;
                return calCmd;
            }
            else if (umLower.StartsWith("calculate "))
            {
                calCmd = GetCalCmd(umLower.Substring(10));
                seccess = calCmd.Length > 1;
                return calCmd;
            }

            else if (umLower == "help" || umLower.StartsWith("help "))
                return "h";
            else if (umLower == "manual" || umLower.StartsWith("manual "))
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
                    if (pars.Length > 1)
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
                                    return "Hi!";
                                else
                                    return "Hello!";
                            }
                        case 'b':
                            return "I am " + machineMessage.Substring(1);
                        case 'c':
                            if (machineMessage.Length > 1)
                                return "Result: " + machineMessage.Substring(1);
                            else
                                return "Can not calculate this formula!";
                        default:
                        case 'h':
                            return
@"Available sentences as follows:
Hi, Hello;
Who are you?
Cal [a+b] (a, b are numbers, and can use other operator such as - * / %)
Calculate [a+b] (same as above)";
                        case 'z':
                            return "Unknow command!";
                    }
                }
            }

            seccess = false;
            return machineMessage;
        }
    }
}