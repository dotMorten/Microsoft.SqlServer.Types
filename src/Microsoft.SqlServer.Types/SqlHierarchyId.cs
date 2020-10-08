#if SQLCLIENT_NEW
using Microsoft.Data.SqlClient.Server;
#else
using Microsoft.SqlServer.Server;
#endif
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
    /// <summary>
    /// The SqlHierarchyId type represents a position in a hierarchical structure, specifying depth and breadth. 
    /// </summary>
    [SqlUserDefinedType(Format.UserDefined, IsByteOrdered = true, MaxByteSize = 892, Name = "SqlHierarchyId")]
    public struct SqlHierarchyId : IBinarySerialize, INullable, IComparable
    {
        private HierarchyId _imp;
        private bool _isNotNull;

        /// <summary>
        /// Gets a value indicating whether the <see cref="SqlHierarchyId"/> is null.
        /// </summary>
        /// <value>Boolean representing true (1) if the <see cref="SqlHierarchyId"/> node is null; otherwise, false (0).</value>
        public bool IsNull => !_isNotNull; // This is a bit backwards, but is done so default(SqlHierarchyId) will return a null id

        /// <summary>
        /// Gets a <see cref="SqlHierarchyId"/> with a hierarchy identification of null.
        /// </summary>
        public static SqlHierarchyId Null
        {
            [SqlMethodAttribute(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None,
                IsDeterministic = true, IsPrecise = true, IsMutator = false)]
            get;
        } = new SqlHierarchyId(new HierarchyId(), true);

        private SqlHierarchyId(HierarchyId imp, bool isNull = false)
        {
            _isNotNull = !isNull;
            _imp = imp;
        }

        /// <summary>
        /// Gets a value representing the root <see cref="SqlHierarchyId"/> node of the hierarchy.
        /// </summary>
        /// <returns>A <see cref="SqlHierarchyId"/> representing the root node of the hierarchical tree. Root value is typically 0x.</returns>
        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public static SqlHierarchyId GetRoot() => new SqlHierarchyId(HierarchyId.GetRoot());

        /// <summary>
        /// Converts the canonical string representation of a <see cref="SqlHierarchyId"/> node to a <see cref="SqlHierarchyId"/> value.
        /// </summary>
        /// <param name="input">String representation of <see cref="SqlHierarchyId"/> node.</param>
        /// <returns><see cref="SqlHierarchyId"/> representing the node described canonically.</returns>
        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public static SqlHierarchyId Parse(SqlString input) => new SqlHierarchyId(HierarchyId.Parse((string)input));

        /// <summary>
        /// Returns a value indicating the results of a comparison between a SqlHierarchyId and an object.
        /// </summary>
        /// <param name="obj">An object to be compared to this.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="table">
        ///   <listheader>  
        ///       <term>Value</term>  
        ///       <description>Meaning</description>  
        ///   </listheader>  
        ///   <item><term>Less than zero</term><description>this is less than <paramref name="obj"/>.</description></item>  
        ///   <item><term>Zero</term><description>this is equal to <paramref name="obj"/>.</description></item>  
        ///   <item><term>Greater than zero</term><description>this is greater than <paramref name="obj"/>.</description></item>  
        /// </list>
        /// </returns>
        /// <remarks>
        /// Throws an exception if <paramref name="obj"/> is not a <see cref="SqlHierarchyId"/> node.
        /// This member is sealed.
        /// </remarks>
        public int CompareTo(object obj) => this.CompareTo((SqlHierarchyId)obj);

        /// <summary>
        /// Returns a value indicating the results of a comparison between two <see cref="SqlHierarchyId"/> nodes.
        /// </summary>
        /// <param name="hid">A <see cref="SqlHierarchyId"/> node to compare to this.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="table">
        ///   <listheader>  
        ///       <term>Value</term>  
        ///       <description>Meaning</description>  
        ///   </listheader>  
        ///   <item><term>Less than zero</term><description>this is less than <paramref name="hid"/>.</description></item>  
        ///   <item><term>Zero</term><description>this is equal to <paramref name="hid"/>.</description></item>  
        ///   <item><term>Greater than zero</term><description>this is greater than <paramref name="hid"/>.</description></item>  
        /// </list>
        /// </returns>
        /// <remarks>
        /// If both <see cref="SqlHierarchyId"/> nodes are null, returns 0.
        /// If one <see cref="SqlHierarchyId"/> node is null, it is considered to be less than the non-null <see cref="SqlHierarchyId"/> node.
        /// </remarks>
        public int CompareTo(SqlHierarchyId hid)
        {
            if(IsNull)
            {
                if (!hid.IsNull)
                    return -1;
                return 0;
            }
            if (hid.IsNull)
                return 1;
            if (this < hid)
                return -1;
            if (this > hid)
                return 1;
            return 0;
        }

        /// <summary>
        /// Evaluates whether <see cref="SqlHierarchyId"/> and obj are equal.
        /// </summary>
        /// <param name="obj">The object against which to compare <c>this</c>.</param>
        /// <returns>Boolean. true (1) if this and obj are equal; otherwise, false (0).</returns>
        /// <remarks>
        /// <para>Returns false (0) if obj is not a SqlHierarchyId node.</para>
        /// <para>Returns true (1) if both this and obj are null.</para>
        /// </remarks>
        public override bool Equals(object obj) => obj is SqlHierarchyId && Equals((SqlHierarchyId)obj);

        private bool Equals(SqlHierarchyId other) => (IsNull && other.IsNull) || (this == other).IsTrue;

        /// <summary>
        /// Retrieves the <see cref="SqlHierarchyId"/> node n levels up the hierarchical tree.
        /// </summary>
        /// <param name="n">An integer representing the number of levels to ascend in the hierarchy. </param>
        /// <returns>
        /// <see cref="SqlHierarchyId"/> representing the nth ancestor of <c>this</c>.
        /// If a number greater than <see cref="GetLevel"/> is passed, <c>null</c> is returned.
        /// If a negative number is passed, an exception is raised indicating that the argument is out of range.
        /// </returns>
        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlHierarchyId GetAncestor(int n)
        {
            if (IsNull || _imp.GetLevel() < n)
            {
                return Null;
            }
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException("24011: SqlHierarchyId.GetAncestor failed because 'n' was negative.");
            }
            return new SqlHierarchyId(_imp.GetAncestor(n));
        }
        /// <summary>
        /// Gets the value of a descendant <see cref="SqlHierarchyId"/> node that is greater than <paramref name="child1"/> and less than <paramref name="child2"/>.
        /// </summary>
        /// <param name="child1">The lower bound.</param>
        /// <param name="child2">The upper bound.</param>
        /// <returns>A SqlHierarchyId with a value greater than the lower bound and less than the upper bound.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>If parent is <c>null</c>, returns <c>null</c>.</item>
        /// <item>If parent is not null, and both <paramref name="child1"/> and <paramref name="child2"/> are <c>null</c>, returns a descendant of parent.</item>
        /// <item>If parent and <paramref name="child1"/> are not <c>null</c>, and <paramref name="child2"/> is <c>null</c>, returns a descendant of parent greater than <paramref name="child1"/>.</item>
        /// <item>If parent and <paramref name="child2"/> are not <c>null</c> and <paramref name="child1"/> is <c>null</c>, returns a descendant of parent less than <paramref name="child2"/>.</item>
        /// <item>If parent, <paramref name="child1"/>, and child2 are not <c>null</c>, returns a descendant of parent greater than <paramref name="child1"/> and less than <paramref name="child2"/>.</item>
        /// <item>An exception is raised if <paramref name="child1"/> or <paramref name="child2"/> are not <c>null</c> and are not a descendant of parent.</item>
        /// <item>If <paramref name="child1"/> >= <paramref name="child2"/>, an exception is raised.</item>
        /// </list>
        /// </remarks>
        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = true, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlHierarchyId GetDescendant(SqlHierarchyId child1, SqlHierarchyId child2) => new SqlHierarchyId(_imp.GetDescendant(
            child1.IsNull ? default(HierarchyId?) : child1._imp, 
            child2.IsNull ? default(HierarchyId?) : child2._imp));

        /// <summary>
        /// Gets a hash of the path from the root node of the hierarchy tree to the <see cref="SqlHierarchyId"/> node.
        /// </summary>
        /// <returns>A 32-bit signed integer representing the hash code for this instance.</returns>
        public override int GetHashCode() => _imp.GetHashCode();

        /// <summary>
        /// Gets a value indicating the level of the <see cref="SqlHierarchyId"/> node in the hierarchical tree.
        /// </summary>
        /// <returns>A 16-bit integer indicating the depth of the <see cref="SqlHierarchyId"/> node in the hierarchical tree. 
        /// The root of the hierarchy is level 0.</returns>
        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlInt16 GetLevel() => _imp.GetLevel();

        /// <summary>
        /// Gets a value representing the location of a new <see cref="SqlHierarchyId"/> node that has a path from newRoot equal to the path from oldRoot to <c>this</c>, effectively moving <c>this</c> to the new location.
        /// </summary>
        /// <param name="oldRoot">An ancestor of the <see cref="SqlHierarchyId"/> node specifying the endpoint of the path segment that is to be moved.</param>
        /// <param name="newRoot">The <see cref="SqlHierarchyId"/> node that represents the new ancestor of <c>this</c>.</param>
        /// <returns>A <see cref="SqlHierarchyId"/> node representing the new hierarchical location of <c>this</c>. Will return <c>null</c> if <paramref name="oldRoot"/>, <paramref name="newRoot"/>, or this are <c>null</c>.</returns>
        /// <remarks>
        /// <para>Returns a node whose path from the root is the path to <paramref name="newRoot"/>, followed by the path from <paramref name="oldRoot"/> to <c>this</c>.</para>
        /// <para>The <see cref="SqlHierarchyId"/> data type represents but does not enforce the hierarchical structure. Users must ensure that the <see cref="SqlHierarchyId"/> node is appropriately structured for the new location.</para>
        /// </remarks>
        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlHierarchyId GetReparentedValue(SqlHierarchyId oldRoot, SqlHierarchyId newRoot)
        {
            if (!IsNull && !oldRoot.IsNull && !newRoot.IsNull)
            {
                if (!IsDescendantOf(oldRoot))
                {
                    throw new HierarchyIdException("Instance is not a descendant of 'oldRoot'");
                }
                return new SqlHierarchyId(_imp.GetReparentedValue(oldRoot._imp, newRoot._imp));
            }
            return Null;
        }
        /// <summary>
        /// Gets a value indicating whether the <see cref="SqlHierarchyId"/> node is the descendant of the parent.
        /// </summary>
        /// <param name="parent">The specified <see cref="SqlHierarchyId"/> node for which the IsDescendantOf test is performed.</param>
        /// <returns><c>Boolean</c>, <c>true</c> (1) for all the nodes in the sub-tree rooted at parent; <c>false</c> (0) for all other nodes.</returns>
        /// <remarks>parent is considered its own descendant.</remarks>
        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public SqlBoolean IsDescendantOf(SqlHierarchyId parent) => _imp.IsDescendantOf(parent._imp);

        /// <summary>
        /// Returns the canonical string representation of a <see cref="SqlHierarchyId"/> node from a <see cref="SqlHierarchyId"/> value.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Called implicitly when a conversion from a <see cref="SqlHierarchyId"/> data type to a string type occurs.
        /// Acts as the opposite of <see cref="Parse"/>.
        /// </remarks>
        /// <example><code lang="sql">
        /// DECLARE @StringValue AS nvarchar(4000), @hierarchyidValue AS hierarchyid
        /// SET @StringValue = '/1/1/3/'
        /// SET @hierarchyidValue = 0x5ADE
        /// SELECT hierarchyid::Parse(@StringValue) AS hierarchyidRepresentation,
        /// @hierarchyidValue.ToString() AS StringRepresentation;
        /// GO
        /// </code></example>
        [SqlMethod(DataAccess = DataAccessKind.None, SystemDataAccess = SystemDataAccessKind.None, InvokeIfReceiverIsNull = false, OnNullCall = false, IsDeterministic = true, IsPrecise = true, IsMutator = false)]
        public override string ToString() => _imp.ToString();

        /// <summary>
        /// Writes a <see cref="SqlHierarchyId"/> to a specified binary writer.
        /// </summary>
        /// <param name="w">The specified binary writer.</param>
        /// <remarks>
        /// Throws an exception if w is <c>null</c>.
        /// Throws an exception if the <see cref="SqlHierarchyId"/> is <c>null</c>.
        /// This member is <c>sealed</c>.
        /// </remarks>
        [SqlMethod(IsDeterministic = true, IsPrecise = true)]
        public void Write(BinaryWriter w)
        {
            if (w is null)
                throw new ArgumentNullException(nameof(w));
            if (IsNull)
                throw new HierarchyIdException("24002: SqlHierarchyId.Write failed because 'this' was a NULL instance.");
            BitWriter bw = new BitWriter(w);

            var nodes = this._imp.GetNodes();

            for (int i = 0; i < nodes.Length; i++)
            {
                var subNodes = nodes[i];
                for (int j = 0; j < subNodes.Length; j++)
                {
                    long val = subNodes[j];

                    BitPattern p = KnownPatterns.GetPatternByValue(val);

                    bool isLast = j == (subNodes.Length - 1);

                    ulong value = p.EncodeValue(val, isLast);

                    bw.Write(value, p.BitLength);
                }
            }

            bw.Finish();
        }

        /// <summary>
        /// Reads from a specified binary reader into a <see cref="SqlHierarchyId"/>.
        /// </summary>
        /// <param name="r">The specified binary reader.</param>
        /// <remarks>
        /// Throws an exception if r is null.<br/>
        /// Throws an exception if the SqlHierarchyId is not null.<br/>
        /// This member is sealed.
        /// </remarks>
        [SqlMethod(IsDeterministic = true, IsPrecise = true)]
        public void Read(BinaryReader r)
        {
            if (r is null)
                throw new ArgumentNullException(nameof(r));
            var bitR = new BitReader(r);
            List<List<long>> result = new List<List<long>>();

            while (true)
            {
                List<long> step = new List<long>();

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

            this._imp = new HierarchyId(result.Select(a => a.ToArray()).ToArray());
            this._isNotNull = !_imp.IsNull;
        }

        /// <summary>
        /// Evaluates whether two <see cref="SqlHierarchyId"/> nodes are equal.
        /// </summary>
        /// <param name="hid1">First node to compare.</param>
        /// <param name="hid2">Second node to compare.</param>
        /// <returns>Boolean. true (1) if <paramref name="hid1"/> and <paramref name="hid2"/> are equal; otherwise, false (0).</returns>
        /// <remarks>Returns null if either <paramref name="hid1"/> or <paramref name="hid2"/> are null.</remarks>
        public static SqlBoolean operator ==(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.IsNull || hid2.IsNull ? SqlBoolean.Null : hid1._imp == hid2._imp;

        /// <summary>
        /// Evaluates whether two <see cref="SqlHierarchyId"/> nodes are unequal.
        /// </summary>
        /// <param name="hid1">First node to compare.</param>
        /// <param name="hid2">Second node to compare.</param>
        /// <returns></returns>
        /// <remarks>Returns null if either <paramref name="hid1"/> or <paramref name="hid2"/> are null.</remarks>
        public static SqlBoolean operator !=(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.IsNull || hid2.IsNull ? SqlBoolean.Null : hid1._imp != hid2._imp;

        /// <summary>
        /// Evaluates whether one specified <see cref="SqlHierarchyId"/> node is less than another.
        /// </summary>
        /// <param name="hid1">First node to compare.</param>
        /// <param name="hid2">Second node to compare.</param>
        /// <returns></returns>
        /// <remarks>Returns null if either <paramref name="hid1"/> or <paramref name="hid2"/> are null.</remarks>
        public static SqlBoolean operator <(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.IsNull || hid2.IsNull ? SqlBoolean.Null : hid1._imp < hid2._imp;

        /// <summary>
        /// Evaluates whether one specified <see cref="SqlHierarchyId"/> node is greater than another.
        /// </summary>
        /// <param name="hid1">First node to compare.</param>
        /// <param name="hid2">Second node to compare.</param>
        /// <returns></returns>
        /// <remarks>Returns null if either <paramref name="hid1"/> or <paramref name="hid2"/> are null.</remarks>
        public static SqlBoolean operator >(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.IsNull || hid2.IsNull ? SqlBoolean.Null : hid1._imp > hid2._imp;

        /// <summary>
        /// Evaluates whether one specified <see cref="SqlHierarchyId"/> node is less than or equal to another.
        /// </summary>
        /// <param name="hid1">First node to compare.</param>
        /// <param name="hid2">Second node to compare.</param>
        /// <returns></returns>
        /// <remarks>Returns null if either <paramref name="hid1"/> or <paramref name="hid2"/> are null.</remarks>
        public static SqlBoolean operator <=(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.IsNull || hid2.IsNull ? SqlBoolean.Null : hid1._imp <= hid2._imp;

        /// <summary>
        /// Evaluates whether one specified <see cref="SqlHierarchyId"/> node is greater than or equal to another.
        /// </summary>
        /// <param name="hid1">First node to compare.</param>
        /// <param name="hid2">Second node to compare.</param>
        /// <returns></returns>
        /// <remarks>Returns null if either <paramref name="hid1"/> or <paramref name="hid2"/> are null.</remarks>
        public static SqlBoolean operator >=(SqlHierarchyId hid1, SqlHierarchyId hid2) => hid1.IsNull || hid2.IsNull ? SqlBoolean.Null : hid1._imp >= hid2._imp;

        //public static SqlHierarchyId Deserialize(SqlBytes bytes)
        //{
        //    using (var r = new BinaryReader(bytes.Stream))
        //    {
        //        var hid = new SqlHierarchyId(new HierarchyId());
        //        hid.Read(r);
        //        return hid;
        //    }
        //}

        //public SqlBytes Serialize()
        //{
        //    using (var ms = new MemoryStream())
        //    {
        //        Write(new BinaryWriter(ms));
        //        return new SqlBytes(ms.ToArray());
        //    }
        //}
    }
}
