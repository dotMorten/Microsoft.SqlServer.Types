using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.SqlServer.Types.SqlHierarchy
{
    /// <summary>
    /// Represents hierarchical data.
    /// </summary>
    [Serializable]
    internal struct HierarchyId : IComparable
    {
        private readonly string _hierarchyId;
        private readonly int[][] _nodes;

        static readonly int[][] __RootNodes = new int[0][];

        internal int[][] GetNodes() => _nodes ?? __RootNodes;

        /// <summary>
        /// The Path separator character
        /// </summary>
        public const string PathSeparator = "/";

        private const string InvalidHierarchyIdExceptionMessage =
            "The input string '{0}' is not a valid string representation of a HierarchyId node.";

        private const string GetReparentedValueOldRootExceptionMessage =
            "HierarchyId.GetReparentedValue failed because 'oldRoot' was not an ancestor node of 'this'.  'oldRoot' was '{0}', and 'this' was '{1}'.";

        private const string GetDescendantMostBeChildExceptionMessage =
            "HierarchyId.GetDescendant failed because '{0}' must be a child of 'this'.  '{0}' was '{1}' and 'this' was '{2}'.";

        private const string GetDescendantChild1MustLessThanChild2ExceptionMessage =
            "HierarchyId.GetDescendant failed because 'child1' must be less than 'child2'.  'child1' was '{0}' and 'child2' was '{1}'.";

        internal HierarchyId(int[][] nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            this._nodes= nodes;
            this._hierarchyId = nodes == null  || nodes.Length == 0 ? PathSeparator :
                (PathSeparator + string.Join(PathSeparator, nodes.Select(IntArrayToString)) + PathSeparator);
        }

        /// <summary>
        ///     Constructs an HierarchyId with the given canonical string representation value.
        /// </summary>
        /// <returns>Hierarchyid value.</returns>
        /// <param name="hierarchyId">Canonical string representation</param>
        public HierarchyId(string hierarchyId)
        {
            _hierarchyId = hierarchyId ?? throw new ArgumentNullException(nameof(hierarchyId));
            if (hierarchyId == "/")
            {
                _nodes = __RootNodes;
            }
            else
            {
                var nodesStr = hierarchyId.Split('/');
                if (!string.IsNullOrEmpty(nodesStr[0]) || !string.IsNullOrEmpty(nodesStr[nodesStr.Length - 1]))
                    throw new HierarchyIdException( string.Format(CultureInfo.InvariantCulture, InvalidHierarchyIdExceptionMessage, hierarchyId));

                int nodesCount = nodesStr.Length - 2;
                var nodes = new int[nodesCount][];
                for (int i = 0; i < nodesCount; i++)
                {
                    string node = nodesStr[i + 1];
                    var intsStr = node.Split('.');
                    var ints = new int[intsStr.Length];
                    for (int j = 0; j < intsStr.Length; j++)
                    {
                        if (!int.TryParse(intsStr[j], out int num))
                            throw new HierarchyIdException(string.Format(CultureInfo.InvariantCulture, InvalidHierarchyIdExceptionMessage, hierarchyId));
                        ints[j] = num;
                    }
                    nodes[i] = ints;
                }
                _nodes = nodes;
            }
        }

        /// <summary>
        /// Returns a hierarchyid representing the nth ancestor of this.
        /// </summary>
        /// <returns>A hierarchyid representing the nth ancestor of this.</returns>
        /// <param name="n">n</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "n")]
        public HierarchyId GetAncestor(int n)
        {
            if (GetLevel() == n)
                return new HierarchyId(__RootNodes);

            if (GetLevel() < n)
                throw new ArgumentException(nameof(n));

            string hierarchyStr = PathSeparator + string.Join(PathSeparator, GetNodes().Take(GetLevel() - n).Select(IntArrayToString)) + PathSeparator;
            return new HierarchyId(hierarchyStr);
        }

        /// <summary>
        /// Returns a child node of the parent.
        /// </summary>
        /// <param name="child1"> null or the hierarchyid of a child of the current node. </param>
        /// <param name="child2"> null or the hierarchyid of a child of the current node. </param>
        /// <returns>
        /// Returns one child node that is a descendant of the parent.
        /// If both child1 and child2 are null, returns a child of parent.
        /// If child1 is not null, and child2 is null, returns a child of parent greater than child1.
        /// If child2 is not null and child1 is null, returns a child of parent less than child2.
        /// If child1 and child2 are not null, returns a child of parent greater than child1 and less than child2.
        /// If child1 is not null and not a child of parent, an exception is raised.
        /// If child2 is not null and not a child of parent, an exception is raised.
        /// If child1 >= child2, an exception is raised.
        /// </returns>
        public HierarchyId GetDescendant(HierarchyId? child1, HierarchyId? child2)
        {
            if (child1 != null && (child1.Value.GetLevel() != GetLevel() + 1 || !child1.Value.IsDescendantOf(this)))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, GetDescendantMostBeChildExceptionMessage, "child1", child1, ToString()), "child1");

            if (child2 != null&& (child2.Value.GetLevel() != GetLevel() + 1 || !child2.Value.IsDescendantOf(this)))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, GetDescendantMostBeChildExceptionMessage, "child2", child1, ToString()), "child2");
           
            if (child1 == null && child2 == null)
                return new HierarchyId(ToString() + 1 + PathSeparator);

            string hierarchyStr;
            if (child1 == null)
            {
                var result = new HierarchyId(child2.ToString());
                var lastNode = result.GetNodes().Last();
                //decrease the last part of the last node of the 1nd child
                lastNode[lastNode.Length - 1]--;
                hierarchyStr = PathSeparator + string.Join(PathSeparator, result.GetNodes().Select(IntArrayToString)) + PathSeparator;
                return new HierarchyId(hierarchyStr);
            }
            if (child2 == null)
            {
                var result = new HierarchyId(child1.ToString());
                var lastNode = result.GetNodes().Last();
                //increase the last part of the last node of the 2nd child
                lastNode[lastNode.Length - 1]++;
                hierarchyStr = PathSeparator + string.Join(PathSeparator, result.GetNodes().Select(IntArrayToString)) + PathSeparator;
                return new HierarchyId(hierarchyStr);
            }
            var child1LastNode = child1.Value.GetNodes().Last();
            var child2LastNode = child2.Value.GetNodes().Last();
            var cmp = CompareIntArrays(child1LastNode, child2LastNode);
            if (cmp >= 0)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, GetDescendantChild1MustLessThanChild2ExceptionMessage, child1, child2),
                    "child1");
            }
            int firstDiffrenceIdx = 0;
            for (; firstDiffrenceIdx < child1LastNode.Length; firstDiffrenceIdx++)
            {
                if (child1LastNode[firstDiffrenceIdx] < child2LastNode[firstDiffrenceIdx])
                {
                    break;
                }
            }
            child1LastNode = child1LastNode.Take(firstDiffrenceIdx + 1).ToArray();
            if(child1LastNode.Length >= firstDiffrenceIdx || child2LastNode.Length >= firstDiffrenceIdx)
            {
                child1LastNode = child1LastNode.Concat(new[] { 0 }).ToArray();
            }
            else if (child1LastNode[firstDiffrenceIdx] + 1 < child2LastNode[firstDiffrenceIdx])
            {
                child1LastNode[firstDiffrenceIdx]++;
            }
            else
            {
                child1LastNode = child1LastNode.Concat(new[] { 1 }).ToArray();
            }
            hierarchyStr = PathSeparator + string.Join(PathSeparator, GetNodes().Select(IntArrayToString)) + PathSeparator + IntArrayToString(child1LastNode) + PathSeparator;
            return new HierarchyId(hierarchyStr);
        }

        /// <summary>
        /// Returns an integer that represents the depth of the node this in the tree.
        /// </summary>
        /// <returns>An integer that represents the depth of the node this in the tree.</returns>
        public short GetLevel()
        {
            return (short)GetNodes().Length;
        }

        /// <summary>
        /// Returns the root of the hierarchy tree.
        /// </summary>
        /// <returns>The root of the hierarchy tree.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static HierarchyId GetRoot()
        {
            return new HierarchyId("/");
        }

        /// <summary>
        /// Returns true if this is a descendant of parent.
        /// </summary>
        /// <returns>True if this is a descendant of parent.</returns>
        /// <param name="parent">parent</param>
        public bool IsDescendantOf(HierarchyId parent)
        {
            if (parent.GetLevel() > GetLevel())
            {
                return false;
            }
            for (int i = 0; i < parent.GetLevel(); i++)
            {
                int cmp = CompareIntArrays(GetNodes()[i], parent.GetNodes()[i]);
                if (cmp != 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a node whose path from the root is the path to newRoot, followed by the path from oldRoot to this.
        /// </summary>
        /// <returns>Hierarchyid value.</returns>
        /// <param name="oldRoot">oldRoot</param>
        /// <param name="newRoot">newRoot</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Reparented")]
        public HierarchyId GetReparentedValue(HierarchyId oldRoot, HierarchyId newRoot)
        {
            if (!IsDescendantOf(oldRoot))
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, GetReparentedValueOldRootExceptionMessage, oldRoot, ToString()), "oldRoot");
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(PathSeparator);
            foreach (var node in newRoot.GetNodes())
            {
                sb.Append(IntArrayToString(node));
                sb.Append(PathSeparator);
            }
            foreach (var node in GetNodes().Skip(oldRoot.GetLevel()))
            {
                sb.Append(IntArrayToString(node));
                sb.Append(PathSeparator);
            }
            return new HierarchyId(sb.ToString());
        }

        /// <summary>
        /// Converts the canonical string representation of a hierarchyid to a hierarchyid value.
        /// </summary>
        /// <returns>Hierarchyid value.</returns>
        /// <param name="input">input</param>
        public static HierarchyId Parse(string input)
        {
            return new HierarchyId(input);
        }

        private static string IntArrayToString(IEnumerable<int> array)
        {
            return string.Join(".", array);
        }

        private static int CompareIntArrays(int[] array1, int[] array2)
        {
            int count = Math.Min(array1.Length, array2.Length);
            for (int i = 0; i < count; i++)
            {
                int item1 = array1[i];
                int item2 = array2[i];

                if (item1 < item2)
                    return -1;

                if (item1 > item2)
                    return 1;
            }

            if (array1.Length > count)
                return 1;

            if (array2.Length > count)
                return -1;

            return 0;
        }

        /// <summary>
        /// Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> 
        /// A 32-bit signed integer that indicates the lexical relationship between the two comparands.
        /// Value Condition Less than zero: hid1 is less than hid2. 
        /// Zero: hid1 equals hid2. 
        /// Greater than zero: hid1 is greater than hid2. 
        /// </returns>
        public static int Compare(HierarchyId hid1, HierarchyId hid2)
        {
            var nodes1 = hid1.GetNodes();
            var nodes2 = hid2.GetNodes();

            int count = Math.Min(nodes1.Length, nodes2.Length);
            for (int i = 0; i < count; i++)
            {
                var node1 = nodes1[i];
                var node2 = nodes2[i];
                int cmp = CompareIntArrays(node1, node2);
                if (cmp != 0)
                    return cmp;

            }

            if (nodes1.Length > count)
                return 1;

            if (nodes2.Length > count)
                return -1;

            return 0;
        }

        /// <summary>
        /// Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> 
        /// true if the the first parameter is less than the second parameter, false otherwise 
        /// </returns>
        public static bool operator <(HierarchyId hid1, HierarchyId hid2) => Compare(hid1, hid2) <  0;

        /// <summary>
        /// Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> 
        /// true if the the first parameter is greater than the second parameter, false otherwise 
        /// </returns>
        public static bool operator >(HierarchyId hid1, HierarchyId hid2) => Compare(hid1, hid2) > 0;

        /// <summary>
        /// Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> 
        /// true if the the first parameter is less or equal than the second parameter, false otherwise 
        /// </returns>
        public static bool operator <=(HierarchyId hid1, HierarchyId hid2) => Compare(hid1, hid2) <= 0;

        /// <summary>
        /// Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> 
        ///      true if the the first parameter is greater or equal than the second parameter, false otherwise 
        /// </returns>
        public static bool operator >=(HierarchyId hid1, HierarchyId hid2) => Compare(hid1, hid2) <= 0;

        /// <summary>
        /// Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> true if the two HierarchyIds are equal, false otherwise </returns>
        public static bool operator ==(HierarchyId hid1, HierarchyId hid2) => Compare(hid1, hid2) == 0;

        /// <summary>
        /// Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> true if the two HierarchyIds are not equal, false otherwise </returns>
        public static bool operator !=(HierarchyId hid1, HierarchyId hid2) => Compare(hid1, hid2) != 0;

        /// <summary>
        /// Compares this instance to a given HierarchyId by their values.
        /// </summary>
        /// <param name="other"> the HierarchyId to compare against this instance </param>
        /// <returns> true if this instance is equal to the given HierarchyId, and false otherwise </returns>
        public bool Equals(HierarchyId other) => Compare(this, other) == 0;

        /// <summary>
        /// Returns a value-based hash code, to allow HierarchyId to be used in hash tables.
        /// </summary>
        /// <returns> the hash value of this HierarchyId </returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Compares this instance to a given HierarchyId by their values.
        /// </summary>
        /// <param name="obj"> the HierarchyId to compare against this instance </param>
        /// <returns> true if this instance is equal to the given HierarchyId, and false otherwise </returns>
        public override bool Equals(object obj)
        {
            return Equals((HierarchyId)obj);
        }

        /// <summary>
        /// Returns a string representation of the hierarchyid value.
        /// </summary>
        /// <returns>A string representation of the hierarchyid value.</returns>
        public override string ToString()
        {
            return _hierarchyId ?? PathSeparator;
        }

        /// <summary>
        /// Implementation of IComparable.CompareTo()
        /// </summary>
        /// <param name="obj"> The object to compare to </param>
        /// <returns> 0 if the HierarchyIds are "equal" (i.e., have the same _hierarchyId value) </returns>
        public int CompareTo(object obj)
        {
            if (obj is HierarchyId h)
            {
                return Compare(this, h);
            }

            Debug.Assert(false, "object is not a HierarchyId");
            return -1;
        }
    }
}

