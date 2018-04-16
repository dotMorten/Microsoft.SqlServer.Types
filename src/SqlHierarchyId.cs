using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Text;

namespace Microsoft.SqlServer.Types
{
    /*
    [SqlUserDefinedType(Format.UserDefined, IsByteOrdered = true, MaxByteSize = 892, Name = "SqlHierarchyId")]
    public struct SqlHierarchyId : IBinarySerialize, INullable, IComparable
    {
        public static SqlHierarchyId Null { get; }

        public bool IsNull { get; }

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public static SqlHierarchyId GetRoot() => throw new NotImplementedException();

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public static SqlHierarchyId Parse(SqlString input) => throw new NotImplementedException();

        public int CompareTo(object obj) => throw new NotImplementedException();

        public int CompareTo(SqlHierarchyId hid) => throw new NotImplementedException();

        public override bool Equals(object obj) => throw new NotImplementedException();

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlHierarchyId GetAncestor(int n) => throw new NotImplementedException();

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = true, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlHierarchyId GetDescendant(SqlHierarchyId child1, SqlHierarchyId child2) => throw new NotImplementedException();

        public override int GetHashCode() => throw new NotImplementedException();

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlInt16 GetLevel() => throw new NotImplementedException();

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlHierarchyId GetReparentedValue(SqlHierarchyId oldRoot, SqlHierarchyId newRoot) => throw new NotImplementedException();

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlBoolean IsDescendantOf(SqlHierarchyId parent) => throw new NotImplementedException();

        [SqlMethod(IsDeterministic = true, IsPrecise = true)]
        public void Read(BinaryReader r) => throw new NotImplementedException();

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public override string ToString() => throw new NotImplementedException();

        [SqlMethod(IsDeterministic = true, IsPrecise = true)]
        public void Write(BinaryWriter w) => throw new NotImplementedException();

        public static SqlBoolean operator ==(SqlHierarchyId hid1, SqlHierarchyId hid2) => throw new NotImplementedException();

        public static SqlBoolean operator !=(SqlHierarchyId hid1, SqlHierarchyId hid2) => throw new NotImplementedException();

        public static SqlBoolean operator <(SqlHierarchyId hid1, SqlHierarchyId hid2) => throw new NotImplementedException();

        public static SqlBoolean operator >(SqlHierarchyId hid1, SqlHierarchyId hid2) => throw new NotImplementedException();

        public static SqlBoolean operator <=(SqlHierarchyId hid1, SqlHierarchyId hid2) => throw new NotImplementedException();

        public static SqlBoolean operator >=(SqlHierarchyId hid1, SqlHierarchyId hid2) => throw new NotImplementedException();
    }*/
}
