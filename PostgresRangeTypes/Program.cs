using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace PostgresRangeTypes;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to PostgreSQL ranges demo");

        await using var demoContext = new DemoContext();
            
        await demoContext.MeetingRooms.ExecuteDeleteAsync();
        await demoContext.Meetings.ExecuteDeleteAsync();

        await demoContext.SaveChangesAsync();

        var winterfellMeetingRoom = new MeetingRoom { Name = "Winterfell", MaxCapacity = 30 };
        var jobriaMeetingRoom = new MeetingRoom { Name = "Jobria", MaxCapacity = 20 };
        var casterlyRockMeetingRoom = new MeetingRoom { Name = "Casterly Rock", MaxCapacity = 15 };

        demoContext.MeetingRooms.Add(winterfellMeetingRoom);
        demoContext.MeetingRooms.Add(jobriaMeetingRoom);
        demoContext.MeetingRooms.Add(casterlyRockMeetingRoom);

        await demoContext.SaveChangesAsync();

        demoContext.Meetings.Add(new Meeting
        {
            Title = "PostgreSQL Demo",
            Room = winterfellMeetingRoom,
            Time = new NpgsqlRange<DateTime>(new DateTime(2023, 12, 23, 11, 30, 0, DateTimeKind.Utc),
                                             new DateTime(2023, 12, 23, 12, 30, 0, DateTimeKind.Utc))
        });

        demoContext.Meetings.Add(new Meeting
        {
            Title = "Npgsql Demo",
            Room = jobriaMeetingRoom,
            Time = new NpgsqlRange<DateTime>(new DateTime(2023, 12, 23, 13, 00, 0, DateTimeKind.Utc),
                                             new DateTime(2023, 12, 23, 13, 30, 0, DateTimeKind.Utc))
        });

        demoContext.Meetings.Add(new Meeting
        {
            Title = "Upgrade PostgreSQL",
            Room = casterlyRockMeetingRoom,
            Time = new NpgsqlRange<DateTime>(new DateTime(2023, 12, 23, 14, 00, 0, DateTimeKind.Utc),
                                             new DateTime(2023, 12, 23, 15, 30, 0, DateTimeKind.Utc))
        });

        await demoContext.SaveChangesAsync();

        var targetRange = new NpgsqlRange<DateTime>(new DateTime(2023, 12, 23, 13, 00, 0, DateTimeKind.Utc),
                                                    new DateTime(2023, 12, 23, 15, 00, 0, DateTimeKind.Utc));

        //Find all meetings that occur in the specified time range
        var meetings = await demoContext.Meetings.Where(m => m.Time.ContainedBy(targetRange)).ToListAsync();

        //Find all meetings that overlap with the specified time range
        meetings = await demoContext.Meetings.Where(m => m.Time.Overlaps(targetRange)).ToListAsync();

        //Find all busy times in the specified time range
        var meetingsWithBusyTimes = await demoContext.Meetings.Where(m => m.Time.Overlaps(targetRange)).Select(
            m => new
            {
                Meeting = m,
                BusyTime = m.Time.Intersect(targetRange)
            }).ToListAsync();

        try
        {
            demoContext.Meetings.Add(new Meeting
            {
                Title = "Conflicting meeting",
                Room = casterlyRockMeetingRoom,
                Time = new NpgsqlRange<DateTime>(new DateTime(2023, 12, 23, 15, 00, 0, DateTimeKind.Utc),
                                                 new DateTime(2023, 12, 23, 16, 00, 0, DateTimeKind.Utc))
            });
            await demoContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var npgsqlException = ex.GetBaseException() as NpgsqlException;
            if (npgsqlException?.SqlState == PostgresErrorCodes.ExclusionViolation)
            {
                Console.WriteLine("This meeting overlaps with another meeting in the same room");
            }
        }
    }
}