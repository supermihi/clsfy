// See https://aka.ms/new-console-template for more information

using MetaBrainz.MusicBrainz;
using Microsoft.Extensions.Logging;
using MusicBrainz.Partial;

await using var dbContext = new MusicBrainzContext();
await dbContext.Database.EnsureCreatedAsync();
var logger = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug)).CreateLogger<Program>();
var q = new Query("Clsfy", (string?)null, new Uri("mailto:michaelhelmling@posteo.de"));
var db = new PartialMusicBrainzDatabase(dbContext, q, logger);
var release = await db.AddRelease(new Guid("bbb1a432-807a-4247-b23b-0d58ac518c75"));
foreach (var medium in release.Media) {
  Console.WriteLine(medium.Position);
}