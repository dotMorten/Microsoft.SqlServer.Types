global using System;
global using System.Collections.Generic;
global using System.Data;
global using System.IO;
global using System.Text;

global using Microsoft.VisualStudio.TestTools.UnitTesting;

global using System.Data.SqlTypes;
#if LEGACY || NETFRAMEWORK
global using System.Data.SqlClient;
#elif NET5_0_OR_GREATER
global using Microsoft.Data.SqlClient;
#endif