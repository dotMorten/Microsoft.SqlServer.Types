using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SqlServer.Types
{
    [Obsolete("IGeographySink is obsolete, use IGeographySink110 instead.")]
    public interface IGeographySink
    {
        void AddLine(double latitude, double longitude, double? z, double? m);
        void BeginFigure(double latitude, double longitude, double? z, double? m);
        void BeginGeography(OpenGisGeographyType type);
        void EndFigure();
        void EndGeography();
        void SetSrid(int srid);
    }
    public interface IGeographySink110 : IGeographySink
    {
        void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2);
    }
}
