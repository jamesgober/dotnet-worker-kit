namespace JG.WorkerKit;

/// <summary>
/// Represents a cron expression.
/// </summary>
public sealed class CronExpression
{
    private readonly int[] _minutes;
    private readonly int[] _hours;
    private readonly int[] _days;
    private readonly int[] _months;
    private readonly int[] _daysOfWeek;

    /// <summary>
    /// Initializes a new instance of the <see cref="CronExpression"/> class.
    /// </summary>
    /// <param name="expression">The cron expression.</param>
    public CronExpression(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
        {
            throw new ArgumentException("Invalid cron expression. Must have 5 fields.", nameof(expression));
        }

        _minutes = ParseField(parts[0], 0, 59);
        _hours = ParseField(parts[1], 0, 23);
        _days = ParseField(parts[2], 1, 31);
        _months = ParseField(parts[3], 1, 12);
        _daysOfWeek = ParseField(parts[4], 0, 6); // 0=Sunday
    }

    /// <summary>
    /// Gets the next occurrence after the specified date.
    /// </summary>
    /// <param name="after">The date after which to find the next occurrence.</param>
    /// <returns>The next occurrence.</returns>
    public DateTime GetNextOccurrence(DateTime after)
    {
        var time = new DateTime(after.Year, after.Month, after.Day, after.Hour, after.Minute, 0).AddMinutes(1);

        while (true)
        {
            if (Matches(time))
            {
                return time;
            }

            time = time.AddMinutes(1);
        }
    }

    private bool Matches(DateTime time)
    {
        return ArrayContains(_minutes, time.Minute) &&
               ArrayContains(_hours, time.Hour) &&
               ArrayContains(_days, time.Day) &&
               ArrayContains(_months, time.Month) &&
               ArrayContains(_daysOfWeek, (int)time.DayOfWeek);
    }

    private static bool ArrayContains(int[] array, int value)
    {
        return Array.IndexOf(array, value) >= 0;
    }

    private static int[] ParseField(string field, int min, int max)
    {
        if (field == "*")
        {
            return Enumerable.Range(min, max - min + 1).ToArray();
        }

        var parts = field.Split(',');
        var values = new List<int>();

        foreach (var part in parts)
        {
            if (part.Contains('/'))
            {
                var subparts = part.Split('/');
                var range = subparts[0];
                var step = int.Parse(subparts[1]);

                int[] rangeValues;
                if (range == "*")
                {
                    rangeValues = Enumerable.Range(min, max - min + 1).ToArray();
                }
                else
                {
                    rangeValues = ParseRange(range, min, max);
                }

                for (int i = 0; i < rangeValues.Length; i += step)
                {
                    values.Add(rangeValues[i]);
                }
            }
            else if (part.Contains('-'))
            {
                values.AddRange(ParseRange(part, min, max));
            }
            else
            {
                values.Add(int.Parse(part));
            }
        }

        return values.Distinct().OrderBy(x => x).ToArray();
    }

    private static int[] ParseRange(string range, int min, int max)
    {
        var parts = range.Split('-');
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid range: {range}", nameof(range));
        }

        var startStr = parts[0];
        var endStr = parts[1];

        int start = startStr == "*" ? min : int.Parse(startStr);
        int end = endStr == "*" ? max : int.Parse(endStr);

        return Enumerable.Range(start, end - start + 1).ToArray();
    }
}
