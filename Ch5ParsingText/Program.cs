using Ch5ParsingText;

var calc = new CalcTokenizer("1 + 2 * 3");
var parser = calc.Parser();
Console.WriteLine(CalcTokenizer.Format(parser));

var calc2 = new CalcTokenizer("1 + 2 +3");
calc2.Print();

var calc3 = new CalcTokenizer("1 + 2 +3 * 4 + 5 *6");
calc3.Print();

var result = new CalcTokenizer("1+  2 * 3.2 / 4");
result.Print();