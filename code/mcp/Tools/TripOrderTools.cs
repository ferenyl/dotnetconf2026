using System.ComponentModel;
using System.Globalization;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

/// <summary>
/// Fake trip ordering. The traveler id is a tool argument; the trip details
/// (from, to, departure time) are asked from the user via elicitation.
/// </summary>
internal class TripOrderTools
{
    private const string TripInputKey = "trip";

    private static readonly string[] AddressNames = ["Hem", "Skola"];

    [McpServerTool]
    [Description("Lägger en (fejkad) beställning av en resa. Användaren tillfrågas om från-adress, till-adress och avgångstid.")]
    public static async Task<string> PlaceTripOrder(
        McpServer server,
        RequestContext<CallToolRequestParams> context,
        [Description("Resenärens id")] string travelerId,
        CancellationToken cancellationToken)
    {
        // Round 2 (MRTR retry): the client has answered our elicitation and resent the call.
        if (context.Params?.InputResponses?.TryGetValue(TripInputKey, out var response) == true)
        {
            return PlaceOrder(travelerId, response.Deserialize(InputResponse.ElicitResultJsonTypeInfo));
        }

        var elicitation = CreateTripElicitation();

        // Round 1 (SEP-2322 MRTR): ask the client to collect the trip details and retry.
        if (server.IsMrtrSupported)
        {
            throw new InputRequiredException(
                inputRequests: new Dictionary<string, InputRequest>
                {
                    [TripInputKey] = InputRequest.ForElicitation(elicitation),
                },
                requestState: "awaiting-trip");
        }

        // Fallback for clients without MRTR (only works on a stateful server).
        try
        {
            return PlaceOrder(travelerId, await server.ElicitAsync(elicitation, cancellationToken));
        }
        catch (InvalidOperationException)
        {
            return "Klienten stödjer inte MRTR (protokoll 2026-07-28) och servern är stateless, " +
                   "så resans detaljer kan inte efterfrågas.";
        }
    }

    private static ElicitRequestParams CreateTripElicitation() => new()
    {
        Message = "Ange detaljer för resan.",
        RequestedSchema = new ElicitRequestParams.RequestSchema
        {
            Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>
            {
                ["from"] = new ElicitRequestParams.UntitledSingleSelectEnumSchema
                {
                    Title = "Från",
                    Description = "Adress resan startar från",
                    Enum = AddressNames,
                    Default = "Hem",
                },
                ["to"] = new ElicitRequestParams.UntitledSingleSelectEnumSchema
                {
                    Title = "Till",
                    Description = "Adress resan går till",
                    Enum = AddressNames,
                    Default = "Skola",
                },
                ["departure"] = new ElicitRequestParams.StringSchema
                {
                    Title = "Avgångstid",
                    Description = "Datum och tid för avresa (ISO 8601, t.ex. 2026-11-12T08:00)",
                    Format = "date-time",
                },
            },
            Required = ["from", "to", "departure"],
        },
    };

    private static string PlaceOrder(string travelerId, ElicitResult? result)
    {
        if (result is not { IsAccepted: true, Content: { } content })
        {
            return "Användaren avbröt eller avböjde – ingen resa beställdes.";
        }

        string? from = content.TryGetValue("from", out var f) ? f.GetString() : null;
        string? to = content.TryGetValue("to", out var t) ? t.GetString() : null;
        string? departureText = content.TryGetValue("departure", out var d) ? d.GetString() : null;

        if (from is null || to is null || !AddressNames.Contains(from) || !AddressNames.Contains(to))
        {
            return $"Ogiltig adress. Välj en av: {string.Join(", ", AddressNames)}.";
        }

        if (from == to)
        {
            return "Från- och till-adress kan inte vara samma.";
        }

        if (!DateTimeOffset.TryParse(departureText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var departure))
        {
            return $"Ogiltig avgångstid: '{departureText}'.";
        }

        var orderId = $"ORD-{Random.Shared.Next(100000, 999999)}";
        return $"Resa beställd! Order {orderId}: resenär {travelerId}, {from} → {to}, avgång {departure:yyyy-MM-dd HH:mm}.";
    }
}
