using Ch9Protocols;

using var f = new ContextFake(nameof(Adder.Add), arr => 10 * (int)arr[0] + (int)arr[1]);
Console.WriteLine(f.Call(2, 3));