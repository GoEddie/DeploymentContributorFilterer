namespace AgileSqlClub.SqlPackageFilter.Filter
{
    public interface IDisplayMessageHandler
    {
        void ShowMessage(string message, DisplayMessageLevel level);
        void SetMessageLevel(DisplayMessageLevel level);
    }
}