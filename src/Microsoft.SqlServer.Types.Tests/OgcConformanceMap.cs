using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SqlServer.Types.Tests
{
    /// <summary>
    /// Generates the OGC Conformance map in the test database.
    /// Source: http://sharpgis.net/post/2008/02/24/Creating-OGC-conformance-test-map-in-SQL-Server-2008
    /// </summary>
    class OgcConformanceMap
    {
        public static string DropTables = @"
IF OBJECT_ID('dbo.lakes', 'U') IS NOT NULL DROP TABLE lakes;
IF OBJECT_ID('dbo.road_segments', 'U') IS NOT NULL DROP TABLE road_segments;
IF OBJECT_ID('dbo.divided_routes', 'U') IS NOT NULL DROP TABLE divided_routes;
IF OBJECT_ID('dbo.forests', 'U') IS NOT NULL DROP TABLE forests;
IF OBJECT_ID('dbo.bridges', 'U') IS NOT NULL DROP TABLE bridges;
IF OBJECT_ID('dbo.streams', 'U') IS NOT NULL DROP TABLE streams;
IF OBJECT_ID('dbo.buildings', 'U') IS NOT NULL DROP TABLE buildings;
IF OBJECT_ID('dbo.ponds', 'U') IS NOT NULL DROP TABLE ponds; 
IF OBJECT_ID('dbo.named_places', 'U') IS NOT NULL DROP TABLE named_places;
IF OBJECT_ID('dbo.map_neatlines', 'U') IS NOT NULL DROP TABLE map_neatlines;";

        public static string CreateTables = @"
-- Lakes
CREATE TABLE lakes (fid INTEGER NOT NULL PRIMARY KEY,name VARCHAR(64),shore Geometry);
-- Road Segments
CREATE TABLE road_segments (fid INTEGER NOT NULL PRIMARY KEY,name VARCHAR(64),aliases VARCHAR(64),num_lanes INTEGER,centerline Geometry);
-- Divided Routes
CREATE TABLE divided_routes (fid INTEGER NOT NULL PRIMARY KEY,name VARCHAR(64),num_lanes INTEGER,centerlines Geometry);
-- Forests
CREATE TABLE forests (fid INTEGER NOT NULL PRIMARY KEY,name VARCHAR(64),boundary Geometry);
-- Bridges
CREATE TABLE bridges (fid INTEGER NOT NULL PRIMARY KEY,name VARCHAR(64),position Geometry);
-- Streams
CREATE TABLE streams (fid INTEGER NOT NULL PRIMARY KEY,name VARCHAR(64),centerline Geometry);
-- Buildings
CREATE TABLE buildings (fid INTEGER NOT NULL PRIMARY KEY,address VARCHAR(64),position Geometry,footprint Geometry);
-- Ponds
CREATE TABLE ponds (fid INTEGER NOT NULL PRIMARY KEY,name VARCHAR(64),type VARCHAR(64),shores Geometry);
-- Named Places
CREATE TABLE named_places (fid INTEGER NOT NULL PRIMARY KEY,name VARCHAR(64),boundary Geometry);
-- Map Neatline
CREATE TABLE map_neatlines (fid INTEGER NOT NULL PRIMARY KEY,neatline Geometry);";

        public static string CreateRows = @"
-- Lakes
INSERT INTO lakes VALUES (101, 'BLUE LAKE',Geometry::STPolyFromText('POLYGON((52 18,66 23,73 9,48 6,52 18),(59 18,67 18,67 13,59 13,59 18))', 101));
-- Road segments
INSERT INTO road_segments VALUES(102, 'Route 5', NULL, 2,Geometry::STLineFromText('LINESTRING( 0 18, 10 21, 16 23, 28 26, 44 31 )' ,101));
INSERT INTO road_segments VALUES(103, 'Route 5', 'Main Street', 4,Geometry::STLineFromText('LINESTRING( 44 31, 56 34, 70 38 )' ,101));
INSERT INTO road_segments VALUES(104, 'Route 5', NULL, 2,Geometry::STLineFromText('LINESTRING( 70 38, 72 48 )' ,101));
INSERT INTO road_segments VALUES(105, 'Main Street', NULL, 4,Geometry::STLineFromText('LINESTRING( 70 38, 84 42 )' ,101));
INSERT INTO road_segments VALUES(106, 'Dirt Road by Green Forest', NULL, 1,Geometry::STLineFromText('LINESTRING( 28 26, 28 0 )',101));
-- DividedRoutes
INSERT INTO divided_routes VALUES(119, 'Route 75', 4,Geometry::STMLineFromText('MULTILINESTRING((10 48,10 21,10 0),(16 0,16 23,16 48))', 101));
-- Forests
INSERT INTO forests VALUES(109, 'Green Forest',Geometry::STMPolyFromText('MULTIPOLYGON(((28 26,28 0,84 0,84 42,28 26),(52 18,66 23,73 9,48 6,52 18)),((59 18,67 18,67 13,59 13,59 18)))', 101));
-- Bridges
INSERT INTO bridges VALUES(110, 'Cam Bridge', Geometry::STPointFromText('POINT( 44 31 )', 101));
-- Streams
INSERT INTO streams VALUES(111, 'Cam Stream',Geometry::STLineFromText('LINESTRING( 38 48, 44 41, 41 36, 44 31, 52 18 )', 101));
INSERT INTO streams VALUES(112, NULL,Geometry::STLineFromText('LINESTRING( 76 0, 78 4, 73 9 )', 101));
-- Buildings
INSERT INTO buildings VALUES(113, '123 Main Street',Geometry::STPointFromText('POINT( 52 30 )', 101),Geometry::STPolyFromText('POLYGON( ( 50 31, 54 31, 54 29, 50 29, 50 31) )', 101));
INSERT INTO buildings VALUES(114, '215 Main Street',Geometry::STPointFromText('POINT( 64 33 )', 101),Geometry::STPolyFromText('POLYGON( ( 66 34, 62 34, 62 32, 66 32, 66 34) )', 101));
-- Ponds
INSERT INTO ponds VALUES(120, NULL, 'Stock Pond',Geometry::STMPolyFromText('MULTIPOLYGON( ( ( 24 44, 22 42, 24 40, 24 44) ),( ( 26 44, 26 40, 28 42, 26 44) ) )', 101));
-- Named Places
INSERT INTO named_places VALUES(117, 'Ashton',Geometry::STPolyFromText('POLYGON( ( 62 48, 84 48, 84 30, 56 30, 56 34, 62 48) )', 101));
INSERT INTO named_places VALUES(118, 'Goose Island',Geometry::STPolyFromText('POLYGON( ( 67 13, 67 18, 59 18, 59 13, 67 13) )', 101));
-- Map Neatlines
INSERT INTO map_neatlines VALUES(115,Geometry::STPolyFromText('POLYGON( ( 0 0, 0 48, 84 48, 84 0, 0 0 ) )', 101));";

    }
}
