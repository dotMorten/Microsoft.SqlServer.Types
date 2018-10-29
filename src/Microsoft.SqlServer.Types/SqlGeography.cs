using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;
using System.IO;

namespace Microsoft.SqlServer.Types
{
    /// <summary>
    /// The SqlGeography type represents data in a geodetic (round earth) coordinate system.
    /// </summary>
    [SqlUserDefinedType(Format.UserDefined, IsByteOrdered = false, MaxByteSize = -1, IsFixedLength = false)]
    public class SqlGeography : INullable, IBinarySerialize
    {
        private ShapeData _geometry;
        private int srid = 0;

        internal SqlGeography(bool isNull) { IsNull = isNull; }
        
        internal SqlGeography(ShapeData g, int srid)
        {
            this.srid = srid;
            this._geometry = g;
        }

        public SqlGeography()
        {
            _geometry = new ShapeData();
        }

        /// <summary>
        /// Constructs a <see cref="SqlGeography"/> instance representing a Point instance from its x and y values and a spatial reference ID (SRID).
        /// </summary>
        /// <param name="latitude">A double that represents the latitude coordinate of the Point being generated.</param>
        /// <param name="longitude">A double that represents the longitude coordinate of the Point being generated.</param>
        /// <param name="srid">An int expression that represents the SRID of the geography instance you wish to return</param>
        /// <returns>A <see cref="SqlGeography"/> instance constructed from the specified latitude, longitude, and SRID values.</returns>
        [SqlMethod]
        public static SqlGeography Point(double latitude, double longitude, int srid)
        {
            if (!double.IsNaN(latitude) && !double.IsInfinity(latitude) && !double.IsNaN(longitude) && !double.IsInfinity(longitude))
            {
                if (Math.Abs(latitude) > 90.0)
                    throw new FormatException("latitude is not a valid value");
                if (Math.Abs(longitude) > 15069.0)
                    throw new FormatException("longitude is not a valid value");
                return new SqlGeography(new ShapeData(latitude, longitude, null, null), srid);
            }
            throw new FormatException("Invalid coordinates");
        }

        /// <summary>
        /// Returns the longitude property of the geography instance.
        /// </summary>
        /// <value>A SqlDouble value that specifies the longitude.</value>
        /// <remarks>
        /// In the OpenGIS model, Long is defined only on geography instances composed of a single point.
        /// This property will return NULL if geography instances contain more than a single point. This 
        /// property is precise and read-only.
        /// </remarks>
        public SqlDouble Long
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => _geometry.Type == OGCGeometryType.Point && _geometry.NumPoints == 1 ? new SqlDouble(_geometry.Y) : SqlDouble.Null;
        }

