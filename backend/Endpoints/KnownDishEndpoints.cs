using Microsoft.EntityFrameworkCore;
using Tambouille.Data;

namespace Tambouille.Endpoints;

public static class KnownDishEndpoints
{
    public static void MapKnownDishEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/known-dishes").RequireAuthorization();

        group.MapGet("/", async (AppDbContext db) =>
        {
            var names = await db.KnownDishes
                .OrderBy(k => k.Name)
                .Select(k => k.Name)
                .ToListAsync();

            return Results.Ok(names);
        });
    }
}
