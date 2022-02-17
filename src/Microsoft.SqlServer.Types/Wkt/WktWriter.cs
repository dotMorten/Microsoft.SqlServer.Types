using System.Linq;
using System.Globalization;

namespace Microsoft.SqlServer.Types.Wkt
{
    /// <summary>
    /// Converts geometries to and from Well-Known Text.
    /// Also supports Z and M values (OGC SFS v1.2.0) as well as reordering of X and Y.
    /// </summary>
    internal static class WktWriter
    {
        /// <summary>
        /// Writes a <see cref="ShapeData"/> instance as Well-Known Text according to the OGC Simple Features Specification 1.2.0.
        /// </summary>
        /// <param name="g">Geometry</param>
        /// <param name="order"></param>
        /// <returns>Well-Known Text</returns>
        public static string Write(ShapeData g, CoordinateOrder order)
        {
            if (g.IsNull)
                return "Null";
            return Write(g, g.HasZ, g.HasM, order);
        }

        /// <summary>
        /// Writes a <see cref="ShapeData"/> instance as Well-Known Text according to the OGC Simple Features Specification 1.2.0.
        /// </summary>
        /// <param name="g">Geometry</param>
        /// <param name="includeZ">Include Z values</param>
        /// <param name="includeM">Include M values</param>
        /// <param name="order"></param>
        /// <returns>Well-Known Text</returns>
        public static string Write(ShapeData g, bool includeZ, bool includeM, CoordinateOrder order)
        {
            StringBuilder sb = new StringBuilder();
            WriteGeometry(g, sb, includeZ, includeM, order);
            return sb.ToString();
        }

        private static void WriteGeometry(ShapeData geometry, StringBuilder sb, bool includeZ, bool includeM, CoordinateOrder order)
        {
            var type = geometry.Type;

            // Special handling for FULLGLOBE
            if (type == OGCGeometryType.FullGlobe)
            {
                sb.Append("FULLGLOBE");
                return;
            }
            if (type == OGCGeometryType.Point)
                WritePoint(geometry, sb, includeZ, includeM, order);
            else if (type == OGCGeometryType.LineString)
                WriteLineString(geometry, sb, includeZ, includeM, order);
            else if (type == OGCGeometryType.Polygon)
                WritePolygon(geometry, sb, includeZ, includeM, order);
            else if (type == OGCGeometryType.MultiPoint)
                WriteMultiPoint(geometry, sb, includeZ, includeM, order);
            else if (type == OGCGeometryType.MultiLineString)
                WriteMultiLineString(geometry, sb, includeZ, includeM, order);
            else if (type == OGCGeometryType.MultiPolygon)
                WriteMultiPolygon(geometry, sb, includeZ, includeM, order);
            else if (type == OGCGeometryType.GeometryCollection)
                WriteGeometryCollection(geometry, sb, includeZ, includeM, order);
            else
                throw new ArgumentException("Invalid Geometry Type");
        }

        private static void WritePoint(ShapeData point, StringBuilder sb, bool includeZ, bool includeM, CoordinateOrder order)
        {
            sb.Append("POINT ");
            if (point.IsEmpty)
                sb.Append("EMPTY");
            else
            {
                sb.Append('(');
                WriteCoordinate(point.GetPointN(1), sb, includeZ, includeM, order);
                sb.Append(')');
            }
        }

        private static void WriteMultiPoint(ShapeData points, StringBuilder sb, bool includeZ, bool includeM, CoordinateOrder order)
        {
            sb.Append("MULTIPOINT ");
            if (points.Shapes.Length < 2)
                sb.Append("EMPTY");
            else
            {
                sb.Append('(');
                for (int i = 1; i < points.Shapes.Length; i++)
                {
                    if (i > 1) sb.Append(", ");
                    var p = points.GetGeometryN(i);
                    if (p.IsEmpty)
                        sb.Append("EMPTY");
                    else
                    {
                        sb.Append('(');
                        WriteCoordinate(p.GetPointN(1), sb, includeZ, includeM, order);
                        sb.Append(')');
                    }
                }
                sb.Append(')');
            }
        }

        private static void WriteLineString(ShapeData line, StringBuilder sb, bool includeZ, bool includeM, CoordinateOrder order)
        {
            sb.Append("LINESTRING ");
            if (line.IsEmpty)
                sb.Append("EMPTY");
            else
            {
                sb.Append('(');
                WriteCoordinateCollection(GetVertices(line), sb, includeZ, includeM, order);
                sb.Append(')');
            }
        }

