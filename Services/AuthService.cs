using System.Linq;
using LostAndFoundAgency.Data;
using LostAndFoundAgency.Models;

namespace LostAndFoundAgency.Services
{
    public class AuthService : IAuthService
    {
        public (Employee? employee, Person? client) Authenticate(string login, string password)
        {
            var normalizedLogin = login.Trim();

            using (var context = new AppDbContext())
            {
                var employee = context.Employees.FirstOrDefault(e => e.Login == normalizedLogin);
                if (employee != null && PasswordHasher.Verify(password, employee.PasswordHash))
                {
                    UpgradeEmployeePasswordIfNeeded(context, employee, password);
                    return (employee, null);
                }

                var client = context.Persons.FirstOrDefault(p => p.Login == normalizedLogin);
                if (client != null && PasswordHasher.Verify(password, client.PasswordHash))
                {
                    UpgradeClientPasswordIfNeeded(context, client, password);
                    return (null, client);
                }

                return (null, null);
            }
        }

        public bool RegisterEmployee(string fullName, string login, string password, string position)
        {
            var normalizedLogin = login.Trim();

            using (var context = new AppDbContext())
            {
                if (context.Employees.Any(e => e.Login == normalizedLogin) || context.Persons.Any(p => p.Login == normalizedLogin))
                    return false;

                var newEmployee = new Employee
                {
                    FullName = fullName.Trim(),
                    Login = normalizedLogin,
                    PasswordHash = PasswordHasher.Hash(password),
                    Position = position.Trim()
                };

                context.Employees.Add(newEmployee);
                context.SaveChanges();
                return true;
            }
        }

        public bool RegisterClient(string fullName, string login, string password, string phone)
        {
            var normalizedLogin = login.Trim();

            using (var context = new AppDbContext())
            {
                if (context.Employees.Any(e => e.Login == normalizedLogin) || context.Persons.Any(p => p.Login == normalizedLogin))
                    return false;

                var newClient = new Person
                {
                    FullName = fullName.Trim(),
                    Login = normalizedLogin,
                    PasswordHash = PasswordHasher.Hash(password),
                    Phone = phone.Trim()
                };

                context.Persons.Add(newClient);
                context.SaveChanges();
                return true;
            }
        }

        private static void UpgradeEmployeePasswordIfNeeded(AppDbContext context, Employee employee, string password)
        {
            if (!PasswordHasher.NeedsRehash(employee.PasswordHash))
            {
                return;
            }

            employee.PasswordHash = PasswordHasher.Hash(password);
            context.SaveChanges();
        }

        private static void UpgradeClientPasswordIfNeeded(AppDbContext context, Person client, string password)
        {
            if (!PasswordHasher.NeedsRehash(client.PasswordHash))
            {
                return;
            }

            client.PasswordHash = PasswordHasher.Hash(password);
            context.SaveChanges();
        }
    }
}
