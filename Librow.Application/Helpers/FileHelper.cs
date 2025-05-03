using Librow.Application.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Helpers;
public class FileHelper
{
    public static async Task<string> GetTemplateFile(string fileName)
    {
        try
        {
            var projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            var templateProject = Assembly.GetExecutingAssembly().GetName().Name;

            string templatesPath = Path.Combine(projectPath, templateProject, FileMessage.TemplatesFolderName);

            if (!Directory.Exists(templatesPath))
            {
                throw new DirectoryNotFoundException(string.Format(FileMessage.DirectoryNotFoundMessage, templatesPath));
            }

            string filePath = Path.Combine(templatesPath, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format(FileMessage.FileNotFoundMessage, fileName), filePath);
            }

            using var reader = new StreamReader(filePath);
            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(string.Format(FileMessage.InvalidOperationMessage, fileName, ex.Message), ex);
        }
    }
}
