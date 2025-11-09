using Dict = System.Collections.Generic.Dictionary<string, object?>;

Dict? shape = null;
shape = new Dict
{
    ["_new"] = new Func<string, Dict>(ShapeNew),
    ["_classname"] = "Shape",
    ["parents"] = null,
    ["density"] = new Func<Dict, double, double>(ShapeDensity)
};

Dict? entity = null;
entity = new Dict
{
    ["_new"] = new Func<string, Dict>(EntityNew),
    ["_classname"] = "entity",
    ["parents"] = null,
    ["display"] = new Action<Dict>(EntityDisplay)
};

Dict? item = null;
item = new Dict
{
    ["_new"] = new Func<string, Dict>(ItemNew),
    ["_classname"] = "item",
    ["parents"] = new[] { entity }
};

Dict? square = null;
square = new Dict
{
    ["_new"] = new Func<string, int, Dict>(SquareNew),
    ["_classname"] = "Square",
    ["parents"] = new[] { shape, item },
    ["perimeter"] = new Func<Dict, double>(SquarePerimeter),
    ["area"] = new Func<Dict, double>(SquareArea),
    ["larger"] = new Func<Dict, double, bool>(SquareLarger)
};

Dict? circle = null;
circle = new Dict
{
    ["_new"] = new Func<string, double, Dict>(CircleNew),
    ["_classname"] = "Circle",
    ["parents"] = new[] { shape },
    ["perimeter"] = new Func<Dict, double>(CirclePerimeter),
    ["area"] = new Func<Dict, double>(CircleArea),
    ["larger"] = new Func<Dict, double, bool>(CircleLarger)
};

var examples = new[] { Make(square, "sq", 3), Make(circle, "ci", 2) };
foreach (var example in examples)
{
    example.TryGetValue("name", out var name);
    var perimeter = Call(example, "perimeter");
    var area = Call(example, "area");
    var classname = ((Dict)example["_class"]!)["_classname"];
    var isLarger = Call(example, "larger", 10);
    var density = Call(example, "density", 5);
    Console.WriteLine($"{name} is a {classname}: perimeter = {perimeter:F2}, area = {area:F2}, density = {density:F2}");
    Console.WriteLine($"is {name} larger 10? {isLarger}");
}

Call(examples[0], "display");

return;

#region Shape

Dict ShapeNew(string name)
{
    return new Dict
    {
        ["name"] = name,
        ["_class"] = shape
    };
}

static double ShapeDensity(Dict thing, double weight)
{
    return weight / Call(thing, "area");
}

#endregion

#region Item & Entity
// Multiple Inheritance 

Dict ItemNew(string name)
{
    return new Dict
    {
        ["name"] = name,
        ["_class"] = item
    };
}

Dict EntityNew(string name)
{
    return new Dict
    {
        ["name"] = name,
        ["_class"] = entity
    };
}

static void EntityDisplay(Dict thing)
{
    Console.WriteLine(thing["name"]);
}

#endregion

#region Square

static double SquarePerimeter(Dict thing)
{
    return 4 * Convert.ToDouble(thing["side"]);
}

static double SquareArea(Dict thing)
{
    return Math.Pow(Convert.ToDouble(thing["side"]), 2);
}

static bool SquareLarger(Dict thing, double size)
{
    return Call(thing, "area") > size;
}

Dict SquareNew(string name, int side)
{
    var dict = Make(shape, name);
    dict["side"] = side;
    dict["_class"] = square;
    return dict;
}

#endregion

#region Cicle

static double CirclePerimeter(Dict thing)
{
    return 2 * Math.PI * Convert.ToDouble(thing["radius"]);
}

static double CircleArea(Dict thing)
{
    return Math.PI * Math.Pow(Convert.ToDouble(thing["radius"]), 2);
}

static bool CircleLarger(Dict thing, double size)
{
    return Call(thing, "area") > size;
}

Dict CircleNew(string name, double radius)
{
    var dict = Make(shape, name);
    dict["radius"] = radius;
    dict["_class"] = circle;
    return dict;
}

#endregion

static Dict Make(Dict cls, params object[] args)
{
    if (!cls.TryGetValue("_new", out var value) || value is not Delegate constructor)
        throw new ArgumentException("Dict has no key named _new or _new is not a function");

    var result = constructor.DynamicInvoke(args);
    if (result is Dict dict) return dict;
    throw new InvalidOperationException("_new did not return a Dict instance");
}

static dynamic? Call(Dict thing, string methodName, params object[] args)
{
    var method = FindMethod(thing, methodName);
    switch (method)
    {
        case Func<Dict, double> func1:
            return func1(thing);
        case Func<Dict, double, bool> func2:
        {
            if (args.Length < 1) throw new ArgumentException($"{methodName} requires one numeric argument");
            var num = Convert.ToDouble(args[0]);
            return func2(thing, num);
        }
        case Func<Dict, double, double> func3:
        {
            if (args.Length < 1) throw new ArgumentException($"{methodName} requires one numeric argument");
            var num = Convert.ToDouble(args[0]);
            return func3(thing, num);
        }
        case Action<Dict> action1:
        {
            action1(thing);
            return null;
        }
        default:
            throw new InvalidOperationException($"{methodName} is not expected delegate");
    }
}

static object FindMethod(Dict thing, string methodName)
{
    if (!thing.TryGetValue("_class", out var myClass) || myClass is not Dict myClassDict)
        throw new ArgumentException("Dict has no key named _class or _class is not a Dict");

    var method = FindParent(myClassDict, methodName);
    return method ?? throw new InvalidOperationException($"{methodName} not found in class hierarchy");
}

static object? FindParent(Dict? dict, string methodName)
{
    if (dict is null) return null;
    if (dict.TryGetValue(methodName, out var method) && method is not null) return method;
    if (!dict.TryGetValue("parents", out var value) || value is not Dict[] parents) return null;
    foreach (var parent in parents)
    {
        var result = FindParent(parent, methodName);
        if (result is not null)
        {
            return result;
        }
    }
    return null;
}