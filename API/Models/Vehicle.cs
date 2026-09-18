namespace VehicleProfitTracker.API.Models;

public class Vehicle : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string VehicleNumber { get; set; } = string.Empty; // e.g. BR05AB1234
    public VehicleType VehicleType { get; set; }

    public List<Transaction> Transactions { get; set; } = new();
}
