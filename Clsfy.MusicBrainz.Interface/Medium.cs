namespace Clsfy.MusicBrainz.Interface;

public record Medium(int Position, IReadOnlyList<Track> Tracks);