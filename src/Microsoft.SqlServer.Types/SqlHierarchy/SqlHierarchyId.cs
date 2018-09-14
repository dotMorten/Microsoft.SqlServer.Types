using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types.SqlHierarchy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.SqlServer.Types
{

    [SqlUserDefinedType(Format.UserDefined, IsByteOrdered = true, MaxByteSize = 892, Name = "SqlHierarchyId")]
    public struct SqlHierarchyId : IBinarySerialize, INullable, IComparable
    {
        HierarchyId imp;
        
        private SqlHierarchyId(HierarchyId imp)
        {
            this.imp = imp;
        }

        public static SqlHierarchyId Null => new SqlHierarchyId();

        public bool IsNull => imp.IsNull;

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public static SqlHierarchyId GetRoot() => new SqlHierarchyId(HierarchyId.GetRoot());

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public static SqlHierarchyId Parse(SqlString input) => new SqlHierarchyId(HierarchyId.Parse((string)input));

        public int CompareTo(object obj) => this.CompareTo((SqlHierarchyId)obj);

        public int CompareTo(SqlHierarchyId hid) => this.imp.CompareTo(hid.imp);

        public override bool Equals(object obj) => Equals((SqlHierarchyId)obj);

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlHierarchyId GetAncestor(int n) => new SqlHierarchyId(imp.GetAncestor(n));

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = true, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlHierarchyId GetDescendant(SqlHierarchyId child1, SqlHierarchyId child2) => new SqlHierarchyId(imp.GetDescendant(child1.imp, child2.imp));

        public override int GetHashCode() => imp.GetHashCode();

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlInt16 GetLevel() => imp.GetLevel();

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlHierarchyId GetReparentedValue(SqlHierarchyId oldRoot, SqlHierarchyId newRoot) => new SqlHierarchyId(imp.GetReparentedValue(oldRoot.imp, newRoot.imp));

        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlBoolean IsDescendantOf(SqlHierarchyId parent) => imp.IsDescendantOf(parent.imp);


        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public override string ToString() => imp.ToString();

      
        public static SqlBoolean operator ==(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.imp == hid2.imp;

        public static SqlBoolean operator !=(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.imp != hid2.imp;

        public static SqlBoolean operator <(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.imp < hid2.imp;

        public static SqlBoolean operator >(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.imp > hid2.imp;

        public static SqlBoolean operator <=(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.imp <= hid2.imp;

        public static SqlBoolean operator >=(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.imp >= hid2.imp;


        [SqlMethod(IsDeterministic = true, IsPrecise = true)]
        public void Write(BinaryWriter w)
        {
            BitWriter bw = new BitWriter(w);

            var nodes = this.imp.GetNodes();

            for (int i = 0; i < nodes.Length; i++)
            {
                var subNodes = nodes[i];
                for (int j = 0; j < subNodes.Length; j++)
                {
                    int val = subNodes[j];

                    BitPattern p = KnownPatterns.GetPatternByValue(val);

                    bool isLast = j == (subNodes.Length - 1);

                    ulong value = p.EncodeValue(val, isLast);

                    bw.Write(value, p.BitLength);
                }
            }

            bw.Finish();
        }

        [SqlMethod(IsDeterministic = true, IsPrecise = true)]
        public void Read(BinaryReader r)
        {
            var bitR = new BitReader(r);
            List<List<int>> result = new List<List<int>>();

            while (true)
            {
                List<int> step = new List<int>();

                while (true)
                {
                    var p = KnownPatterns.GetPatternByPrefix(bitR);

                    if (p == null)
                        goto finish;

                    ulong encodedValue = bitR.Read(p.BitLength);

                    int value = p.Decode(encodedValue, out bool isLast);

                    step.Add(value);

                    if (isLast)
                        break;
                }

                result.Add(step);
            }

            finish:

            this.imp = new HierarchyId(result.Select(a => a.ToArray()).ToArray());
        }
    }
}
