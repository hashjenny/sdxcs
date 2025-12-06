using Ch9Protocols;
using static Ch9Protocols.Env;

using var ex2Timer = new Ex2Timer();

using var f = FakeIt();
var r = Adder.Add(2, 3, 4, 5);
Console.WriteLine(r);

Foo();
Foo();
Foo();

return;

[LogFile("log.txt")]
void Foo()
{
    var arr = new int[9000000];
    for (var i = 0; i < arr.Length; i++) arr[i] = i;
}