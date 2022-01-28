namespace Clsfy.MusicBrainz.Interface;

public record Work(Guid Id, string Title, IReadOnlyList<Guid> PartOf, IReadOnlyList<Guid> Composers);

public record Artist(Guid Id, string Name);
