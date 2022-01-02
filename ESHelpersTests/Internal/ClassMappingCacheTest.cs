using System.Collections.Generic;
using ESHelpers.EventSourcing;
using ESHelpers.Internal;
using Xunit;

namespace ESHelpersTests.Internal
{
    public class ClassMappingCacheTest
    {
        
        [Fact]
        public void it_can_initialize_and_lookup_cache()
        {
            var classMappingCache = ClassMappingCache.Instance;
            classMappingCache.initializeCache(typeof(IDomainEvent));
            var fullClassPath = classMappingCache.lookup("FooEvent");
            var expectedPath = "ESHelpersTests.Infrastructure.Event.FooEvent, ESHelpersTests";
            Assert.Equal(expectedPath, fullClassPath);
        }

        [Fact]
        public void it_throws_when_class_path_cannot_be_found()
        {
            var classMappingCache = ClassMappingCache.Instance;
            classMappingCache.initializeCache(typeof(IDomainEvent));
            Assert.Throws<KeyNotFoundException>(() => classMappingCache.lookup("SomeRandomClass"));
        }
    }
}