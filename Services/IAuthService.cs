using LostAndFoundAgency.Models;

namespace LostAndFoundAgency.Services
{
    public interface IAuthService
    {
        (Employee? employee, Person? client) Authenticate(string login, string password);
        bool RegisterEmployee(string fullName, string login, string password, string position);
        bool RegisterClient(string fullName, string login, string password, string phone);
    }
}
