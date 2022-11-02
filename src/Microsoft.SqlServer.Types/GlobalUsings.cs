global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Runtime.Serialization;
global using System.Text;

global using System.Data.SqlTypes;
#if LEGACY
global using Microsoft.SqlServer.Server;
#else
global using Microsoft.Data.SqlClient.Server;
#endif

#if MSV5
global using Microsoft.SqlServer.Server;
#endif