using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using HangFireExtension.CustomMemoryAndFileStorage.Models;
using HangFireExtension.CustomMemoryAndFileStorage.Helpers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HangFireExtension.CustomMemoryAndFileStorage.Constants;

namespace HangFireExtension.CustomMemoryAndFileStorage
{
    internal class MemoryAndFileStorageMemoryCache
    {
        private readonly static System.Runtime.Caching.MemoryCache _memoryCache = new System.Runtime.Caching.MemoryCache("hangfireCaching") { };
        private static string _filePath = string.Empty;

        private static bool _isDirty = false;
        public static bool IsDirty { get { return _isDirty; } set { _isDirty = value; } }

        public static void SetFilePath(string filePath)
        {
            _filePath = filePath;
        }

        public static void LoadHistoryFile()
        {
            if (!Path.Exists(_filePath))
            {
                return;
            }
            var historyFiles = Directory.GetFiles(_filePath, "history_*.txt");
            if (historyFiles.Any() == false)
            {
                return;
            }
            var mostRecentFile = historyFiles.OrderByDescending(rec => rec).First();

            var serializedData = File.ReadAllText(Path.Combine(_filePath, mostRecentFile));
            var dictionaryObject = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(serializedData);
            if (dictionaryObject == null)
            {
                return;
            }

            lock (_memoryCache)
            {
                foreach (var item in dictionaryObject) 
                {
                    object? value = null;
                    switch (item.Key) 
                    {
                        case ConstantsTableNames.Job:
                            value = System.Text.Json.JsonSerializer.Deserialize<List<TableHangFireJob>>(item.Value);
                            break;
                        case ConstantsTableNames.State:
                            value = System.Text.Json.JsonSerializer.Deserialize<List<TableHangFireState>>(item.Value);
                            break;
                        case ConstantsTableNames.Counter:
                            value = System.Text.Json.JsonSerializer.Deserialize<List<TableHangFireCounter>>(item.Value);
                            break;
                        case ConstantsTableNames.Hash:
                            value = System.Text.Json.JsonSerializer.Deserialize<List<TableHangFireHash>>(item.Value);
                            break;
                        case ConstantsTableNames.Set:
                            value = System.Text.Json.JsonSerializer.Deserialize<List<TableHangFireSet>>(item.Value);
                            break;
                    }
                    if (value != null)
                    {
                        MemoryAndFileStorageMemoryCache.SetItemInCache(item.Key, value);
                    }
                }
            }

        }

        public static CacheItem GetItemFromCache(string key) => _memoryCache.GetCacheItem(key);

        public static void SetItemInCache(string key, object value)
        {
            lock (_memoryCache)
            {
                _isDirty = true;

                var item = GetItemFromCache(key);
                if (item != null)
                {
                    _memoryCache[key] = value;
                }
                else
                {
                    _memoryCache.Add(new CacheItem(key, value), new CacheItemPolicy());
                }
            }
        }

        public static void SaveItemsToFileSystem()
        {
            if (string.IsNullOrEmpty(_filePath) || !Directory.Exists(_filePath))
            {
                throw new InvalidOperationException($"Path to save file ({_filePath}) does not exist or is not set.");
            }

            lock (_memoryCache) 
            {
                var saveResult = new Dictionary<string, string>();

                _memoryCache.ToList().ForEach(item => {
                    saveResult.Add(item.Key, System.Text.Json.JsonSerializer.Serialize(item.Value));
                });

                var output = System.Text.Json.JsonSerializer.Serialize(saveResult);
                var removeFiles = Directory.GetFiles(_filePath, "history_*.txt");
                var fileName = Path.Combine(_filePath, $"history_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt");
                var timeOut = 0;
                while (File.Exists(fileName)) 
                {
                    if (timeOut >= 5)
                    {
                        throw new Exception($"Could not save memory-data to file.");
                    }
                    Thread.Sleep(1500);
                    fileName = Path.Combine(_filePath, $"history_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt");
                    timeOut++;
                }
                File.WriteAllText(fileName, output);
                // if saving is ok, now we can remove old files
                if (removeFiles.Count() < 10)
                {
                    return;
                }
                var items = removeFiles.Take(removeFiles.Count() - 10);
                foreach (var item in items)
                {
                    try {
                        File.Delete(item);
                    }
                    catch (Exception) { /* locked or other reason, just continue */ }
                }
            }
        }

    }
}
