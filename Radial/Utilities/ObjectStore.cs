using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Radial.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Radial.Utilities
{
    public class ObjectStore<T>
    {
        private readonly ConcurrentDictionary<string, T> _cache = new ConcurrentDictionary<string, T>();
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        private readonly TimeSpan _saveInterval = TimeSpan.FromMinutes(30);

        private readonly System.Timers.Timer _saveTimer;

        private readonly IServiceProvider _serviceProvider;

        public ObjectStore(string name, IServiceProvider serviceProvider)
        {
            Name = name;
            _serviceProvider = serviceProvider;

            _saveTimer = new System.Timers.Timer()
            {
                AutoReset = false,
                Interval = _saveInterval.TotalMilliseconds
            };
            _saveTimer.Elapsed += SaveTimer_Elapsed;

            Load();
        }

        public ICollection<T> All => _cache.Values;

        public string Name { get; }

        public T AddOrUpdate(string key, T item)
        {
            return _cache.AddOrUpdate(key, item, (k, v) => item);
        }

        public T Get(string key)
        {
            return _cache[key];
        }

        public T Find(Func<T, bool> match)
        {
            return _cache.Values.FirstOrDefault(match);
        }

        public void Load()
        {
            var scope = _serviceProvider.CreateScope();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var storePath = Path.Combine(env.ContentRootPath, "App_Data", $"{Name}.json");
            try
            {
                _fileLock.Wait();
                Directory.CreateDirectory(Path.GetDirectoryName(storePath));
                if (!File.Exists(storePath))
                {
                    return;
                }

                var content = File.ReadAllText(storePath);
                var savedEntries = JsonSerializer.Deserialize<ConcurrentDictionary<string, T>>(content);
                foreach (var entry in savedEntries)
                {
                    _cache.TryAdd(entry.Key, entry.Value);
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ObjectStore<T>>>();
                logger.LogError(ex, "Error while loading object store. Store Name: {name}.", Name);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task Save()
        {
            var scope = _serviceProvider.CreateScope();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var storePath = Path.Combine(env.ContentRootPath, "App_Data", $"{Name}.json");
            try
            {
                await _fileLock.WaitAsync();
                Directory.CreateDirectory(Path.GetDirectoryName(storePath));
                await File.WriteAllTextAsync(storePath, JsonSerializer.Serialize(_cache));
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ObjectStore<T>>>();
                logger.LogError(ex, "Error while saving object store. Store Name: {name}.", Name);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public bool TryGet(string key, out T result)
        {
            return _cache.TryGetValue(key, out result);
        }
        private async void SaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await Save();
            }
            finally
            {
                _saveTimer.Start();
            }
        }
    }
}
