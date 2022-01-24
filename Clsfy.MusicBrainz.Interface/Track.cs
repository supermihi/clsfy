namespace Clsfy.MusicBrainz.Interface;

public record Track(Guid Id, int Position, TimeSpan? Length, Guid RecordingId);