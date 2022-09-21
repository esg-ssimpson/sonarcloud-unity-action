using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EastSideGames.Utility
{
    [InitializeOnLoad]
    public class UpdateSonar
    {
        //private const string DockerPath = @"D:/a/trailerparkboys/trailerparkboys";
        private const string DockerPath = @"D:/a/sonarcloud-unity-action/sonarcloud-unity-action";
        
        static UpdateSonar()
        {
            Debug.Log("Executed editor script... Unity editor version: " + Application.unityVersion);
            EditorApplication.projectChanged += OnProjectChanged;
            OnProjectChanged();
        }

        private static void OnProjectChanged()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(".");
            Debug.Log("Directory: " + directoryInfo.Name);
            var filesInfo = directoryInfo.GetFiles();

            //Directory.CreateDirectory("sonarproj");

            foreach (var fileInfo in filesInfo)
            {
                string fileName = fileInfo.Name;

                // We don't need to update any project files that are Editor projects,
                // since they won't be built by MSBuild during analysis 
                //if (fileName.EndsWith(".csproj") && !fileName.Contains("Editor"))
                if (fileName == "Assembly-CSharp.csproj")
                {
                    UpdateProjectFile(fileInfo, directoryInfo.Name);
                }
                else if (fileName.EndsWith(".sln"))
                {
                    //ToDo
                }
            }
        }

        private static void UpdateProjectFile(FileInfo fileInfo, string directoryName)
        {
            string fileName = fileInfo.Name;
            string directoryPath = $"/{directoryName}/";
            
            Debug.Log($"[UpdateSonar::UpdateProjectFile] Updating project '{fileName}'");
            StreamReader streamReader = fileInfo.OpenText();
            
            string readLine;
            StringBuilder outputString = new StringBuilder();
       
            do
            {
                readLine = streamReader.ReadLine();

                if (readLine != null)
                {
                    if (readLine.Contains("<SonarQubeTestProject>"))
                    {
                        Debug.Log("[UpdateSonar::UpdateProjectFile] Project file already up-to-date, skipping");
                        break;
                    }
                    
                    if (readLine.Contains("<Project "))
                    {
                        outputString.AppendLine(readLine);
                        
                        // Only the Assembly-CSharp needs to be marked as not a test project
                        // All other projects can be safely ignored during analysis
                        if (fileName == "Assembly-CSharp.csproj")
                        {
                            outputString.AppendLine("\t<PropertyGroup>");
                            outputString.AppendLine("\t\t<SonarQubeTestProject>false</SonarQubeTestProject>");
                            outputString.AppendLine("\t</PropertyGroup>");
                        }
                           
                        // outputString.AppendLine("\t<PropertyGroup>");
                        // outputString.AppendLine(
                        //     $"<ReferencePath>C:\\Program Files\\Unity\\Hub\\Editor\\{Application.unityVersion}\\Editor\\Data\\Managed;C:\\Program Files\\Unity\\Hub\\Editor\\{Application.unityVersion}\\Editor\\Data\\Managed\\UnityEngine;$(ReferencePath)</ReferencePath>");
                        // outputString.AppendLine("\t</PropertyGroup>");
                    }
                    // else if (readLine.Contains("<Compile Include=\""))
                    // {
                    //     outputString.AppendLine(ProcessCompileInclude(readLine));
                    // }
                    else if (readLine.Contains("<HintPath>"))
                    {
                        outputString.AppendLine(ProcessHintPath(readLine, directoryPath));
                    }
                    else if (readLine.Contains("AssemblyName"))
                    {
                        outputString.AppendLine(readLine.Replace("Assembly-CSharp", "Assembly-Sonar"));
                    }
                    else
                    {
                        outputString.AppendLine(readLine);
                    }
                }
            } while (readLine != null);
            
            streamReader.Close();
            streamReader.Dispose();

            // We should only overwrite the file with the new contents if readLine is null,
            // otherwise we found duplicate ReferencePath values and do not need to update
            if (readLine == null)
            {
                //File.WriteAllText(@"sonarproj/" + fileName, outputString.ToString());
                File.WriteAllText("Assembly-Sonar.csproj", outputString.ToString());
            }
        }

        private static string ProcessCompileInclude(string readLine)
        {
            string compileInclude = "";
            string pathString;
            int pathIndex;

            pathIndex = readLine.IndexOf("<Compile Include=\"") + "<Compile Include=\"".Length;
            pathString = readLine.Substring(pathIndex, readLine.Length - pathIndex);
            compileInclude = $"\t\t\t<Compile Include=\"..\\{pathString}";
            
            return compileInclude;
        }

        private static string ProcessHintPath(string readLine, string directoryPath)
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
                            
                pathIndex = readLine.IndexOf("Editor/Data/");
                if (pathIndex > 0)
                {
                    pathIndex += "Editor/Data/".Length;
                }
                else
                {
                    pathIndex = readLine.IndexOf("Unity.app/Contents/") + "Unity.app/Contents/".Length;
                }

                pathString = readLine.Substring(pathIndex, readLine.Length - pathIndex);
                hintPath =
                    $"\t\t\t<HintPath>C:/Program Files/Unity/Hub/Editor/{Application.unityVersion}/Editor/Data/{pathString}";
            }
            else if (readLine.Contains(directoryPath))
            {
                // if the .dll is in the Library/PackageCache folder, copy it to the 
                // ScriptAssemblies folder
                if (readLine.Contains("PackageCache"))
                {
                    int fileIndex = readLine.IndexOf("<HintPath>") + "<HintPath>".Length;
                    string filePath = readLine.Substring(fileIndex, readLine.Length - fileIndex - "</HintPath>".Length);
                    fileIndex = filePath.LastIndexOf("/");
                    string fileName = filePath.Substring(fileIndex, filePath.Length - fileIndex);
                    File.Copy(filePath, "Library/ScriptAssemblies/" + fileName);
                    readLine = directoryPath + "Library/ScriptAssemblies/" + fileName + "</HintPath>";
                }
                pathIndex = readLine.IndexOf(directoryPath) + directoryPath.Length;
                //Debug.Log($"Directory path: '{directoryPath}', path index: '{pathIndex}', line: '{readLine}'");
                pathString = readLine.Substring(pathIndex, readLine.Length - pathIndex);
                hintPath = $"\t\t\t<HintPath>{DockerPath}{directoryPath}{pathString}";
            }

            return hintPath;
        }
        
        private static void UpdateSolutionFile(FileInfo fileInfo)
        {
            string fileName = fileInfo.Name;
            var newFile = fileInfo.CopyTo("../" + fileName);
            Debug.Log($"[UpdateSonar::UpdateProjectFile] Updating solution '{fileName}'");
            StreamReader streamReader = newFile.OpenText();
            
            string readLine;
            StringBuilder outputString = new StringBuilder();
       
            do
            {
                readLine = streamReader.ReadLine();

                if (readLine != null)
                {
                    outputString.AppendLine(readLine);

                    if (readLine.Contains("<ReferencePath>"))
                    {
                        Debug.Log("[UpdateSonar::UpdateProjectFile] Project file already up-to-date, skipping");
                        break;
                    }
                    
                    if (readLine.Contains("DUMMY"))
                    {
                        
                    }
                }
            } while (readLine != null);
            
            streamReader.Close();
            streamReader.Dispose();

            // We should only overwrite the file with the new contents if readLine is null,
            // otherwise we found duplicate ReferencePath values and do not need to update
            if (readLine == null)
            {
                File.WriteAllText("../" + fileName, outputString.ToString());
            }
        }
    }
}

