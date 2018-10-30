using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.SqlServer.Types.Wkt
{
    internal ref struct WktReaderSpanChar
    {
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
        ReadOnlySpan<char> wkt;
        private WktReaderSpanChar(ReadOnlySpan<char> str, CoordinateOrder order)
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

        public static ShapeData Parse(ReadOnlySpan<char> str, CoordinateOrder order)
        {
            var reader = new WktReaderSpanChar(str, order);
            return reader.ReadShape();
        }

        private ShapeData ReadShape(int parentOffset = -1)
        {
            SkipSpaces();
            var nextToken = ReadNextToken();
            //This is a very optimistic way to detect the token based on length
            // if(nextToken.Length == 5) // "POINT"
            //     ReadPoint(parentOffset);
            // else if (nextToken.Length == 10 && nextToken[0] == 'M') //"MULTIPOINT"
            //     ReadMultiPoint(parentOffset);
            // else if (nextToken.Length == 10 && nextToken[0] == 'L') //"LINESTRING": 
            //     ReadLineString(parentOffset);
            // else if (nextToken.Length == 15) //"MULTILINESTRING"
            //     ReadMultiLineString(parentOffset);
            // else if (nextToken.Length == 7) //"POLYGON"
            //     ReadPolygon(parentOffset);
            // else if (nextToken.Length == 12) //"MULTIPOLYGON"
            //     ReadMultiPolygon(parentOffset);
            // else if (nextToken.Length == 18) //"GEOMETRYCOLLECTION"
            //     ReadGeometryCollection(parentOffset);
            // else
            //     throw new FormatException("Invalid Well-known Text");

            if(MemoryExtensions.Equals(nextToken, "POINT", StringComparison.InvariantCultureIgnoreCase))
                    ReadPoint(parentOffset);
            else if (MemoryExtensions.Equals(nextToken, "LINESTRING", StringComparison.InvariantCultureIgnoreCase))
                    ReadLineString(parentOffset);
            else if (MemoryExtensions.Equals(nextToken, "POLYGON", StringComparison.InvariantCultureIgnoreCase))
                    ReadPolygon(parentOffset);
            else if (MemoryExtensions.Equals(nextToken, "MULTIPOINT", StringComparison.InvariantCultureIgnoreCase))
                    ReadMultiPoint(parentOffset);
            else if (MemoryExtensions.Equals(nextToken, "MULTILINESTRING", StringComparison.InvariantCultureIgnoreCase))
                    ReadMultiLineString(parentOffset);
            else if (MemoryExtensions.Equals(nextToken, "MULTIPOLYGON", StringComparison.InvariantCultureIgnoreCase))
                    ReadMultiPolygon(parentOffset);
            else if (MemoryExtensions.Equals(nextToken, "GEOMETRYCOLLECTION", StringComparison.InvariantCultureIgnoreCase))
                    ReadGeometryCollection(parentOffset);
            else
                throw new FormatException("Invalid Well-known Text");

            return new ShapeData(_vertices.ToArray(), _figures.ToArray(), _shapes.ToArray(), hasZ ? _z.ToArray() : null, hasM ? _m.ToArray() : null, _segments?.ToArray());
        }

        #region Read Geometry Primitives

        private void ReadPoint(int parentOffset = -1)
        {
            if (ReadOptionalEmptyToken())
            {
                //TODO
                return;
            }
            _shapes.Add(new Shape() { type = OGCGeometryType.Point, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            _figures.Add(new Figure() { FigureAttribute = FigureAttributes.Point, VertexOffset = _vertices.Count });
            ReadToken('(');
            ReadCoordinate();
            ReadToken(')');
        }

        private void ReadMultiPoint(int parentOffset = -1)
        {
            _shapes.Add(new Shape() { type = OGCGeometryType.MultiPoint, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            _figures.Add(new Figure() { FigureAttribute = FigureAttributes.Point, VertexOffset = _vertices.Count });
            ReadCoordinateCollection();
        }

        private void ReadLineString(int parentOffset = -1)
        {
            if (ReadOptionalEmptyToken())
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
            ReadToken('(');
            int parentIndex = _shapes.Count;
            _shapes.Add(new Shape() { type = OGCGeometryType.MultiLineString, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            do
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.LineString, FigureOffset = _figures.Count, ParentOffset = parentIndex });
                _figures.Add(new Figure() { FigureAttribute = FigureAttributes.Line, VertexOffset = _vertices.Count });
                ReadCoordinateCollection();
            }
            while (ReadOptionalChar(','));
            ReadToken(')');
        }

        private void ReadPolygon(int parentOffset = -1)
        {
            if (ReadOptionalEmptyToken())
            {
                //TODO
            }
            _shapes.Add(new Shape() { type = OGCGeometryType.Polygon, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            _figures.Add(new Figure() { FigureAttribute = FigureAttributes.ExteriorRing, VertexOffset = _vertices.Count });
            ReadToken('(');
            ReadCoordinateCollection(); //Exterior ring
            while (ReadOptionalChar(',')) //Interior rings
            {
                _figures.Add(new Figure() { FigureAttribute = FigureAttributes.InteriorRing, VertexOffset = _vertices.Count });
                ReadCoordinateCollection();
            }
            ReadToken(')');
        }

        private void ReadMultiPolygon(int parentOffset = -1)
        {
            if (ReadOptionalEmptyToken())
            {
                //TODO
            }

            int index = _shapes.Count;
            _shapes.Add(new Shape() { type = OGCGeometryType.MultiPolygon, FigureOffset = _figures.Count, ParentOffset = parentOffset });

            ReadToken('(');
            do
            {
                _shapes.Add(new Shape() { type = OGCGeometryType.Polygon, FigureOffset = _figures.Count, ParentOffset = index });
                _figures.Add(new Figure() { FigureAttribute = FigureAttributes.ExteriorRing, VertexOffset = _vertices.Count });
                ReadToken('(');
                ReadCoordinateCollection(); //Exterior ring
                while (ReadOptionalChar(',')) //Interior rings
                {
                    _figures.Add(new Figure() { FigureAttribute = FigureAttributes.InteriorRing, VertexOffset = _vertices.Count });
                    ReadCoordinateCollection();
                }
                ReadToken(')');
            }
            while (ReadOptionalChar(','));
            ReadToken(')');
        }

        private void ReadGeometryCollection(int parentOffset = -1)
        {
            if (ReadOptionalEmptyToken())
            {
                // TODO
            }
            int index = _shapes.Count;
            _shapes.Add(new Shape() { type = OGCGeometryType.GeometryCollection, FigureOffset = _figures.Count, ParentOffset = parentOffset });
            ReadToken('(');
            do
            {
                ReadShape(index);
            }
            while (ReadOptionalChar(','));
            ReadToken(')');
        }

        private void ReadCoordinateCollection()
        {
            ReadToken('(');
            do { ReadCoordinate(); }
            while ( ReadOptionalChar(','));
            ReadToken(')');
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
            SkipSpaces();
            if (double.TryParse(ReadNextToken(), NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                return value;
            throw new FormatException("Not a number");
        }

        private bool ReadOptionalDouble(out double d)
        {
            d = double.NaN;
            if (wkt[_index] != ' ')
            {
                return false;
            }
            d = ReadDouble();
            return true;
        }

        private void SkipSpaces()
        {
            while (_index < length && wkt[_index] == ' ')
            {
                _index++;
            }
        }

        private ReadOnlySpan<char> ReadNextToken()
        {
            SkipSpaces();
            int start = _index;
            for (; _index < wkt.Length; _index++)
            {
                var c = wkt[_index];
                if(c == ' ' || c == '(' || c == ')' || c == ',')
                    break;
            }
            return wkt.Slice(start, _index - start);
        }

        private void ReadToken(char token)
        {
            SkipSpaces();
            if (_index >= wkt.Length || wkt[_index] != token)
            {
                throw new FormatException(String.Format("Token '{0}' not found", token));
            }
            _index++;
        }
        
        private bool ReadOptionalChar(char token)
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
            SkipSpaces();
            if (_index + 5 < length && 
                wkt[_index] == 'E' &&
                wkt[_index + 1] == 'M' &&
                wkt[_index + 2] == 'P' &&
                wkt[_index + 3] == 'T' &&
                wkt[_index + 4] == 'Y')
            {
                _index += 5;
            }
            return false;
        }
    }
}