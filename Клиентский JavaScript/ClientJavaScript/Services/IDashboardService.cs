using ClientJavaScript.Models;

namespace ClientJavaScript.Services;

public interface IDashboardService
{
    DashboardSnapshot GetSnapshot();
    Activity? AddActivity(string title, string category);
}
