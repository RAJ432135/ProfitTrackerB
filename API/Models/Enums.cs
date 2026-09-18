namespace VehicleProfitTracker.API.Models;

public enum TransactionType
{
    Income,
    Expense
}

public enum TransactionCategory
{
    Trip,       // Income entries
    Diesel,
    Toll,
    Driver,
    Maintenance,
    Food,
    Other
}

public enum VehicleType
{
    Truck,
    Bus,
    MiniTruck,
    Pickup,
    Other
}
