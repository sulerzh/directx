using System;
using System.Globalization;
using System.Threading;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public interface ILatLonProvider
  {
    Action<Exception> OnInternalError { get; set; }

    CultureInfo ModelCulture { get; set; }

    void GetLatLon(GeoField geoField, int firstGeoCol, int count, Func<int, int, string> geoValuesAccessor, Func<int, int> geoRowsAccessor, CancellationToken cancellationToken, double[] lat, double[] lon, GeoResolutionBorder[] boundingBox, GeoAmbiguity[] ambiguities);

    void GetLatLonAsync(GeoField geoField, int firstGeoCol, int count, Func<int, int, string> geoValuesAccessor, Func<int, int> geoRowsAccessor, CancellationToken cancellationToken, double[] lat, double[] lon, GeoResolutionBorder[] boundingBox, GeoAmbiguity[] ambiguities, CompletionStats stats, Action<object, int, int, int> latLonResolvedCallback = null, Action<object> completionCallback = null, object context = null);

    bool GetLatLon(string geoQuery, CancellationToken cancellationToken, out double lat, out double lon, out GeoResolutionBorder boundingBox, out GeoAmbiguity ambiguity);

    void GetLatLonAsync(string geoQuery, CancellationToken cancellationToken, Action<object, bool, double, double, GeoResolutionBorder, GeoAmbiguity> completionCallback = null, object context = null);
  }
}
