using System.Text.Json;
using Ch7Interpreter;
using static Ch7Interpreter.Interpreter;

var env = new Dictionary<string, object>();

var dir = Path.Combine(AppContext.BaseDirectory, "Code");
Directory.CreateDirectory(dir);
var files = Directory.EnumerateFiles(dir,
        "*",
        SearchOption.TopDirectoryOnly)
    .Where(file => file.Contains(".json"))
    .Where(file => file.Contains("while"))
    .Order()
    .ToArray();

foreach (var file in files)
{
    var json = File.ReadAllText(file);
    using var doc = JsonDocument.Parse(json);
    var parsed = ParseElement(doc.RootElement);

    Console.WriteLine(file.Split(Path.DirectorySeparatorChar)[^1]);
    Console.WriteLine(json.Replace("\n", "").Replace(" ", ""));
    try
    {
        var result = Do(env, parsed);
        Console.WriteLine($"=> {result}");
    }
    catch (Exception e)
    {
        if (e.InnerException is TLLException tllException) Console.WriteLine(tllException.Message);
    }

    Console.WriteLine(new string('=', 40));
}

PrintEnv(env);