namespace RacingHubCarRental;

/// <summary>
/// Represents a single car item returned from a car search query.
/// Clean, modern, immutable-friendly version of the auto-generated EF result.
/// </summary>
public sealed class CarSearchResult
{
    /// <summary>
    /// License plate number of the vehicle.
    /// </summary>
    public string LicensePlate { get; init; } = string.Empty;

    /// <summary>
    /// Manufacturer name (e.g., Toyota, Honda).
    /// </summary>
    public string ManufacturerName { get; init; } = string.Empty;

    /// <summary>
    /// Model name (e.g., Prius, Civic).
    /// </summary>
    public string ManufacturerModelName { get; init; } = string.Empty;

    /// <summary>
    /// Year the vehicle was produced.
    /// </summary>
    public int ProductionYear { get; init; }

    /// <summary>
    /// Indicates whether the car uses a manual transmission.
    /// </summary>
    public bool ManualGear { get; init; }

    /// <summary>
    /// Standard daily rental price.
    /// </summary>
    public decimal DailyPrice { get; init; }

    /// <summary>
    /// Additional fee applied per day of late return.
    /// </summary>
    public decimal DayDelayPrice { get; init; }

    /// <summary>
    /// Path or URL to the car's image.
    /// </summary>
    public string CarImage { get; init; } = string.Empty;
}
 
