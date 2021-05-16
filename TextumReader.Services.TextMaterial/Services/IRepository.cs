using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TextumReader.Services.TextMaterial.Services
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetItemsAsync(string query);
        Task<T> GetItemAsync(string id);
        Task AddItemAsync(T item);
        Task UpdateItemAsync(string id, T item);
        Task DeleteItemAsync(string id);
    }
}
