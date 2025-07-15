using DP.Base.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Test
{
    public class StringExtensionTests
    {
        [Fact]
        public void RemoveCharacters_Null()
        {
            string s = null;
            var result = s.RemoveCharacters('a', 'b', 'c');
            Assert.Equal(null, result);
        }

        [Fact]
        public void RemoveCharacters_Empty()
        {
            string s = string.Empty;
            var result = s.RemoveCharacters('a', 'b', 'c');
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void RemoveCharacters_OneChar()
        {
            string s = "apple";
            var result = s.RemoveCharacters('a', 'b', 'c');
            Assert.Equal("pple", result);
        }

        [Fact]
        public void RemoveCharacters_AllChars()
        {
            string s = "apple";
            var result = s.RemoveCharacters('a', 'p', 'l', 'e');
            Assert.Equal(string.Empty, result);
        }
    }
}
