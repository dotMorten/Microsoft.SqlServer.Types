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
#pragma warning disable CS0618 // Type or member is obsolete
    public interface IGeographySink110 : IGeographySink
#pragma warning restore CS0618 // Type or member is obsolete
    {
        void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2);
    }
}
