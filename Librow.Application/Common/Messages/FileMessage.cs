using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Application.Common.Messages;
public static class FileMessage
{
    public const string TemplatesFolderName = "Templates";
    public const string DirectoryNotFoundMessage = "The Templates folder does not exist at: {0}";
    public const string FileNotFoundMessage = "File {0} does not exist in the Templates folder.";
    public const string InvalidOperationMessage = "Unable to read template file {0}: {1}";
}
