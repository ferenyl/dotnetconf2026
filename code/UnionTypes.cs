#:property LangVersion=preview

foreach (var arg in args)
{
    Volume volume = int.TryParse(arg, out var number) ? new Volume(number) : new Volume(arg);

    Console.WriteLine(volume switch
    {
        int i => $"int: {i}",
        string s => $"string: {s}",
    });
}


//or maybe data

List<Trips> trips = [
    new Trip(10, DateTimeOffset.Now),
    new RTTrip("20", DateTimeOffset.Now)
];
// inline. använd hellre property i din union
// foreach (var item in trips)
// {
//     (int id, DateTimeOffset date) = item switch
//     {
//         Trip t => (t.Id, t.Date),
//         RTTrip rt => (int.Parse(rt.SamtransId), rt.TripDate)
//     };
//
//     Console.WriteLine($"Trip {id} {date}");
// }
foreach (var item in trips)
{
    Console.WriteLine($"Trip {item.Desc}");
}

public record Trip(int Id, DateTimeOffset Date);
public record RTTrip(string SamtransId, DateTimeOffset TripDate);

public union Volume(string, int);
public union Trips(Trip, RTTrip)
{
    public string Desc => this switch
    {
        Trip t => $"{t.Id} {t.Date}",
        RTTrip rt => $"{rt.SamtransId} {rt.TripDate}"

    };
};
