using DashboardAdmin.Models;

namespace DashboardAdmin.Services;

public interface IDashboardRepository
{
    List<DashboardCard> GetAllCards();
    DashboardCard? GetById(int id);
    List<DashboardCard> GetLatest(int count);
}
