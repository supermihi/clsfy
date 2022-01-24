using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace Clsfy.MusicBrainz.Interface;

public static class Extensions {
  private static readonly string[] Locales = new[] { "de", "en" };
  public static string? GetPreferredAlias(this IAliasedEntity entity) {
    if ((entity.Aliases?.Count ?? 0) == 0) {
      return null;
    }
    var localAliases = entity.Aliases!.Where(a => a.Locale != null && Locales.Contains(a.Locale));
    return localAliases.OrderBy(a => Array.IndexOf(Locales, a)).ThenBy(a => a.Primary ? 1 : 2).FirstOrDefault()?.Name;
  }
}