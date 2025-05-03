using System.Linq;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.DacExtensions
{
    public static class ObjectNameParser
    {
        //get the schema - basically it needs to have at least two parts, maybe return null
        public static string GetSchemaName(this ObjectIdentifier src, ModelTypeClass type)
        {
            if (type == ModelSchema.Schema)
            {
                return src.Parts.Last();
            }
            else
            {
                if (src.Parts.Count > 1)
                {
                    return src.Parts[src.Parts.Count - (ExpectThreeParts(type) ? 3 : 2)];
                }
                else
                {
                    return null;
                }
            }
        }

        public static string GetSchemaName(this TSqlObject src)
        {
            if (src == null) return null;

            return GetSchemaName(src.Name, src.ObjectType);
        }

        /// <summary>
        /// Returns true if this model type is represented by a three part name.
        /// Otherwise returns false, meaning a 2 part name.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static bool ExpectThreeParts(ModelTypeClass type) => type == ModelSchema.Index || type == ModelSchema.ColumnStoreIndex;
    }
}
