namespace DashboardAdmin.Models;

// Динамика показателя
public enum Trend
{
    Up,
    Down,
    Stable
}

// Карточка показателя на дашборде
public class DashboardCard
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Value { get; set; }
    public Trend Trend { get; set; }
    public string Unit { get; set; } = "";
    public string Description { get; set; } = "";
}
