using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;


namespace CH.Business.Services
{
	public static class CacheKeys
	{
		public static string ChMemberCleansedPhoneNumbers => "ChMemberCleansedPhoneNumbers";
	}

	public interface ICacheService
	{
		T GetObjectFromCache<T>(string cacheKey, TimeSpan absoluteExpiration, Func<T> objectSettingFunction);
	}

	public class CacheService : ICacheService
	{
		private readonly IMemoryCache _cache;
		private readonly IConfiguration _config;


		public CacheService(
			IMemoryCache cache,
			IConfiguration config)
		{
			_cache = cache;
			_config = config;
		}


		public T GetObjectFromCache<T>(string cacheKey, TimeSpan absoluteExpiration, Func<T> objectSettingFunction)
		{
			if (!_cache.TryGetValue(cacheKey, out T cacheEntry))
			{
				cacheEntry = objectSettingFunction();

				// Set cache options.
				var cacheEntryOptions = new MemoryCacheEntryOptions()
					// Keep in cache for this time, reset time if accessed.
					.SetAbsoluteExpiration(absoluteExpiration);

				// Save data in cache.
				_cache.Set(cacheKey, cacheEntry, cacheEntryOptions);
			}

			return cacheEntry;
		}
	}
}
