using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.SqlServer.Types
{
    internal enum OGCGeometryType : byte
    {
        Unknown = 0,
        Point = 1,
        LineString = 2,
        Polygon = 3,
        MultiPoint = 4,
        MultiLineString = 5,
        MultiPolygon = 6,
        GeometryCollection = 7,
        CircularString = 8,
        CompoundCurve = 9,
        CurvePolygon = 10,
        FullGlobe = 11
    }

    internal struct Point
    {
        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        public double X;

        public double Y;
    }

    internal struct PointZM
    {
        internal double X;
        internal double Y;
        internal double Z;
        internal double M;
    }

    internal struct Segment
    {
        //TODO: This is version 2.0 stuff
    }

    internal struct Shape
    {
        public int FigureOffset;
        public int ParentOffset;
        public OGCGeometryType type;
    }

    internal enum FigureAttributes : byte
    {
        /// <summary>
        /// V1 -Figure is an interior ring in a polygon. Interior rings represent holes in exterior rings. 
        /// </summary>
        InteriorRing = 0,
        /// <summary>
        /// V2 - Figure is a point
        /// </summary>
        Point = 0,
        /// <summary>
        /// V1 -Figure is a stroke. A stroke is a point or a line. 
        /// </summary>
        Stroke = 1,
        /// <summary>
        /// V2 - Figure is a line. 
        /// </summary>
        Line = 1,
        /// <summary>
        /// V1 - Figure is an exterior ring in a polygon. An exterior ring represents the outer boundary of a polygon. 
        /// </summary>
        ExteriorRing = 1,
        /// <summary>
        /// V2 - Figure is an arc
        /// </summary>
        Arc = 2,
        /// <summary>
        /// V2 -  Figure is a composite curve, that is, it contains both line and arc segments. 
        /// </summary>
        Curve = 3
    }

    internal struct Figure
    {
        public int VertexOffset;
        public FigureAttributes FigureAttribute;
    }

    /// <summary>
    /// Used as data store for both Geometry and Geography,
    /// rather than duplicating implementation twice
    /// </summary>
    internal struct ShapeData
    {
        private bool _isValid;
        private bool _isLargerThanAHemisphere;
        private double[] _mValues;
        private double[] _zValues;
        private Point[] _vertices;
        private Segment[] _segments;
        private Figure[] _figures;
        private Shape[] _shapes;

        public OGCGeometryType Type => _shapes == null || _shapes.Length == 0 ? OGCGeometryType.Unknown :  _shapes[0].type;

        public bool IsNull => _shapes == null;

        public ShapeData(double x, double y, double? z = null, double? m = null)
        {
            _isValid = true;
            _vertices = new[] { new Point(x, y) };
            _figures = new[] { new Figure() { VertexOffset = 0, FigureAttribute = FigureAttributes.Line } };
            _shapes = new[] { new Shape() { FigureOffset = 0, ParentOffset = -1, type = OGCGeometryType.Point } };
            _segments = null;
            _zValues = z.HasValue && !double.IsNaN(z.Value) ? new[] { z.Value } : null;
            _mValues = m.HasValue && !double.IsNaN(m.Value) ? new[] { m.Value } : null;
            _isLargerThanAHemisphere = false;
        }

        public ShapeData(Point[] points, Figure[] figures, Shape[] shapes)
        {
            this._vertices = points;
            this._figures = figures;
            this._shapes = shapes;
            this._isValid = false;
            this._isLargerThanAHemisphere = false;
            _zValues = null;
            _mValues = null;
            _segments = null;
        }

        public ShapeData(Point[] points, Figure[] figures, Shape[] shapes, double[] zValues, double[] mValues, Segment[] mSegments)
        {
            this._vertices = points;
            this._zValues = zValues;
            this._mValues = mValues;
            this._figures = figures;
            this._shapes = shapes;
            this._segments = mSegments;
            this._isValid = false;
            this._isLargerThanAHemisphere = false;
        }

        public PointZM GetPointN(int index)
        {
            Point p = _vertices[index - 1];
            return new PointZM()
            {
                X = p.X,
                Y = p.Y,
                Z = _zValues != null ? _zValues[index - 1] : double.NaN,
                M = _mValues != null ? _mValues[index - 1] : double.NaN
            };
        }

        private bool IsV2Data
        {
            get
            {
                return (_segments != null || _isLargerThanAHemisphere || 
                    Type == OGCGeometryType.CircularString ||
                    Type == OGCGeometryType.CompoundCurve ||
                    Type == OGCGeometryType.CurvePolygon ||
                    Type == OGCGeometryType.FullGlobe);
            }
        }

        public double X => this._vertices[0].X;
        public double Y => this._vertices[0].Y;
        public double Z => this._zValues[0];
        public double M => this._mValues[0];
        public int NumPoints => _vertices == null ? 0 : _vertices.Length;
        public int NumRings => _figures?.Length ?? 0;
        public int NumSegments => _segments?.Length ?? 0;        
        public int NumGeometries
        {
            get
            {
                if (IsEmpty)
                {
                    return 0;
                }
                if (_shapes[0].type != OGCGeometryType.GeometryCollection)
                {
                    if (!HasChildren(0))
                    {
                        return 1;
                    }
                    return (_shapes != null ? _shapes.Length : 0) - 1;
                }
                return _shapes.Where(s => s.ParentOffset == 0).Count();
            }
        }

        private bool HasChildren(int shapeIndex)
        {
            if (shapeIndex == (_shapes?.Length ?? 0) - 1) //If last shape, it can't have children
                return false;
            return _shapes[shapeIndex + 1].ParentOffset == shapeIndex; //next shape is a child of this shape
        }

        public int NumInteriorRing => _figures == null || _figures.Length == 1 ? 0 : _figures.Length - 1;
        public PointZM StartPoint => GetPointN(1);
        public PointZM EndPoint => GetPointN(_mValues.Length);
        public ShapeData GetRing(int index) => AsRing(index);
        public bool HasZ => _zValues != null;
        public bool HasM => _mValues != null;
        public bool IsEmpty => !(_vertices != null && _vertices.Length > 0 || _shapes[0].type == OGCGeometryType.FullGlobe);

        public ShapeData GetGeometryN(int index)
        {
            if (index == 1 && _shapes != null && _shapes.Length == 1)
                return this;
            return ShapeToGeometry(index);
        }

        private ShapeData AsRing(int figureIndex)
        {
            var mFigures = _figures[figureIndex].FigureAttribute;
            if (IsV2Data)
            {
                if (mFigures == FigureAttributes.Line)
                {
                    return AsLineString(figureIndex);
                }
                else if (mFigures == FigureAttributes.Curve)
                    throw new NotImplementedException("TODO: Return CompoundCurve");
                else
                    throw new NotImplementedException("TODO: Return CircularString");
            }
            else
            {
                return AsLineString(figureIndex);
            }
        }

        private ShapeData ShapeToGeometry(int shapeIndex)
        {
            if (shapeIndex == 0)
                return this;
            ShapeData geoDatum = new ShapeData();
            var shape = _shapes[shapeIndex];
            var nextShape = shapeIndex + 1;
            for (; nextShape < _shapes.Length; nextShape++)
            {
                if (_shapes[nextShape].ParentOffset == shape.ParentOffset)
                    break;
            }

            List<Shape> shapes = new List<Shape>(nextShape - shapeIndex);
            List<Figure> figures = new List<Figure>();
            List<Point> vertices = new List<Point>();
            List<double> zvalues = _zValues == null ? null : new List<double>();
            List<double> mvalues = _mValues == null ? null : new List<double>();
            for (int i = shapeIndex; i < nextShape; i++)
            {
                var s = _shapes[i];
                var nextFigure = i + 1 < _shapes.Length ? _shapes[i + 1].FigureOffset : _figures.Length;
                var figureOffset = figures.Count;
                for (int j = s.FigureOffset; j < nextFigure; j++)
                {
                    var f = _figures[j];
                    figures.Add(new Figure() { FigureAttribute = f.FigureAttribute, VertexOffset = vertices.Count });
                    var nextFigureVertexOffset = (j + 1 < _figures.Length ) ? _figures[j + 1].VertexOffset : _vertices.Length;
                    vertices.AddRange(_vertices.Skip(f.VertexOffset).Take(nextFigureVertexOffset - f.VertexOffset));
                    if (zvalues != null)
                        zvalues.AddRange(_zValues.Skip(f.VertexOffset).Take(nextFigureVertexOffset - f.VertexOffset));
                    if (mvalues != null)
                        mvalues.AddRange(_mValues.Skip(f.VertexOffset).Take(nextFigureVertexOffset - f.VertexOffset));
                }
                shapes.Add(new Shape() { type = s.type, ParentOffset = shape.ParentOffset - s.ParentOffset - 1, FigureOffset = figureOffset });
            }
            geoDatum._shapes = shapes.ToArray();
            geoDatum._figures = figures!=null || figures.Any() ? figures?.ToArray() : null;
            geoDatum._vertices = vertices.ToArray();
            geoDatum._zValues = zvalues?.ToArray();
            geoDatum._mValues = mvalues?.ToArray();
            geoDatum._isLargerThanAHemisphere = this._isLargerThanAHemisphere;
            //TODO: Segments
            geoDatum._isValid = _isValid;
            return geoDatum;
        }

        private ShapeData AsLineString(int nFigure)
        {
            var data = GetFigure(0, nFigure);
            return new ShapeData()
            {
                _isLargerThanAHemisphere = false,
                _vertices = data.points,
                _zValues = data.zvalues,
                _mValues = data.mvalues,
                _figures = new[] { new Figure() { VertexOffset = 0, FigureAttribute = FigureAttributes.Line } },
                _shapes = new[] { new Shape() { FigureOffset = 0, ParentOffset = -1, type = OGCGeometryType.LineString } },
                _isValid = _isValid
            };
        }

        private (Point[] points, double[] zvalues, double[] mvalues) GetFigure(int shapeIndex, int figureIndex)
        {
            Shape shape = _shapes[shapeIndex];
            var fidx = shape.FigureOffset + figureIndex;
            var start = _figures[fidx].VertexOffset;
            int end = _vertices.Length;
            if (_figures.Length > fidx + 1)
                end = _figures[fidx + 1].VertexOffset;
            var points = _vertices.Skip(start).Take(end - start);
            var z = _zValues?.Skip(start)?.Take(end - start);
            var m = _mValues?.Skip(start)?.Take(end - start);
            return (
                points.ToArray(),
                z?.Any(v => !double.IsNaN(v)) == true ? z.ToArray() : null,
                m?.Any(v => !double.IsNaN(v)) == true ? m.ToArray() : null);
        }

        public void SetIsValid(bool fValid)
        {
            this._isValid = fValid;
        }

        internal bool IsValid => _isValid;

        [Flags]
        internal enum SerializationProps : byte
        {
            None = 0,
            HasZ = 1,
            HasM = 2,
            IsValid = 4,
            IsSinglePoint = 8,
            IsSingleLineSegment = 16,
            IsLargerThanAHemisphere = 32
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write((byte)0x01); // 01 = version. NotE: Write 0x02 if there's curves involved

            var props = SerializationProps.None;
            if (this._zValues != null)
                props = SerializationProps.HasZ;
            if (this._mValues != null)
                props |= SerializationProps.HasM;
            if (this._isValid)
                props |= SerializationProps.IsValid;
            if (_shapes[0].type == OGCGeometryType.Point && !this.IsEmpty)
                props |= SerializationProps.IsSinglePoint;
            if (_shapes[0].type == OGCGeometryType.LineString)
            {
                Point[] mPoints = this._vertices;
                if (_vertices != null && _vertices.Length == 2)
                {
                    props |= SerializationProps.IsSingleLineSegment;
                }
            }
            if (this._isLargerThanAHemisphere)
                props |= SerializationProps.IsLargerThanAHemisphere;
            bw.Write((byte)props); //Properties

            if (!props.HasFlag(SerializationProps.IsSingleLineSegment) &&
                !props.HasFlag(SerializationProps.IsSinglePoint))
                bw.Write(NumPoints); // Number of Points = 0 (no points) 
            foreach (var p in _vertices)
            {
                bw.Write(p.X); //X
                bw.Write(p.Y); //Y
            }
            if (_zValues != null)
                foreach (var z in _zValues)
                {
                    bw.Write(z);
                }
            if (_mValues != null)
                foreach (var m in _mValues)
                {
                    bw.Write(m);
                }
            if (!props.HasFlag(SerializationProps.IsSingleLineSegment) &&
                !props.HasFlag(SerializationProps.IsSinglePoint))
            {
                bw.Write(_figures?.Length ?? 0); // Number of Figures = 0 (no figures
                if (_figures != null)
                {
                    foreach (var f in _figures)
                    {
                        bw.Write((byte)f.FigureAttribute);
                        bw.Write(f.VertexOffset);
                    }
                }
                bw.Write(_shapes.Length); // Number of Shapes = 1
                foreach (var s in _shapes)
                {
                    bw.Write(s.ParentOffset);
                    bw.Write(s.FigureOffset);
                    bw.Write((byte)s.type);
                }
            }
        }

        internal void Read(BinaryReader r, byte version)
        {
            var v = r.ReadByte();
            if (v > version)
                throw new FormatException("One of the identified items was in an invalid format.");

            var props = (SerializationProps)r.ReadByte();
            _isValid = props.HasFlag(SerializationProps.IsValid);

            int vertexCount = 0;
            if (props.HasFlag(SerializationProps.IsSinglePoint))
                vertexCount = 1;
            else if (props.HasFlag(SerializationProps.IsSingleLineSegment))
                vertexCount = 2;
            else
                vertexCount = r.ReadInt32();
            _vertices = new Point[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                _vertices[i] = new Point(r.ReadDouble(), r.ReadDouble());
            }
            if (props.HasFlag(SerializationProps.HasZ))
            {
                _zValues = new double[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    _zValues[i] = r.ReadDouble();
                }
            }
            if (props.HasFlag(SerializationProps.HasM))
            {
                _mValues = new double[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    _mValues[i] = r.ReadDouble();
                }
            }
            if (props.HasFlag(SerializationProps.IsSingleLineSegment) || props.HasFlag(SerializationProps.IsSinglePoint))
            {
                // Figures and shapes aren't used for points and two-vertex lines
                _shapes = new[] { new Shape()
                    {
                        FigureOffset = 0,
                        ParentOffset = -1,
                        type = props.HasFlag(SerializationProps.IsSinglePoint) ? OGCGeometryType.Point : OGCGeometryType.LineString
                    }
                };
            }
            else
            {
                int figureCount = r.ReadInt32();
                if (figureCount > 0)
                {
                    _figures = new Figure[figureCount];
                    for (int i = 0; i < figureCount; i++)
                    {
                        _figures[i] = new Figure()
                        {
                            FigureAttribute = (FigureAttributes)r.ReadByte(),
                            VertexOffset = r.ReadInt32()
                        };
                    }
                }
                int shapeCount = r.ReadInt32();
                if (shapeCount > 0)
                {
                    _shapes = new Shape[shapeCount];
                    for (int i = 0; i < shapeCount; i++)
                    {
                        _shapes[i] = new Shape()
                        {
                            ParentOffset = r.ReadInt32(),
                            FigureOffset = r.ReadInt32(),
                            type = (OGCGeometryType)r.ReadByte()
                        };
                    }
                }
            }
        }
    }
}