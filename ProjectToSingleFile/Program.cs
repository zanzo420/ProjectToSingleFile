using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace ProjectToSingleFile
{
    class Program
    {
        public static void Main(string[] args)
        {
            // parse command line

            if(args == null || args.Length == 0)
            {
                Console.WriteLine("Wrong argument (1)! Press enter to exit");

                Console.ReadLine();

                return;
            }

            string projectPath = args[0];

            if(!projectPath.EndsWith(".sln") && !projectPath.EndsWith(".csproj"))
            {
                Console.WriteLine("Wrong argument (2)! Press enter to exit");

                Console.ReadLine();

                return;
            }

            string resultFileName = Path.GetFileNameWithoutExtension(projectPath);

            if(resultFileName.Contains(" "))
            {
                resultFileName = resultFileName.Replace(" ", "");
            }

            resultFileName += ".Include.cs";

            string rootDirectory = Path.GetDirectoryName(projectPath);

            // get a file list

            string[] tmpFileList = Directory.GetFiles(rootDirectory, "*.cs", SearchOption.AllDirectories);

            List<string> csFiles = new List<string>(tmpFileList.Length);

            // filter

            foreach(var fileName in tmpFileList)
            {
                if (fileName.EndsWith("AssemblyInfo.cs")
                    || fileName.Contains("TemporaryGeneratedFile")
                    || fileName.Contains(".g.i.cs")) continue;

                csFiles.Add(fileName);
            }

            // read them all

            List<string[]> fileLines = new List<string[]>(csFiles.Count);

            foreach(var file in csFiles)
            {
                fileLines.Add(File.ReadAllLines(file));
            }

            // find all usings

            List<string> usings = new List<string>();

            for(int x = 0; x < fileLines.Count; x++)
            {
                for(int y = 0; y < fileLines[x].Length; y++)
                {
                    if(fileLines[x][y].Contains("using ") && fileLines[x][y].EndsWith(";"))
                    {
                        // add to our list

                        int start = fileLines[x][y].IndexOf("using ");
                        int end = fileLines[x][y].Length - start;

                        usings.Add(fileLines[x][y].Substring(start, end));

                        // filter them

                        fileLines[x][y] = string.Empty;
                    }
                }
            }

            // remove duplicates

            var distinctUsings = usings.Distinct();

            // write usings into a buffer

            StringBuilder sb = new StringBuilder();

            foreach (var u in distinctUsings)
                sb.AppendLine(u);

            // write files into a buffer

            foreach(var content in fileLines)
            {
                foreach(var line in content)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith(Environment.NewLine)) continue;

                    sb.AppendLine(line);
                }
            }

            // write to file

            string outputFilePath = Path.Combine(rootDirectory, resultFileName);

            if (File.Exists(outputFilePath)) File.Delete(outputFilePath);

            File.WriteAllText(outputFilePath, sb.ToString());
        }
    }
}