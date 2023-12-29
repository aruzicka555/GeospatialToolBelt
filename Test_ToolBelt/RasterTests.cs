using GeoTBelt;
using GeoTBelt.GeoTiff;
using System.Linq;

namespace Test_ToolBelt
{
    [TestClass]
    public class RasterTests
    {
        private static string currentDirectory;
        private static string ascTestFileName;
        private static string ascTestFileFullPath;
        private static Raster ascRaster;
        private static string ascOutputTestFile;
        private static string ascOutputTestFileFullPath;

        private static GeoTiffRaster geoTiffRaster;
        private static string geoTiffFileName;
        private static string geoTiffFileFullPath;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            currentDirectory = improvedGetCurrentDirectory("Test_ToolBelt");
            // geoTiffFileName = "NCSU Biltmore Hall small uncomp.tif";
            geoTiffFileName = "TestData_00765413_uncompressed.tif";
            geoTiffFileFullPath = Path.Combine(currentDirectory, geoTiffFileName);
            geoTiffRaster = (GeoTiffRaster) Raster.Load(geoTiffFileFullPath);

            ascTestFileName = "TestData_00765413.asc";
            ascOutputTestFile = "TestData_00765413_out.asc";
            ascOutputTestFileFullPath = Path.Combine(currentDirectory, ascOutputTestFile);

            var pathAsList = currentDirectory.Split("\\").ToList();
            pathAsList = pathAsList.Take(pathAsList.Count - 3).ToList();
            //currentDirectory = string.Join("\\", pathAsList);
            ascTestFileFullPath = Path.Combine(currentDirectory, ascTestFileName);
            ascRaster = Raster.Load(ascTestFileFullPath);

        }

        private static string improvedGetCurrentDirectory(string desiredDirectory)
        {
            var currDirList = Directory.GetCurrentDirectory().Split("\\").ToList();
            while (currDirList.Last() != desiredDirectory)
            {
                currDirList.RemoveAt(currDirList.Count - 1);
            }
            return string.Join("\\", currDirList);
        }

        #region ASC format tests
        [TestMethod]
        public void ASC_PopulatesSingleBand() 
        {
            Assert.AreEqual(1, ascRaster.bands.Count);

            double expected = 4477.26d;
            double? actual = ascRaster.bands[0].At(0, 0);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected: expected, actual: (double) actual,
                delta: 0.005);

            expected = 4476.672d;
            actual = ascRaster.bands[0].At(1, 1);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected: expected, actual: (double)actual,
                delta: 0.005);

        }

        [TestMethod]
        public void ASC_WriteASCRaster_BlockWriteMethod_correctly()
        {
            Assert.IsNotNull(ascRaster);
            double? intermediateValue = ascRaster.bands[0].At(3, 3);
            Assert.IsNotNull(intermediateValue);
            double expected = (double)intermediateValue + 5.0;
            ascRaster.bands[0].Set(expected, 3, 3);
            ascRaster.WriteASCRaster(ascOutputTestFileFullPath);

            var outputRaster = Raster.Load(ascOutputTestFileFullPath);
            Assert.IsNotNull(outputRaster);
            Assert.AreEqual(1, outputRaster.bands.Count);
            double? actual = outputRaster.bands[0].At(3, 3);
            Assert.AreEqual(expected: expected, actual: (double)actual,
                delta: 0.005);
        }
        #endregion ASC format tests

        [TestMethod]
        public void GeoTiff_PopulatesSingleBand()
        {
            Assert.IsNotNull(geoTiffRaster);

            Assert.AreEqual(
                expected: 2, 
                actual: geoTiffRaster.RowsPerStrip);

            Assert.AreEqual(expected: 1,
                actual: geoTiffRaster.bands.Count);

            float actual0_0 = (float) geoTiffRaster.bands[0].At(0, 0);
            Assert.AreEqual(expected: 4477.26f, actual: actual0_0, delta: 0.0001);

            float actual799_799 = (float)geoTiffRaster.bands[0].At(799, 799);
            Assert.AreEqual(expected: 5055.912f, actual: actual799_799, delta: 0.0001);

            float actual798_797 = (float)geoTiffRaster.bands[0].At(798, 797);
            Assert.AreEqual(expected: 5055.26f, actual: actual798_797, delta: 0.0001);

            bool exceptionThrown = false;
            try
            {
                float fails = (float)geoTiffRaster.bands[0].At(800, 800);
            }
            catch (IndexOutOfRangeException)
            { exceptionThrown = true; } 
            catch (Exception ex) { }
            Assert.IsTrue(exceptionThrown);

            exceptionThrown = false;
            try
            {
                float fails = (float)geoTiffRaster.bands[0].At(100, 800);
            }
            catch (IndexOutOfRangeException)
            { exceptionThrown = true; }
            catch (Exception ex) { }
            Assert.IsTrue(exceptionThrown);

            exceptionThrown = false;
            try
            {
                float fails = (float)geoTiffRaster.bands[0].At(800, 100);
            }
            catch (IndexOutOfRangeException)
            { exceptionThrown = true; }
            catch (Exception ex) { }
            Assert.IsTrue(exceptionThrown);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if(File.Exists(ascOutputTestFileFullPath))
                File.Delete(ascOutputTestFileFullPath);
        }
    }
}
