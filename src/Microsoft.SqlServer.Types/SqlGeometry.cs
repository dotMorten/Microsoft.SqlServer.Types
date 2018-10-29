using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;

namespace Microsoft.SqlServer.Types
{
    /// <summary>
    /// The SqlGeometry type represents data in a Euclidean (flat) coordinate system.
    /// </summary>
    [SqlUserDefinedType(Format.UserDefined, IsByteOrdered = false, MaxByteSize = -1, IsFixedLength = false)]
    public class SqlGeometry : INullable, IBinarySerialize
    {
        private ShapeData _geometry;
        private int srid = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlGeometry"/> class.
        /// </summary>
        public SqlGeometry()
        {
            _geometry = new ShapeData();
        }

        internal SqlGeometry(bool isNull) { IsNull = isNull; }

        internal SqlGeometry(ShapeData g, int srid)
        {
            this.srid = srid;
            this._geometry = g;
        }

        /// <summary>
        /// Constructs a <see cref="SqlGeometry"/> instance that represents a Point instance from its X and Y values and an SRID.
        /// </summary>
        /// <param name="x">A double that represents the X-coordinate of the Point being generated.</param>
        /// <param name="y">A double that represents the Y-coordinate of the Point being generated.</param>
        /// <param name="srid">An int expression that represents the spatial reference ID (SRID) of the <see cref="SqlGeometry"/> instance you wish to return.</param>
        /// <returns>A <see cref="SqlGeometry"/> instance that represents a point on a Euclidian coordinate system.</returns>
        [SqlMethod]
        public static SqlGeometry Point(double x, double y, int srid)
        {
            if (!double.IsNaN(x) && !double.IsInfinity(x) && !double.IsNaN(y) && !double.IsInfinity(y))
            {
                return new SqlGeometry(new ShapeData(x, y, null, null), srid);
            }
            throw new FormatException("Invalid coordinates");
        }


        /// <summary>
        /// Gets the X-coordinate property of a Point instance. 
        /// </summary>
        /// <value>
        /// A SqlDouble value that represents the X-coordinate value of a point.
        /// </value>
        /// <remarks>
        /// The value of this property will be null if the SqlGeometry instance is not a point.
        /// </remarks>
        public SqlDouble STX
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => _geometry.Type == OGCGeometryType.Point && _geometry.NumPoints == 1 ? new SqlDouble(_geometry.X) : SqlDouble.Null;
        }

