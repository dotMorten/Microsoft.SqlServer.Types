using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SqlServer.Types
{
    /// <summary>
    /// Lists supported and extended Open GIS geography types.
    /// </summary>
    public enum OpenGisGeographyType : byte
    {
        /// <summary>
        /// Point is a 0-dimensional object that represents a single location. 
        /// </summary>
        Point = 1,
        /// <summary>
        /// LineString is a one-dimensional object that represents a sequence of points and the line segments connecting them.
        /// </summary>
        LineString = 2,
        /// <summary>
        /// Polygon is a two-dimensional surface stored as a sequence of points defining an exterior bounding ring and zero or more interior rings. 
        /// </summary>
        Polygon = 3,
        /// <summary>
        /// MultiPoint is a collection of zero or more points. 
        /// </summary>
        MultiPoint = 4,
        /// <summary>
        /// MultiLineString is a collection of zero or more geographyLineString instances. 
        /// </summary>
        MultiLineString = 5,
        /// <summary>
        /// MultiPolygon is a collection of zero or more Polygon instances. 
        /// </summary>
        MultiPolygon = 6,
        /// <summary>
        /// GeometryCollection is a collection of zero or more geography instances.
        /// </summary>
        GeometryCollection = 7,
        /// <summary>
        /// A collection of zero or more continuous circular arc segments
        /// </summary>
        CircularString = 8,
        /// <summary>
        /// A collection of zero or more continuous CircularString or LineString instances of either geometry or geography types.
        /// </summary>
        CompoundCurve = 9,
        /// <summary>
        /// CurvePolygon is a topologically closed surface defined by an exterior bounding ring and zero or more interior rings
        /// </summary>
        CurvePolygon = 10,
        /// <summary>
        /// FullGlobe is a special type of Polygon that covers the entire globe.
        /// </summary>
        FullGlobe = 11,
    }
}