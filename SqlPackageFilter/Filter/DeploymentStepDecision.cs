namespace AgileSqlClub.SqlPackageFilter.Filter
{
  public class DeploymentStepDecision
  {
    public bool Remove { get; set; }
    public StepType StepType { get; set; }
    public string ObjectName { get; set; }
  }
}