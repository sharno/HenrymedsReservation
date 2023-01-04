using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HenrymedsReservation;

public class DB : DbContext
{
    public DbSet<PeriodDB> Periods { get; set; }
    public DbSet<SlotDB> Slots { get; set; }

    public string DbPath { get; }

    public DB()
    {
        DbPath = "reservation.sqlite";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
=> options.UseSqlite($"Data Source={DbPath}");

}

[Index(nameof(ProviderId))]
public class PeriodDB
{
    [Key]
    public int PeriodId { get; set; }
    public int ProviderId { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
    public List<SlotDB> Slots { get; } = new();
}

public class SlotDB
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SlotId { get; set; }
    public int ClientId { get; set; }
    public int PeriodId { get; set; }
    [ForeignKey("PeriodId")]
    public PeriodDB? Period { get; set; }
    public SlotStatus Status { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset ReservationTime { get; set; }
}

public enum SlotStatus
{
    Reserved, Confirmed
}
