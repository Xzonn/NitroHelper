namespace NitroHelper
{
  internal class Program
  {
    static void Main(string[] args)
    {
      var inputFile = args[0];
      var inputFolder = args[1];
      var outputFile = args[2];

      var helper = new NitroHelper(inputFile);
      ReplaceFile(helper.root, inputFolder);
      helper.SaveAs(outputFile);
    }

    static void ReplaceFile(sFolder sFolder, string inputFolder, string path = "")
    {
      if (sFolder.files != null)
      {
        foreach (var file in sFolder.files)
        {
          string replacedPath = Path.Join(new string[] { inputFolder, path, file.name });
          if (!File.Exists(replacedPath)) { continue; }
          file.size = (uint)new FileInfo(replacedPath).Length;
          file.path = replacedPath;
          file.offset = 0;
        }
      }
      if (sFolder.folders != null)
      {
        foreach (var folder in sFolder.folders)
        {
          ReplaceFile(folder, inputFolder, Path.Join(path, folder.name));
        }
      }
    }
  }
}