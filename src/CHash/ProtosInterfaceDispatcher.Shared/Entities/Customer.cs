namespace ProtosInterfaceDispatcher.Shared.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}