        private static void WriteMultiLineString(ShapeData lines, StringBuilder sb, bool includeZ, bool includeM, CoordinateOrder order)
        {
            sb.Append("MULTILINESTRING ");
            if (lines.Shapes.Length < 2)
                sb.Append("EMPTY");
            else
            {
                sb.Append('(');
                for (int i = 1; i < lines.Shapes.Length; i++)
                {
                    if (i > 1) sb.Append(", ");
                    var line = lines.GetGeometryN(i);
                    if (line.IsEmpty)
                        sb.Append("EMPTY");
                    else
                    {
                        sb.Append('(');
                        WriteCoordinateCollection(GetVertices(line), sb, includeZ, includeM, order);
                        sb.Append(')');
                    }
                }
                sb.Append(')');
            }
        }

        private static void WritePolygon(ShapeData polygon, StringBuilder sb, bool includeZ, bool includeM, CoordinateOrder order)
        {
            sb.Append("POLYGON ");
            if (polygon.IsEmpty)
                sb.Append("EMPTY");
            else
            {
                sb.Append('(');
                WritePolygonContents(polygon, sb, includeZ, includeM, order);
                sb.Append(')');
            }
        }

        private static IEnumerable<PointZM> GetVertices(ShapeData g)
        {
            return Enumerable.Range(1, g.NumPoints).Select(s => g.GetPointN(s));
        }

        private static void WritePolygonContents(ShapeData polygon, StringBuilder sb, bool includeZ, bool includeM, CoordinateOrder order)
        {
            sb.Append('(');
            WriteCoordinateCollection(GetVertices(polygon.GetRing(0)), sb, includeZ, includeM, order);
            for (int i = 0; i < polygon.NumInteriorRing; i++)
            {
                sb.Append("), (");
                WriteCoordinateCollection(GetVertices(polygon.GetRing(i + 1)), sb, includeZ, includeM, order);
            }
            sb.Append(')');
        }

        private static void WriteMultiPolygon(ShapeData polys, StringBuilder sb, bool includeZ, bool includeM, CoordinateOrder order)
        {
            sb.Append("MULTIPOLYGON ");
            if (polys.Shapes.Length < 2)
                sb.Append("EMPTY");
            else
            {
                sb.Append('(');
                for (int i = 1; i < polys.Shapes.Length; i++)
                {
                    if (i > 1) sb.Append(", ");
                    var poly = polys.GetGeometryN(i);
                    if (poly.IsEmpty)
                        sb.Append("EMPTY");
                    else
                    {
                        sb.Append('(');
                        WritePolygonContents(poly, sb, includeZ, includeM, order);
                        sb.Append(')');
                    }
                }
                sb.Append(')');
            }
        }

        private static void WriteGeometryCollection(ShapeData geoms, StringBuilder sb, bool includeZ, bool includeM, CoordinateOrder order)
        {
            sb.Append("GEOMETRYCOLLECTION ");
            if (geoms.Shapes.Length < 2)
                sb.Append("EMPTY");
            else
            {
                sb.Append('(');
                for (int i = 1; i < geoms.Shapes.Length; i++)
                {
                    if (i > 1) sb.Append(", ");
                    var geom = geoms.GetGeometryN(i);
                    WriteGeometry(geom, sb, includeZ, includeM, order);
                    i += geom.Shapes.Length - 1;
                }
                sb.Append(')');
            }
        }

        private static void WriteCoordinateCollection(IEnumerable<PointZM> coords, StringBuilder sb, bool includeZ, bool includeM, CoordinateOrder order)
        {
            bool firstItem = true;
            foreach (var c in coords)
            {
                if (firstItem) firstItem = false;
                else sb.Append(", ");
                WriteCoordinate(c, sb, includeZ, includeM, order);
            }
        }

        private static void WriteCoordinate(PointZM coord, StringBuilder sb, bool includeZ, bool includeM, CoordinateOrder order)
        {
            if (order == CoordinateOrder.XY)
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1}", coord.X, coord.Y);
            else
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1}", coord.Y, coord.X);
            if (includeZ)
            {
                if (!double.IsNaN(coord.Z))
                    sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", coord.Z);
                else if (includeM && !double.IsNaN(coord.M))
                    sb.Append(" NULL");
            }

            if (includeM)
            {
                if (!double.IsNaN(coord.M))
                    sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", coord.M);
            }
        }
    }
}