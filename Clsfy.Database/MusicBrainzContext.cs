using Microsoft.EntityFrameworkCore;

namespace Clsfy.Database;

public class MusicBrainzContext : DbContext {
  public MusicBrainzContext(DbContextOptions<MusicBrainzContext> options) : base(options) { }

  public DbSet<Artist> Artists => Set<Artist>();
  public DbSet<Release> Releases => Set<Release>();
  public DbSet<Medium> Media => Set<Medium>();
  public DbSet<Track> Tracks => Set<Track>();
  public DbSet<Work> Works => Set<Work>();
  public DbSet<Instrument> Instruments => Set<Instrument>();
  public DbSet<Recording> Recordings => Set<Recording>();

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Release>()
                .HasMany(r => r.Artists)
                .WithMany(a => a.Releases)
                .UsingEntity<ReleaseArtistCredit>(
                  j => j.HasOne(rac => rac.Artist).WithMany(a => a.ReleaseArtistCredits)
                        .HasForeignKey(rac => rac.ArtistId),
                  j => j.HasOne(rac => rac.Release).WithMany(r => r.ReleaseArtistCredits)
                        .HasForeignKey(rac => rac.ReleaseId),
                  j => j.HasKey(rac => new { rac.ArtistId, rac.ReleaseId }));

    modelBuilder.Entity<Work>()
                .HasMany(w => w.Artists)
                .WithMany(a => a.RelatedWorks)
                .UsingEntity<WorkArtistRelation>(
                  j => j.HasOne(war => war.Artist).WithMany(a => a.WorkRelations).HasForeignKey(war => war.ArtistId),
                  j => j.HasOne(war => war.Work).WithMany(w => w.ArtistRelations).HasForeignKey(war => war.WorkId),
                  j => j.HasKey(war => new { war.ArtistId, war.WorkId }));
    modelBuilder.Entity<WorkWorkRelation>()
                .HasKey(r => new { r.ContainingId, r.PartId });
    modelBuilder.Entity<WorkWorkRelation>()
                .HasOne(r => r.Containing)
                .WithMany(w => w.PartRelations)
                .HasForeignKey(r => r.ContainingId);
    modelBuilder.Entity<WorkWorkRelation>()
                .HasOne(r => r.Part)
                .WithMany(p => p.ContainingRelations)
                .HasForeignKey(r => r.PartId);

    modelBuilder.Entity<Artist>()
                .HasMany(artist => artist.Recordings)
                .WithMany(recording => recording.Performers)
                .UsingEntity<RecordingArtistRelation>();
    /*modelBuilder.Entity<RecordingArtistRelation>()
                .HasKey(r => new { r.ArtistId, r.RecordingId, r.Type });
    modelBuilder.Entity<RecordingArtistRelation>()
                .HasOne(r => r.Recording)
                .WithMany(r => r.Performers)
                .HasForeignKey(r => r.RecordingId);
    modelBuilder.Entity<RecordingArtistRelation>()
                .HasOne(r => r.Artist)
                .WithMany(a => a.RecordingRelations)
                .HasForeignKey(r => r.ArtistId);*/
  }
}
