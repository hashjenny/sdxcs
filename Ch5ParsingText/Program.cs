using Ch5ParsingText;

var result = new ListParser("[1, [2, [3, 4], 5]]").Parse();
Console.WriteLine(ListParser.FormatList(result));