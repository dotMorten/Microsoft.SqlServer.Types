using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.SqlServer.Types
{
    /// <summary>
    /// Shared implementation for Geography and Geometry builders
    /// </summary>
    internal class ShapeDataBuilder
    {
        private readonly List<Figure> _figures = new List<Figure>();
        private readonly List<Shape> _shapes = new List<Shape>();
        private readonly Stack<int> _parents = new Stack<int>();
        private List<double> _zValues;
        private List<double> _mValues;
        private List<Segment> _segments;
        private readonly List<Point> _vertices = new List<Point>();
        private readonly bool _ignoreZM;
        private FigureAttributes _nextFigureAttribute;
        private ShapeDataBuilder.State _state;
        private Stack<Operation> operationStack = new Stack<Operation>();
        private Operation CurrentOperation => operationStack.Count > 0 ? operationStack.Peek() : Operation.None;
        internal string GeoType { get; set; } = "Geometry";

        public ShapeData ConstructedShapeData
        {
            get
            {
                if (_shapes.Count == 0)
                    throw new FormatException($"24300: Expected a call to Begin{GeoType}, but Finish was called.");
                if (_vertices.Count <= 0)
                {
                    return new ShapeData(null, null, _shapes.ToArray());
                }
                return new ShapeData(_vertices.ToArray(),
                    _figures.ToArray(),
                    _shapes.ToArray(),
                    _zValues?.ToArray(),
                    _mValues?.ToArray(),
                    _segments?.ToArray());
            }
        }

        public ShapeDataBuilder()
        {
            _parents.Push(-1);
        }

        public ShapeDataBuilder(bool ignoreZM)
        {
            _parents.Push(-1);
            _ignoreZM = ignoreZM;
        }

        public void BeginGeo(OGCGeometryType type)
        {
            Shape shape = new Shape()
            {
                ParentOffset = _parents.Peek(),
                FigureOffset = -1,
                type = type
            };
            if (type == OGCGeometryType.CompoundCurve && _segments == null)
            {
                _segments = new List<Segment>();
            }
            else if (type == OGCGeometryType.CurvePolygon)
            {
                _nextFigureAttribute = FigureAttributes.Point;
            }
            _parents.Push(_shapes.Count);
            _shapes.Add(shape);
            operationStack.Push(Operation.Geo);
        }

        public void EndGeo()
        {
            if (CurrentOperation == Operation.Figure)
                EndFigure();
            if(CurrentOperation != Operation.Geo)
                throw new FormatException($"24300: Expected a call to Begin{GeoType}, but End{GeoType} was called.");
            operationStack.Pop();
            _parents.Pop();
        }

        public void BeginFigure()
        {
            if (CurrentOperation != Operation.Geo)
                throw new FormatException($"24300: Expected a call to Begin{GeoType}, but BeginFigure was called.");

            FigureAttributes nextFigureAttribute = FigureAttributes.Line;
            var shapeType = _shapes[_shapes.Count - 1].type;
            if (shapeType == OGCGeometryType.CircularString)
            {
                nextFigureAttribute = FigureAttributes.Arc;
            }
            else if (shapeType == OGCGeometryType.CompoundCurve)
            {
                nextFigureAttribute = FigureAttributes.Curve;
            }
            else if (shapeType == OGCGeometryType.CurvePolygon)
            {
                nextFigureAttribute = _nextFigureAttribute;
                if (nextFigureAttribute == FigureAttributes.Curve && _segments == null)
                {
                    _segments = new List<Segment>();
                }
            }
            var figure = new Figure()
            {
                FigureAttribute = nextFigureAttribute,
                VertexOffset = _vertices.Count
            };
            if (nextFigureAttribute != FigureAttributes.Curve)
            {
                _state = State.Figure;
            }
            else if (_state != State.Segment)
            {
                _state = State.Segment;
            }
            _figures.Add(figure);
            operationStack.Push(Operation.Figure);
        }

        public void EndFigure()
        {
            if (CurrentOperation != Operation.Figure)
                throw new FormatException($"24301: Expected a call to BeginFigure or End{GeoType}, but EndFigure was called.");

            operationStack.Pop();
            UpdateFigureOffsets(_figures.Count - 1, _shapes.Count - 1);
            if (_shapes[_shapes.Count - 1].type == OGCGeometryType.Polygon)
            {
                _vertices[_vertices.Count - 1] = _vertices[_figures[_figures.Count - 1].VertexOffset];
            }
            _state = State.Start;
        }

        private void UpdateFigureOffsets(int iFigureIndex, int iShapeIndex)
        {
            do
            {
                if (_shapes[iShapeIndex].FigureOffset == -1)
                {
                    var item = _shapes[iShapeIndex];
                    item.FigureOffset = iFigureIndex;
                    _shapes[iShapeIndex] = item;
                }
                iShapeIndex = _shapes[iShapeIndex].ParentOffset;
            }
            while (iShapeIndex != -1);
        }

        public void AddLine(double x, double y, double? z, double? m)
        {
            if (CurrentOperation != Operation.Figure)
                throw new FormatException($"24301: Expected a call to BeginFigure or End{GeoType}, but AddLine was called.");
            if (_shapes.Last().type == OGCGeometryType.Point)
                throw new FormatException("24300: Expected a call to EndFigure, but AddLine was called.");
           
            AddPoint(x, y, z, m);
        }

        public void AddPoint(double x, double y, double? z, double? m)
        {
            _vertices.Add(new Point(x, y));
            if (!_ignoreZM)
            {
                SetZMValue(ref _zValues, z);
                SetZMValue(ref _mValues, m);
            }
        }

        private void SetZMValue(ref List<double> list, double? d)
        {
            // Only adds Z or M values if necessary
            if (d.HasValue && list == null)
            {
                list = new List<double>(_vertices.Count);
                for (int i = 0; i < _vertices.Count - 1; i++)
                {
                    list.Add(double.NaN);
                }
            }
            if (list != null)
            {
                list.Add((d.HasValue ? d.Value : double.NaN));
            }
        }

        private enum State : int
        {
            Start = 0,
            Figure = 1,
            Segment = 2,
            End = 3,
        }

        private enum Operation : int
        {
            None = 0,
            Geo = 1,
            Figure = 2,
            Segment = 3,
        }

        private enum SegmentType : byte
        {
            Line = 0,
            Arc = 1,
            FirstLine = 2,
            FirstArc = 3
        }
    }
}
