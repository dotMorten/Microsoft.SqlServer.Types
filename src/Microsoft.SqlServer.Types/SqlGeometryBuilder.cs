using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.SqlServer.Types
{
    /// <summary>
    /// Constructs instances of <see cref="SqlGeometry"/> objects by using <see cref="IGeometrySink"/> interface.
    /// </summary>
    /// <remarks>
    /// Throws <see cref="FormatException"/> for an invalid call sequence or when a call sequence is incomplete when <see cref="ConstructedGeometry"/>() is invoked.
    /// </remarks>
    /// <example>
    /// <para>The following example constructs a <see cref="SqlGeometry"/> object from a <c>MultiLineString</c>.</para>
    /// <code>
    /// SqlGeometryBuilder b = new SqlGeometryBuilder();
    /// b.SetSrid(0);
    /// b.BeginGeometry(OpenGisGeometryType.MultiLineString);
    /// b.BeginGeometry(OpenGisGeometryType.LineString);
    /// b.BeginFigure(1, 1);
    /// b.AddLine(3, 4);
    /// b.EndFigure();
    /// b.EndGeometry();
    /// b.BeginGeometry(OpenGisGeometryType.LineString);
    /// b.BeginFigure(-5, -3);
    /// b.AddLine(2, 2);
    /// b.EndFigure(); 
    /// b.EndGeometry();
    /// b.EndGeometry();
    /// SqlGeometry g = b.ConstructedGeometry();
    /// </code>
    /// </example>
    public class SqlGeometryBuilder : IGeometrySink110
    {
        private readonly ShapeDataBuilder _builder;
        private int _srid = 0;

        /// <summary>
        /// Constructs a <see cref="SqlGeometryBuilder"/> object.
        /// </summary>
        public SqlGeometryBuilder()
        {
            _builder = new ShapeDataBuilder();
        }

        /// <summary>
        /// Retrieves constructed spatial geometry object.
        /// </summary>
        public virtual SqlGeometry ConstructedGeometry => new SqlGeometry(_builder.ConstructedShapeData, _srid);

        /// <summary>
        /// Initializes a call sequence for a geometry type.
        /// </summary>
        /// <param name="type"></param>
        public virtual void BeginGeometry(OpenGisGeometryType type) => _builder.BeginGeo((OGCGeometryType)type);

        /// <summary>
        /// Starts the call sequence for a geometry figure.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void BeginFigure(double x, double y) => BeginFigure(x, y, null, null);

        /// <summary>
        /// Starts the call sequence for a geometry figure.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="m"></param>
        public virtual void BeginFigure(double x, double y, double? z, double? m)
        {
            if (double.IsNaN(x))
                throw new ArgumentException(nameof(x));
            if (double.IsNaN(y))
                throw new ArgumentException(nameof(y));
            _builder.BeginFigure();
            _builder.AddPoint(x, y, z, m);
        }

        /// <summary>
        /// Constructs additional points in a <c>geometry</c> type figure.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddLine(double x, double y) => AddLine(x, y, null, null);

        /// <summary>
        /// Constructs additional points in the call sequence for a geometry type.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="m"></param>
        public virtual void AddLine(double x, double y, double? z, double? m)
        {
            if (double.IsNaN(x) || double.IsInfinity(x))
                throw new ArgumentException(nameof(x));
            if (double.IsNaN(y) || double.IsInfinity(y))
                throw new ArgumentException(nameof(y));
            _builder.AddPoint(x, y, z, m);
        }

        /// <summary>
        /// Finishes a call sequence for a geometry figure.
        /// </summary>
        public virtual void EndFigure() => _builder.EndFigure();

        /// <summary>
        /// Finishes a call sequence for a geometry type.
        /// </summary>
        public virtual void EndGeometry() => _builder.EndGeo();

        /// <summary>
        /// Sets the Spatial Reference Identifier (SRID) for a geometry type call sequence.
        /// </summary>
        /// <param name="srid"></param>
        public virtual void SetSrid(int srid)
        {
            _srid = srid;
        }

        /// <summary>
        /// Adds a circular arc to the path.
        /// </summary>
        /// <param name="x1">The x1 value.</param>
        /// <param name="y1">The y1 value.</param>
        /// <param name="z1">The z1 value.</param>
        /// <param name="m1">The m1 value.</param>
        /// <param name="x2">The x2 value.</param>
        /// <param name="y2">The y2 value.</param>
        /// <param name="z2">The z2 value.</param>
        /// <param name="m2">The m2 value.</param>
        public virtual void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a circular arc to the path.
        /// </summary>
        /// <param name="x1">The first latitude for the arc.</param>
        /// <param name="y1">The first longitude for the arc.</param>
        /// <param name="x2">The second latitude for the arc.</param>
        /// <param name="y2">The second longitude for the arc.</param>
        public void AddCircularArc(double x1, double y1, double x2, double y2) => AddCircularArc(x1, y2, null, null, x2, y2, null, null);
    }
}
