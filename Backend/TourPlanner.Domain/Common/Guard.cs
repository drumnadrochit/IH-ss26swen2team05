namespace TourPlanner.Domain;

public static class Guard
{
    public static void NotEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{name} must not be empty.", name);
        }
    }

    public static void NotNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} must not be empty.", name);
        }
    }

    public static void Range(int value, int min, int max, string name)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be between {min} and {max}.");
        }
    }

    public static void AgainstNull(object? value, string name) {
        if (value == null) 
        {
            throw new ArgumentNullException(name, $"{name} cannot be null.");
        }
    }
}

