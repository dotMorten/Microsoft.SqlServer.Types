using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SqlServer.Types
{
    public class SqlGeographyBuilder : IGeographySink110
    {
        private readonly ShapeDataBuilder _builder;
        private int _srid = 4326;

        public SqlGeographyBuilder()
        {
            _builder = new ShapeDataBuilder();
        }

        public SqlGeography ConstructedGeography => new SqlGeography(_builder.ConstructedShapeData, _srid);

        public void BeginGeography(OpenGisGeographyType type) => _builder.BeginGeo((OGCGeometryType)type);

        public void BeginFigure(double lat, double lon) => BeginFigure(lat, lon, null, null);

        public void BeginFigure(double lat, double lon, double? z, double? m)
        {
            ValidateLatLon(lat, lon);
            _builder.BeginFigure();
            _builder.AddPoint(lat, lon, z, m);
        }

        public void AddLine(double lat, double lon) => AddLine(lat, lon, null, null);

        public void AddLine(double lat, double lon, double? z, double? m)
        {
            ValidateLatLon(lat, lon);
            _builder.AddPoint(lat, lon, z, m);
        }

        private void ValidateLatLon(double lat, double lon)
        {
            if (lat < -90 || lat > 90)
                throw new ArgumentOutOfRangeException(nameof(lat));
            if (lon < -15069 || lon > 15069)
                throw new ArgumentOutOfRangeException(nameof(lon));
            if (double.IsNaN(lat) || double.IsInfinity(lat))
                throw new ArgumentException(nameof(lat));
            if (double.IsNaN(lon) || double.IsInfinity(lon))
                throw new ArgumentException(nameof(lon));
        }

        public void EndFigure() => _builder.EndFigure();

        public void EndGeography() => _builder.EndGeo();

        public void SetSrid(int srid)
        {
            if (srid < 4120 || srid > 4999)
                throw new ArgumentOutOfRangeException(nameof(srid), "SRID must be between 4120 and 4999 (inclusive)");
            _srid = srid;
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new NotImplementedException();
        }
    }
}
