namespace Tambouille.DTOs;

public record MealDto(int Id, string Date, int Slot, string Name, int Persons);

public record CreateMealRequest(string Date, int Slot, string Name, int Persons);

public record UpdateMealRequest(string Name, int Persons);

public record MoveMealRequest(string Date, int Slot);
