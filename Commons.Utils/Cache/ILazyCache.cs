using System;
using System.Threading.Tasks;

namespace Commons.Utils.Cache
{
    public interface ILazyCache<T>
    {
        Task<T> GetOrCreate(string key, Func<Task<T>> createItem);
    }
}
