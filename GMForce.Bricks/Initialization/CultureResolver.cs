using System.Globalization;

namespace GMForce.Bricks.Initialization;
public class CultureResolver
{
    private readonly string _defaultCulture;
    private readonly IEnumerable<CultureInfo> _supportedCultures;

    public CultureResolver(IList<string> supportedLanguages)
    {
        ArgumentNullException.ThrowIfNull(supportedLanguages);
        if (supportedLanguages.Count == 0)
        {
            throw new ArgumentException("There must be at least one supported language!", nameof(supportedLanguages));
        }

        _defaultCulture = supportedLanguages.First();
        _supportedCultures = supportedLanguages.Select(language => new CultureInfo(language));
    }

    public void SetCulture(string culture)
    {
        var found = GetCultureByName(culture);
        SetCurrentCulture(found);
    }

    private CultureInfo GetCultureByName(string name) => _supportedCultures.SingleOrDefault(culture => culture.Name == name) ??
           _supportedCultures.Single(culture => culture.Name == _defaultCulture);

    private static void SetCurrentCulture(CultureInfo given)
    {
        CultureInfo.CurrentCulture = given;
        CultureInfo.CurrentUICulture = given;
    }
}
