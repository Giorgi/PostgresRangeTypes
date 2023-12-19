using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace PostgresRangeTypes;

class DemoContext : DbContext
{
    public DbSet<MeetingRoom> MeetingRooms { get; set; }
    public DbSet<Meeting> Meetings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Server = 127.0.0.1; Port = 5432;Database=RangeDemo; User Id =postgres; Password = qwerty;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meeting>().Property<string>(e => e.Title).HasMaxLength(100);
        modelBuilder.Entity<MeetingRoom>().Property<string>(e => e.Name).HasMaxLength(100);
    }
}

class MeetingRoom
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int MaxCapacity { get; set; }
}

class Meeting
{
    public int Id { get; set; }

    public string Title { get; set; }

    public MeetingRoom Room { get; set; }

    public NpgsqlRange<DateTime> Time { get; set; }   
}