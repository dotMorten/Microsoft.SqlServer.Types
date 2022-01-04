namespace Microsoft.SqlServer.Types.Tests
{
    public static class AssertEx
    {
        public static void ThrowsException(Action action, Type exceptionType, string message)
        {
            try
            {
                action();
            }
            catch (System.Exception ex)
            {
                Assert.IsInstanceOfType(ex, exceptionType);
                Assert.AreEqual(message, ex.Message);
                return;
            }
            Assert.Fail("Expected exception, but no exception was thrown");
        }
    }
}
