using System.Reflection;
using MethodDecorator.Fody.Interfaces;

namespace Ch9Protocols;

[AttributeUsage(AttributeTargets.Method)]
public class LogFileAttribute(string fileName): Attribute, IMethodDecorator
{
    private string FileName { get; } = fileName;
    
    public void Init(object instance, MethodBase method, object[] args)
    {
        if (!File.Exists(FileName))
        {
            File.Create(FileName).Close();
        }
    }

    public void OnEntry()
    {
    }

    public void OnExit()
    {
        File.AppendAllText(FileName, "hello\n");
    }

    public void OnException(Exception exception)
    {
    }
}