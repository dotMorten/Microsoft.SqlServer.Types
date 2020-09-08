using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.SqlServer.Types.Tests
{
    [TestClass]
    public static class AssemblyLoader
    {
        [AssemblyInitialize]
        public static void TestInit(TestContext context)
        {
            Init();
#if !NETCOREAPP
            LoadNativeAssemblies();
#endif
        }

        private static void Init()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name == "Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" ||
                args.Name == "Microsoft.SqlServer.Types, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" ||
                args.Name == "Microsoft.SqlServer.Types, Version=12.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91")
                return typeof(SqlGeography).Assembly;
            return null;
        }

#if !NETCOREAPP
    
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string path);

        /// <summary>
        /// Loads the required native assemblies for the current architecture (x86 or x64)
        /// </summary>
        /// <param name="rootApplicationPath">
        /// Root path of the current application. Use Server.MapPath(".") for ASP.NET applications
        /// and AppDomain.CurrentDomain.BaseDirectory for desktop applications.
        /// </param>
        public static void LoadNativeAssemblies()
        {
            var nativeBinaryPath = IntPtr.Size > 4 ? "x64" : "x86";

            LoadNativeAssembly(nativeBinaryPath, "msvcr120.dll");
            LoadNativeAssembly(nativeBinaryPath, "SqlServerSpatial140.dll");
        }

        private static void LoadNativeAssembly(string nativeBinaryPath, string assemblyName)
        {
            var path = System.IO.Path.Combine(nativeBinaryPath, assemblyName);
            var ptr = LoadLibrary(path);
            if (ptr == IntPtr.Zero)
            {
                throw new Exception(string.Format(
                    "Error loading {0} (ErrorCode: {1})",
                    assemblyName,
                    Marshal.GetLastWin32Error()));
            }
        }
#endif
    }
}
