using System;
using System.Collections.Generic;
using System.Linq;
using LostAndFoundAgency.Models;
using LostAndFoundAgency.Services;

namespace LostAndFoundAgency.Data
{
    public static class DatabaseSeeder
    {
        private static readonly string[] PublicLocations =
        {
            "Парк",
            "Пляж",
            "Двір біля будинку",
            "Магазин",
            "Кафе",
            "Зупинка транспорту",
            "Торговий центр",
            "Університет"
        };

        public static void Seed()
        {
            using (var context = new AppDbContext())
            {
                EnsureEmployee(context, "admin", "admin", "Головний адміністратор", "Керівник відділу");

                EnsureCategory(context, "Електроніка", "Телефони, ноутбуки, навушники та інша техніка");
                EnsureCategory(context, "Документи", "Паспорти, посвідчення, студентські квитки, банківські картки");
                EnsureCategory(context, "Одяг та аксесуари", "Одяг, сумки, гаманці, парасольки та аксесуари");
                EnsureCategory(context, "Ключі", "Ключі, брелоки та зв'язки ключів");
                MergeCategoryAliases(context, "Одяг та аксесуари", "Одяг, сумки, гаманці, парасольки та аксесуари",
                    "Одяг/Аксесуари", "Одяг і аксесуари", "Аксесуари");

                EnsureLocation(context, "Парк", "Міський парк", "Відкрита зона відпочинку");
                EnsureLocation(context, "Пляж", "Міський пляж", "Зона біля води");
                EnsureLocation(context, "Двір біля будинку", "Житловий двір", "Прибудинкова територія");
                EnsureLocation(context, "Магазин", "Магазин або супермаркет", "Торгова точка");
                EnsureLocation(context, "Кафе", "Кафе або ресторан", "Заклад харчування");
                EnsureLocation(context, "Зупинка транспорту", "Автобусна або трамвайна зупинка", "Місце очікування транспорту");
                EnsureLocation(context, "Торговий центр", "ТРЦ", "Великий торговий комплекс");
                EnsureLocation(context, "Університет", "Навчальний заклад", "Корпус або територія університету");

                EnsureClient(context, "ivan", "123", "Іван Франко", "0991112233");
                EnsureClient(context, "lesya", "123", "Леся Українка", "0671112233");
                EnsureClient(context, "olena", "123", "Олена Коваль", "0502223344");

                context.SaveChanges();
            }
        }

        public static void GenerateDemoRequests(int pairCount = 5)
        {
            Seed();

            using (var context = new AppDbContext())
            {
                var clients = context.Persons.OrderBy(p => p.PersonId).ToList();
                if (!clients.Any())
                {
                    return;
                }

                var templates = new[]
                {
                    new DemoPair("Одяг та аксесуари", "Парк", "Чорна парасолька", "Чорна парасолька", "Загублена біля лавки в парку.", "Знайдена чорна парасолька біля входу в парк."),
                    new DemoPair("Електроніка", "Кафе", "Навушники AirPods", "Навушники AirPods", "Білий кейс, могли залишитися на столику.", "Знайдено AirPods у білому кейсі в кафе."),
                    new DemoPair("Документи", "Університет", "Студентський квиток", "Студентський квиток", "Студентський квиток у прозорій обкладинці.", "Знайдено студентський квиток біля аудиторії."),
                    new DemoPair("Ключі", "Двір біля будинку", "Ключі з синім брелоком", "Ключі з синім брелоком", "Зв'язка з трьох ключів і синім брелоком.", "Знайдено ключі з синім брелоком у дворі."),
                    new DemoPair("Одяг та аксесуари", "Магазин", "Коричневий гаманець", "Коричневий гаманець", "Коричневий шкіряний гаманець.", "Знайдено коричневий гаманець біля каси."),
                    new DemoPair("Електроніка", "Торговий центр", "Телефон Samsung", "Телефон Samsung", "Чорний Samsung у прозорому чохлі.", "Знайдено телефон Samsung у торговому центрі.")
                };

                for (int i = 0; i < pairCount; i++)
                {
                    var template = templates[i % templates.Length];
                    var lostClient = clients[i % clients.Count];
                    var finder = clients[(i + 1) % clients.Count];
                    var categoryId = GetCategoryId(context, template.CategoryName);
                    var locationId = GetLocationId(context, template.LocationName);
                    var dateLost = DateTime.Now.Date.AddDays(-(i + 3));

                    context.LostRequests.Add(new LostRequest
                    {
                        PersonId = lostClient.PersonId,
                        CategoryId = categoryId,
                        LocationId = locationId,
                        ItemName = template.LostName,
                        Description = template.LostDescription,
                        DateLost = dateLost,
                        RequestDate = DateTime.Now,
                        Status = ItemStatuses.Pending
                    });

                    context.FoundItems.Add(new FoundItem
                    {
                        FinderId = finder.PersonId,
                        CategoryId = categoryId,
                        LocationId = locationId,
                        ItemName = template.FoundName,
                        Description = template.FoundDescription,
                        DateFound = dateLost.AddDays(1),
                        StorageLocation = $"Полиця демо-{i + 1}",
                        Status = ItemStatuses.Pending
                    });
                }

                context.SaveChanges();
            }
        }

