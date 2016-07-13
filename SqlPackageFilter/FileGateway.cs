using System.IO;

namespace AgileSqlClub.SqlPackageFilter
{
    public class FileGateway : IFileGateway
    {
        public string GetContents(string path)
        {
            return new StreamReader(path).ReadToEnd();
        }
    }

    public interface IFileGateway
    {
        string GetContents(string path);
    }
}
