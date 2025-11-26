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

UnitTest.RunAllTests(tests);