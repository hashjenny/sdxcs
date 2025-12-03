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
    .Where(file => file.Contains("func_ex"))
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

var o = ObjectFactory.MakeObject("i","j","k");
Console.WriteLine(o.get("i") ?? "null");
o.set("i",6);
Console.WriteLine(o.get("i"));

// ex7
// Modify the getter/setter example so that:
// 修改 getter / setter 示例，使其：
// make_object accepts any number of named parameters and copies them into the private dictionary.
// make_object 接受任意数量的命名参数，并将它们复制到 private 字典中。
// getter takes a name as an argument and returns the corresponding value from the dictionary.
// getter 接受一个名称作为参数，并返回字典中对应的值。
// setter takes a name and a new value as arguments and updates the dictionary.
// setter 接受一个名称和一个新值作为参数，并更新字典。
internal static class ObjectFactory
{
    public static (Func<string, object?> get, Action<string, object> set) MakeObject(params string[] initValue)
    {
        var v = initValue.ToDictionary(item => item, object? (_) =>null);
        return ((name) => v.GetValueOrDefault(name), 
            (key, newValue) => v[key] = newValue);
    }
}

// ex8
// Explain why this program doesn’t work:
// 解释为什么这个程序不工作：
// 
// def make_counter():
//     value = 0
//     def _inner():
//         value += 1
//         return value
//     return _inner
// 
// c = make_counter()
// for i in range(3):
//     print(c())
// counter_fail.py
// 
/*
 * 在嵌套函数 _inner() 中对 value 做了赋值（value += 1），Python 将其视为局部变量。
 * 于是在执行 value += 1 时需要先读取局部变量的旧值，但该局部变量还未被绑定，导致 UnboundLocalError。
 * 要修改外层作用域的变量，需要显式声明 nonlocal（或使用可变对象）。
 */
// Explain why this one does:
// 解释为什么这个有效：
// 
// def make_counter():
//     value = [0]
//     def _inner():
//         value[0] += 1
//         return value[0]
//     return _inner
// 
// c = make_counter()
// for i in range(3):
//     print(c())
/*
 * 因为 value 指向一个可变对象（列表），内部函数只是就地修改列表的元素（value[0] += 1），
 * 并没有在 _inner 中重新绑定名字 value。
 * Python 只有在函数体内对名字做赋值时才把它当作局部变量；
 * 修改可变对象的内容不会产生这种“局部绑定”问题。
 * 闭包捕获的是列表对象，所以状态保存在该对象里）。
 */

// ex9
// If the data in a closure is private, explain why lines 1 and 2 are the same in the output of this program but lines 3 and 4 are different.
// 如果闭包中的数据是私有的，解释为什么这个程序的输出中第 1 行和第 2 行相同，但第 3 行和第 4 行不同。
// 
// def wrap(extra):
//     def _inner(f):
//         return [f(x) for x in extra]
//     return _inner
// 
// odds = [1, 3, 5]
// first = wrap(odds)
// print("1.", first(lambda x: 2 * x))
// 
// odds = [7, 9, 11]
// print("2.", first(lambda x: 2 * x))
// 
// evens = [2, 4, 6]
// second = wrap(evens)
// print("3.", second(lambda x: 2 * x))
// 
// evens.append(8)
// print("4.", second(lambda x: 2 * x))
// 
// 1. [2, 6, 10]
// 2. [2, 6, 10]
// 3. [4, 8, 12]
// 4. [4, 8, 12, 16]

/*
 * 闭包捕获的是对象（引用），不是外层变量名本身。
 * odds = [7, 9, 11] 是把名字 odds 重新绑定到一个新列表，不会影响已闭包捕获的原列表；所以第 1 行和第 2 行相同。
 * evens.append(8) 是就地修改闭包捕获的列表对象，闭包看到的是被修改后的列表；所以第 3 行和第 4 行不同。
 */
