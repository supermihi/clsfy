using Clsfy.MusicBrainz.Interface;
using MetaBrainz.MusicBrainz;
using Microsoft.Extensions.Options;

namespace Clsfy.MusicBrainz;

public class QueryOptions {
  public string? MusicBrainzHost { get; set; } = "http://localhost:5000";
}

public class QueryWrapper : IQueryWrapper {
  public Query Query { get; }
  public QueryWrapper(IOptions<QueryOptions> options) {
    Query = new Query("clsfy", "0.1.0", "michaelhelmling@posteo.de");
    var host = options.Value.MusicBrainzHost;
    if (host != null) {
      var uri = new Uri(host);
      Query.Server = uri.Host;
      Query.Port = uri.Port;
      Query.UrlScheme = uri.Scheme;
      Query.DelayBetweenRequests = 0;
    }
  }
}
