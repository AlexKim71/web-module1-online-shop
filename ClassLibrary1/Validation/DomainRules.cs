using System.Globalization;
using System.Text.RegularExpressions;
using Domain.Models;

namespace Domain.Validation;

public static class DomainRules
{
    private static readonly Regex CodeRegex = new("^[A-Z0-9-]{3,20}$", RegexOptions.Compiled);

    public static string RequireTrimmed(string? value, int min, int max, string field)
    {
        var s = (value ?? string.Empty).Trim();
        if (s.Length < min || s.Length > max)
            throw new ArgumentException($"{field}: довжина має бути від {min} до {max} символів.");
        return s;
    }

    public static void ValidateRequestInput(string customerName, int quantity)
    {
        RequireTrimmed(customerName, 2, 80, "Ім’я замовника");
        if (quantity <= 0 || quantity > 100_000)
            throw new ArgumentException("Quantity має бути додатним і не більше 100 000.");
    }

    public static void ValidateResource(Resource r)
    {
        if (r.CategoryId < 1) throw new ArgumentException("Категорія обов’язкова.");
        if (!CodeRegex.IsMatch(r.Code))
            throw new ArgumentException("Code: 3-20 символів A-Z, 0-9 або дефіс.");

        r.Name = RequireTrimmed(r.Name, 2, 120, "Назва товару");
        r.Sku = RequireTrimmed(r.Sku, 3, 30, "Sku");
        r.Brand = RequireTrimmed(r.Brand, 1, 60, "Brand");

        if (r.PriceMinor < 0 || r.PriceMinor > 1_000_000_000)
            throw new ArgumentException("Ціна має бути в межах 0..1 000 000 000 копійок.");
        if (r.Capacity < 0 || r.Capacity > 100_000)
            throw new ArgumentException("Capacity має бути 0..100 000.");
        if (r.Available < 0 || r.Available > r.Capacity)
            throw new ArgumentException("Available має бути 0..Capacity.");
    }

    public static bool TryParseUahToMinor(string? input, out long minor)
    {
        minor = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var s = input.Trim().Replace(',', '.');
        if (!decimal.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var uah))
            return false;

        if (uah < 0) return false;

        var rounded = decimal.Round(uah, 2, MidpointRounding.AwayFromZero);
        if (rounded != uah) return false; // більше ніж 2 знаки після коми

        var kop = rounded * 100m;
        if (kop > 1_000_000_000m) return false;

        minor = (long)kop;
        return true;
    }
}