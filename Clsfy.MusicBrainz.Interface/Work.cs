namespace Clsfy.MusicBrainz.Interface;

public record Work(Guid Id, string Title, List<Guid> partOf, IReadOnlyList<Guid> PartOf);
