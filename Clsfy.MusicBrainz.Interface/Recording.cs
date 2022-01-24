using Clsfy.Model.Common;

namespace Clsfy.MusicBrainz.Interface;

public record Recording(
    Guid Id,
    string Title,
    IReadOnlyList<Guid> RecordedWorkIds,
    IReadOnlyList<Performer> Performers);

public record Performer(Guid ArtistId, PerformanceType Type, string? Instrument);
