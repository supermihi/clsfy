using Microsoft.EntityFrameworkCore;

namespace MusicBrainz.Partial;

public class MusicBrainzContext : DbContext {
  private readonly string _path;

  public MusicBrainzContext(string path) {
    _path = path;
  }
  public DbSet<Artist> Artists { get; set; }
  public DbSet<Release> Releases { get; set; }
  public DbSet<Medium> Media { get; set; }
  public DbSet<Track> Tracks { get; set; }
  public DbSet<Work> Works { get; set; }
  public DbSet<Recording> Recordings { get; set; }
  public DbSet<ReleaseArtistCredit> ReleaseArtistCredits { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Release>()
                .HasMany(r => r.Artists)
                .WithMany(a => a.Releases)
                .UsingEntity<ReleaseArtistCredit>(j => j.HasOne(rac => rac.Artist).WithMany(a => a.ReleaseArtistCredits)
                                                        .HasForeignKey(rac => rac.ArtistId),
                                                  j => j.HasOne(rac => rac.Release).WithMany(r => r.ReleaseArtistCredits)
                                                        .HasForeignKey(rac => rac.ReleaseId),
                                                  j => j.HasKey(rac => new { rac.ArtistId, rac.ReleaseId }));

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
  }

  protected override void OnConfiguring(DbContextOptionsBuilder options) {
    options.UseSqlite($"Data Source={_path}");
  }
}