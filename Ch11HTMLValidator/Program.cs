using System.Text.Json;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Ch11HTMLValidator;

const string text = """
                    <html>
                        <body>
                        <h1>Title</h1>
                        <p>paragraph</p>
                        <a></a>
                        </body>
                        </html>
                    """;

const string text2 = """
                     <html lang="en">
                     <body class="outline narrow">
                     <p align="left" align="right">paragraph</p>
                     </body>
                     </html>
                     """;

const string text3 = """
                     <html>
                       <head>
                         <title>Software Design by Example</title>
                       </head>
                       <body>
                         <h1>Main Title</h1>
                         <p>introductory paragraph</p>
                         <ul>
                           <li>first item</li>
                           <li>second item is <em>emphasized</em></li>
                         </ul>
                       </body>
                     </html>
                     """;

const string text4 = """
                     <html>
                     <body>
                     <img src="" alt="1"/>
                     <img src=""/>
                     <figure><figcaption>2</figcaption></figure>
                     <figure><figcaption>2</figcaption><figcaption>2</figcaption></figure>

                     </body>
                     </html>
                     """;

const string text5 = """
                     <html>
                     <body>
                     <figure><img src="" alt="3"/><figcaption>3</figcaption></figure>
                     <figure><img src="" alt="4"/><figcaption>5</figcaption></figure>
                     </body>
                     </html>
                     """;

const string text6 = """
                     <html>
                     <body>
                     <h1>1</h1>
                     <h2>2</h2>
                     <h4>4</h4>
                     </body>
                     </html>
                     """;

const string text7 = """
                     <html>
                     <body>
                     <h2>2</h2>
                     <h3>3</h3>
                     <h4>4</h4>
                     </body>
                     </html>
                     """;

var str = new string('=', 30);
var parser = new HtmlParser(new HtmlParserOptions
{
    IsKeepingSourceReferences = true
});
using var doc = await parser.ParseDocumentAsync(text);
using var doc2 = await parser.ParseDocumentAsync(text2);
using var doc3 = await parser.ParseDocumentAsync(text3);
using var doc6 = await parser.ParseDocumentAsync(text6);
using var doc7 = await parser.ParseDocumentAsync(text7);

Display(doc.DocumentElement);
Console.WriteLine(str);

DisplayAttrs(doc2.DocumentElement);
Console.WriteLine(str);

var dict = new Dictionary<string, HashSet<string>>();
dict = Recurse(doc3.DocumentElement, dict);
foreach (var pair in dict)
{
    Console.Write($"{pair.Key}:  ");
    foreach (var item in pair.Value) Console.Write($"{item}, ");

    Console.WriteLine();
}

Console.WriteLine(str);

var catalog = new Catalog();
catalog.Visit(doc3.DocumentElement);
var result = catalog.CatalogDict;
foreach (var pair in result) Console.WriteLine($"{pair.Key}: {string.Join(", ", pair.Value)}");
Console.WriteLine(str);

var checker = new Checker("style.yaml");
checker.Visit(doc3.DocumentElement);
foreach (var pair in checker.Problems) Console.WriteLine($"{pair.Key}: {string.Join(", ", pair.Value)}");
Console.WriteLine(str);

// ex2
var detector = new EmptyDetector();
detector.Visit(doc.DocumentElement);
foreach (var node in detector.EmptyNodes)
    Console.WriteLine($"name:{node.Name}, html: {node.OuterHtml}, line:{node.Line}");
Console.WriteLine(str);

// ex4
var flatten = new Flatten();
flatten.Visit(doc3.DocumentElement);
Console.WriteLine("Flatten List:");
foreach (var (index, item) in flatten.Result.Index())
    if (index != flatten.Result.Count - 1)
        Console.Write($"{item.LocalName} - ");
    else
        Console.WriteLine(item.LocalName);

Console.WriteLine(str);

// ex5
await new MultiChecker(text4, text5).ProcessAsync();
Console.WriteLine(str);

// ex6
var headingChecker = new HeadingChecker();
headingChecker.Visit(doc6.DocumentElement);
var headingChecker2 = new HeadingChecker();
headingChecker2.Visit(doc7.DocumentElement);

return;

static void Display(INode node, int indent = 0)
{
    // 创建缩进字符串
    var indentStr = new string(' ', indent * 2);

    switch (node)
    {
        // 1. 判断是否为文本节点
        case IText textNode:
        {
            // 注意：需要过滤掉纯空白符的文本节点（可选，根据需求调整）
            if (!string.IsNullOrWhiteSpace(textNode.Text))
                Console.WriteLine($"{indentStr}string: '{textNode.Text.Replace("\n", "\\n").Replace("\r", "\\r")}'");

            break;
        }
        // 2. 判断是否为元素节点
        case IElement elementNode:
        {
            Console.WriteLine($"{indentStr}node: {elementNode.TagName.ToLower()}");

            // 递归遍历所有子节点
            foreach (var child in elementNode.ChildNodes) Display(child, indent + 1);

            break;
        }
        // 例如，可以这样处理注释节点
        case IComment commentNode:
            Console.WriteLine($"{indentStr}comment: {commentNode.NodeValue}");
            break;
        default:
            Console.WriteLine($"{indentStr}other: [{node.NodeType}]");
            break;
    }
}

static void DisplayAttrs(INode node)
{
    if (node is IElement element)
    {
        // 处理属性：当同一属性多次出现时，取最后一个值（符合HTML解析器常规行为）
        var attrsDict = new Dictionary<string, string>();
        foreach (var attr in element.Attributes) attrsDict[attr.Name] = attr.Value; // 后出现的属性会覆盖先出现的

        // 使用更清晰的JSON格式输出属性
        var json = JsonSerializer.Serialize(attrsDict);
        Console.WriteLine($"node: {element.LocalName} {json}");

        // 递归遍历所有子节点
        foreach (var child in element.ChildNodes) DisplayAttrs(child);
    }
}

static Dictionary<string, HashSet<string>> Recurse(INode node, Dictionary<string, HashSet<string>> catalog)
{
    switch (node)
    {
        case null:
            throw new ArgumentNullException(nameof(node), "节点不能为空");
        case IElement ele:
        {
            var name = ele.LocalName;
            if (!catalog.ContainsKey(name)) catalog[name] = [];
            foreach (var child in ele.Children)
            {
                catalog[name].Add(child.LocalName);
                Recurse(child, catalog);
            }

            break;
        }
    }

    return catalog;
}