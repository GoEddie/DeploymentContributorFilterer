using System.Text.RegularExpressions;
using Microsoft.SqlServer.Dac.Deployment;
using Microsoft.SqlServer.Dac.Model;

namespace AgileSqlClub.SqlPackageFilter.Filter
{
  internal class DataLossCheckStep
  {
    public bool IsDataLossCheck => ObjectName != null;
    public ObjectIdentifier ObjectName { get; private set; }

    public DataLossCheckStep(DeploymentStep step)
    {
      var scriptStep = step as DeploymentScriptStep;

      var scriptLines = scriptStep?.GenerateTSQL();
      var script = scriptLines != null && scriptLines.Count > 0 ? scriptLines[0] : "";

      var match = Regex.Match(script, @"Table (.*) is being dropped\.  Deployment will halt if the table contains data\.");

      ObjectName = match.Groups.Count > 0 ? ObjectIdentifierFactory.Create(match.Groups[1].Value) : null;
    }
  }
}