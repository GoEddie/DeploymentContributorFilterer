using System.Linq;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Filter
{
  public static class ObjectIdentifierFactory
  {
    public static ObjectIdentifier Create(string fullName)
    {
      var parts = fullName.Split('.').Select(n => n.Trim('[', ']')).ToList();

      return parts.Count <= 2 ? new ObjectIdentifier(parts) : new ObjectIdentifier(parts.Take(parts.Count - 2).ToList(), parts.Skip(parts.Count - 2).ToList());
    }
  }
}