        public static bool IsPublicLocationName(string locationName)
        {
            return PublicLocations.Contains(locationName);
        }

        private static int GetCategoryId(AppDbContext context, string categoryName)
        {
            return context.Categories.First(c => c.CategoryName == categoryName).CategoryId;
        }

        private static int GetLocationId(AppDbContext context, string locationName)
        {
            return context.Locations.First(l => l.LocationName == locationName).LocationId;
        }

        private static void EnsureEmployee(AppDbContext context, string login, string password, string fullName, string position)
        {
            var employee = context.Employees.FirstOrDefault(e => e.Login == login);
            if (employee == null)
            {
                context.Employees.Add(new Employee
                {
                    FullName = fullName,
                    Login = login,
                    PasswordHash = PasswordHasher.Hash(password),
                    Position = position
                });
                return;
            }

            employee.FullName = string.IsNullOrWhiteSpace(employee.FullName) ? fullName : employee.FullName;
            employee.Position = string.IsNullOrWhiteSpace(employee.Position) ? position : employee.Position;
            if (PasswordHasher.NeedsRehash(employee.PasswordHash) && PasswordHasher.Verify(password, employee.PasswordHash))
            {
                employee.PasswordHash = PasswordHasher.Hash(password);
            }
        }

        private static void EnsureClient(AppDbContext context, string login, string password, string fullName, string phone)
        {
            var client = context.Persons.FirstOrDefault(p => p.Login == login);
            if (client == null)
            {
                context.Persons.Add(new Person
                {
                    FullName = fullName,
                    Login = login,
                    PasswordHash = PasswordHasher.Hash(password),
                    Phone = phone
                });
                return;
            }

            client.FullName = string.IsNullOrWhiteSpace(client.FullName) ? fullName : client.FullName;
            client.Phone = string.IsNullOrWhiteSpace(client.Phone) ? phone : client.Phone;
            if (PasswordHasher.NeedsRehash(client.PasswordHash) && PasswordHasher.Verify(password, client.PasswordHash))
            {
                client.PasswordHash = PasswordHasher.Hash(password);
            }
        }

        private static void EnsureCategory(AppDbContext context, string name, string description)
        {
            if (context.Categories.Any(c => c.CategoryName == name))
            {
                return;
            }

            context.Categories.Add(new Category { CategoryName = name, Description = description });
            context.SaveChanges();
        }

        private static void MergeCategoryAliases(AppDbContext context, string canonicalName, string description, params string[] aliases)
        {
            var names = aliases.Concat(new[] { canonicalName }).ToList();
            var matchingCategories = context.Categories
                .Where(c => names.Contains(c.CategoryName))
                .OrderBy(c => c.CategoryId)
                .ToList();

            var canonical = matchingCategories.FirstOrDefault(c => c.CategoryName == canonicalName)
                ?? matchingCategories.FirstOrDefault();

            if (canonical == null)
            {
                canonical = new Category { CategoryName = canonicalName, Description = description };
                context.Categories.Add(canonical);
                context.SaveChanges();
                return;
            }

            canonical.CategoryName = canonicalName;
            canonical.Description = description;
            context.SaveChanges();

            foreach (var duplicate in matchingCategories.Where(c => c.CategoryId != canonical.CategoryId).ToList())
            {
                foreach (var lostRequest in context.LostRequests.Where(r => r.CategoryId == duplicate.CategoryId))
                {
                    lostRequest.CategoryId = canonical.CategoryId;
                }

                foreach (var foundItem in context.FoundItems.Where(i => i.CategoryId == duplicate.CategoryId))
                {
                    foundItem.CategoryId = canonical.CategoryId;
                }

                context.Categories.Remove(duplicate);
            }

            context.SaveChanges();
        }

        private static void EnsureLocation(AppDbContext context, string name, string address, string description)
        {
            if (context.Locations.Any(l => l.LocationName == name))
            {
                return;
            }

            context.Locations.Add(new Location { LocationName = name, Address = address, Description = description });
            context.SaveChanges();
        }

        private sealed record DemoPair(
            string CategoryName,
            string LocationName,
            string LostName,
            string FoundName,
            string LostDescription,
            string FoundDescription);
    }
}
