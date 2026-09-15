using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using LostAndFoundAgency.Data;
using LostAndFoundAgency.Models;

namespace LostAndFoundAgency.Services
{
    public class ClientService : IClientService
    {
        public bool AddLostRequest(LostRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ItemName))
            {
                return false;
            }

            using (var context = new AppDbContext())
            {
                request.ItemName = request.ItemName.Trim();
                request.Description = request.Description?.Trim();
                request.CategoryId = ResolveCategoryId(context, request.CategoryId);
                request.LocationId = ResolveLocationId(context, request.LocationId);
                request.DateLost = request.DateLost == default ? DateTime.Now : request.DateLost;
                request.RequestDate = DateTime.Now;
                request.Status = ItemStatuses.Pending;

                context.LostRequests.Add(request);
                context.SaveChanges();
                return true;
            }
        }

        public bool AddFoundItem(FoundItem item)
        {
            if (string.IsNullOrWhiteSpace(item.ItemName))
            {
                return false;
            }

            using (var context = new AppDbContext())
            {
                item.ItemName = item.ItemName.Trim();
                item.Description = item.Description?.Trim();
                item.StorageLocation = string.IsNullOrWhiteSpace(item.StorageLocation)
                    ? "У клієнта"
                    : item.StorageLocation.Trim();
                item.CategoryId = ResolveCategoryId(context, item.CategoryId);
                item.LocationId = ResolveLocationId(context, item.LocationId);
                item.DateFound = item.DateFound == default ? DateTime.Now : item.DateFound;
                item.Status = ItemStatuses.Pending;

                context.FoundItems.Add(item);
                context.SaveChanges();
                return true;
            }
        }

        public List<LostRequest> GetMyLostRequests(int clientId)
        {
            using (var context = new AppDbContext())
            {
                return context.LostRequests
                    .AsNoTracking()
                    .Where(r => r.PersonId == clientId)
                    .OrderByDescending(r => r.RequestDate)
                    .ToList();
            }
        }

        public List<FoundItem> GetMyFoundItems(int clientId)
        {
            using (var context = new AppDbContext())
            {
                return context.FoundItems
                    .AsNoTracking()
                    .Where(i => i.FinderId == clientId)
                    .OrderByDescending(i => i.DateFound)
                    .ToList();
            }
        }

        public List<Category> GetCategories()
        {
            using (var context = new AppDbContext())
            {
                return context.Categories
                    .AsNoTracking()
                    .OrderBy(c => c.CategoryName)
                    .ToList();
            }
        }

        public List<Location> GetLocations()
        {
            using (var context = new AppDbContext())
            {
                return context.Locations
                    .AsNoTracking()
                    .ToList()
                    .Where(l => DatabaseSeeder.IsPublicLocationName(l.LocationName))
                    .OrderBy(l => GetPublicLocationOrder(l.LocationName))
                    .ThenBy(l => l.LocationName)
                    .ToList();
            }
        }

        private static int ResolveCategoryId(AppDbContext context, int requestedCategoryId)
        {
            if (requestedCategoryId > 0 && context.Categories.Any(c => c.CategoryId == requestedCategoryId))
            {
                return requestedCategoryId;
            }

            var category = context.Categories.FirstOrDefault(c => c.CategoryName == "Одяг та аксесуари")
                ?? context.Categories.FirstOrDefault();

            if (category != null)
            {
                return category.CategoryId;
            }

            category = new Category { CategoryName = "Одяг та аксесуари" };
            context.Categories.Add(category);
            context.SaveChanges();
            return category.CategoryId;
        }

        private static int ResolveLocationId(AppDbContext context, int requestedLocationId)
        {
            if (requestedLocationId > 0 && context.Locations.Any(l => l.LocationId == requestedLocationId))
            {
                return requestedLocationId;
            }

            var location = context.Locations.FirstOrDefault(l => l.LocationName == "Парк")
                ?? context.Locations.FirstOrDefault();

            if (location != null)
            {
                return location.LocationId;
            }

            location = new Location { LocationName = "Парк", Address = "Міський парк" };
            context.Locations.Add(location);
            context.SaveChanges();
            return location.LocationId;
        }

        private static int GetPublicLocationOrder(string locationName)
        {
            string[] preferredOrder =
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

            var index = Array.IndexOf(preferredOrder, locationName);
            return index >= 0 ? index : int.MaxValue;
        }
    }
}
