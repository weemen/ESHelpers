using ESHelpers.Encryption;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace ESHelpersTests.Encryption
{
    public class HashingConverterTest
    {
        private readonly string _salt = "some_random_salt2222";
        
        [Fact]
        public void it_can_hash_a_value()
        {
            var converter = new HashingConverter(_salt);
            var expectedOutcome =
                "9b71d224bd62f3785d96d46ad3ea3d73319bfbc2890caadae2dff72519673ca72323c3d99ba5c11d7c7acc6e14b8c5da0c4663475c2e5c3adef46f73bcdec043";
            Assert.Equal(expectedOutcome, converter.ComputeSha512Hash("hello2"));
        }
    }
}