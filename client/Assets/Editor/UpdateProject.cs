using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EastSideGames.ThirdParty.SonarCloud
{
    [InitializeOnLoad]
    public static class UpdateProject
    {
        //DockerPath must be D:/a/<repo name>/<repo name>
        private const string DockerPath = @"D:/a/sonarcloud-unity-action/sonarcloud-unity-action";
        
        static UpdateProject()
        {
            EditorApplication.projectChanged += OnProjectChanged;
            OnProjectChanged();
        }

        private static void OnProjectChanged()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(".");
            var filesInfo = directoryInfo.GetFiles();

            Directory.CreateDirectory("SonarAssemblies");

            foreach (var fileInfo in filesInfo)
            {
                string fileName = fileInfo.Name;
                
                if (fileName.EndsWith(".csproj") && !fileName.Contains("-Sonar"))
                {
                    UpdateProjectFile(fileInfo, directoryInfo.Name);
                }
                else if (fileName.EndsWith(".sln"))
                {
                    UpdateSolutionFile(fileInfo, directoryInfo.Name);
                }
            }
        }

        private static void UpdateProjectFile(FileInfo fileInfo, string directoryName)
        {
            string fileName = fileInfo.Name;
            string directoryPath = $"/{directoryName}/";
            string readLine;
            
            StreamReader streamReader = fileInfo.OpenText();
            StringBuilder outputString = new StringBuilder();
       
            do
            {
                readLine = streamReader.ReadLine();

                if (readLine != null)
                {
                    if (readLine.Contains("<SonarQubeTestProject>"))
                    {
                        break;
                    }
                    
                    if (readLine.StartsWith("<Project "))
                    {
                        // Without this property, Sonar will only analyze the emitted code
                        // with the Roslyn ruleset, which is only appropriate for test projects
                        // and will produce unusable results.
                        outputString.AppendLine(readLine);
                        outputString.AppendLine("\t<PropertyGroup>");
                        outputString.AppendLine("\t\t<SonarQubeTestProject>false</SonarQubeTestProject>");
                        outputString.AppendLine("\t</PropertyGroup>");
                    }
                    else if (readLine.Contains("<HintPath>"))
                    {
                        outputString.AppendLine(FixHintPath(readLine, directoryPath));
                    }
                    else if (readLine.Contains(".csproj"))
                    {
                        outputString.AppendLine(readLine.Replace(".csproj", "-Sonar.csproj"));
                    }
                    else if (readLine.Contains("AssemblyName"))
                    {
                        outputString.AppendLine(readLine.Replace("</AssemblyName>", "-Sonar</AssemblyName>"));
                    }
                    else
                    {
                        outputString.AppendLine(readLine);
                    }
                }
            } while (readLine != null);
            
            streamReader.Close();
            streamReader.Dispose();

            File.WriteAllText(fileName.Replace(".csproj", "-Sonar.csproj"), outputString.ToString());
        }

        private static string FixHintPath(string readLine, string directoryPath)
        {
            string hintPath = "";
            string pathString;
            int pathIndex;

            if (readLine.Contains("PlaybackEngines"))
            {
                // PlaybackEngines won't impact builds and can be left unmodified
                hintPath = readLine;
            }
            else if (readLine.Contains("Unity/Hub/Editor"))
            {
                            
                pathIndex = readLine.IndexOf("Editor/Data/", StringComparison.CurrentCulture);
                if (pathIndex > 0)
                {
                    pathIndex += "Editor/Data/".Length;
                }
                else
                {
                    pathIndex = readLine.IndexOf("Unity.app/Contents/", StringComparison.CurrentCulture) + "Unity.app/Contents/".Length;
                }

                pathString = readLine.Substring(pathIndex, readLine.Length - pathIndex);
                hintPath =
                    $"\t\t\t<HintPath>C:/Program Files/Unity/Hub/Editor/{Application.unityVersion}/Editor/Data/{pathString}";
            }
            else if (readLine.Contains(directoryPath))
            {
                // if the .dll is in the Library/PackageCache or Library/ScriptAssemblies
                // folder, copy it to the SonarAssemblies folder
                if (readLine.Contains("PackageCache") || readLine.Contains("ScriptAssemblies"))
                {
                    int fileIndex = readLine.IndexOf("<HintPath>", StringComparison.CurrentCulture) + "<HintPath>".Length;
                    string filePath = readLine.Substring(fileIndex, readLine.Length - fileIndex - "</HintPath>".Length);
                    fileIndex = filePath.LastIndexOf("/", StringComparison.CurrentCulture);
                    string fileName = filePath.Substring(fileIndex, filePath.Length - fileIndex);
                    File.Copy(filePath, "SonarAssemblies" + fileName, true);
                    readLine = directoryPath + "SonarAssemblies/" + fileName + "</HintPath>";
                }
                pathIndex = readLine.IndexOf(directoryPath, StringComparison.CurrentCulture) + directoryPath.Length;
                pathString = readLine.Substring(pathIndex, readLine.Length - pathIndex);
                hintPath = $"\t\t\t<HintPath>{DockerPath}{directoryPath}{pathString}";
            }

            return hintPath;
        }
        
        private static void UpdateSolutionFile(FileInfo fileInfo, string directoryName)
        {
            string fileName = fileInfo.Name;
            StreamReader streamReader = fileInfo.OpenText();
            
            string readLine;
            StringBuilder outputString = new StringBuilder();
       
            do
            {
                readLine = streamReader.ReadLine();

                if (readLine != null)
                {
                    if (readLine.Contains(".csproj"))
                    {
                        readLine = readLine.Replace(".csproj", "-Sonar.csproj");
                    }

                    if (readLine.StartsWith("Project("))
                    {
                        int assemblyIndex = readLine.IndexOf(@" = """, StringComparison.CurrentCulture) + @" = """.Length;
                        int assemblyEnd = readLine.IndexOf(@"""", assemblyIndex, StringComparison.CurrentCulture);
                        readLine = readLine.Insert(assemblyEnd, "-Sonar");
                        int pathIndex = readLine.IndexOf(@", """, assemblyEnd, StringComparison.CurrentCulture) + @", """.Length;
                        readLine = readLine.Insert(pathIndex, directoryName + "/");
                        outputString.AppendLine(readLine);
                    }
                    else
                    {
                        outputString.AppendLine(readLine);
                    }
                }
            } while (readLine != null);
            
            streamReader.Close();
            streamReader.Dispose();

            File.WriteAllText("../" + fileName, outputString.ToString());
        }
    }
}

