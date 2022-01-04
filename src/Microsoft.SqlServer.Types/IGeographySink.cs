namespace Microsoft.SqlServer.Types
{
    /// <summary>
    /// Interface used by SqlGeographyBuilder to construct a SqlGeography object. This API is obsolete. <see cref="IGeographySink110"/> should be used instead.
    /// </summary>
    [Obsolete("IGeographySink is obsolete, use IGeographySink110 instead.")]
    public interface IGeographySink
    {
        /// <summary>
        /// Constructs additional points other than the starting endpoint in a geography type figure. This API is obsolete. IGeographySink110 should be used instead.
        /// </summary>
        /// <param name="latitude">A double that specifies the latitude of a point in a geography figure.</param>
        /// <param name="longitude">A double that specifies the longitude of a point in a geography figure.</param>
        /// <param name="z">A double that specifies the altitude of a point in a geography figure. Is Nullable.</param>
        /// <param name="m">A double that specifies the measure type for the point. Is Nullable.</param>
        void AddLine(double latitude, double longitude, double? z, double? m);

        /// <summary>
        /// Starts the call sequence for a geography figure. 
        /// </summary>
        /// <param name="latitude">A double that specifies the latitude of the starting endpoint in a geography figure.</param>
        /// <param name="longitude">A double that specifies the longitude of the starting endpoint in a geography figure.</param>
        /// <param name="z">A double that specifies the altitude of the starting endpoint in a geography figure. Is Nullable.</param>
        /// <param name="m">A double that specifies the measure type for the starting endpoint. Is Nullable.</param>
        void BeginFigure(double latitude, double longitude, double? z, double? m);

        /// <summary>
        /// Initializes a call sequence for a geography type. 
        /// </summary>
        /// <param name="type">OpenGisGeometryType object that indicates the type being created by the call sequence.</param>
        void BeginGeography(OpenGisGeographyType type);

        /// <summary>
        /// Finishes a call sequence for a geography figure. 
        /// </summary>
        void EndFigure();

        /// <summary>
        /// Finishes a call sequence for a geography type.
        /// </summary>
        void EndGeography();

        /// <summary>
        /// Sets the Spatial Reference Identifier (SRID) for a geography type call sequence. 
        /// </summary>
        /// <param name="srid">An int that contains the Spatial Reference Identifier for the geography type.</param>
        void SetSrid(int srid);
    }

    /// <summary>
    /// Defines the interface used by SqlGeographyBuilder to construct a SqlGeography object.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public interface IGeographySink110 : IGeographySink
#pragma warning restore CS0618 // Type or member is obsolete
    {
        /// <summary>
        /// Adds a circular arc geography type figure with the specified startpoint and endpoint.
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
