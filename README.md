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

Due to the "real" `Microsoft.SqlServer.Types` assembly isn't available, the types won't be deserialized automatically.
Instead you'll need to use the `Deserialize` method:

```cs
   var binvalue = reader.GetSqlBytes(rowid);
   var geometry = SqlGeometry.Deserialize(binvalue);
```

### Notes:

The spatial operations like intersection, area etc are not included here. You can perform these as part of your query instead and get them returned in a column.
