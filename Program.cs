using QueuingModels;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Serves wwwroot/index.html at "/"
app.UseDefaultFiles();
app.UseStaticFiles();

// Seconds contained in one unit of each time option
var secondsPer = new Dictionary<string, double>
{
    ["secs"] = 1.0,
    ["mins"] = 60.0,
    ["hrs"] = 3600.0
};

app.MapPost("/api/queue", (CalcRequest req) =>
{
    if (double.IsNaN(req.A) || double.IsInfinity(req.A) ||
        double.IsNaN(req.B) || double.IsInfinity(req.B))
        return Results.BadRequest(new { error = "Please enter valid numbers." });

    if (req.A <= 0 || req.B <= 0)
        return Results.BadRequest(new { error = "Both values must be greater than 0." });

    if (req.Servers < 1 || req.Servers > 50)
        return Results.BadRequest(new { error = "Number of servers must be a whole number from 1 to 50." });

    if (!secondsPer.TryGetValue(req.InUnit ?? "", out double inSec) ||
        !secondsPer.TryGetValue(req.OutUnit ?? "", out double outSec))
        return Results.BadRequest(new { error = "Please choose a valid time unit." });

    try
    {
        // The model works in the unit the user typed their inputs in.
        // Servers = 1 gives M/M/1, any larger number gives M/M/c.
        MMcQueueModel m = req.Mode == "times"
            ? MMcQueueModel.FromMeanTimes(req.A, req.B, req.Servers)
            : new MMcQueueModel(req.A, req.B, req.Servers);

        // Convert to the unit chosen for the results:
        //  - times (W, Wq) scale by inSec/outSec
        //  - rates (lambda, mu) scale by outSec/inSec
        //  - rho, a, idle time, rho0, L and Lq have no time unit, so they don't change
        double timeFactor = inSec / outSec;
        double rateFactor = outSec / inSec;

        return Results.Ok(new
        {
            servers = m.Servers,
            lambda = m.Lambda * rateFactor,
            mu = m.Mu * rateFactor,
            rho = m.Rho,
            idle = m.IdleFraction,
            a = m.OfferedLoad,
            rho0 = m.Rho0,
            lq = m.Lq,
            wq = m.Wq * timeFactor,
            w = m.W * timeFactor,
            l = m.L
        });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();

record CalcRequest(string Mode, double A, double B, string InUnit, string OutUnit, int Servers);