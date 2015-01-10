using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.DacExtensions
{
    public static class ObjectNameParser
    {
        //get the schema - basically it needs to have at least two parts, maybe return enum
        public static string GetSchemaName(this ObjectIdentifier src)
        {
            if (src.Parts.Count > 1)
            {
                return src.Parts[src.Parts.Count - 2];
            }

            return null;
        }
    }
}
