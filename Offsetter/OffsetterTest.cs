using Offsetter.Entities;
using Offsetter.Math;
using Offsetter.Solver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Offsetter
{
    using GChainList = List<GChain>;

    public partial class Offsetter : Form
    {
        private void Test()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                string path = Properties.Settings.Default.testPath;
                if (path == string.Empty)
                    path = @"c:\";

                if (Path.GetExtension(path) == ".txt")
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(path);
                    dialog.FileName = Path.GetFileName(path);
                }
                else
                {
                    dialog.InitialDirectory = path;
                }

                dialog.Filter = "test files (*.txt)|*.txt";
                dialog.AddExtension = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default.testPath = dialog.FileName;
                    Properties.Settings.Default.Save();

                    TestsExecute(dialog.FileName);
                }
            }
        }

        // These exist to simplify/clarify code in TestsExecute().
        private GChain Part { get { return ichains[0]; } }
        private GChain Tool { get { return ichains[1]; } }

        private void TestsExecute(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            GLogger.Active = true;

            string defDxfFolder = AppDomain.CurrentDomain.BaseDirectory;
            int indx = defDxfFolder.IndexOf(@"\bin\");
            if (indx > 0)
            {
                defDxfFolder = defDxfFolder.Substring(0, indx);
                indx = defDxfFolder.LastIndexOf(@"\");
                if (indx > 0)
                    defDxfFolder = defDxfFolder.Substring(0, indx) + @"\data";
            }

            string[] buffer = File.ReadAllLines(filePath);
            foreach (string statement in buffer)
            {
                List<TestParameter> testParams;
                string dxfPath = TestParse(defDxfFolder, statement, out testParams);
                if (dxfPath.Length < 1)
                    continue;

                // TODO: DxfRead() should return bool (success, failure)
                DxfRead(dxfPath);
                ViewBase();
                Render();

                GChainList results = new GChainList();
                foreach (TestParameter param in testParams)
                {
                    try
                    {
                        if (param.Test == GConst.UNIFORM)
                        {
                            GUniformOffsetter offsetter = new GUniformOffsetter();
                            offsetter.Offset(ichains, param.Side, param.Dist, results);
                        }
                        else if (param.Test == GConst.NONUNIFORM)
                        {
                            GNonUniformOffseter ch = new GNonUniformOffseter(false);
                            ch.Offset(Part, Tool, param.Side, param.Dist, results);
                        }
                        else if (param.Test == GConst.NEST)
                        {
                            GNonUniformOffseter ch = new GNonUniformOffseter(true);
                            ch.Offset(Part, Tool, param.Side, param.Dist, results);
                        }

                        foreach (GChain chain in results)
                        { chain.Log("result", null); }
                    }
                    catch (Exception ex)
                    {
                        GLogger.Log(ex.Message);
                        if (ex.StackTrace != null)
                            GLogger.Log(ex.StackTrace);
                    }

                    ResultsCollate(results);
                    PreviewKeyEvent(Keys.F);

                    string resultPath = LogWrite(dxfPath, param);
                    LogsDiff(resultPath);

                    GC.Collect();
                }
            }

            GLogger.Active = false;
        }

        private string LogWrite(string dxfPath, TestParameter param)
        {
            if (!GLogger.Active)
                return string.Empty;

            string resultFolder = AppDomain.CurrentDomain.BaseDirectory;
            int indx = resultFolder.IndexOf(@"\bin\");
            if (indx > 0)
            {
                resultFolder = resultFolder.Substring(0, indx);
                indx = resultFolder.LastIndexOf(@"\");
                if (indx > 0)
                    resultFolder = resultFolder.Substring(0, indx) + @"\test\results\";
            }

            string dxfName = Path.GetFileNameWithoutExtension(dxfPath);
            string logPath = string.Format("{0}{1}{2}{3}.log", resultFolder, dxfName, ((param.Side == GConst.LEFT) ? "L" : "R"), param.Dist);

            GLogger.LogFileCreate(logPath);
            GLogger.Clear();

            return logPath;
        }

        private void LogsDiff(string resultPath)
        {
            if (resultPath == string.Empty)
                return;

            string baselinePath = resultPath.Replace(@"\results\", @"\baseline\");
            string diffPath = Path.ChangeExtension(resultPath, ".diff");
            if (File.Exists(diffPath))
                File.Delete(diffPath);

            if (File.Exists(baselinePath))
            {
                // From https://stackoverflow.com/questions/61296870/system-diagnostics-process-start-arguments-dotnet-and-diff
                Process process = new Process();
                process.StartInfo.FileName = "diff";
                process.StartInfo.Arguments = string.Format("{0} {1}", baselinePath, resultPath);
                process.StartInfo.RedirectStandardOutput = true;
                // process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();

                using (var outputFile = File.OpenWrite(diffPath))
                {
                    process.StandardOutput.BaseStream.CopyTo(outputFile);
                }

                process.WaitForExit();// Waits here for the process to exit.

                try
                {
                    if (new FileInfo(diffPath).Length == 0)
                        File.Delete(diffPath);
                }
                catch { }
            }
            else
            {
                using (StreamWriter fs = new StreamWriter(diffPath))
                {
                    fs.WriteLine("No baseline found.");
                }
            }
        }

        // Each statement in the test file is of the form:
        //    statement   ::= dxf_file = params
        //    dxf_file    ::= fully qualified path to a .dxf (not usual) | just a .dxf file name
        //    params      ::= params, param | param
        //    param       ::= offset_dir offset_dist
        //    offset_dir  ::= L | R   (e.g. left or right)
        //    offset_dist ::= a floating point value
        private string TestParse(string defDxfFolder, string statement, out List<TestParameter> testParams)
        {
            string cmd = statement.Replace(" ", "");

            testParams = new List<TestParameter>();

            if (cmd.StartsWith('/'))
                return string.Empty;

            int indx = cmd.IndexOf('=');
            if (indx < 0)
                return string.Empty;

            string testPath = cmd.Substring(0, indx);
            string folder = Path.GetDirectoryName(testPath)!;
            if ((folder == null) || (folder.Length == 0))
                testPath = Path.Combine(defDxfFolder, testPath);

            cmd = cmd.Substring(indx + 1);
            string[] tokens = cmd.Split(',');
            if (tokens.Length < 1)
                return string.Empty;

            int side = 0;
            double dist = 0;
            foreach (string token in tokens)
            {
                indx = 0;
                int testType;
                switch (token[indx])
                {
                    case 'C': testType = GConst.NONUNIFORM; break;
                    case 'N': testType = GConst.NEST; break;
                    default: testType = GConst.UNIFORM; break;
                }

                if (testType != GConst.UNIFORM)
                    ++indx;

                if (token[indx] == 'L')
                    side = GConst.LEFT;
                else if (token[indx] == 'R')
                    side = GConst.RIGHT;
                else
                    return string.Empty;

                if (!double.TryParse(token.Substring(indx + 1), out dist))
                    return string.Empty;

                testParams.Add(new TestParameter(testType, side, dist));
            }

            return testPath;
        }
    }

    internal class TestParameter
    {
        public TestParameter(int test, int side, double dist)
        {
            Test = test;
            Side = side;
            Dist = dist;
        }

        public int Test { get; set; }
        public int Side { get; set; }
        public double Dist { get; set; }
    }
}
