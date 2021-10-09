using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtCookieAuth.Services
{
    public class CachedService : ICachedService
    {
        private readonly IDistributedCache _db;

        public CachedService(
            IDistributedCache db

            )
        {
            this._db = db;
        }

        /// <summary>
        /// 非同步取得快取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key) where T : class
        {
            string data = await _db.GetStringAsync(key);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonConvert.DeserializeObject<T>(data);
            }
            return null;
        }

        /// <summary>
        /// 同步取得快取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key) where T : class
        {
            string data = _db.GetString(key);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonConvert.DeserializeObject<T>(data);
            }
            return null;
        }

        public async Task<bool> SetAsync<T>(string key, T data, TimeSpan? time = null) where T : class
        {
            if (!time.HasValue)
            {
                time = TimeSpan.FromDays(30);
            }

            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = time
            };

            await _db.SetStringAsync(
                  key, JsonConvert.SerializeObject(data),
                  options);

            return true;
        }

        public bool Set<T>(string key, T data, TimeSpan? time) where T : class
        {
            if (!time.HasValue)
            {
                time = TimeSpan.FromDays(30);
            }

            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = time
            };

            _db.SetString(key, JsonConvert.SerializeObject(data),
                  options);

            return true;
        }

        public async Task<T> GetAndSetAsync<T>(string key, T data, TimeSpan? time = null) where T : class
        {
            var cachedData = await GetAsync<T>(key);
            if (cachedData != null)
            {
                return cachedData;
            }
            var result = await SetAsync<T>(key, data, time);
            if (result)
            {
                return await GetAsync<T>(key);
            }
            return default(T);
        }

        public async Task<T> GetAndSetAsync<T>(string key, Func<Task<T>> acquire, TimeSpan? time = null) where T : class
        {
            var cachedData = await GetAsync<T>(key);
            if (cachedData != null)
            {
                return cachedData;
            }
            var data = await acquire();

            if (data != null)
            {
                await SetAsync<T>(key, data, time);
            }

            return data;
        }

        public T GetAndSet<T>(string key, Func<T> acquire, TimeSpan? time = null) where T : class
        {
            var cachedData = Get<T>(key);
            if (cachedData != null)
            {
                return cachedData;
            }
            var data = acquire();

            if (data != null)
            {
                Set<T>(key, data, time);
            }
            return data;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            await _db.RemoveAsync(id);
            return true;
        }
    }

}
