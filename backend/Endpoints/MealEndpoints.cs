using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Tambouille.Data;
using Tambouille.DTOs;
using Tambouille.Models;

namespace Tambouille.Endpoints;

public static class MealEndpoints
{
    public static void MapMealEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/meals").RequireAuthorization();

        group.MapGet("/", async (string weekStart, AppDbContext db) =>
        {
            if (!DateOnly.TryParse(weekStart, out var startDate))
                return Results.BadRequest("weekStart must be a valid date (ISO 8601)");

            var endDate = startDate.AddDays(6);
            var meals = await db.Meals
                .Where(m => string.Compare(m.Date, startDate.ToString("yyyy-MM-dd")) >= 0
                         && string.Compare(m.Date, endDate.ToString("yyyy-MM-dd")) <= 0)
                .OrderBy(m => m.Date).ThenBy(m => m.Slot)
                .Select(m => new MealDto(m.Id, m.Date, m.Slot, m.Name, m.Persons))
                .ToListAsync();

            return Results.Ok(meals);
        });

        group.MapPost("/", async (CreateMealRequest request, IValidator<CreateMealRequest> validator, AppDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var existing = await db.Meals.AnyAsync(m => m.Date == request.Date && m.Slot == request.Slot);
            if (existing)
                return Results.Conflict("A meal already exists for this day and slot");

            var count = await db.Meals.CountAsync(m => m.Date == request.Date);
            if (count >= 2)
                return Results.UnprocessableEntity("Maximum 2 meals per day");

            var meal = new Meal
            {
                Date = request.Date,
                Slot = request.Slot,
                Name = request.Name,
                Persons = request.Persons
            };

            db.Meals.Add(meal);
            await db.SaveChangesAsync();

            return Results.Created($"/api/meals/{meal.Id}", new MealDto(meal.Id, meal.Date, meal.Slot, meal.Name, meal.Persons));
        });

        group.MapPut("/{id:int}", async (int id, UpdateMealRequest request, IValidator<UpdateMealRequest> validator, AppDbContext db) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var meal = await db.Meals.FindAsync(id);
            if (meal is null)
                return Results.NotFound();

            meal.Name = request.Name;
            meal.Persons = request.Persons;
            await db.SaveChangesAsync();

            return Results.Ok(new MealDto(meal.Id, meal.Date, meal.Slot, meal.Name, meal.Persons));
        });

        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var meal = await db.Meals.FindAsync(id);
            if (meal is null)
                return Results.NotFound();

            db.Meals.Remove(meal);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}
