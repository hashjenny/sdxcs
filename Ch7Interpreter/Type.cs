namespace Ch7Interpreter;

public abstract record Node;

public record NumberNode(double Value) : Node;

public record StringNode(string Value) : Node;

public record BoolNode(bool Value) : Node;

public record NullNode : Node;

public record ArrayNode(List<Node> Items) : Node;

[AttributeUsage(AttributeTargets.Method)]
public class MethodAttribute : Attribute;

public class TLLException(string msg) : Exception(msg);