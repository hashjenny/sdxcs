using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ch11HTMLValidator;

public class Visitor
{
    // ex7
    // Modify the checking tool so that it reports the full path for style violations when it finds a problem, e.g., reports ‘div.div.p(meaning "a paragraph in a div in another div") instead of justp`.
    // 修改检查工具，使其在发现问题时报告样式违规的完整路径，例如报告“div.div.p (meaning "a paragraph in a div in another div") instead of just p”。
    public void Visit(INode node, List<string>? path = null)
    {
        path ??= [];
        switch (node)
        {
            case IText textNode:
                VisitText(textNode);
                break;
            case IElement ele:
                path.Add(ele.LocalName);
                ElementEnter(ele, path);
                foreach (var child in ele.ChildNodes) Visit(child, [..path]);
                ElementExit(ele, path);
                break;
        }
    }

    protected virtual void VisitText(IText textNode)
    {
    }

    protected virtual void ElementEnter(IElement ele, List<string> path)
    {
    }

    protected virtual void ElementExit(IElement ele, List<string> path)
    {
    }
}

public class Catalog : Visitor
{
    public Dictionary<string, HashSet<string>> CatalogDict { get; } = [];

    protected override void ElementEnter(IElement ele, List<string> path)
    {
        var name = ele.LocalName;
        if (!CatalogDict.ContainsKey(name)) CatalogDict[name] = [];
        foreach (var child in ele.ChildNodes)
            if (child is IElement childEle)
                CatalogDict[name].Add(childEle.LocalName);
    }
}

public class Checker(string manifest) : Visitor
{
    private Dictionary<string, HashSet<string>> Manifest { get; } = ReadManifest(manifest);
    public Dictionary<string, HashSet<string>> Problems { get; } = [];

    // ex1
    // Rewrite it to make it easier to understand.
    // 改写它，使其更容易理解。
    protected override void ElementEnter(IElement ele, List<string> path)
    {
        // var errors = ele.Children
        //     .Select(child => child.LocalName)
        //     .Except(Manifest.GetValueOrDefault(ele.LocalName) ?? [])
        //     .ToList();
        // if (errors.Count == 0) return;
        // if (Problems.TryGetValue(ele.LocalName, out var problem))
        //     problem.UnionWith(errors);
        // else
        //     Problems[ele.LocalName] = errors.ToHashSet();

        var allowedChildren = Manifest.GetValueOrDefault(ele.LocalName);
        if (allowedChildren is null) return;
        var errors = ele.Children
            .Select(child => child.LocalName)
            .Where(name => !allowedChildren.Contains(name))
            .ToHashSet();
        if (Problems.TryGetValue(ele.LocalName, out var problem))
            problem.UnionWith(errors);
        else
            Problems[ele.LocalName] = errors;
    }

    private static Dictionary<string, HashSet<string>> ReadManifest(string filename)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, filename);
        var manifest = File.ReadAllText(fullPath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = deserializer.Deserialize<Dictionary<string, List<object>>>(manifest);
        var catalog = new Dictionary<string, HashSet<string>>();
        foreach (var entry in yaml)
        {
            var set = new HashSet<string>();
            foreach (var child in entry.Value) set.Add(child.ToString()!);

            catalog[entry.Key] = set;
        }

        return catalog;
    }
}

public class EmptyDetector : Visitor
{
    public List<ElementInfo> EmptyNodes { get; } = [];

    protected override void ElementEnter(IElement ele, List<string> path)
    {
        if (!IsEmptyButNotSelfClosing(ele)) return;
        var sourceRef = ele.SourceReference;
        var info = new ElementInfo
        {
            Name = ele.LocalName,
            OuterHtml = ele.OuterHtml,
            Line = sourceRef?.Position.Line
        };

        if (info.Line is > 0) // 只保留有真实行号的
            EmptyNodes.Add(info);
    }

    private static bool IsEmptyButNotSelfClosing(IElement ele)
    {
        // 完全没有子节点
        if (ele.ChildNodes.Length == 0)
            return true;

        // 有子节点，但都是空白文本
        return ele.ChildNodes.All(node =>
            node is IText textNode
            && string.IsNullOrWhiteSpace(textNode.Text));
    }
}

