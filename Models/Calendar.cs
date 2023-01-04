using Microsoft.EntityFrameworkCore;

namespace HenrymedsReservation.Models;

public class Calendar
{
    public readonly record struct TimeRange(DateTimeOffset From, DateTimeOffset To);

    public static void AddTimeRanges(ProviderId provider, IEnumerable<TimeRange> ranges)
    {
        using var db = new DB();
        var periods = ranges.Select(r => new PeriodDB
        {
            ProviderId = provider.Id,
            From = r.From,
            To = r.To
        });
        db.Periods.AddRange(periods);
        db.SaveChanges();
    }

    public static List<Slot> GetAvailableSlots(ProviderId provider)
    {
        RemoveExpiredReservedSlots();

        using var db = new DB();
        var periods = db.Periods.Include(p => p.Slots)
            .Where(p => p.ProviderId == provider.Id).ToList();

        var availableSlots = new List<Slot>();
        foreach (var period in periods)
        {
            var time = period.From;
            var bookedSlots = period.Slots.ToDictionary(s => s.Start);
            while (time.AddMinutes(15) <= period.To)
            {
                if (!bookedSlots.ContainsKey(time))
                {
                    var slot = new FreeSlot(time);
                    availableSlots.Add(slot);
                }
                time = time.AddMinutes(15);
            }
        }
        return availableSlots;
    }

    public static void ReserveSlot(ClientId client, ProviderId provider, ValidatedDTO dto)
    {
        RemoveExpiredReservedSlots();

        using var db = new DB();
        var periods = db.Periods.Include(p => p.Slots)
            .Where(p => p.ProviderId == provider.Id).ToList();
        var period = periods.FirstOrDefault(period =>
                period.From <= dto.DTO &&
                period.To >= dto.DTO.AddMinutes(15));

        // if the time period doesn't exist or there's a slot already in that time
        if (period == null || period.Slots.Any(s => s.Start == dto.DTO))
        {
            throw new ArgumentException("This slot is unavailable");
        }
        // reservations must be made at least 24 hours in advance
        if (dto.DTO > DateTimeOffset.Now.AddDays(1))
        {
            throw new ArgumentException("Reservations must be made at least 24 hours in advance");
        }

        db.Slots.Add(new SlotDB
        {
            ClientId = client.Id,
            PeriodId = period.PeriodId,
            Start = dto.DTO,
            Status = SlotStatus.Reserved,
            ReservationTime = DateTimeOffset.UtcNow
        });

        db.SaveChanges();
    }

    public static void ConfirmSlot(ClientId client, ProviderId provider, ValidatedDTO dto)
    {
        // assuming nothing else removed the reserved slot even if it expired
        // I'll allow confirming this slot
        // it's easier I think than removing it and asking the patient to reserve it again

        using var db = new DB();

        var slot = db.Slots
            .FirstOrDefault(s => s.ClientId == client.Id && s.Period!.ProviderId == provider.Id && s.Start == dto.DTO);

        // if the time period doesn't exist or there's a slot already in that time
        if (slot == null)
        {
            throw new ArgumentException("This slot is unavailable or expired");
        }

        slot.Status = SlotStatus.Confirmed;
        db.SaveChanges();
    }

    public static void RemoveExpiredReservedSlots()
    {
        using var db = new DB();

        // EntityFramework isn't able to translate time in sqlite it seems, so we need to load it in memory
        // filter reservations that are older than 30 min
        var toRemove = db.Slots.Where(s => s.Status == SlotStatus.Reserved).ToList()
            .Where(s => s.ReservationTime < DateTimeOffset.UtcNow.AddMinutes(-30));

        db.Slots.RemoveRange(toRemove);
        db.SaveChanges();
    }

    public static ValidatedDTO ValidateDTO(DateTimeOffset dto)
    {
        if (dto.Second == 0 &&
            (dto.Minute == 0 || dto.Minute == 15 || dto.Minute == 30 || dto.Minute == 45))
        {
            return new ValidatedDTO(dto);
        }
        throw new ArgumentException("The supplied time should be in 15 minutes increments only");
    }

    public static void ValidateTimeRange(TimeRange trange)
    {
        ValidateDTO(trange.From);
        ValidateDTO(trange.To);
        if (trange.From >= trange.To)
        {
            throw new ArgumentException("The supplied time range has its start after or equal to its end");
        }
    }
}
