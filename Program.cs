using QueuingModels;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Serves wwwroot/index.html at "/"
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/mm1", (CalcRequest req) =>
{
    if (double.IsNaN(req.A) || double.IsInfinity(req.A) ||
        double.IsNaN(req.B) || double.IsInfinity(req.B))
        return Results.BadRequest(new { error = "Please enter valid numbers." });

    if (req.A <= 0 || req.B <= 0)
        return Results.BadRequest(new { error = "Both values must be greater than 0." });

    try
    {
        // The existing model does all the maths and validation (including rho < 1).
        MM1QueueModel m = req.Mode == "times"
            ? MM1QueueModel.FromMeanTimes(req.A, req.B)
            : new MM1QueueModel(req.A, req.B);

        int maxN = Math.Clamp(req.MaxN, 1, 50);
        var pn = Enumerable.Range(0, maxN + 1)
            .Select(n => new { n, p = m.Pn(n), atLeast = m.PAtLeast(n) })
            .ToList();

        return Results.Ok(new
        {
            lambda = m.Lambda,
            mu = m.Mu,
            rho = m.Rho,
            p0 = m.P0,
            l = m.L,
            lq = m.Lq,
            ls = m.Ls,
            w = m.W,
            wq = m.Wq,
            probWait = m.ProbWait,
            pn
        });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();

record CalcRequest(string Mode, double A, double B, int MaxN);