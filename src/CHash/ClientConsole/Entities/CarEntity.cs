
using Core.Abstractions;

namespace ClientConsole.Entities;

public class CarEntity : EntityBase
{
    public string Name { get; set; }
    
    public double Price { get; set; }
    
    public DateTime StartDate { get; set; }
}