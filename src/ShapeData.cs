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
        None = 0,
        Line = 1,
        Arc = 2,
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

        public OGCGeometryType Type => _shapes[0].type;

        public ShapeData(double x, double y, double? z = null, double? m = null)
        {
            _isValid = true;
            _vertices = new[] { new Point(x, y) };
            _figures = new[] { new Figure() { VertexOffset = 0, FigureAttribute = FigureAttributes.Line } };
            _shapes = new[] { new Shape() { FigureOffset = 0, ParentOffset = -1, type = OGCGeometryType.Point } };
            _segments = null;
            _zValues = z.HasValue ? new[] { z.Value } : null;
            _mValues = m.HasValue ? new[] { m.Value } : null;
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
                int num = (!this.IsEmpty || this.HasChildren(0) ? 0 : 1);
                if (IsEmpty || !HasChildren(0))
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
        public ShapeData ExteriorRing => AsRing(0);
        public bool HasZ => _zValues != null;
        public bool HasM => _mValues != null;
        public bool IsEmpty => !(_vertices != null && _vertices.Length > 0 || _shapes[0].type == OGCGeometryType.FullGlobe);

        public ShapeData GetGeometryN(int nGeometry)
        {
            if (nGeometry == 1 && _shapes != null && _shapes.Length == 1)
                return this;
            return ShapeToGeometry(nGeometry);
        }

        public ShapeData GetRingN(int nRing)
        {
            return AsRing(nRing - 1);
        }

        private ShapeData AsRing(int iFigure)
        {
            var mFigures = _figures[iFigure].FigureAttribute;
            if (mFigures == FigureAttributes.Line)
            {
                return AsLineString(iFigure);
            }
            else if (mFigures == FigureAttributes.Curve)
                throw new NotImplementedException("TODO: Return CompoundCurve");
            else
                throw new NotImplementedException("TODO: Return CircularString");
        }
        public int IndexOfNthChildShape(int iShape)
        {
            if (_shapes[0].type != OGCGeometryType.GeometryCollection)
            {
                return iShape;
            }
            int j = 0;
            int shapeCount = _shapes?.Length ?? 0;
            if (1 < shapeCount)
            {
                int i = 1;
                do
                {
                    if (_shapes[i].ParentOffset == 0)
                    {
                        j++;
                        if (j == iShape)
                        {
                            return i;
                        }
                    }
                    i++;
                }
                while (i < shapeCount);
            }
            return -1;
        }
        private ShapeData ShapeToGeometry(int nShape)
        {
            if (nShape == 0)
                return this;
            ShapeData geoDatum = new ShapeData();
            var shape = _shapes[nShape];
            var figureOffset = _figures != null ? shape.FigureOffset : -1;
            List<int> shapeIndeces = new List<int>();
            shapeIndeces.Add(nShape);
            for (int i = nShape + 1; i < _shapes.Length; i++)
            {
                if (shapeIndeces.Contains(_shapes[i].ParentOffset))
                    shapeIndeces.Add(i);
                else
                    break;
            }
            List<Shape> shapes = new List<Shape>(shapeIndeces.Count);
            List<Figure> figures = new List<Figure>();
            int pointOffset = _figures[shape.FigureOffset].VertexOffset;
            foreach(var i in shapeIndeces)
            {
                var s = _shapes[i];
                int findex = -1;
                if(s.FigureOffset > -1)
                {
                    var f = _figures[s.FigureOffset];
                    findex = figures.Count;
                    figures.Add(new Figure() { FigureAttribute = f.FigureAttribute, VertexOffset = f.VertexOffset - pointOffset });
                    pointOffset = f.VertexOffset;
                }
                shapes.Add(new Shape() { type = s.type, ParentOffset = shape.ParentOffset - s.ParentOffset - 1, FigureOffset = findex });
            }
            var nextFigure = _shapes[shapeIndeces.Last()].FigureOffset + 1;
            var firstVertex = _figures == null ? 0 : _figures[shape.FigureOffset].VertexOffset;
            var lastVertex = _figures == null || _figures.Length <= nextFigure ? _vertices.Length : _figures[nextFigure].VertexOffset;
            geoDatum._shapes = shapes.ToArray();
            geoDatum._figures = figures!=null || figures.Any() ? figures?.ToArray() : null;
            geoDatum._vertices = _vertices.Skip(firstVertex).Take(lastVertex - firstVertex).ToArray();
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
            Shape shape = this._shapes[shapeIndex];
            var fidx = shape.FigureOffset + figureIndex ;
            var start = this._figures[fidx].VertexOffset;
            int end = _vertices.Length;
            if (_figures.Length > fidx+1)
                end = this._figures[fidx + 1].VertexOffset - 1;
            else if (_shapes.Length > shapeIndex+1)
                end = _shapes[shapeIndex + 1].FigureOffset - 1;
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
                bw.Write(X); //X
                bw.Write(Y); //Y
            }
            if (_zValues != null)
                foreach (var p in _zValues)
                {
                    bw.Write(Z);
                }
            if (_mValues != null)
                foreach (var p in _mValues)
                {
                    bw.Write(Z);
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
                throw new FormatException("Version not supported");

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