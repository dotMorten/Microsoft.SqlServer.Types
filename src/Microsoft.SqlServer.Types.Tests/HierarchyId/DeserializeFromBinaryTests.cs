namespace Microsoft.SqlServer.Types.Tests.HierarchyId
{
    /// <summary>
    /// Deserialize tests based on examples in the UDT specification
    /// </summary>
    [TestClass]
    [TestCategory("Deserialize")]
    [TestCategory("SqlHierarchyId")]
    public class DeserializeFromBinaryTests
    {

        [TestMethod]
        public void TestSqlHiarchy1()
        {
            // The first child of the root node, with a logical representation of / 1 /, is represented as the following bit sequence:
            // 01011000
            // The first two bits, 01, are the L1 field, meaning that the first node has a label between 0(zero) and 3.The next two bits,
            // 01, are the O1 field and are interpreted as the integer 1.Adding this to the beginning of the range specified by the L1 yields 1.
            // The next bit, with the value 1, is the F1 field, which means that this is a "real" level, with 1 followed by a slash in the logical
            // representation.The final three bits, 000, are the W field, padding the representation to the nearest byte.
            byte[] bytes = { 0x58 }; //01011000
            var hid = new Microsoft.SqlServer.Types.SqlHierarchyId();
            using (var r = new BinaryReader(new MemoryStream(bytes)))
            {
                hid.Read(r);
            }
            Assert.AreEqual("/1/", hid.ToString());
        }

        [TestMethod]
        public void TestSqlHiarchy2()
        {
            // As a more complicated example, the node with logical representation / 1 / -2.18 / (the child with label - 2.18 of the child with label 1 of the root node) is represented as the following sequence of bits(a space has been inserted after every grouping of 8 bits to make the sequence easier to follow):
            // 01011001 11111011 00000101 01000000
            // The first three fields are the same as in the first example.That is, the first two bits(01) are the L1 field, the second two bits(01) are the O1 field, and the fifth bit(1) is the F1 field.This encodes the / 1 / portion of the logical representation.
            // The next 5 bits(00111) are the L2 field, so the next integer is between - 8 and - 1.The following 3 bits(111) are the O2 field, representing the offset 7 from the beginning of this range.Thus, the L2 and O2 fields together encode the integer - 1.The next bit(0) is the F2 field.Because it is 0(zero), this level is fake, and 1 has to be subtracted from the integer yielded by the L2 and O2 fields. Therefore, the L2, O2, and F2 fields together represent -2 in the logical representation of this node.
            // The next 3 bits(110) are the L3 field, so the next integer is between 16 and 79.The subsequent 8 bits(00001010) are the L4 field. Removing the anti - ambiguity bits from there(the third bit(0) and the fifth bit(1)) leaves 000010, which is the binary representation of 2.Thus, the integer encoded by the L3 and O3 fields is 16 + 2, which is 18.The next bit(1) is the F3 field, representing the slash(/) after the 18 in the logical representation.The final 6 bits(000000) are the W field, padding the physical representation to the nearest byte.
            byte[] bytes = { 0x59, 0xFB, 0x05, 0x40 }; //01011001 11111011 00000101 01000000
            var hid = new Microsoft.SqlServer.Types.SqlHierarchyId();
            using (var r = new BinaryReader(new MemoryStream(bytes)))
            {
                hid.Read(r);
            }
            Assert.AreEqual("/1/-2.18/", hid.ToString());
        }

        [TestMethod]
        public void TestSqlHiarchy3()
        {
            byte[] bytes =
            {
                0xFE, 0x61, 0x63, 0x47, 0x11, 0xD0, 0x21, 0x2F, 0xCF, 0x90, 0x37, 0xE3, 0xC0, 0x99, 0x10, 0xFA, 0x9B,
                0x70, 0xB9, 0x63, 0xFF, 0xE4, 0xC2, 0xEC, 0xD4, 0x0B, 0x66, 0x39, 0xF9, 0xBA, 0x46, 0x17, 0xE3, 0x31,
                0x32, 0x1F, 0x5C, 0x7C, 0xD0, 0x40, 0x5F, 0xF3, 0x01, 0xC0, 0x1C, 0xDA, 0x1A, 0xE6, 0x3F, 0xD1, 0x45,
                0x4C, 0x39, 0xF0, 0x0E, 0xD3, 0xEB, 0x34, 0xB0, 0x58, 0xD9, 0x80,
            };
            var hid = new Microsoft.SqlServer.Types.SqlHierarchyId();
            using (var r = new BinaryReader(new MemoryStream(bytes)))
            {
                hid.Read(r);
            }

            Assert.AreEqual(
                "/167515058144400.68456395185647.2799456567/221588961120181.60793214790911.3103372627/52901172697923.230105430619337.3008044937/",
                hid.ToString());
        }
    }
}
