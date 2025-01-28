namespace Nefarious.Common.Events;

// I wanna implement event driven design for the spotify component so that I can later on scale and log the whole history of them. Potentially have a ledger of all tracked entities.
public abstract record Event(Guid StreamId)
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}