using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ESHelpers.EventSourcing;

namespace ESHelpers.Internal
{
    public sealed class ClassMappingCache
    {
        private Dictionary<string, string> _cache = new Dictionary<string, string>();
        
        private static readonly Lazy<ClassMappingCache> lazy =
            new Lazy<ClassMappingCache>(() => new ClassMappingCache());

        public static ClassMappingCache Instance { get { return lazy.Value; } }

        private ClassMappingCache()
        {
            if (_cache.Count == 0)
            {
                this.initializeCache(typeof(IDomainEvent));
            }
        }
        
        public void initializeCache(Type typeToCache)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeToCache.IsAssignableFrom(t));

            foreach (var foundType in types)
            {
                if (Equals(foundType.ToString(), typeToCache.ToString()))
                {
                    continue;
                }

                if (_cache.ContainsKey(foundType.Name))
                {
                    var record = _cache.GetValueOrDefault(foundType.Name);
                    if (record == $"{foundType.FullName}, {foundType.Assembly.GetName().Name}")
                    {
                        continue;
                    }
                }
                _cache.Add(foundType.Name, $"{foundType.FullName}, {foundType.Assembly.GetName().Name}");
            }

            if (_cache.Count == 0)
            {
                throw new ConstraintException(
                    $"Class mapping cache cannot be empty, no class of {typeToCache.ToString()} found");
            }
        }

        public string lookup(string eventName)
        {
            if (_cache.ContainsKey(eventName))
            {
                return _cache.GetValueOrDefault(eventName);
            }

            throw new KeyNotFoundException($"Key: {eventName} cannot be found in class mapping cache");
        }
    }
}