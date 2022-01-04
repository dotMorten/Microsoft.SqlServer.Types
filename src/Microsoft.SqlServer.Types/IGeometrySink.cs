namespace Microsoft.SqlServer.Types
{
    /// <summary>
    /// Defines the interface that the <see cref="SqlGeometryBuilder"/> class uses to construct a <see cref="SqlGeometryBuilder"/> object. This API is obsolete. <see cref="IGeometrySink110"/> should be used instead.
    /// </summary>
    [Obsolete("IGeometrySink is obsolete, use IGeometrySink110 instead.")]
    public interface IGeometrySink
    {
        /// <summary>
        /// Defines points other than the starting endpoint in a geometry instance.
        /// </summary>
        /// <param name="x">A double that specifies the x-coordinate of a point in a geometry instance.</param>
        /// <param name="y">A double that specifies the y-coordinate of a point in a geometry instance.</param>
        /// <param name="z">A double that specifies the z-coordinate of a point in a geometry instance. Is Nullable.</param>
        /// <param name="m">A double that specifies the measure for the point. Is Nullable.</param>
        void AddLine(double x, double y, double? z, double? m);

        /// <summary>
        /// Defines the starting endpoint for a geometry instance.
        /// </summary>
        /// <param name="x">A double that specifies the x-coordinate of the starting endpoint of a geometry instance.</param>
        /// <param name="y">A double that specifies the y-coordinate of the starting endpoint of a geometry instance.</param>
        /// <param name="z">A double that specifies the z-coordinate of the starting endpoint of a geometry instance. Is Nullable.</param>
        /// <param name="m">A double that specifies the measure for the point. Is Nullable.</param>
        void BeginFigure(double x, double y, double? z, double? m);

        /// <summary>
        /// Starts the call sequence of a geometry type.
        /// </summary>
        /// <param name="type">OpenGisGeometryType object that indicates the type being created by the call sequence.</param>
        void BeginGeometry(OpenGisGeometryType type);

        /// <summary>
        /// Finishes a call sequence for a geometry figure.
        /// </summary>
        void EndFigure();

        /// <summary>
        /// Finishes a call sequence for a geometry figure.
        /// </summary>
        void EndGeometry();

        /// <summary>
        /// Sets the Spatial Reference Identifier (SRID) for a geometry type call sequence. 
        /// </summary>
        /// <param name="srid">An int that contains the Spatial Reference Identifier for the geometry type.</param>
        void SetSrid(int srid);
    }

    /// <summary>
    /// Defines the interface used by <see cref="SqlGeometryBuilder"/> to construct a <see cref="SqlGeometry"/> object.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public interface IGeometrySink110 : IGeometrySink
#pragma warning restore CS0618 // Type or member is obsolete
    {
        /// <summary>
        /// Adds a circular arc geometry type figure with the specified startpoint and endpoint.
        /// </summary>
        /// <param name="x1">The startpoint x-coordinate (latitude) of the circular arc.</param>
        /// <param name="y1">The startpoint y-coordinate (longitude) of the circular arc.</param>
        /// <param name="z1">The startpoint z-coordinate (altitude) of the circular arc. Is Nullable.</param>
        /// <param name="m1">The startpoint m-coordinate (measure) of the circular arc. Is Nullable.</param>
        /// <param name="x2">The endpoint x-coordinate (latitude) of the circular arc.</param>
        /// <param name="y2">The endpoint y-coordinate (longitude) of the circular arc.</param>
        /// <param name="z2">The endpoint z-coordinate (altitude) of the circular arc. Is Nullable.</param>
        /// <param name="m2">The endpoint m-coordinate (measure) of the circular arc. Is Nullable.</param>
        void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2);
    }
}
