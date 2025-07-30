using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;



public static class ZipUtil
{
    public static void ExtractZipFile(string zipFilePath, string targetDir)
    {
        if (!File.Exists(zipFilePath))
        {
            Debug.LogError($"ZIP 文件不存在: {zipFilePath}");
            return;
        }

        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        using (FileStream fs = File.OpenRead(zipFilePath))
        {
            using (var zipInputStream = new ZipInputStream(fs))
            {
                ZipEntry entry;
                while ((entry = zipInputStream.GetNextEntry()) != null)
                {
                    string filePath = Path.Combine(targetDir, entry.Name);

                    // 创建子目录
                    string directoryName = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directoryName))
                        Directory.CreateDirectory(directoryName);

                    if (!entry.IsDirectory)
                    {
                        using (FileStream streamWriter = File.Create(filePath))
                        {
                            byte[] data = new byte[4096];
                            int size;
                            while ((size = zipInputStream.Read(data, 0, data.Length)) > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"解压完成 -> {targetDir}");
    }
}
