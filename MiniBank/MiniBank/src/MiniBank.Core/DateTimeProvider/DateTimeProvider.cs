namespace MiniBank.Core.DateTimeProvider;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow()
    {
        return DateTime.UtcNow;
    }
}