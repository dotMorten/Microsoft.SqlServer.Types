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

    internal ref struct WktReader
    {
        private const byte WHITE_SPACE = 0x20;
        private const byte COMMA = 0x2C;
        private const byte PARAN_START = 0x28;
        private const byte PARAN_END = 0x29;
        private const byte M_UPPER = 0x4D;
        private const byte M_LOWER = 0x6D;
        private const byte L_UPPER = 0x4C;
        private const byte L_LOWER = 0x6C;
        private int length;
        private int _index;
        bool hasZ;
        bool hasM;

        List<Point> _vertices;
        List<double> _z;
        List<double> _m;
        List<Figure> _figures;
        List<Segment> _segments;
        List<Shape> _shapes;
        CoordinateOrder _order;
        ReadOnlySpan<byte> wkt;
        private WktReader(ReadOnlySpan<byte> str, CoordinateOrder order)
        {
            hasZ = false;
            hasM = false;
            _vertices = new List<Point>();
            _z = new List<double>();
            _m = new List<double>();
            _figures = new List<Figure>();
            _segments = new List<Segment>();
            _shapes = new List<Shape>();
            _index = 0;
            length = str.Length;
            _order = order;
            wkt = str;
        }

        public static ShapeData Parse(ReadOnlySpan<byte> str, CoordinateOrder order)
        {
            var reader = new WktReader(str, order);
            return reader.ReadShape();
        }

        private ShapeData ReadShape(int parentOffset = -1)
        {
            SkipSpaces();
            var nextToken = ReadNextToken();
            //This is a very optimistic way to detect the token based on length
            if (nextToken.Length == 5) // "POINT"
                ReadPoint(parentOffset);
            else if (nextToken.Length == 10 && (nextToken[0] == M_UPPER || nextToken[0] == M_LOWER)) //"MULTIPOINT"
                ReadMultiPoint(parentOffset);
            else if (nextToken.Length == 10 && (nextToken[0] == L_UPPER || nextToken[0] == L_LOWER)) //"LINESTRING": 
                ReadLineString(parentOffset);
            else if (nextToken.Length == 15) //"MULTILINESTRING"
                ReadMultiLineString(parentOffset);
            else if (nextToken.Length == 7) //"POLYGON"
                ReadPolygon(parentOffset);
            else if (nextToken.Length == 12) //"MULTIPOLYGON"
                ReadMultiPolygon(parentOffset);
            else if (nextToken.Length == 18) //"GEOMETRYCOLLECTION"
                ReadGeometryCollection(parentOffset);
            else
                throw new FormatException("Invalid Well-known Text");
            //switch (Encoding.UTF8.GetString(nextToken))
            //{
            //    case "POINT":
            //        ReadPoint(parentOffset);
            //        break;
            //    case "LINESTRING":
            //        ReadLineString(parentOffset);
            //        break;
            //    case "POLYGON":
            //        ReadPolygon(parentOffset);
            //        break;
            //    case "MULTIPOINT":
            //        ReadMultiPoint(parentOffset);
            //        break;
            //    case "MULTILINESTRING":
            //        ReadMultiLineString(parentOffset);
            //        break;
            //    case "MULTIPOLYGON":
            //        ReadMultiPolygon(parentOffset);
            //        break;
            //    case "GEOMETRYCOLLECTION":
            //        ReadGeometryCollection(parentOffset);
            //        break;
            //    default:
            //        throw new FormatException("Invalid Well-known Text");
            //}
            return new ShapeData(_vertices.ToArray(), _figures.ToArray(), _shapes.ToArray(), hasZ ? _z.ToArray() : null, hasM ? _m.ToArray() : null, _segments?.ToArray());
        }

        #region Read Geometry Primitives

        private void ReadPoint(int parentOffset = -1)
        {
            if (ReadOptionalEmptyToken())
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.Point, FigureOffset = -1, ParentOffset = parentOffset });
                return;
            }
            _shapes.Add(new Shape() { type = OGCGeometryType.Point, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            _figures.Add(new Figure() { FigureAttribute = FigureAttributes.Point, VertexOffset = _vertices.Count });
            ReadToken(PARAN_START);
            ReadCoordinate();
            ReadToken(PARAN_END);
        }

        private void ReadMultiPoint(int parentOffset = -1)
        {
            if (ReadOptionalEmptyToken())
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.MultiPoint, FigureOffset = -1, ParentOffset = parentOffset });
                return;
            }
            int index = _shapes.Count;
            _shapes.Add(new Shape() { type = OGCGeometryType.MultiPoint, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            _figures.Add(new Figure() { FigureAttribute = FigureAttributes.Point, VertexOffset = _vertices.Count });
            ReadToken(PARAN_START);
            do
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.Point, FigureOffset = _figures.Count, ParentOffset = index });
                _figures.Add(new Figure() { FigureAttribute = FigureAttributes.Point, VertexOffset = _vertices.Count });
                if (ReadOptionalEmptyToken())
                {
                    _vertices.Add(new Point(double.NaN, double.NaN));
                    _z.Add(double.NaN);
                    _m.Add(double.NaN);
                }
                else
                {
                    ReadToken(PARAN_START);                    
                    ReadCoordinate();
                    ReadToken(PARAN_END);
                }
            }
            while (ReadOptionalChar(COMMA));
            ReadToken(PARAN_END);
        }

        private void ReadLineString(int parentOffset = -1)
        {
            if (ReadOptionalEmptyToken())
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.LineString, FigureOffset = -1, ParentOffset = parentOffset });
                return;
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
            if (ReadOptionalEmptyToken())
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.MultiLineString, FigureOffset = -1, ParentOffset = parentOffset });
                return;
            }
            ReadToken(PARAN_START);
            int parentIndex = _shapes.Count;
            _shapes.Add(new Shape() { type = OGCGeometryType.MultiLineString, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            do
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.LineString, FigureOffset = _figures.Count, ParentOffset = parentIndex });
                _figures.Add(new Figure() { FigureAttribute = FigureAttributes.Line, VertexOffset = _vertices.Count });
                ReadCoordinateCollection();
            }
            while (ReadOptionalChar(COMMA));
            ReadToken(PARAN_END);
        }

        private void ReadPolygon(int parentOffset = -1)
        {
            if (ReadOptionalEmptyToken())
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.Polygon, FigureOffset = -1, ParentOffset = parentOffset });
                return;
            }
            _shapes.Add(new Shape() { type = OGCGeometryType.Polygon, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            _figures.Add(new Figure() { FigureAttribute = FigureAttributes.ExteriorRing, VertexOffset = _vertices.Count });
            ReadToken(PARAN_START);
            ReadCoordinateCollection(); //Exterior ring
            while (ReadOptionalChar(COMMA)) //Interior rings
            {
                _figures.Add(new Figure() { FigureAttribute = FigureAttributes.InteriorRing, VertexOffset = _vertices.Count });
                ReadCoordinateCollection();
                if(_figures[_figures.Count-1].VertexOffset == _vertices.Count)
                {
                    // Remove empty interior rings
                    _figures.RemoveAt(_figures.Count - 1);
                }
            }
            ReadToken(PARAN_END);
        }

        private void ReadMultiPolygon(int parentOffset = -1)
        {
            if (ReadOptionalEmptyToken())
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.MultiPolygon, FigureOffset = -1, ParentOffset = parentOffset });
                return;
            }

            int index = _shapes.Count;
            _shapes.Add(new Shape() { type = OGCGeometryType.MultiPolygon, FigureOffset = _figures.Count, ParentOffset = parentOffset });

            ReadToken(PARAN_START);
            do
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.Polygon, FigureOffset = _figures.Count, ParentOffset = index });
                if(ReadOptionalEmptyToken())
                {

                }
                else
                {
                    _figures.Add(new Figure() { FigureAttribute = FigureAttributes.ExteriorRing, VertexOffset = _vertices.Count });
                    ReadToken(PARAN_START);
                    ReadCoordinateCollection(); //Exterior ring
                    while (ReadOptionalChar(COMMA)) //Interior rings
                    {
                        _figures.Add(new Figure() { FigureAttribute = FigureAttributes.InteriorRing, VertexOffset = _vertices.Count });
                        ReadCoordinateCollection();
                    }
                    ReadToken(PARAN_END);
                }
            }
            while (ReadOptionalChar(COMMA));
            ReadToken(PARAN_END);
        }

        private void ReadGeometryCollection(int parentOffset = -1)
        {
            if (ReadOptionalEmptyToken())
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.GeometryCollection, FigureOffset = -1, ParentOffset = parentOffset });
                return;
            }
            int index = _shapes.Count;
            _shapes.Add(new Shape() { type = OGCGeometryType.GeometryCollection, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            ReadToken(PARAN_START);
            do
            {
                ReadShape(index);
            }
            while (ReadOptionalChar(COMMA));
            ReadToken(PARAN_END);
        }

        private void ReadCoordinateCollection()
        {
            if (ReadOptionalEmptyToken())
                return;
            ReadToken(PARAN_START);
            do { ReadCoordinate(); }
            while (ReadOptionalChar(COMMA));
            ReadToken(PARAN_END);
        }

        private void ReadCoordinate()
        {
            var x = ReadDouble();
            var y = ReadDouble();
            if (_order == CoordinateOrder.XY)
                _vertices.Add(new Point(x, y));
            else
                _vertices.Add(new Point(y, x));
            hasZ = ReadOptionalDouble(out double z) || hasZ;
            _z.Add(z);
            if (hasZ)
            {
                hasM = ReadOptionalDouble(out double m) || hasM;
                _m.Add(m);
            }
            else
            {
                _m.Add(double.NaN);
            }
        }
        #endregion

        private double ReadDouble()
        {
            SkipSpaces();
            if (System.Buffers.Text.Utf8Parser.TryParse(ReadNextToken(), out double value, out int bytesConsumed))
                return value;
            throw new FormatException("Not a number");
        }

        private bool ReadOptionalDouble(out double d)
        {
            d = double.NaN;
            if (ReadOptionalNull())
                return true;
            if (wkt[_index] == COMMA || wkt[_index] == PARAN_END)
            {
                return false;
            }
            d = ReadDouble();
            return true;
        }

        private void SkipSpaces()
        {
            while (_index < length && (wkt[_index] == WHITE_SPACE || wkt[_index] == 0x09 || wkt[_index] == 0x0A || wkt[_index] == 0x0D))
            {
                _index++;
            }
        }

        private ReadOnlySpan<byte> ReadNextToken()
        {
            SkipSpaces();
            int start = _index;
            for (; _index < wkt.Length; _index++)
            {
                var c = wkt[_index];
                if (c == WHITE_SPACE || c == PARAN_START || c == PARAN_END || c == COMMA || c == 0x09 || wkt[_index] == 0x0A || c == 0x0D)
                    break;
            }
            return wkt.Slice(start, _index - start);
        }

        private void ReadToken(byte token)
        {
            SkipSpaces();
            if (_index >= wkt.Length || wkt[_index] != token)
            {
                throw new FormatException($"Token '{(char)token}' not found");
            }
            _index++;
        }

        private bool ReadOptionalChar(byte token)
        {
            SkipSpaces();
            if (_index < length && wkt[_index] == token)
            {
                _index++;
                return true;
            }
            return false;
        }

        private bool ReadOptionalEmptyToken()
        {
            //Checks for the token "EMPTY"
            SkipSpaces();
            if (_index + 5 <= length &&
                wkt[_index] == 0x45 &&
                wkt[_index + 1] == 0x4D &&
                wkt[_index + 2] == 0x50 &&
                wkt[_index + 3] == 0x54 &&
                wkt[_index + 4] == 0x59)
            {
                _index += 5;
                return true;
            }
            return false;
        }
        private bool ReadOptionalNull()
        {
            //Checks for the token "NULL"
            SkipSpaces();
            if (_index + 4 <= length &&
                wkt[_index] == 0x4E &&
                wkt[_index + 1] == 0x55 &&
                wkt[_index + 2] == 0x4C &&
                wkt[_index + 3] == 0x4C)
            {
                _index += 4;
                return true;
            }
            return false;
        }
    }
}