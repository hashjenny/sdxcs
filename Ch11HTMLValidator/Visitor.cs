using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Ch11HTMLValidator;

internal class Visitor
{
    public void Visit(INode node)
    {
        switch (node)
        {
            case IText textNode:
                VisitText(textNode);
                break;
            case IElement ele:
                ElementEnter(ele);
                foreach (var child in ele.ChildNodes) Visit(child);
                ElementExit(ele);
                break;
        }
    }

    protected virtual void VisitText(IText textNode)
    {
    }

    protected virtual void ElementEnter(IElement ele)
    {
    }

    protected virtual void ElementExit(IElement ele)
    {
    }
}

internal class Catalog : Visitor
{
    public Dictionary<string, HashSet<string>> CatalogDict { get; } = [];
    
    protected override void ElementEnter(IElement ele)
    {
        var name = ele.LocalName;
        if (!CatalogDict.ContainsKey(name)) CatalogDict[name] = [];
        foreach (var child in ele.ChildNodes)
            if (child is IElement childEle)
                CatalogDict[name].Add(childEle.LocalName);
    }
}

internal class Checker(string manifest) : Visitor
{
    private Dictionary<string, HashSet<string>> Manifest { get; } = ReadManifest(manifest);
    public Dictionary<string, HashSet<string>> Problems { get; } = [];

    // ex1
    // Rewrite it to make it easier to understand.
    // 改写它，使其更容易理解。
    protected override void ElementEnter(IElement ele)
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

internal record ElementInfo(string Name = "", string OuterHtml = "", int? Line = null);

internal class EmptyDetector : Visitor
{
    public List<ElementInfo> EmptyNodes { get; } = [];

    protected override void ElementEnter(IElement ele)
    {
        if (!IsEmptyButNotSelfClosing(ele)) return;
        var sourceRef = ele.SourceReference;
        var info = new ElementInfo
        {
            Name = ele.LocalName,
            OuterHtml = ele.OuterHtml,
            Line = sourceRef?.Position.Line
        };
        
        if (info.Line is > 0)  // 只保留有真实行号的
        {
            EmptyNodes.Add(info);
        }
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