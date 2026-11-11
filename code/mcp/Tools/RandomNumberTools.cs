using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

/// <summary>
/// Sample MCP tools for demonstration purposes.
/// These tools can be invoked by MCP clients to perform various operations.
/// </summary>
internal class RandomNumberTools
{
    private const string RangeInputKey = "range";

    [McpServerTool]
    [Description("Generates a random number. The user is asked for the minimum and maximum values.")]
    public static async Task<string> GetRandomNumber(
        McpServer server,
        RequestContext<CallToolRequestParams> context,
        CancellationToken cancellationToken)
    {
        // Round 2 (MRTR retry): the client has answered our elicitation and resent the call.
        if (context.Params?.InputResponses?.TryGetValue(RangeInputKey, out var response) == true)
        {
            var result = response.Deserialize(InputResponse.ElicitResultJsonTypeInfo);
            return Generate(result);
        }

        var elicitation = CreateRangeElicitation();

        // Round 1 (new way, SEP-2322 MRTR): return an input-required result instead of
        // holding the request open. The client asks the user and retries tools/call
        // with the answers in InputResponses.
        if (server.IsMrtrSupported)
        {
            throw new InputRequiredException(
                inputRequests: new Dictionary<string, InputRequest>
                {
                    [RangeInputKey] = InputRequest.ForElicitation(elicitation),
                },
                requestState: "awaiting-range");
        }

        // Fallback for clients without MRTR: classic server-to-client elicitation/create request.
        // Only possible on a stateful (session-based) server; a stateless server cannot send requests.
        try
        {
            return Generate(await server.ElicitAsync(elicitation, cancellationToken));
        }
        catch (InvalidOperationException)
        {
            return "Klienten stödjer inte MRTR (protokoll 2026-07-28) och servern är stateless, " +
                   "så användaren kan inte tillfrågas om min- och maxvärde.";
        }
    }

    private static ElicitRequestParams CreateRangeElicitation() => new()
    {
        Message = "Ange intervallet för slumptalet.",
        RequestedSchema = new ElicitRequestParams.RequestSchema
        {
            Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>
            {
                ["min"] = new ElicitRequestParams.NumberSchema
                {
                    Type = "integer",
                    Title = "Minvärde",
                    Description = "Minsta värde (inklusive)",
                    Default = 0,
                },
                ["max"] = new ElicitRequestParams.NumberSchema
                {
                    Type = "integer",
                    Title = "Maxvärde",
                    Description = "Största värde (exklusive)",
                    Default = 100,
                },
            },
            Required = ["min", "max"],
        },
    };

    private static string Generate(ElicitResult? result)
    {
        if (result is not { IsAccepted: true, Content: { } content })
        {
            return "Användaren avbröt eller avböjde – inget slumptal genererades.";
        }

        int min = content.TryGetValue("min", out var minEl) ? (int)minEl.GetDouble() : 0;
        int max = content.TryGetValue("max", out var maxEl) ? (int)maxEl.GetDouble() : 100;

        if (min >= max)
        {
            return $"Ogiltigt intervall: min ({min}) måste vara mindre än max ({max}).";
        }

        return Random.Shared.Next(min, max).ToString();
    }
}
