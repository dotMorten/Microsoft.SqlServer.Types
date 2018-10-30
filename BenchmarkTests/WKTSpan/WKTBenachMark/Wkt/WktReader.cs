using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.SqlServer.Types.Wkt
{
    internal enum CoordinateOrder
    {
        XY,
        LatLong
    }

    internal class WktReader
    {
        private int length = 0;
        private int _nextIndex = 1;
        private String wkt;
        private bool _readEndOfStream = false;
        private char Current;
        bool hasZ;
        bool hasM;

        List<Point> _vertices;
        List<double> _z;
        List<double> _m;
        List<Figure> _figures;
        List<Segment> _segments;
        List<Shape> _shapes;
        CoordinateOrder _order;

        private WktReader(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentException("Empty string");
            hasZ = false;
            hasM = false;
            _vertices = new List<Point>();
            _z = new List<double>();
            _m = new List<double>();
            _figures = new List<Figure>();
            _segments = new List<Segment>();
            _shapes = new List<Shape>();

            length = str.Length;
            wkt = str;
            Current = wkt[0];
        }

        public static ShapeData Parse(string str, CoordinateOrder order)
        {
            WktReader reader = new WktReader(str);
            reader._order = order;
            return reader.ReadShape();
        }

        private ShapeData ReadShape(int parentOffset = -1)
        {
            SkipSpaces();
            var nextToken = ReadNextToken();
            switch (nextToken.ToUpper())
            {
                case "POINT":
                    ReadPoint(parentOffset);
                    break;
                case "LINESTRING":
                    ReadLineString(parentOffset);
                    break;
                case "POLYGON":
                    ReadPolygon(parentOffset);
                    break;
                case "MULTIPOINT":
                    ReadMultiPoint(parentOffset);
                    break;
                case "MULTILINESTRING":
                    ReadMultiLineString(parentOffset);
                    break;
                case "MULTIPOLYGON":
                    ReadMultiPolygon(parentOffset);
                    break;
                case "GEOMETRYCOLLECTION":
                    ReadGeometryCollection(parentOffset);
                    break;
                default:
                    throw new FormatException("Invalid Well-known Text");
            }
            return new ShapeData(_vertices.ToArray(), _figures.ToArray(), _shapes.ToArray(), hasZ ? _z.ToArray() : null, hasM ? _m.ToArray() : null, _segments?.ToArray());
        }

        #region Read Geometry Primitives

        private void ReadPoint(int parentOffset = -1)
        {
            if (ReadOptionalToken("EMPTY"))
            {
                //TODO
                return;
            }
            _shapes.Add(new Shape() { type = OGCGeometryType.Point, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            _figures.Add(new Figure() { FigureAttribute = FigureAttributes.Point, VertexOffset = _vertices.Count });
            ReadToken("(");
            ReadCoordinate();
            ReadToken(")");
        }

        private void ReadMultiPoint(int parentOffset = -1)
        {
            _shapes.Add(new Shape() { type = OGCGeometryType.MultiPoint, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            _figures.Add(new Figure() { FigureAttribute = FigureAttributes.Point, VertexOffset = _vertices.Count });
            ReadCoordinateCollection();
        }

        private void ReadLineString(int parentOffset = -1)
        {
            if (ReadOptionalToken("EMPTY"))
            {
                //TODO
            }
            else
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.LineString, FigureOffset = _figures.Count, ParentOffset = parentOffset });
                _figures.Add(new Figure() { FigureAttribute = FigureAttributes.Line, VertexOffset = _vertices.Count });
                ReadCoordinateCollection();
            }
        }
        private void ReadMultiLineString(int parentOffset = -1)
        {
            ReadToken("(");
            int parentIndex = _shapes.Count;
            _shapes.Add(new Shape() { type = OGCGeometryType.MultiLineString, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            do
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.LineString, FigureOffset = _figures.Count, ParentOffset = parentIndex });
                _figures.Add(new Figure() { FigureAttribute = FigureAttributes.Line, VertexOffset = _vertices.Count });
                ReadCoordinateCollection();
            }
            while (ReadOptionalChar(','));
            ReadToken(")");
        }

        private void ReadPolygon(int parentOffset = -1)
        {
            if (ReadOptionalToken("EMPTY"))
            {
                //TODO
            }
            _shapes.Add(new Shape() { type = OGCGeometryType.Polygon, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            _figures.Add(new Figure() { FigureAttribute = FigureAttributes.ExteriorRing, VertexOffset = _vertices.Count });
            ReadToken("(");
            ReadCoordinateCollection(); //Exterior ring
            while (ReadOptionalChar(',')) //Interior rings
            {
                _figures.Add(new Figure() { FigureAttribute = FigureAttributes.InteriorRing, VertexOffset = _vertices.Count });
                ReadCoordinateCollection();
            }
            ReadToken(")");
        }

        private void ReadMultiPolygon(int parentOffset = -1)
        {
            if (ReadOptionalToken("EMPTY"))
            {
                //TODO
            }

            int index = _shapes.Count;
            _shapes.Add(new Shape() { type = OGCGeometryType.MultiPolygon, FigureOffset = _figures.Count, ParentOffset = parentOffset });

            ReadToken("(");
            do
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.Polygon, FigureOffset = _figures.Count, ParentOffset = index });
                _figures.Add(new Figure() { FigureAttribute = FigureAttributes.ExteriorRing, VertexOffset = _vertices.Count });
                ReadToken("(");
                ReadCoordinateCollection(); //Exterior ring
                while (ReadOptionalChar(',')) //Interior rings
                {
                    _figures.Add(new Figure() { FigureAttribute = FigureAttributes.InteriorRing, VertexOffset = _vertices.Count });
                    ReadCoordinateCollection();
                }
                ReadToken(")");
            }
            while (ReadOptionalChar(','));
            ReadToken(")");
        }

        private void ReadGeometryCollection(int parentOffset = -1)
        {
            if (ReadOptionalToken("EMPTY"))
            {
                // TODO
            }
            int index = _shapes.Count;
            _shapes.Add(new Shape() { type = OGCGeometryType.GeometryCollection, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            ReadToken("(");
            do
            {
                ReadShape(index);
            }
            while (ReadOptionalChar(','));
            ReadToken(")");
        }

        private void ReadCoordinateCollection()
        {
            ReadToken("(");
            do { ReadCoordinate(); }
            while ( ReadOptionalChar(','));
            ReadToken(")");
        }

        private void ReadCoordinate()
        {
            var x = ReadDouble();
            var y = ReadDouble();
            if(_order == CoordinateOrder.XY)
                _vertices.Add(new Point(x, y));
            else
                _vertices.Add(new Point(y, x));
            hasZ = ReadOptionalDouble(out double z) || hasZ;
            _z.Add(z);
            hasM = ReadOptionalDouble(out double m) || hasM;
            _m.Add(m);
        }
        #endregion

        private double ReadDouble()
        {
            StringBuilder builder = new StringBuilder(0x10);
            SkipSpaces();
            if (_readEndOfStream)
            {
                throw new System.IO.EndOfStreamException();
            }
            do
            {
                builder.Append(Current);
            }
            while (Read() && !CurrentIsValueSeparator());
            string s = builder.ToString();
            double num;
            if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out num))
            {
                throw new FormatException("Not a number");
            }
            return num;
        }

        private bool ReadOptionalDouble(out double d)
        {
            d = double.NaN;
            if (!char.IsWhiteSpace(Current) || _readEndOfStream)
            {
                return false;
            }
            d = ReadDouble();
            return true;
        }

        private bool CurrentIsValueSeparator()
        {
            if ((Current != ',') && (Current != ')'))
            {
                return char.IsWhiteSpace(Current);
            }
            return true;
        }

        private void SkipSpaces()
        {
            while ((!_readEndOfStream && char.IsWhiteSpace(Current)) && Read()) { }
        }

        private bool CanRead
        {
            get { return (_nextIndex < length); }
        }

        private bool CanReadNChars(int c)
        {
            return (!_readEndOfStream && (((_nextIndex + c) - 1) <= length));
        }

        private string ReadNextToken()
        {
            SkipSpaces();
            int start = _nextIndex - 1;
            char c = Current;
            int i;
            for (; _nextIndex < wkt.Length; _nextIndex++)
            {
                c = wkt[_nextIndex];
                if(c == ' ' || c == '(' || c == ')' || c == ',')
                {
                    break;
                }
            }
            Current = c;
            _nextIndex++;
            return wkt.Substring(start, _nextIndex - start - 1);
        }

        private void ReadToken(string token)
        {
            SkipSpaces();
            if (!CanReadNChars(token.Length))
            {
                throw new FormatException(String.Format("Token '{0}' not found", token));
            }
            string strB = Read(token.Length);
            if (string.Compare(token, strB, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new FormatException(String.Format("Token '{0}' not found", token));
            }
        }

        private bool ReadOptionalChar(char token)
        {
            SkipSpaces();
            bool flag = Current == token;
            if (flag)
            {
                Read();
            }
            return flag;
        }

        private bool ReadOptionalToken(string token)
        {
            SkipSpaces();
            bool flag = char.ToUpperInvariant(token[0]) == char.ToUpperInvariant(Current);
            if (flag)
            {
                ReadToken(token);
            }
            return flag;
        }

        private bool Read()
        {
            bool canRead = CanRead;
            if (canRead)
            {
                Current = wkt[_nextIndex++];
                return canRead;
            }
            _readEndOfStream = true;
            return canRead;
        }

        private string Read(int n)
        {
            char[] buffer = new char[n];
            buffer[0] = Current;
            if (n > 1)
            {
                for (int i = 1; i < n; i++)
                    buffer[i] = wkt[_nextIndex + i];
                _nextIndex += n;
            }
            Read();
            return new string(buffer);
        }
    }
}