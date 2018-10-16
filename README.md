# Microsoft.SqlServer.Types
a .NET Standard implementation of the spatial types in `Microsoft.SqlServer.Types`

### NuGet:

Install the package `dotMorten.Microsoft.SqlServer.Types` from NuGet.

### Usage


**Input parameter**

Assigning SqlGeometry or SqlGeography to a command parameter:

```cs
   command.Parameters.AddWithValue("@GeographyColumn", mySqlGeography);
   command.Parameters["@GeometryColumn"].UdtTypeName = "Geography";

   command.Parameters.AddWithValue("@GeographyColumn", mySqlGeometry);
   command.Parameters["@GeometryColumn"].UdtTypeName = "Geometry" 
```
The geometry will automatically be correctly serialized.

**Reading geometry and geography**

Use the common methods for getting fields of specific types:

```cs
   var geom1 = reader.GetValue(geomColumn) as SqlGeometry;
   var geom2 = reader.GetFieldValue<SqlGeometry>(geomColumn);
   var geom3 =  = SqlGeometry.Deserialize(reader.GetSqlBytes(geomColumn)); //Avoids any potential assembly-redirect issue. See https://docs.microsoft.com/en-us/sql/sql-server/install/warning-about-client-side-usage-of-geometry-geography-and-hierarchyid?view=sql-server-2014#corrective-action
```

### Notes:

The spatial operations like intersection, area etc are not included here. You can perform these as part of your query instead and get them returned in a column.
