using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TextumReader.DataAccess
{
    public interface IRepository<T> where T: BaseModel
    {
        Task<IEnumerable<T>> GetItemsAsync(string query);
        Task<T> GetItemAsync(string id);
        Task AddItemAsync(T item);
        Task UpdateItemAsync(string id, T item);
        Task DeleteItemAsync(string id);
    }
}
