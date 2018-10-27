using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Microsoft.SqlServer.Types
{
    /// <summary>
    /// Converts geometries to and from Well-Known Text.
    /// Also supports Z and M values (OGC SFS v1.2.0) as well as reordering of X and Y.
    /// </summary>
    internal static class WktWriter
    { 
		/// <summary>
		/// Writes a <see cref="Geometry"/> instance as Well-Known Text according to the OGC Simple Features Specification 1.2.0.
		/// </summary>
		/// <param name="g">Geometry</param>
		/// <returns>Well-Known Text</returns>
		public static string Write(ShapeData g)
		{
			return Write(g, g.HasZ, g.HasM);
		}

		/// <summary>
		/// Writes a <see cref="Geometry"/> instance as Well-Known Text according to the OGC Simple Features Specification 1.2.0.
		/// </summary>
		/// <param name="g">Geometry</param>
		/// <param name="includeZ">Include Z values</param>
		/// <param name="includeM">Include M values</param>
		/// <returns>Well-Known Text</returns>
		public static string Write(ShapeData g, bool includeZ, bool includeM)
		{
			StringBuilder sb = new StringBuilder();
			WriteGeometry(g, sb, includeZ, includeM);
			return sb.ToString();
		}

		private static void WriteGeometry(ShapeData geometry, StringBuilder sb, bool includeZ, bool includeM)
		{
            var type = geometry.Type;
            sb.Append(type.ToString().ToUpperInvariant());
            if (geometry.IsEmpty)
            {
                sb.Append(" EMPTY");
                return;
            }
            else sb.Append(" (");
			if (type == OGCGeometryType.Point)
				WritePoint(geometry, sb, includeZ, includeM);
			else if (type == OGCGeometryType.LineString)
				WriteLineString(geometry, sb, includeZ, includeM);
			else if (type == OGCGeometryType.Polygon)
				WritePolygon(geometry, sb, includeZ, includeM);
			else if (type == OGCGeometryType.MultiPoint)
				WriteMultiPoint(geometry, sb, includeZ, includeM);
			else if (type == OGCGeometryType.MultiLineString)
				WriteMultiLineString(geometry, sb, includeZ, includeM);
			else if (type == OGCGeometryType.MultiPolygon)
				WriteMultiPolygon(geometry, sb, includeZ, includeM);
			else if (type == OGCGeometryType.GeometryCollection)
				WriteGeometryCollection(geometry, sb, includeZ, includeM);
			else
				throw new ArgumentException("Invalid Geometry Type");
            sb.Append(")");
        }

        private static void WritePoint(ShapeData point, StringBuilder sb, bool includeZ, bool includeM)
        {
            WriteCoordinate(point.GetPointN(1), sb, includeZ, includeM);
        }

		private static void WriteMultiPoint(ShapeData points, StringBuilder sb, bool includeZ, bool includeM)
		{
			for (int i = 0; i < points.NumPoints; i++)
			{
				if (i > 0) sb.Append(",");
				WriteCoordinate(points.GetPointN(i+1), sb, includeZ, includeM);
			}
		}

		private static void WriteLineString(ShapeData line, StringBuilder sb, bool includeZ, bool includeM)
		{
			WriteCoordinateCollection(GetVertices(line), sb, includeZ, includeM);			
		}

		private static void WriteMultiLineString(ShapeData lines, StringBuilder sb, bool includeZ, bool includeM)
		{
            sb.Append('(');
            for (int i = 0; i < lines.NumGeometries; i++)
			{
                if (i > 0) sb.Append("),(");
				WriteCoordinateCollection(GetVertices(lines.GetRing(i)), sb, includeZ, includeM);
            }
            sb.Append(")");
        }

        private static void WritePolygon(ShapeData polygon, StringBuilder sb, bool includeZ, bool includeM)
		{
			WritePolygonContents(polygon, sb, includeZ, includeM);
		}

        private static IEnumerable<PointZM> GetVertices(ShapeData g)
        {
            return Enumerable.Range(1, g.NumPoints).Select(s => g.GetPointN(s));
        }

		private static void WritePolygonContents(ShapeData polygon, StringBuilder sb, bool includeZ, bool includeM)
		{
            sb.Append('(');
            WriteCoordinateCollection(GetVertices(polygon.GetRing(0)), sb, includeZ, includeM);
            for (int i = 0; i < polygon.NumInteriorRing; i++)
            {
                sb.Append("),(");
                WriteCoordinateCollection(GetVertices(polygon.GetRing(i + 1)), sb, includeZ, includeM);
            }
            sb.Append(')');
        }

        private static void WriteMultiPolygon(ShapeData polys, StringBuilder sb, bool includeZ, bool includeM)
		{
            sb.Append('(');
            for (int i = 0; i < polys.NumGeometries; i++)
            {
                if (i > 0) sb.Append("),(");
                WritePolygonContents(polys.GetGeometryN(i+1), sb, includeZ, includeM);
            }
            sb.Append(')');
        }

		private static void WriteGeometryCollection(ShapeData geoms, StringBuilder sb, bool includeZ, bool includeM)
		{
			for (int i = 0; i < geoms.NumGeometries; i++)
			{
				if (i > 0) sb.Append(",");
				WriteGeometry(geoms.GetGeometryN(i+1), sb, includeZ, includeM);
			}
		}

		private static void WriteCoordinateCollection(IEnumerable<PointZM> coords, StringBuilder sb, bool includeZ, bool includeM)
		{
            bool firstItem = true;
			foreach(var c in coords)
			{
                if (firstItem) firstItem = false;
                else sb.Append(", ");
				WriteCoordinate(c, sb, includeZ, includeM);
			}
		}

		private static void WriteCoordinate(PointZM coord, StringBuilder sb, bool includeZ, bool includeM)
		{
			sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1}", coord.X, coord.Y);
            if (includeZ)
            {
                if (!double.IsNaN(coord.Z))
                    sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", coord.Z);
            }

            if (includeM)
            {
                if (!double.IsNaN(coord.M))
                    sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", coord.M);
            }
		}
	}
}
