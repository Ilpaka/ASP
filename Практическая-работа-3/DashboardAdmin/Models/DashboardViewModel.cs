namespace DashboardAdmin.Models;

// Модель одной строки сводки по тренду (для ViewComponent "TrendSummary")
public class TrendSummaryItem
{
    public Trend Trend { get; set; }
    public int Count { get; set; }
    public decimal Sum { get; set; }
}
