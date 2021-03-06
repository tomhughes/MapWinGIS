﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MapWinGIS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MapWinGISTests
{
    [TestClass]
    [DeploymentItem("Testdata")]
    public class ShapefileTests : ICallback
    {
        [TestInitialize]
        public void Init()
        {
            Helper.DebugMsg("Test was run at " + DateTime.Now);
        }

        [TestMethod]
        public void SaveShapefileTest()
        {
            var tempFolder = Path.GetTempPath();
            var tempFilename = Path.Combine(tempFolder, "CreateShapefileTest.shp");
            Helper.DeleteShapefile(tempFilename);

            bool result;
            // Create shapefile
            var sf = new Shapefile { GlobalCallback = this };
            try
            {
                result = sf.CreateNewWithShapeID(tempFilename, ShpfileType.SHP_POINT);
                Assert.IsTrue(result, "Could not create shapefile");

                Assert.IsTrue(sf.EditingShapes, "Shapefile is not in edit shapes mode");
                Assert.IsTrue(sf.EditingTable, "Shapefile is not in edit table mode");

                // Add fields:
                var fieldIndex = sf.EditAddField("date", FieldType.STRING_FIELD, 0, 50);
                Assert.AreEqual(1, fieldIndex, "Could not add field");
                fieldIndex = sf.EditAddField("remarks", FieldType.STRING_FIELD, 0, 100);
                Assert.AreEqual(2, fieldIndex, "Could not add field");
                fieldIndex = sf.EditAddField("amount", FieldType.INTEGER_FIELD, 0, 3);
                Assert.AreEqual(3, fieldIndex, "Could not add field");
                Assert.AreEqual(fieldIndex + 1, sf.NumFields, "Number of fields are incorrect");

                result = sf.Save();
                Assert.IsTrue(result, "Could not save shapefile");
                Assert.AreEqual(fieldIndex + 1, sf.NumFields, "Number of fields are incorrect");
            }
            finally
            {
                // Close the shapefile:
                result = sf.Close();
                Assert.IsTrue(result, "Could not close shapefile");
            }
        }

        [TestMethod]
        public void CreateShapefileTest()
        {
            var tempFolder = Path.GetTempPath();
            var tempFilename = Path.Combine(tempFolder, "CreateShapefileTest.shp");
            Helper.DeleteShapefile(tempFilename);

            bool result;
            // Create shapefile
            var sf = new Shapefile { GlobalCallback = this };
            try
            {
                result = sf.CreateNewWithShapeID(tempFilename, ShpfileType.SHP_POINT);
                Assert.IsTrue(result, "Could not create shapefile");

                Assert.IsTrue(sf.EditingShapes, "Shapefile is not in edit shapes mode");
                Assert.IsTrue(sf.EditingTable, "Shapefile is not in edit table mode");

                // Add fields:
                Assert.IsTrue(sf.Table.EditingTable, "Table is not in edit table mode");
                var fieldIndex = sf.Table.EditAddField("date", FieldType.STRING_FIELD, 0, 50);
                Assert.AreEqual(1, fieldIndex, "Could not add field");
                fieldIndex = sf.Table.EditAddField("remarks", FieldType.STRING_FIELD, 0, 100);
                Assert.AreEqual(2, fieldIndex, "Could not add field");
                fieldIndex = sf.Table.EditAddField("amount", FieldType.INTEGER_FIELD, 0, 3);
                Assert.AreEqual(3, fieldIndex, "Could not add field");
                Assert.AreEqual(fieldIndex + 1, sf.NumFields, "Number of fields are incorrect");

                result = sf.Table.Save();
                Assert.IsTrue(result, "Could not save table");
                Assert.AreEqual(fieldIndex + 1, sf.NumFields, "Number of fields are incorrect");

                // Create shape:
                var shp = new Shape();
                result = shp.Create(sf.ShapefileType);
                Assert.IsTrue(result, "Could not create shape");
                // Create point:
                var pnt = new Point
                {
                    x = 200,
                    y = 200
                };
                // Add point:
                var pointIndex = shp.numPoints;
                result = shp.InsertPoint(pnt, ref pointIndex);
                Assert.IsTrue(result, "Could not insert point");
                var shapeIndex = sf.NumShapes;
                result = sf.EditInsertShape(shp, ref shapeIndex);
                Assert.IsTrue(result, "Could not insert shape");
                // Update attributes:
                sf.EditCellValue(fieldIndex, shapeIndex, 3);

                result = sf.Save();
                Assert.IsTrue(result, "Could not save shapefile");

                // Check shapefile:
                Assert.AreEqual(shapeIndex + 1, sf.NumShapes, "Number of shapes are incorrect");
                Assert.AreEqual(fieldIndex + 1, sf.NumFields, "Number of fields are incorrect");

                // Close shapefile and re-open:
                result = sf.Close();
                Assert.IsTrue(result, "Could not close shapefile");

                result = sf.Open(tempFilename);
                Assert.IsTrue(result, "Could not open shapefile");
                // Check shapefile:
                Assert.AreEqual(shapeIndex + 1, sf.NumShapes, "Number of shapes are incorrect");
                Assert.AreEqual(fieldIndex + 1, sf.NumFields, "Number of fields are incorrect");

            }
            finally
            {
                // Close the shapefile:
                result = sf.Close();
                Assert.IsTrue(result, "Could not close shapefile");
            }
        }

        /// <summary>
        /// Checks the null value table data.
        /// </summary>
        /// <remarks>https://mapwindow.atlassian.net/projects/CORE/issues/CORE-177</remarks>
        [TestMethod]
        public void CheckNullValueTableData()
        {
            bool result;
            var sf = new Shapefile { GlobalCallback = this };

            try
            {
                // Create in-memory shapefile
                result = sf.CreateNewWithShapeID(string.Empty, ShpfileType.SHP_POINT);
                Assert.IsTrue(result, "Could not create shapefile");

                // Add fields:
                var fieldIndex = sf.Table.EditAddField("string", FieldType.STRING_FIELD, 0, 50);
                Assert.AreEqual(1, fieldIndex, "Could not add field");

                fieldIndex = sf.Table.EditAddField("integer", FieldType.INTEGER_FIELD, 0, 50);
                Assert.AreEqual(2, fieldIndex, "Could not add field");

                fieldIndex = sf.Table.EditAddField("double", FieldType.DOUBLE_FIELD, 2, 50);
                Assert.AreEqual(3, fieldIndex, "Could not add field");

                fieldIndex = sf.Table.EditAddField("double", FieldType.DOUBLE_FIELD, 0, 50);
                Assert.AreEqual(-1, fieldIndex, "Field was added. This is not correct.");

                Assert.AreEqual(4, sf.NumFields, "Wrong number of fields");

                // Create shape:
                var shp = new Shape();
                result = shp.Create(ShpfileType.SHP_POINT);
                Assert.IsTrue(result, "Could not create point shape");
                // Create point:
                var pnt = new Point
                {
                    x = 200,
                    y = 200
                };
                // Add point:
                var pointIndex = shp.numPoints;
                result = shp.InsertPoint(pnt, ref pointIndex);
                Assert.IsTrue(result, "Could not insert point");
                var shapeIndex = sf.NumShapes;
                result = sf.EditInsertShape(shp, ref shapeIndex);
                Assert.IsTrue(result, "Could not insert shape");

                // Read attribute data, skip the first because that is the ID which always has a value
                for (var i = 1; i < sf.NumFields; i++)
                {
                    var value = sf.CellValue[i, 0];
                    var field = sf.Field[i];
                    Console.WriteLine($"Is the value of fieldId {i} NULL: {value == null} Type of field is {field.Type}");
                    // Assert.IsNull(value, $"Value with fieldId {i} is not null, but is '{value}'");
                }
            }
            finally
            {
                // Close the shapefile:
                result = sf.Close();
                Assert.IsTrue(result, "Could not close shapefile");
            }

        }

        /// <summary>
        /// Adds the field.
        /// </summary>
        /// <remarks>https://mapwindow.atlassian.net/browse/MWGIS-55</remarks>
        [TestMethod]
        public void AddField()
        {
            bool result;
            var sf = new Shapefile { GlobalCallback = this };

            try
            {
                // Create in-memory shapefile
                result = sf.CreateNewWithShapeID(string.Empty, ShpfileType.SHP_POINT);
                Assert.IsTrue(result, "Could not create shapefile");

                Assert.IsTrue(sf.EditingShapes, "Shapefile is not in edit shapes mode");
                Assert.IsTrue(sf.EditingTable, "Shapefile is not in edit table mode");

                // Add fields:
                Assert.IsTrue(sf.Table.EditingTable, "Table is not in edit table mode");
                // This should work:
                var fieldIndex = sf.Table.EditAddField("date", FieldType.STRING_FIELD, 0, 50);
                Assert.AreEqual(sf.NumFields - 1, fieldIndex, "Could not add string field");
                // This should work:
                fieldIndex = sf.Table.EditAddField("double", FieldType.DOUBLE_FIELD, 10, 20);
                Assert.AreEqual(sf.NumFields - 1, fieldIndex, "Could not add double field");

                // This should fail, because width cannot be 0:
                fieldIndex = sf.Table.EditAddField("date", FieldType.STRING_FIELD, 50, 0);
                Assert.AreEqual(-1, fieldIndex, "Field is not added but -1 is not returned. ");
                Console.WriteLine("Expected error: " + sf.Table.ErrorMsg[sf.Table.LastErrorCode]);

                // This should fail, because precsion cannot be 0 when type is double:
                fieldIndex = sf.Table.EditAddField("date", FieldType.DOUBLE_FIELD, 0, 20);
                Assert.AreEqual(-1, fieldIndex, "Field is not added but -1 is not returned. ");
                Console.WriteLine("Expected error: " + sf.Table.ErrorMsg[sf.Table.LastErrorCode]);
            }
            finally
            {
                // Close the shapefile:
                result = sf.Close();
                Assert.IsTrue(result, "Could not close shapefile");
            }
        }

        [TestMethod]
        public void FixUpShapes()
        {
            // MWGIS-90
            // Open shapefile:
            var sfInvalid = new Shapefile { GlobalCallback = this };
            Shapefile sfFixed = null;
            try
            {
                var result = sfInvalid.Open(@"sf\invalid.shp");
                Assert.IsTrue(result, "Could not open shapefile");

                Assert.IsTrue(sfInvalid.HasInvalidShapes(), "Shapefile has no invalid shapes");
                Helper.PrintExtents(sfInvalid.Extents);

                result = sfInvalid.FixUpShapes(out sfFixed);
                Assert.IsTrue(result, "Could not fix shapefile");
                Assert.IsFalse(sfFixed.HasInvalidShapes(), "Returning shapefile has invalid shapes");

                Assert.AreEqual(sfInvalid.NumShapes, sfFixed.NumShapes, "Number of shapes are not equal");
                Helper.PrintExtents(sfFixed.Extents);
            }
            finally
            {
                sfInvalid.Close();
                sfFixed?.Close();
            }
        }

        // Missing data: [TestMethod]
        /*
        private void Reproject2281Test()
        {
            var sf = new Shapefile {GlobalCallback = this};
            const string filename = @"Issues/MWGIS-91/utah_central_arcs.shp"; // In NAD83 / Utah Central (ft), EPSG:2281
            if (!sf.Open(filename)) Assert.Fail("Could not open shapefile: " + sf.ErrorMsg[sf.LastErrorCode]);
            Assert.IsTrue(sf.NumShapes == 1, "Unexpected number of shapes in " + filename);
            Console.WriteLine(sf.GeoProjection.ProjectionName);
            Helper.PrintExtents(sf.Extents);

            var proj = new GeoProjection();
            proj.ImportFromEPSG(32612); // WGS 84 / UTM zone 12N
            var numShps = 0;
            var reprojectedSf = sf.Reproject(proj, numShps);
            Assert.IsTrue(numShps > 0, "Nothing is reprojected. Error: " + sf.ErrorMsg[sf.LastErrorCode]);
            Assert.IsNotNull(reprojectedSf, "reprojectedSf == null");
            Assert.AreEqual(sf.NumShapes, reprojectedSf.NumShapes);
            Helper.PrintExtents(reprojectedSf.Extents);

            Helper.SaveAsShapefile(reprojectedSf, Path.ChangeExtension(filename, ".WGS84-UTM12N.shp"));
        }
        */

        [TestMethod]
        public void Reproject2280Test()
        {
            var sf = new Shapefile { GlobalCallback = this };
            const string filename = @"Issues/MWGIS-91/utah_north_arcs.shp"; // In NAD83 / Utah North (ft), EPSG:2280
            if (!sf.Open(filename)) Assert.Fail("Could not open shapefile: " + sf.ErrorMsg[sf.LastErrorCode]);
            Assert.IsTrue(sf.NumShapes == 1, "Unexpected number of shapes in " + filename);
            Console.WriteLine(sf.GeoProjection.ProjectionName);
            Helper.PrintExtents(sf.Extents);

            var proj = new GeoProjection();
            proj.ImportFromEPSG(32612); // WGS 84 / UTM zone 12N
            var numShps = 0;
            var reprojectedSf = sf.Reproject(proj, ref numShps);
            Assert.IsTrue(numShps > 0, "Nothing is reprojected. Error: " + sf.ErrorMsg[sf.LastErrorCode]);
            Assert.IsNotNull(reprojectedSf, "reprojectedSf == null. Error: " + sf.ErrorMsg[sf.LastErrorCode]);
            Assert.AreEqual(sf.NumShapes, reprojectedSf.NumShapes);
            Helper.PrintExtents(reprojectedSf.Extents);

            Helper.SaveAsShapefile(reprojectedSf, Path.Combine(Path.GetTempPath(), "Reproject2280Test.shp"));

            Assert.AreNotEqual(Math.Round(sf.Extents.xMin, MidpointRounding.AwayFromZero),
                Math.Round(reprojectedSf.Extents.xMin, MidpointRounding.AwayFromZero), "xMin are the same, no projection has happened.");
        }

        [TestMethod]
        public void CreateFishnet()
        {
            // Create shape from WKT:
            var shp = new Shape();
            if (!shp.ImportFromWKT(
                "POLYGON ((330918.422383554 5914432.9952417,330791.425601288 5914677.56286955,330799.804294765 5914682.67199867,330809.295198231 5914690.83057468,330851.753425698 5914726.8399904,330890.161005985 5914760.37492299,330891.883975456 5914761.87973075,330894.499450693 5914766.14773636,330895.001406323 5914766.9673645,330894.821345632 5914768.00066471,330895.626814712 5914772.6474656,330898.544123647 5914779.26299206,331042.140051675 5914906.23861184,331066.955908721 5914928.22692301,331071.290848669 5914932.66233604,331075.531881961 5914935.23930972,331086.549669788 5914904.76350951,331104.67722032 5914852.28308518,331120.597430814 5914804.83997655,331131.133792741 5914775.21848511,331118.884180716 5914770.93369604,331091.649916887 5914758.80097565,331072.712088731 5914748.36652613,331052.802159239 5914734.4060014,331043.093305417 5914725.75786093,331036.001117512 5914716.44788158,331028.749419581 5914706.61634015,331024.121040336 5914698.88986961,331020.742433359 5914692.31867847,331016.329278393 5914678.37004658,331011.623099594 5914661.25749118,331005.798813818 5914627.24754255,331002.264592162 5914601.86413354,330997.932682632 5914565.92459662,330994.902438802 5914545.28431325,330991.611136112 5914516.84204983,330989.261968268 5914496.56381567,330984.441627117 5914474.55626726,330974.218375295 5914440.74529109,330918.422383554 5914432.9952417))"))
                Assert.Fail("Could not create shape from wkt: " + shp.ErrorMsg[shp.LastErrorCode]);

            // Create fishnet for bounds of shape:
            var sf = Helper.CreateFishnet(shp.Extents, 20, 20);
            Helper.SaveAsShapefile(sf, Path.Combine(Path.GetTempPath(), "CreateFishnet.shp"));
        }


        [TestMethod]
        public void SaveAs()
        {
            var filename = @"D:\dev\GIS-Data\MapWindow-Projects\UnitedStates\Shapefiles\states.shp";

            // Check file:
            if (!File.Exists(filename)) Assert.Fail(filename + " does not exists.");
            // Open shapefile:
            var sf = new Shapefile { GlobalCallback = this };
            if (!sf.Open(filename))
                Assert.Fail("Failed to open shapefile: " + sf.ErrorMsg[sf.LastErrorCode]);

            // Save shapefile:
            var tmpFilename = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetTempFileName()), ".shp");
            if (!sf.SaveAs(tmpFilename))
                Assert.Fail("Failed to save shapefile: " + sf.ErrorMsg[sf.LastErrorCode]);
        }

        /// <summary>
        /// Merges the sf.
        /// </summary>
        /// <remarks>MWGIS-69</remarks>
        [TestMethod]
        public void MergeSf()
        {
            const string sf3Location = @"Issues\MWGIS-69\SHP3_POINT.shp";
            const string sf4Location = @"Issues\MWGIS-69\SHP4_POINT.shp";

            var sf3 = new Shapefile { GlobalCallback = this };
            if (!sf3.Open(sf3Location)) Assert.Fail("Can't open " + sf3Location + " Error: " + sf3.ErrorMsg[sf3.LastErrorCode]);

            var sf4 = new Shapefile { GlobalCallback = this };
            if (!sf4.Open(sf4Location)) Assert.Fail("Can't open " + sf4Location + " Error: " + sf4.ErrorMsg[sf4.LastErrorCode]);

            var sfMerged = sf3.Merge(false, sf4, false);
            Assert.IsNotNull(sfMerged, "Merge failed. Error: " + sf3.ErrorMsg[sf3.LastErrorCode]);
            Assert.AreEqual(2, sfMerged.NumShapes, "Incorrect number of shapes");
            Helper.SaveAsShapefile(sfMerged, Path.Combine(Path.GetTempPath(), "MergeSf.shp"));
        }

        /// <summary>
        /// Merges the M shapefiles
        /// </summary>
        /// <remarks>MWGIS-69</remarks>
        [TestMethod]
        public void MergeM()
        {
            const string sf1Location = @"Issues\MWGIS-69\SHP1_POINT_M.shp";
            const string sf2Location = @"Issues\MWGIS-69\SHP2_POINT_M.shp";

            var sf1 = new Shapefile { GlobalCallback = this };
            if (!sf1.Open(sf1Location)) Assert.Fail("Can't open " + sf1Location + " Error: " + sf1.ErrorMsg[sf1.LastErrorCode]);
            Console.WriteLine("num shapes in sf1: " + sf1.NumShapes);

            var sf2 = new Shapefile { GlobalCallback = this };
            if (!sf2.Open(sf2Location)) Assert.Fail("Can't open " + sf2Location + " Error: " + sf2.ErrorMsg[sf2.LastErrorCode]);
            Console.WriteLine("num shapes in sf2: " + sf2.NumShapes);

            Console.WriteLine("Before merge");
            var sfMerged = sf1.Merge(false, sf2, false);
            Assert.IsNotNull(sfMerged, "Merge failed. Error: " + sf1.ErrorMsg[sf1.LastErrorCode]);
            Assert.AreEqual(2, sfMerged.NumShapes, "Incorrect number of shapes");
            Helper.SaveAsShapefile(sfMerged, Path.Combine(Path.GetTempPath(), "MergeM.shp"));
        }

        [TestMethod]
        public void LoadAmericanData()
        {
            const string sfLocation = @"D:\dev\GIS-Data\MapWindow-Projects\UnitedStates\Shapefiles\states.shp";

            var sf = new Shapefile { GlobalCallback = this };
            if (!sf.Open(sfLocation)) Assert.Fail("Can't open " + sfLocation + " Error: " + sf.ErrorMsg[sf.LastErrorCode]);

            var value = sf.CellValue[1, 0] as string;
            sf.Close();
            Console.WriteLine(value);
            Assert.IsNotNull(value, "CellValue failed");
            // Value should be Washington
            Assert.AreEqual("washington", value.ToLower());
        }

        [TestMethod]
        public void ReadRussianDataFromTable()
        {
            const string sfLocation = @"Issues\MWGIS-72\point.shp";
            const int fieldIndex = 2;

            var sf = new Shapefile { GlobalCallback = this };
            if (!sf.Open(sfLocation))
                Assert.Fail("Can't open " + sfLocation + " Error: " + sf.ErrorMsg[sf.LastErrorCode]);

            var value = sf.CellValue[fieldIndex, 0] as string;
            Assert.IsNotNull(value, "No value returned");
            sf.Close();
            Console.WriteLine(value);
            // Value should be Воздух
            Assert.AreEqual('д', value[3]);
        }

        [TestMethod]
        public void CreateRussianCategories()
        {
            const string sfLocation = @"Issues\MWGIS-72\point.shp";
            const int fieldIndex = 2;

            var sf = new Shapefile { GlobalCallback = this };
            if (!sf.Open(sfLocation))
                Assert.Fail("Can't open " + sfLocation + " Error: " + sf.ErrorMsg[sf.LastErrorCode]);

            // create visualization categories
            sf.DefaultDrawingOptions.FillType = tkFillType.ftStandard;
            sf.Categories.Generate(fieldIndex, tkClassificationType.ctUniqueValues, 0);
            sf.Categories.ApplyExpressions();

            // apply color scheme
            var scheme = new ColorScheme();
            scheme.SetColors2(tkMapColor.LightBlue, tkMapColor.LightYellow);
            sf.Categories.ApplyColorScheme(tkColorSchemeType.ctSchemeGraduated, scheme);
            Assert.IsTrue(sf.Categories.Count > 0, "No categories were made");

            var cat = sf.Categories.Item[0];
            Console.WriteLine(cat.Name);
            Assert.AreNotEqual(cat.Name[0], '?', "The category name is invalid");
        }

        [TestMethod]
        public void PointInShapefile()
        {
            // It goes too fast for DotMemory:
            Thread.Sleep(2000);

            const string folder = @"D:\dev\GIS-Data\Issues\Point in Polygon";
            Assert.IsTrue(Directory.Exists(folder), "Input folder doesn't exists");
            var sfPolygons = new Shapefile { GlobalCallback = this };
            var sfPoints = new Shapefile { GlobalCallback = this };
            var found = 0;
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                var retVal = sfPolygons.Open(Path.Combine(folder, "CatchmentBuilderShapefile.shp"));
                Assert.IsTrue(retVal, "Can't open polygon shapefile");

                retVal = sfPoints.Open(Path.Combine(folder, "Sbk_FGrPt_n.shp"));
                Assert.IsTrue(retVal, "Can't open point shapefile");

                // Caches the coordinates of shapefile points for faster point in shape test:
                retVal = sfPolygons.BeginPointInShapefile();
                Assert.IsTrue(retVal, "Can't cache points");

                var numPoints = sfPoints.NumShapes;
                Assert.IsTrue(numPoints > 0, "No point shapes in shapefile");

                for (var i = 0; i < numPoints; i++)
                {
                    var pointShape = sfPoints.Shape[i];
                    Assert.IsNotNull(pointShape, "pointShape == null");

                    double x = 0d, y = 0d;
                    retVal = pointShape.XY[0, ref x, ref y];
                    Assert.IsTrue(retVal, "Can't get XY from first point");

                    // Returns a number which indicates the index of shapes within which a test point is situated:
                    var shapeIndex = sfPolygons.PointInShapefile(x, y);
                    Console.WriteLine($"Point {i} lies within polygon {shapeIndex}");
                    found++;
                }
            }
            finally
            {
                // Clear cache:
                sfPolygons.EndPointInShapefile();

                // Close shapefiles:
                sfPolygons.Close();
                sfPoints.Close();
            }

            stopWatch.Stop();
            Console.WriteLine("The process took " + stopWatch.Elapsed);
            Console.WriteLine(found + " matching polygons where found");

        }

        public void Progress(string KeyOfSender, int Percent, string Message)
        {
            Console.Write(".");
        }

        public void Error(string KeyOfSender, string ErrorMsg)
        {
            Assert.Fail("Found error: " + ErrorMsg);
        }
    }
}
