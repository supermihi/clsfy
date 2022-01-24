namespace Clsfy.MusicBrainz.Interface;

public record Release(Guid Id, string Title, DateTime? Date, IReadOnlyList<Medium> Media, IReadOnlyList<Guid> ArtistCredits);