public record ElementInfo(string Name = "", string OuterHtml = "", int? Line = null);

// ex4
// Write a visitor that returns a flat list containing all the nodes in a DOM tree in the order in which they would be traversed. 
// 编写一个访问者，它返回一个包含 DOM 树中所有节点的扁平列表，节点顺序与遍历顺序相同。
public class Flatten : Visitor
{
    public readonly List<IElement> Result = [];

    protected override void ElementEnter(IElement ele, List<string> path)
    {
        Result.Add(ele);
    }
}

// ex5
// Write a program that reads one or more HTML pages and reports images in them that do not have an alt attribute.
// 编写一个程序，它读取一个或多个 HTML 页面，并报告其中没有 alt 属性的图像。
// Extend your program so that it also reports any figure elements that do not contain exactly one figcaption element.
// 扩展你的程序，使其也能报告任何不包含恰好一个 figcaption 元素的 figure 元素。
// Extend your program again so that it warns about images with redundant text (i.e., images in figures whose alt attribute contains the same text as the figure’s caption).
// 再次扩展你的程序，使其能警告包含冗余文本的图像（即，在图形中，图像的 alt 属性包含与图形标题相同的文本）。
public class MultiChecker(params string[] html) : Visitor
{
    private string[] HtmlFiles { get; } = html;

    public async Task ProcessAsync()
    {
        var parser = new HtmlParser(new HtmlParserOptions
        {
            IsKeepingSourceReferences = true
        });
        foreach (var htmlFile in HtmlFiles)
        {
            using var doc = await parser.ParseDocumentAsync(htmlFile);
            Visit(doc.DocumentElement);
        }
    }

    protected override void ElementEnter(IElement ele, List<string> path)
    {
        var sourceRef = ele.SourceReference;
        var hasError = false;
        switch (ele.LocalName)
        {
            case "img":
                if (!ele.HasAttribute("alt"))
                    hasError = true;
                break;
            case "figure":
                var images = ele.QuerySelectorAll("img");
                var figcaptions = ele.QuerySelectorAll("figcaption");
                if (figcaptions.Length != 1)
                {
                    hasError = true;
                    break;
                }

                if (images.Length != 0)
                {
                    var figcaptionText = figcaptions[0].TextContent.Trim();
                    var imgAlt = images[0].GetAttribute("alt")?.Trim() ?? "";
                    if (figcaptionText == imgAlt) hasError = true;
                }

                break;
        }

        if (hasError)
        {
            Console.WriteLine($"{string.Join("->", path)}");
            Console.WriteLine($"tag: {ele.LocalName}, outer: {ele.OuterHtml}, line: {sourceRef?.Position.Line}");
        }
    }
}

// ex6
// Write a program that checks the ordering of headings in a page:
// 编写一个检查页面标题顺序的程序：
// There should be exactly one h1 element, and it should be the first heading in the page.
// 应该恰好有一个 h1 元素，并且它应该是页面的第一个标题。
// Heading levels should never increase by more than 1, i.e., an h1 should only ever be followed by an h2, an h2 should never be followed directly by an h4, and so on.
// 标题级别不应该超过 1 级增加，也就是说， h1 只能由 h2 跟随， h2 不应该直接由 h4 跟随，以此类推。

public class HeadingChecker : Visitor
{
    private readonly HashSet<string> headingTags = ["h1", "h2", "h3", "h4", "h5", "h6"];
    private List<string> Headings { get; } = [];

    protected override void ElementEnter(IElement ele, List<string> path)
    {
        var name = ele.LocalName;
        var sourceRef = ele.SourceReference;
        var hasError = false;
        if (headingTags.Contains(name))
        {
            if (Headings.Count == 0)
            {
                if (name != "h1") hasError = true;
                else
                    Headings.Add(name);
            }
            else
            {
                var latest = int.Parse(Headings[^1][1..]);
                var current = int.Parse(name[1..]);
                if (current == latest || current - 1 == latest) Headings.Add(name);
                else
                    hasError = true;
            }
        }


        if (hasError)
        {
            Console.WriteLine($"{string.Join("->", Headings)}-x-{name}");
            Console.WriteLine($"tag: {ele.LocalName}, outer: {ele.OuterHtml}, line: {sourceRef?.Position.Line}");
        }
    }
}