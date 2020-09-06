using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SqlServer.Types
{
    public class SqlGeographyBuilder : IGeographySink110
    {
        private readonly ShapeDataBuilder _builder;
        private int _srid = -1;

        public SqlGeographyBuilder()
        {
            _builder = new ShapeDataBuilder();
        }

        public virtual SqlGeography ConstructedGeography
        {
            get
            {
                if (_srid < 0)
                    throw new FormatException($"System.FormatException: 24300: Expected a call to SetSrid, but Finish was called.");
                return new SqlGeography(_builder.ConstructedShapeData, _srid);
            }
        }

        public virtual void BeginGeography(OpenGisGeographyType type)
        {
            if (_srid < 0)
                throw new FormatException($"System.FormatException: 24300: Expected a call to SetSrid, but BeginGeography({type}) was called.");
            _builder.BeginGeo((OGCGeometryType)type);
        }

        public void BeginFigure(double latitude, double longitude) => BeginFigure(latitude, longitude, null, null);

        public virtual void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            ValidateLatLon(latitude, longitude);
            _builder.BeginFigure();
            _builder.AddPoint(latitude, longitude, z, m);
        }

        public void AddLine(double latitude, double longitude) => AddLine(latitude, longitude, null, null);

        public virtual void AddLine(double latitude, double longitude, double? z, double? m)
        {
            ValidateLatLon(latitude, longitude);
            _builder.AddPoint(latitude, longitude, z, m);
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

        public virtual void EndFigure() => _builder.EndFigure();

        public virtual void EndGeography() => _builder.EndGeo();

        public virtual void SetSrid(int srid)
        {
            if ((srid < 4120 || srid > 4999) && srid != 104001)
                throw new ArgumentOutOfRangeException(nameof(srid), "SRID must be between 4120 and 4999 (inclusive)");
            _srid = srid;
        }

        public virtual void AddCircularArc(double latitude1, double longitude1, double? z1, double? m1, double latitude2, double longitude2, double? z2, double? m2)
        {
            throw new NotImplementedException();
        }
    }
}
