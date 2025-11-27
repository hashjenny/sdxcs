using System.Reflection;
using Ch6RunningTests;

var testMethods = Assembly.GetExecutingAssembly()
    .GetTypes()
    .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
    .Where(m => m.GetCustomAttribute<TestAttribute>() is not null
                && m.GetParameters().Length == 0
                && m.ReturnType == typeof(void))
    .ToArray();

var tests = testMethods
    .Select(m =>
        ($"{m.DeclaringType?.FullName}.{m.Name}",
            (Action)(() => m.Invoke(null, null))))
    .ToList();

UnitTest.SetUp += () => Console.WriteLine("setup 1");
UnitTest.SetUp += () => Console.WriteLine("setup 2");
UnitTest.TearDown += () => Console.WriteLine("teardown 1");
UnitTest.TearDown += () => Console.WriteLine("teardown 2");
UnitTest.RunAllTests(tests, "Pass");


// ex1
// Looping Over globals  遍历 globals
// What happens if you run this code?
// 如果运行这段代码会怎样？
//
// for name in globals():
//     print(name)
//
// What happens if you run this code instead?
// 如果运行这段代码会怎样？
//
// name = None
// for name in globals():
//     print(name)
// Why are the two different?
// 为什么它们是不同的？

// 直接运行：
// for name in globals():
//     print(name)
// 可能行为：
// 如果 'name' 不在 globals()，循环在第一次迭代赋值 name = <first_key> 时会在 globals 中创建 'name'（改变字典大小），这通常会导致 RuntimeError；
// 或者（如果某些键/实现让它不报错）它也可能顺利打印初始那些键，但输出中不一定包含 'name'（因为 globals() 的键集合是在迭代器创建时决定的）。
// 运行结束后，name 变量存在，并等于最后一次迭代得到的键（字符串）。
// 先设再迭代：
// name = None
// for name in globals():
// print(name)
// 因为在调用 globals() 之前就把 name 放入全局命名空间了，迭代器会包含 'name' 作为一个键，所以输出会包含 name（通常在输出的某处看到 "name"）。
// 循环结束后 name 等于最后一次迭代得到的键（覆盖了 None）。