namespace MadTomDev.App.Translators
{
    public interface ITranslator
    {
        string Name { get; }
        string ToUser(string machineMessage, out bool seccess);
        string ToMachine(string userMessage, out bool seccess);
    }
}