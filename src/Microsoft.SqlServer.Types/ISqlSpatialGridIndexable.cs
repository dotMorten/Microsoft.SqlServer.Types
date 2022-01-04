namespace Microsoft.SqlServer.Types
{
    /// <summary>
    /// <para>This API supports the product infrastructure and is not intended to be used directly from your code.</para>
    /// <para>Defines the interface that is implemented by <see cref="SqlGeography"/> and <see cref="SqlGeometry"/> type objects to support spatial indexing.</para>
    /// </summary>
    public interface ISqlSpatialGridIndexable
    {
        /// <summary>
        /// <para>This API supports the product infrastructure and is not intended to be used directly from your code.</para>
        /// <para>Gets the grid coverage.</para>
        /// </summary>
        /// <param name="isTopmostGrid">Indicates whether the grid is a top level (level 1) grid.</param>
        /// <param name="rGridMinX">The x-coordinate of the lower-left corner of the grid.</param>
        /// <param name="rGridMinY">The y-coordinate of the lower-left corner of the grid.</param>
        /// <param name="rGridWidth">The width of the grid.</param>
        /// <param name="rGridHeight">The height of the grid.</param>
        /// <param name="rFuzzX">The x-coordinate tolerance value.</param>
        /// <param name="rFuzzY">The y-coordinate tolerance value.</param>
        /// <param name="cGridRows">The number of rows in the grid.</param>
        /// <param name="cGridColumns">The number of columns in the grid.</param>
        /// <param name="touched">A two-dimensional array of bool values that specifies whether the cells touched the object.</param>
        /// <param name="contained">A two-dimensional array of bool values that specifies whether the cells contained the object.</param>
        /// <param name="cCellsTouched">When this method returns, contains the number of cells that the object touches.</param>
        /// <param name="cCellsContained">When this method returns, contains the number of cells that the object contains.</param>
        /// <param name="fGeometryExceedsGrid">When this method returns, contains a value that indicates whether the object exceeds the grid.</param>
        /// <param name="fHasAmbiguousTouchedCells">When this method returns, contains a value that indicates whether the object includes ambiguously touched cells.</param>
        [SqlMethod]
        void GetGridCoverage(bool isTopmostGrid, double rGridMinX, double rGridMinY, double rGridWidth, double rGridHeight, double rFuzzX, double rFuzzY, int cGridRows, int cGridColumns, bool[,] touched, bool[,] contained, out int cCellsTouched, out int cCellsContained, out bool fGeometryExceedsGrid, out bool fHasAmbiguousTouchedCells);

        /// <summary>
        /// <para>This API supports the product infrastructure and is not intended to be used directly from your code.</para>
        /// <para>Returns the bounding box corners of the <see cref="SqlGeography"/> or <see cref="SqlGeometry"/> instance.</para>
        /// </summary>
        /// <param name="minX">When this method returns, contains the x-coordinate of the lower-left corner of the bounding box.</param>
        /// <param name="minY">When this method returns, contains the y-coordinate of the lower-left corner of the bounding box.</param>
        /// <param name="maxX">When this method returns, contains the x-coordinate of the upper-right corner of the bounding box.</param>
        /// <param name="maxY">When this method returns, contains the y-coordinate of the upper-right corner of the bounding box.</param>
        void GetBoundingBoxCorners(out double minX, out double minY, out double maxX, out double maxY);

        /// <summary>
        /// <para>This API supports the product infrastructure and is not intended to be used directly from your code.</para>
        /// <para>Constructs a buffer for the given distance.</para>
        /// </summary>
        /// <param name="distance">The distance used to calculate the buffer.</param>
        /// <param name="disableInternalFiltering">When this method returns, contains a value that indicates whether internal filtering is disabled.</param>
        /// <returns>The <see cref="ISqlSpatialGridIndexable"/> object that represents the buffer for the given distance.</returns>
        [SqlMethod]
        ISqlSpatialGridIndexable BufferForDistanceQuery(double distance, out bool disableInternalFiltering);

        /// <summary>
        /// This API supports the product infrastructure and is not intended to be used directly from your code. 
        /// Constructs an interior buffer for the given distance.
        /// </summary>
        /// <param name="distance">The distance used to calculate the buffer.</param>
        /// <returns>The <see cref="ISqlSpatialGridIndexable"/> object that represents the interior buffer for the given distance.</returns>
        [SqlMethod]
        ISqlSpatialGridIndexable InteriorBufferForDistanceQuery(double distance);
    }
}
