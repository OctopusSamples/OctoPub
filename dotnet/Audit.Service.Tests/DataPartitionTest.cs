using Audit.Service.Lambda;
using NUnit.Framework;

namespace Audit.Service.Tests
{
    public class DataPartitionTest
    {
        [Test]
        [TestCase(new [] {"application/vnd.api+json", "application/vnd.api+json; dataPartition=test"}, "test")]
        [TestCase(new [] {"application/vnd.api+json", "application/vnd.api+json; dataPartition= test"}, "test")]
        [TestCase(new [] {"application/vnd.api+json", "application/vnd.api+json; dataPartition= test "}, "test")]
        [TestCase(new [] {"application/vnd.api+json", "application/vnd.api+json; dataPartition=test "}, "test")]
        [TestCase(new [] {"application/vnd.api+json", "application/vnd.api+json; dataPartition=main"}, "main")]
        [TestCase(new [] {"application/vnd.api+json", "application/vnd.api+json"}, "main")]
        [TestCase(new [] {"application/vnd.api+json"}, "main")]
        public void ExtractDataPartition(string[] headerValues, string expected)
        {
            Assert.AreEqual(expected, RequestWrapperFactory.GetDataPartition(headerValues));
        }
    }
}