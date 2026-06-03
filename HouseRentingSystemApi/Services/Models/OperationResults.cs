namespace HouseRentingSystemApi.Services.Models
{
    /// <summary>Резултат при опит за наемане на къща.</summary>
    public enum RentResult
    {
        Success,
        NotFound,
        AlreadyRented
    }

    /// <summary>Резултат при опит за освобождаване на къща.</summary>
    public enum ReleaseResult
    {
        Success,
        NotFound,
        NotRented,
        Forbidden
    }
}
