using Ch9Protocols;

using var ex2Timer = new Ex2Timer();

using var f = new ContextFake(nameof(Adder.Add), arr => 10 * (int)arr[0] + (int)arr[1]);
Console.WriteLine(f.Call(2, 3));

Foo();
Foo();
Foo();



[LogFile("log.txt")]
void Foo()
{
    var arr = new int[9000000];
    for (var i = 0; i < arr.Length; i++) arr[i] = i;
}