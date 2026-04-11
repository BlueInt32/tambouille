namespace Tambouille.Models;

public class Meal
{
    public int Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public int Slot { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Persons { get; set; } = 2;
}
