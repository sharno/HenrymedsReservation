namespace HenrymedsReservation.Models;

public abstract record Slot(DateTimeOffset Start);
public record FreeSlot(DateTimeOffset Start) : Slot(Start);
public record ReservedSlot(DateTimeOffset Start, DateTimeOffset ReservationTime) : Slot(Start);
public record ConfirmedSlot(DateTimeOffset Start) : Slot(Start);


// I would normally use GUID but int for now for ease of testing
public record ProviderId(int Id);
public record ClientId(int Id);


public record ValidatedDTO(DateTimeOffset DTO);