using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var newsInterface = new NextcloudNewsInterface.NextcloudNewsInterface("", "", "");
            var result = await newsInterface.getItems();
        }
    }
}
