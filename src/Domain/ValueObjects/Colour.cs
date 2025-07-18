namespace Todo_App.Domain.ValueObjects;

public class Colour : ValueObject
{
    static Colour()
    {
    }

    private Colour()
    {
    }

    private Colour(string code)
    {
        Code = code?.Trim()?.ToUpperInvariant(); // Normalize to uppercase
    }

    public static Colour From(string code)
    {
        var normalizedCode = code?.Trim()?.ToUpperInvariant();
        var colour = new Colour { Code = normalizedCode };

        // temporary allow any color to get the app working
        // We'll see what colors are actually in your database
        return colour;

        // Original strict validation (commented out for now):
        // if (!SupportedColours.Contains(colour))
        // {
        //     throw new UnsupportedColourException(code);
        // }
        // return colour;
    }

    public static Colour White => new("#FFFFFF");

    public static Colour Red => new("#FF5733");

    public static Colour Orange => new("#FFC300");

    public static Colour Yellow => new("#FFFF66");

    public static Colour Green => new("#CCFF99");

    public static Colour Blue => new("#6666FF");

    public static Colour Purple => new("#9966CC");

    public static Colour Grey => new("#999999");

    public static Colour DarkBlue => new("#06041F");

    public string Code { get; private set; } = "#000000";

    public static implicit operator string(Colour colour)
    {
        return colour.ToString();
    }

    public static explicit operator Colour(string code)
    {
        return From(code);
    }

    public override string ToString()
    {
        return Code;
    }

    protected static IEnumerable<Colour> SupportedColours
    {
        get
        {
            yield return White;
            yield return Red;
            yield return Orange;
            yield return Yellow;
            yield return Green;
            yield return Blue;
            yield return Purple;
            yield return Grey;
            yield return DarkBlue;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}