        /// <summary>
        /// Returns the latitude property of the geography instance.
        /// </summary>
        /// <value>A SqlDouble value that specifies the latitude.</value>
        /// <remarks>
        /// In the OpenGIS model, Lat is defined only on geography instances composed of a single point.
        /// This property will return NULL if geography instances contain more than a single point. This property
        /// is precise and read-only.
        /// </remarks>
        public SqlDouble Lat
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => _geometry.Type == OGCGeometryType.Point && _geometry.NumPoints == 1 ? new SqlDouble(_geometry.X) : SqlDouble.Null;
        }

        /// <summary>
        /// Gets the Z (elevation) value of the instance. The semantics of the elevation value are user-defined.
        /// </summary>
        /// <value>true if at least one point in a spatial object contains value Z; otherwise false.</value>
        /// <remarks>
        /// <para>The value of this property is null if the geography instance is not a point, as well as for any Point instance for which it is not set.</para>
        /// <para>This property is read-only.</para>
        /// <para>Z-coordinates are not used in any calculations made by the library and are not carried through any library calculations.</para>
        /// </remarks>
        public SqlDouble Z {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => _geometry.Type == OGCGeometryType.Point && _geometry.NumPoints == 1 && _geometry.HasZ ? new SqlDouble(_geometry.Z) : SqlDouble.Null;
        }

        /// <summary>
        /// Returns the M (measure) value of the geography instance.
        /// </summary>
        /// <remarks>
        /// <para>The semantics of the measure value are user-defined but generally describe the distance along a linestring. For example, the measure value could be used to keep track of mileposts along a road.</para>
        /// <para>The value of this property is null if the geography instance is not a Point, as well as for any Point instance for which it is not set.</para>
        /// <para>This property is read-only.</para>
        /// <para>M values are not used in any calculations made by the library and will not be carried through any library calculations.</para>
        /// </remarks>
        public SqlDouble M {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => _geometry.Type == OGCGeometryType.Point && _geometry.NumPoints == 1 && _geometry.HasM ? new SqlDouble(_geometry.M) : SqlDouble.Null;
        }

        /// <summary>
        /// Gets or sets id is an integer representing the Spatial Reference Identifier (SRID) of the instance.
        /// </summary>
        /// <value>A SqlInt32 that represents the SRID of the SqlGeography instance.</value>
        public SqlInt32 STSrid
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => new SqlInt32(srid);
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            set
            {
                if (value.IsNull)
                    throw new System.ArgumentNullException();
                if ((srid < 4120 || srid > 4999) && srid != 104001)
                    throw new ArgumentOutOfRangeException(nameof(srid), "SRID must be between 4120 and 4999 (inclusive)");
                srid = value.Value;
            }
        }

        /// <summary>
        /// Returns true if at least one point in a spatial object contains value Z; otherwise returns false. This property is read-only.
        /// </summary>
        /// <value>true if at least one point in a spatial object contains value Z; otherwise false.</value>
        public bool HasZ
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => _geometry.HasZ;
        }

        /// <summary>
        /// Returns true if at least one point in a spatial object contains value M; otherwise returns false. This property is read-only.
        /// </summary>
        /// <value>true if at least one point in a spatial object contains value M; otherwise false.</value>
        public bool HasM
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => _geometry.HasM;
        }

        /// <summary>
        /// Determines whether the SqlGeography instance is null.
        /// </summary>
        /// <value>A bool value that specifies whether the SqlGeography instance is null. If true, the instance is null. Otherwise, false.</value>
        public bool IsNull
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get;
        }

        /// <summary>
        /// Returns a read-only property providing a null instance of the SqlGeography type.
        /// </summary>
        /// <value>A null instance of the SqlGeography class.</value>
        public static SqlGeography Null {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get;
        } = new SqlGeography(true);

        [SqlMethod(IsDeterministic = true, IsPrecise = true)]
        public SqlInt32 NumRings()
        {
            if (IsNull || (_geometry.Type != OGCGeometryType.Polygon && _geometry.Type != OGCGeometryType.CurvePolygon))
            {
                return SqlInt32.Null;
            }
            return this._geometry.NumRings;
        }

        /// <summary>
        /// Returns the Open Geospatial Consortium (OGC) type name represented by a geography instance.
        /// </summary>
        /// <returns>A SqlString value containing the OGC type name.</returns>
        /// <remarks>
        /// The OGC type names that can be returned by the STGeometryType method are Point, LineString, Polygon, GeometryCollection, MultiPoint, MultiLineString, and MultiPolygon.
        /// </remarks>
        public SqlString STGeometryType()
        {
            if (IsNull) return SqlString.Null;
            return new SqlString(_geometry.Type.ToString());
        }

        /// <summary>
        /// Returns the number of geometries that make up a SqlGeography instance.
        /// </summary>
        /// <returns>A SqlInt32 value that specifies the number of geometries that make up the <see cref="SqlGeography"/> instance. </returns>
        /// <remarks>
        /// This method returns 1 if the geography instance is not a MultiPoint, MultiLineString, MultiPolygon, or GeometryCollection instance, or 0 if the SqlGeography instance is empty.
        /// </remarks>
        public SqlInt32 STNumGeometries() => IsNull ? SqlInt32.Null : _geometry.NumGeometries;

        /// <summary>
        /// Returns the total number of points in each of the figures in a SqlGeography instance
        /// </summary>
        /// <returns>A SqlInt32 value specifying the total number of points in each figure of the <see cref="SqlGeography"/> instance.</returns>
        /// <remarks>
        /// This method counts the points in the description of a SqlGeography instance. Duplicate points are counted.
        /// If this instance is a GeometryCollection, this method returns of the total number of points in each of the
        /// elements in the collection.
        /// </remarks>
        public SqlInt32 STNumPoints() => IsNull ? SqlInt32.Null : _geometry.NumPoints;

        /// <summary>
        /// Returns the number of curves in a one-dimensional SqlGeography instance.
        /// </summary>
        /// <returns>The number of curves.</returns>
        public SqlInt32 STNumCurves()
        {
            if (IsNull) return SqlInt32.Null;

            if (_geometry.Type == OGCGeometryType.LineString)
                return _geometry.IsEmpty ? 0 : _geometry.NumPoints - 1;

            if (_geometry.Type == OGCGeometryType.CircularString)
            {
                if (_geometry.IsEmpty) return 0;
                return (_geometry.NumPoints - 1) / 2;
            }
            if (_geometry.Type != OGCGeometryType.CompoundCurve)
                return SqlInt32.Null;
            return _geometry.NumSegments;
        }

        /// <summary>
        /// Returns a specified geography element in a GeometryCollection or one of its subtypes. 
        /// </summary>
        /// <param name="n">An int expression between 1 and the number of SqlGeography instances in the GeometryCollection.</param>
        /// <returns>A SqlGeography element from the specified instance in the GeometryCollection.</returns>
        /// <remarks>
        /// <para>When this method is used on a subtype of a GeometryCollection, such as MultiPoint or MultiLineString, this method returns the SqlGeography instance if called with N=1.</para>
        /// <para>This method returns null if the parameter is larger than the result of STNumGeometries and will throw an ArgumentOutOfRangeException if the expression parameter is less than 1.</para>
        /// </remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlGeography STGeometryN(int n)
        {
            if (n < 1)
                throw new ArgumentOutOfRangeException(nameof(n));

            if (IsNull || n > STNumGeometries())
                return SqlGeography.Null;
            return new SqlGeography(_geometry.GetGeometryN(n), srid);
        }

        /// <summary>
        /// Returns the specified point in a SqlGeography instance.
        /// </summary>
        /// <param name="n">An int expression between 1 and the number of points in the SqlGeography instance.</param>
        /// <returns>A <see cref="SqlGeography"/> representing the specified point in the calling instance.</returns>
        /// <remarks>
        /// <para>If a SqlGeography instance is user-created, the STPointN method returns the point specified by expression by ordering the points in the order in which they were originally input.</para>
        /// <para>If a SqlGeography instance is constructed by the system, STPointN returns the point specified by expression by ordering all the points in the same order they would be output: first by geography instance, then by ring within the instance(if appropriate), and then by point within the ring.This order is deterministic.</para>
        /// <para>If this method is called with a value less than 1, it throws an ArgumentOutOfRangeException.</para>
        /// <para>If this method is called with a value greater than the number of points in the instance, it returns null.</para>
        /// </remarks>
        public SqlGeography STPointN(int n)
        {
            if (n < 1)
                throw new ArgumentOutOfRangeException(nameof(n));
            if (IsNull)
                return SqlGeography.Null;

            if (n > this._geometry.NumPoints)
                return SqlGeography.Null;

            var p = _geometry.GetPointN(n);
            return new SqlGeography(new ShapeData(p.X, p.Y, HasZ ? (double?)p.Z : null, HasM ? (double?)p.M : null), srid);
        }

        /// <summary>
        /// Returns the start point of a SqlGeography instance. 
        /// </summary>
        /// <returns>A SqlGeography value that represents the start point of the calling SqlGeography.</returns>
        /// <remarks>STStartPoint is the equivalent of STPointN(1).</remarks>
        [SqlMethod(IsDeterministic = true, IsPrecise = true)]
        public SqlGeography STStartPoint() => this.STPointN(1);

        // public SqlGeography STUnion(SqlGeography sqlGeography)
        // {
        //     throw new NotSupportedException();
        // }

        /// <summary>
        /// Returns the end point of a SqlGeography instance.
        /// </summary>
        /// <returns>A SqlGeography value containing the end point.</returns>
        /// <remarks>
        /// <para>STEndPoint is the equivalent of SqlGeography.STPointN(x.STNumPoints()).</para>
        /// <para>This method returns null if called on an empty geography instance.</para>
        /// </remarks>
        [SqlMethod(IsDeterministic = true, IsPrecise = true)]
        public SqlGeography STEndPoint() => STPointN(Math.Max(1, _geometry.NumPoints));

        /// <summary>
        /// Returns the specified ring of the SqlGeography instance: 1 ≤ n ≤ NumRings().
        /// </summary>
        /// <param name="n">An int expression between 1 and the number of rings in a polygon instance.</param>
        /// <returns>A SqlGeography object that represents the ring specified by n.</returns>
        /// <remarks>
        /// If the value of the ring index n is less than 1, this method throws an ArgumentOutOfRangeException. The ring index value must be greater than or equal to 1 and should be less than or equal to the number returned by NumRings.
        /// </remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlGeography RingN(int n)
        {
            if (n < 1)
                throw new ArgumentOutOfRangeException(nameof(n));
            if (IsNull || (_geometry.Type != OGCGeometryType.Polygon && _geometry.Type != OGCGeometryType.CurvePolygon) || n > this._geometry.NumRings)
            {
                return SqlGeography.Null;
            }
            ShapeData ring = _geometry.GetRing(n - 1);
            ring.SetIsValid(false);
            return new SqlGeography(ring, this.srid);
        }

        /// <summary>
        /// Determines whether the <see cref="SqlGeography"/> instance is empty.
        /// </summary>
        /// <returns>A SqlBoolean value that indicates whether the calling instance is empty. Returns true if it is empty. Otherwise, returns false.</returns>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlBoolean STIsEmpty()
        {
            if (this.IsNull)
                return SqlBoolean.Null;
            return _geometry.IsEmpty;
        }

        /// <summary>
        /// Returns the Open Geospatial Consortium (OGC) Well-Known Text (WKT) representation of a <see cref="SqlGeography"/> instance. 
        /// </summary>
        /// <returns>A SqlChars object containing the WKT representation of the SqlGeography.</returns>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = false)]
        public SqlChars STAsText() => new SqlChars(ToString());


        /// <summary>
        /// Tests if the <see cref="SqlGeography"/> instance is the same as the specified type.
        /// </summary>
        /// <param name="geometryType">Specifies the type of geometry that the calling <see cref="SqlGeography"/> will be compared to.</param>
        /// <returns>A SqlBoolean value indicating if the calling <see cref="SqlGeography"/> is of the specified geometry type.
        /// Returns true if the type of a <see cref="SqlGeography"/> instance is the same as the specified type, or if the specified 
        /// type is an ancestor of the instance type. Otherwise, returns false.</returns>
        /// <remarks>
        /// The input for the method must be one of the following: Geometry, Point, Curve, LineString, Surface, Polygon, GeometryCollection,
        /// MultiSurface, MultiPolygon, MultiCurve, MultiLineString, FullGlobe, and MultiPoint. This method throws an ArgumentException if 
        /// any other strings are used for the input.
        /// </remarks>
        [SqlMethod]
        public SqlBoolean InstanceOf(string geometryType)
        {
            if (geometryType == null)
                throw new ArgumentNullException(nameof(geometryType));
            if (IsNull)
            {
                return SqlBoolean.Null;
            }
            if (_geometry.IsValid)
                throw new ArgumentException("Geometry is not valid");

            string[] array = _parentGeometryTypeNames[(uint)_geometry.Type];
            for (int i = 0; i < array.Length; i++)
            {
                if (string.Compare(geometryType, array[i], StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }
            for (int j = 0; j < _validGeometryTypeNames.Length; j++)
            {
                if (string.Compare(geometryType, _validGeometryTypeNames[j], StringComparison.OrdinalIgnoreCase) == 0)
                    return false;
            }
            throw new ArgumentException("Invalid geometryType name", nameof(geometryType));
        }

        private static readonly string[] _validGeometryTypeNames = new string[]
        {
            "Geometry", "Point", "LineString", "Polygon", "Curve",
            "Surface", "MultiPoint", "MultiLineString", "MultiPolygon",
            "MultiCurve", "MultiSurface", "GeometryCollection", "FullGlobe",
            "CircularString", "CompoundCurve", "CurvePolygon"
        };

        private static readonly string[][] _parentGeometryTypeNames = new string[][]
        {
            new string[] { },
            new [] { "Geometry", "Point" },
            new [] { "Geometry", "Curve", "LineString" },
            new [] { "Geometry", "Surface", "Polygon" },
            new [] { "Geometry", "GeometryCollection", "MultiPoint" },
            new [] { "Geometry", "GeometryCollection", "MultiCurve", "MultiLineString" },
            new [] { "Geometry", "GeometryCollection", "MultiSurface", "MultiPolygon" },
            new [] { "Geometry", "GeometryCollection" },
            new [] { "Geometry", "Curve", "CircularString" },
            new [] { "Geometry", "Curve", "CompounCurve" },
            new [] { "Geometry", "Surface", "CurvePolygon" },
            new [] { "Geometry", "FullGlobe" }
        };

        /// <summary>
        /// Returns a SqlGeography instance from an Open Geospatial Consortium (OGC) Well-Known Text (WKT) representation augmented with any Z (elevation) and M (measure) values carried by the instance.
        /// </summary>
        /// <param name="geometryTaggedText">The WKT representation of the SqlGeography instance you wish to return. </param>
        /// <param name="srid">An int expression that represents the spatial reference ID (SRID) of the SqlGeography instance you wish to return.</param>
        /// <returns>A SqlGeography instance constructed from the WKY representation.</returns>
        /// <remarks>
        /// <para>The OGC type of the SqlGeography instance returned by STGeomFromText is set to the corresponding WKT input.</para>
        /// <para>This method will throw a <see cref="FormatException"/> if the input is not well-formatted.</para>
        /// </remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = false)]
        public static SqlGeography STGeomFromText(SqlChars geometryTaggedText, int srid)
        {
            if (geometryTaggedText.IsNull)
                return SqlGeography.Null;
            var data = Wkt.WktReader.Parse(geometryTaggedText.ToString(), Wkt.CoordinateOrder.LatLong);
            return new SqlGeography(data, srid);
        }

        /// <summary>
        /// Returns a SqlGeography instance from an Open Geospatial Consortium (OGC) Well-Known Text (WKT) representation. 
        /// </summary>
        /// <param name="s">The WKT representation of the SqlGeography instance you wish to return. </param>
        /// <returns>A SqlGeography value constructed from the specified WKT representation.</returns>
        /// <remarks>
        /// The Parse method is equivalent to <see cref="STGeomFromText"/> except that it assumes a spatial reference ID (SRID) of 4326 as a parameter. The input may carry optional Z (elevation) and M (measure) values.
        /// </remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = false)]
        public static SqlGeography Parse(SqlString s)
        {
            if (s.IsNull)
                return SqlGeography.Null;
            var data = Wkt.WktReader.Parse(s.ToString(), Wkt.CoordinateOrder.LatLong);
            return new SqlGeography(data, 4326);
        }

        /// <summary>
        /// Reads a binary representation of a geography type into a SqlGeometry object.
        /// </summary>
        /// <param name="r">BinaryReader object that reads a binary representation of a geography type.</param>
        /// <remarks>
        /// <para>This member is sealed.</para>
        /// <para>This method will throw a FormatException if SRID value read by r is invalid.</para>
        /// </remarks>
        public void Read(BinaryReader r)
        {
            srid = r.ReadInt32();
            this._geometry = new ShapeData();
            this._geometry.Read(r, 1);
        }

        /// <summary>
        /// Writes a SqlGeography object to a binary stream.
        /// </summary>
        /// <param name="w">BinaryWriter object that writes a SqlGeography object to a binary stream.</param>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public void Write(BinaryWriter w)
        {
            w.Write((!IsNull && !STSrid.IsNull ? STSrid.Value : 0)); //SRID
            _geometry.Write(w);
        }

        /// <summary>
        /// Returns a constructed SqlGeometry from an internal SQL Server format for spatial data. Can be used for sending spatial data over the network or reading them from files.
        /// </summary>
        /// <param name="bytes">The data representing the spatial data being sent across the network.</param>
        /// <returns>The data being sent over the network.</returns>
        public static SqlGeography Deserialize(SqlBytes bytes)
        {
            using (var r = new BinaryReader(bytes.Stream))
            {
                var srid = r.ReadInt32();
                var geometry = new ShapeData();
                geometry.Read(r, 1);
                return new SqlGeography(geometry, srid);
            }
        }

        /// <summary>
        /// Used for sending spatial data across the network.
        /// </summary>
        /// <returns>A SqlBytes stream representing the spatial data being sent across the network.</returns>
        /// <remarks>
        /// Used in conjunction with <see cref="Deserialize"/>() for sending spatial data across the network.
        /// </remarks>
        public SqlBytes Serialize()
        {
            using (var ms = new MemoryStream())
            {
                Write(new BinaryWriter(ms));
                return new SqlBytes(ms.ToArray());
            }
        }

        public override string ToString() => Wkt.WktWriter.Write(_geometry, Wkt.CoordinateOrder.LatLong);
    }
}