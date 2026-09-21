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

public enum UserRole
{
    User,
    Admin
}

public enum SubscriptionStatus
{
    None,
    Active,
    Cancelled,
    Expired,
    Pending
}
