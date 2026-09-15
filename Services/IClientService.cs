using System.Collections.Generic;
using LostAndFoundAgency.Models;

namespace LostAndFoundAgency.Services
{
    public interface IClientService
    {
        List<LostRequest> GetMyLostRequests(int personId);

        List<FoundItem> GetMyFoundItems(int personId);

        List<Category> GetCategories();

        List<Location> GetLocations();

        bool AddLostRequest(LostRequest request);
        bool AddFoundItem(FoundItem item);
    }
}
