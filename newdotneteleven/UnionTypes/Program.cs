foreach (var arg in args)
{
    Volume volume = int.TryParse(arg, out var number) ? new Volume(number) : new Volume(arg);

    Console.WriteLine(volume switch
    {
        int i => $"int: {i}",
        string s => $"string: {s}",
    });
}

public union Volume(string, int);
