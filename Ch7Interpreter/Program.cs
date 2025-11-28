// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using static Ch7Interpreter.Interpreter;

var env = new Dictionary<string, object>();
// var funcDict = Assembly.GetExecutingAssembly()
//     .GetTypes()
//     .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
//     .Where(m => m.ReturnType == typeof(object))
//     .Where(m =>
//     {
//         var p = m.GetParameters();
//         return p.Length == 2
//                && p[0].ParameterType == typeof(Dictionary<string, object>)
//                && p[1].ParameterType == typeof(List<Node>[]);
//     })
//     .ToDictionary(m => m.Name,
//         m => (Func<Dictionary<string, object>, List<Node>, object>)((env2, list) =>
//         {
//             var args = new object[] { env2, new[] { list } };
//             return m.Invoke(null, args)!;
//         }));

var dir = Path.Combine(AppContext.BaseDirectory, "Code");
Directory.CreateDirectory(dir);
var files = Directory.EnumerateFiles(dir, 
        "*", 
        SearchOption.TopDirectoryOnly)
    .Where(file => file.Contains("code"))
    .ToArray();

foreach (var file in files)
{
    var json = File.ReadAllText(file);
    using var doc = JsonDocument.Parse(json);
    var parsed = ParseElement(doc.RootElement);
    var result = Do(env, parsed);
    Console.WriteLine(json.Replace("\n", "").Replace(" ", ""));
    Console.WriteLine($"  => {result}");
    Console.WriteLine("==============");
}