        /// <summary>
        /// Gets the Y-coordinate property of a Point instance.
        /// </summary>
        /// <value>
        /// A SqlDouble value that represents the Y-coordinate value of a point.
        /// </value>
        /// <remarks>
        /// The value of this property will be null if the SqlGeometry instance is not a point.
        /// </remarks>
        public SqlDouble STY
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => _geometry.Type == OGCGeometryType.Point && _geometry.NumPoints == 1 ? new SqlDouble(_geometry.Y) : SqlDouble.Null;
        }

        /// <summary>
        /// Gets the Z (elevation) value of the instance. The semantics of the elevation value are user-defined.
        /// </summary>
        /// <value>A SqlDouble value that represents the elevation of the instance.</value>
        /// <remarks>
        /// <para>The value of this property will be null if the SqlGeometry instance is not a point, as well as for any Point instance for which it is not set.</para>
        /// <para>This property is read-only.</para>
        /// <para>Z-coordinates are not used in any calculations made by the library and is not carried through any library calculations.</para>
        /// </remarks>
        public SqlDouble Z
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => _geometry.Type == OGCGeometryType.Point && _geometry.NumPoints == 1 && _geometry.HasZ && !double.IsNaN(_geometry.Z) ? new SqlDouble(_geometry.Z) : SqlDouble.Null;
        }

        /// <summary>
        /// Gets the M (measure) value of the SqlGeometry instance. The semantics of the measure value are user-defined.
        /// </summary>
        /// <value>A SqlDouble value containing the measure of the SqlGeometry value.</value>
        /// <remarks>
        /// <para>The value of this property will be null if the SqlGeometry instance is not a point, as well as for any Point instance for which it is not set.</para>
        /// <para>This property is read-only.</para>
        /// <para>M values are not used in any calculations made by the library and is not carried through any library calculations.</para>
        /// </remarks>
        public SqlDouble M
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => _geometry.Type == OGCGeometryType.Point && _geometry.NumPoints == 1 && _geometry.HasM && !double.IsNaN(_geometry.M) ? new SqlDouble(_geometry.M) : SqlDouble.Null;
        }

        /// <summary>
        /// Gets or sets an integer that represents the Spatial Reference Identifier (SRID) of the instance.
        /// </summary>
        /// <remarks>A SqlInt32 value that contains the SRID of the SqlGeometry instance.</remarks>
        public SqlInt32 STSrid
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get => new SqlInt32(srid);
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            set
            {
                if (value.IsNull)
                    throw new System.ArgumentNullException();
                if (value < 0 || value > 999999)
                    throw new ArgumentOutOfRangeException("SRID must be between 0 and 999999");
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
        /// Gets a value that indicates whether the SqlGeometry object is null.
        /// </summary>
        /// <value>A bool value that indicates whether the object is null. If true, the object is null. Otherwise, false.</value>
        public bool IsNull
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get;
        }

        /// <summary>
        /// Gets a read-only property providing a null instance of the SqlGeometry type. 
        /// </summary>
        /// <remarks>This member is static.</remarks>
        public static SqlGeometry Null
        {
            [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
            get;
        } = new SqlGeometry(true);


        /// <summary>
        /// Returns the Open Geospatial Consortium (OGC) type name represented by a geometry instance. SqlGeometry
        /// </summary>
        /// <returns>A <see cref="SqlString"/> value containing the OGC type.</returns>
        /// <remarks>
        /// The OGC type names that can be returned by STGeometryType are Point, LineString, Polygon, GeometryCollection, MultiPoint, MultiLineString, and MultiPolygon.
        /// </remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlString STGeometryType()
        {
            if (IsNull) return SqlString.Null;
            return new SqlString(_geometry.Type.ToString());
        }

        /// <summary>
        /// Returns the number of SqlGeometry that comprise a SqlGeometry instance.
        /// </summary>
        /// <returns>Returns 1 if the SqlGeometry instance is not a MultiPoint, MultiLineString, MultiPolygon, or
        /// GeometryCollection instance, and 0 if the SqlGeometry instance is empty.</returns>
        /// <remarks>
        /// If a geometry collection has nested empty elements, STNumGeometries will not return 0.
        /// Though the elements in the geometry collection instance are empty, the instance itself is not an empty set.
        /// </remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlInt32 STNumGeometries() => IsNull ? SqlInt32.Null : _geometry.NumGeometries;

        /// <summary>
        /// Returns the sum of the number of points in each of the figures in a SqlGeometry instance.
        /// </summary>
        /// <returns>A SqlInt32 value that contains the sum of the number of points in each of the figures in the calling instance.</returns>
        /// <remarks>
        /// This method counts the points in the description of a SqlGeometry instance. Duplicate points are counted. 
        /// If this instance is a collection type, this method returns the sum of the points in each of its elements.
        /// </remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlInt32 STNumPoints() => IsNull ? SqlInt32.Null : _geometry.NumPoints;

        /// <summary>
        /// Returns the number of interior rings of a Polygon SqlGeometry instance.
        /// </summary>
        /// <returns>A SqlInt32 value that specifies the number of interior rings.</returns>
        /// <remarks>This method returns null if the SqlGeometry instance is not a polygon.</remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlInt32 STNumInteriorRing() => (IsNull || _geometry.Type != OGCGeometryType.Polygon) ? SqlInt32.Null : _geometry.NumInteriorRing;

        /// <summary>
        /// Returns the specified SqlGeometry in a SqlGeometry collection.
        /// </summary>
        /// <param name="n">An int expression between 1 and the number of SqlGeometry instances in the SqlGeometry collection that specifies the instance to return.</param>
        /// <returns>The SqlGeometry specified by n.</returns>
        /// <remarks>
        /// This method returns null if the parameter is larger than the result of <see cref="STNumGeometries"/> and will throw an 
        /// <see cref="ArgumentOutOfRangeException "/> if the expression parameter is less than 1
        /// </remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlGeometry STGeometryN(int n)
        {
            if (n < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(n));
            }
            if (IsNull || n > STNumGeometries())
                return SqlGeometry.Null;
            return new SqlGeometry(_geometry.GetGeometryN(n), srid);
        }

        /// <summary>
        /// Returns a specified point in a SqlGeometry instance. 
        /// </summary>
        /// <param name="n">An int expression between 1 and the number of points in the SqlGeometry instance.</param>
        /// <returns>A SqlGeometry that represents the specified point in the calling instance.</returns>
        /// <remarks>
        /// <para>If a SqlGeometry instance is user created, STPointN returns the point specified by expression by ordering the points in the order in which they were originally input.</para>
        /// <para>If a SqlGeometry instance was constructed by the system, STPointN returns the point specified by expression by ordering all the points in the same order they would be output: first by geometry, then by ring within the geometry(if appropriate), and then by point within the ring.This order is deterministic.</para>
        /// <para>If this method is called with a value less than 1, it throws an <see cref="ArgumentOutOfRangeException"/>.</para>
        /// <para>If this method is called with a value greater than the number of points in the instance, it returns null.</para>
        /// </remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlGeometry STPointN(int n)
        {
            if (n < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(n));
            }
            if (n > this._geometry.NumPoints)
                return SqlGeometry.Null;

            var p = _geometry.GetPointN(n);
            return new SqlGeometry(new ShapeData(p.X, p.Y, HasZ ? (double?)p.Z : null, HasM ? (double?)p.M : null), srid);

        }

        /// <summary>
        /// Returns the specified interior ring of a Polygon SqlGeometry instance. 
        /// </summary>
        /// <param name="n">An int expression between 1 and the number of interior rings in the SqlGeometry instance.</param>
        /// <returns>A <see cref="SqlGeometry"/> object that represents the interior ring of the Polygon.</returns>
        /// <remarks>
        /// This method returns null if the <see cref="SqlGeometry"/> instance is not a polygon. This method will also throw an <see cref="ArgumentOutOfRangeException"/> 
        /// if the expression is larger than the number of rings. The number of rings can be returned using <see cref="STNumInteriorRing"/>.
        /// </remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlGeometry STInteriorRingN(int n)
        {
            if (n < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(n));
            }
            if (IsNull || (_geometry.Type != OGCGeometryType.Polygon && _geometry.Type != OGCGeometryType.CurvePolygon) || _geometry.IsEmpty)
            {
                return SqlGeometry.Null;
            }
            return new SqlGeometry(_geometry.GetRing(n), srid);
        }

        /// <summary>
        /// Returns the exterior ring of a SqlGeometry instance that is a polygon. 
        /// </summary>
        /// <returns>A <see cref="SqlGeometry"/> object that represents the exterior ring of the calling instance.</returns>
        /// <remarks>his method returns null if the SqlGeometry instance is not a polygon.</remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlGeometry STExteriorRing()
        {
            if (IsNull || (_geometry.Type != OGCGeometryType.Polygon && _geometry.Type != OGCGeometryType.CurvePolygon) || _geometry.IsEmpty)
            {
                return SqlGeometry.Null;
            }
            return new SqlGeometry(_geometry.GetRing(0), srid);
        }

        /// <summary>
        /// Indicates whether the calling <see cref="SqlGeometry"/> instance is empty.
        /// </summary>
        /// <returns>Returns true if the calling instance is empty. Returns false if it is not empty.</returns>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = true)]
        public SqlBoolean STIsEmpty()
        {
            if (this.IsNull)
                return SqlBoolean.Null;
            return _geometry.IsEmpty;
        }

        /// <summary>
        /// Returns the Open Geospatial Consortium (OGC) Well-Known Text (WKT) representation of a SqlGeometry instance. 
        /// </summary>
        /// <returns>A SqlChars object containing the WKT representation of the SqlGeometry.</returns>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = false)]
        public SqlChars STAsText() => new SqlChars(ToString());

        /// <summary>
        /// Tests if the <see cref="SqlGeometry"/> instance is the same as the specified type.
        /// </summary>
        /// <param name="geometryType">Specifies the type of geometry that the calling <see cref="SqlGeometry"/> will be compared to.</param>
        /// <returns>A SqlBoolean value indicating if the calling <see cref="SqlGeometry"/> is of the specified geometry type.
        /// Returns true if the type of a <see cref="SqlGeometry"/> instance is the same as the specified type, or if the specified 
        /// type is an ancestor of the instance type. Otherwise, returns false.</returns>
        /// <remarks>
        /// The input for the method must be one of the following: Geometry, Point, Curve, LineString, Surface, Polygon, GeometryCollection,
        /// MultiSurface, MultiPolygon, MultiCurve, MultiLineString, and MultiPoint. This method throws an ArgumentException if 
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

        private static readonly string[] _validGeometryTypeNames = new string[15]
        {
            "Geometry", "Point", "LineString", "Polygon", "Curve",
            "Surface", "MultiPoint", "MultiLineString", "MultiPolygon",
            "MultiCurve", "MultiSurface", "GeometryCollection",
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
            new [] { "Geometry", "Curve", "CompoundCurve" },
            new [] { "Geometry", "Surface", "CurvePolygon" }
        };

        public static SqlGeometry Deserialize(SqlBytes bytes)
        {
            using (var r = new BinaryReader(bytes.Stream))
            {
                var srid = r.ReadInt32();
                var geometry = new ShapeData();
                geometry.Read(r, 1);
                return new SqlGeometry(geometry, srid);
            }
        }

        public SqlBytes Serialize()
        {
            using (var ms = new MemoryStream())
            {
                Write(new BinaryWriter(ms));
                return new SqlBytes(ms.ToArray());
            }
        }

        /// <summary>
        /// Returns a SqlGeometry instance from an Open Geospatial Consortium (OGC) Well-Known Text (WKT) representation augmented with any Z (elevation) and M (measure) values carried by the instanc
        /// </summary>
        /// <param name="geometryTaggedText">The WKT representation of the SqlGeometry instance you wish to return. </param>
        /// <param name="srid">An int expression that represents the spatial reference ID (SRID) of the SqlGeometry instance you wish to return.</param>
        /// <returns>A SqlGeometry instance constructed from the specified WKT representation.</returns>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = false)]
        public static SqlGeometry STGeomFromText(SqlChars geometryTaggedText, int srid)
        {
            if (geometryTaggedText.IsNull)
                return SqlGeometry.Null;
            var data = Wkt.WktReader.Parse(geometryTaggedText.ToString(), Wkt.CoordinateOrder.XY);
            return new SqlGeometry(data, srid);
        }

        /// <summary>
        /// Returns a SqlGeometry instance from an Open Geospatial Consortium (OGC) Well-Known Text (WKT) representation.
        /// </summary>
        /// <param name="s">Specifies the the WKT representation of the SqlGeometry instance you wish to return.</param>
        /// <returns>A SqlGeometry instance interpreted from the provided WKT representation.
        /// Returns null if s is null.</returns>
        /// <remarks>
        /// <para>Parse is equivalent to STGeomFromText, with the exception that it assumes a spatial reference ID (SRID) of 0 as a parameter.</para>
        /// <para>The input may carry optional Z(elevation) and M(measure) values.</para>
        /// 
        /// </remarks>
        [SqlMethodAttribute(IsDeterministic = true, IsPrecise = false)]
        public static SqlGeometry Parse(SqlString s)
        {
            if (s.IsNull)
                return SqlGeometry.Null;
            var data = Wkt.WktReader.Parse(s.ToString(), Wkt.CoordinateOrder.XY);
            return new SqlGeometry(data, 0);
        }

        public void Read(BinaryReader r)
        {
            srid = r.ReadInt32();
            this._geometry = new ShapeData();
            this._geometry.Read(r, 1);
        }

        public void Write(BinaryWriter w)
        {
            w.Write((!IsNull && !STSrid.IsNull ? STSrid.Value : 0)); //SRID
            _geometry.Write(w);
        }

        public override string ToString() => Wkt.WktWriter.Write(_geometry, Wkt.CoordinateOrder.XY);
    }
}
