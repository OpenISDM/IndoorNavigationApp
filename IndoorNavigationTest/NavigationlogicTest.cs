using IndoorNavigation.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IndoorNavigationTest
{
    [TestClass]
    public class NavigationlogicTest
    {
        [TestInitialize]
        public void TestInit()
        {
            GenerateMapData.Generate();
        }

        [TestMethod]
        public void StorageTest()
        {
            MapStorage.SaveMapInformation("test1","");
            MapStorage.SaveMapInformation("test1", "");
            string[] Maps = MapStorage.GetAllPlace();
            Assert.AreEqual(1, Maps.Length);

            MapStorage.SaveMapInformation("test2", "");
            MapStorage.SaveMapInformation("test3", "");
            Maps = MapStorage.GetAllPlace();
            Assert.AreEqual(3, Maps.Length);

            MapStorage.DeleteMap("test4");
            Maps = MapStorage.GetAllPlace();
            Assert.AreEqual(3, Maps.Length);

            MapStorage.DeleteMap("test3");
            Maps = MapStorage.GetAllPlace();
            Assert.AreEqual(2, Maps.Length);

            MapStorage.DeleteAllMap();
            Maps = MapStorage.GetAllPlace();
            Assert.AreEqual(0, Maps.Length);
        }
    }
}
