using DP.Base.DateTimeUtilities;
using System;


namespace DP.Base.Test
{
    public class DateTimeUtilsTests
    {
        /// <summary>
        /// Test Data care of ChatGPT: "list the dates of the first sunday in November for a 30 year spread in the following format: M/d/yyyy h:mm:ss tt"
        /// </summary>
        [Theory]
        [InlineData("11/3/2024 12:00:00 AM")]
        [InlineData("11/2/2025 12:00:00 AM")]
        [InlineData("11/1/2026 12:00:00 AM")]
        [InlineData("11/7/2027 12:00:00 AM")]
        [InlineData("11/5/2028 12:00:00 AM")]
        [InlineData("11/4/2029 12:00:00 AM")]
        [InlineData("11/3/2030 12:00:00 AM")]
        [InlineData("11/2/2031 12:00:00 AM")]
        [InlineData("11/7/2032 12:00:00 AM")]
        [InlineData("11/6/2033 12:00:00 AM")]
        [InlineData("11/5/2034 12:00:00 AM")]
        [InlineData("11/4/2035 12:00:00 AM")]
        [InlineData("11/2/2036 12:00:00 AM")]
        [InlineData("11/1/2037 12:00:00 AM")]
        [InlineData("11/7/2038 12:00:00 AM")]
        [InlineData("11/6/2039 12:00:00 AM")]
        [InlineData("11/4/2040 12:00:00 AM")]
        [InlineData("11/3/2041 12:00:00 AM")]
        [InlineData("11/2/2042 12:00:00 AM")]
        [InlineData("11/1/2043 12:00:00 AM")]
        [InlineData("11/6/2044 12:00:00 AM")]
        [InlineData("11/5/2045 12:00:00 AM")]
        [InlineData("11/4/2046 12:00:00 AM")]
        [InlineData("11/3/2047 12:00:00 AM")]
        [InlineData("11/1/2048 12:00:00 AM")]
        [InlineData("11/7/2049 12:00:00 AM")]
        [InlineData("11/6/2050 12:00:00 AM")]
        [InlineData("11/5/2051 12:00:00 AM")]
        [InlineData("11/3/2052 12:00:00 AM")]
        [InlineData("11/2/2053 12:00:00 AM")]
        [InlineData("11/4/2091 12:00:00 AM")]
        public void FirstSundayOfNovember_Valid(string dtStr)
        {
            // Arrange
            var expected = DateTime.Parse(dtStr);

            // Act
            var actual = DateTimeUtils.FirstSundayOfNovember(expected.Year);

            // Assert
            Assert.Equal(expected, actual);
        }
        [Theory]
        [InlineData("11/13/2024 12:00:00 AM")]
        [InlineData("11/19/2025 12:00:00 AM")]
        [InlineData("3/1/2026 12:00:00 AM")]
        [InlineData("3/7/2027 12:00:00 AM")]
        [InlineData("3/5/2028 12:00:00 AM")]
        [InlineData("3/4/2029 12:00:00 AM")]
        [InlineData("3/3/2030 12:00:00 AM")]
        [InlineData("3/2/2031 12:00:00 AM")]
        [InlineData("3/7/2032 12:00:00 AM")]
        [InlineData("3/6/2033 12:00:00 AM")]
        [InlineData("3/5/2034 12:00:00 AM")]
        [InlineData("3/4/2035 12:00:00 AM")]
        [InlineData("3/2/2036 12:00:00 AM")]
        [InlineData("3/1/2037 12:00:00 AM")]
        [InlineData("3/7/2038 12:00:00 AM")]
        [InlineData("3/6/2039 12:00:00 AM")]
        [InlineData("3/4/2040 12:00:00 AM")]
        [InlineData("3/3/2041 12:00:00 AM")]
        [InlineData("3/2/2042 12:00:00 AM")]
        [InlineData("3/1/2043 12:00:00 AM")]
        [InlineData("3/6/2044 12:00:00 AM")]
        [InlineData("3/5/2045 12:00:00 AM")]
        [InlineData("3/4/2046 12:00:00 AM")]
        [InlineData("3/3/2047 12:00:00 AM")]
        [InlineData("3/1/2048 12:00:00 AM")]
        [InlineData("3/7/2049 12:00:00 AM")]
        [InlineData("3/6/2050 12:00:00 AM")]
        [InlineData("3/5/2051 12:00:00 AM")]
        [InlineData("3/3/2052 12:00:00 AM")]
        [InlineData("3/2/2053 12:00:00 AM")]
        [InlineData("3/4/2091 12:00:00 AM")]
        public void FirstSundayOfNovember_Invalid(string dtStr)
        {
            // Arrange
            var expected = DateTime.Parse(dtStr);

            // Act
            var actual = DateTimeUtils.FirstSundayOfNovember(expected.Year);

            // Assert
            Assert.NotEqual(expected, actual);
        }

        /// <summary>
        /// Test Data care of ChatGPT: "list the dates of the second sunday in March for a 30 year spread in the following format: M/d/yyyy h:mm:ss tt"
        /// </summary>
        /// <param name="dtStr"></param>
        [Theory]
        [InlineData("3/10/2024 12:00:00 AM")]
        [InlineData("3/9/2025 12:00:00 AM")]
        [InlineData("3/8/2026 12:00:00 AM")]
        [InlineData("3/14/2027 12:00:00 AM")]
        [InlineData("3/12/2028 12:00:00 AM")]
        [InlineData("3/11/2029 12:00:00 AM")]
        [InlineData("3/10/2030 12:00:00 AM")]
        [InlineData("3/9/2031 12:00:00 AM")]
        [InlineData("3/14/2032 12:00:00 AM")]
        [InlineData("3/13/2033 12:00:00 AM")]
        [InlineData("3/12/2034 12:00:00 AM")]
        [InlineData("3/11/2035 12:00:00 AM")]
        [InlineData("3/9/2036 12:00:00 AM")]
        [InlineData("3/8/2037 12:00:00 AM")]
        [InlineData("3/14/2038 12:00:00 AM")]
        [InlineData("3/13/2039 12:00:00 AM")]
        [InlineData("3/11/2040 12:00:00 AM")]
        [InlineData("3/10/2041 12:00:00 AM")]
        [InlineData("3/9/2042 12:00:00 AM")]
        [InlineData("3/8/2043 12:00:00 AM")]
        [InlineData("3/13/2044 12:00:00 AM")]
        [InlineData("3/12/2045 12:00:00 AM")]
        [InlineData("3/11/2046 12:00:00 AM")]
        [InlineData("3/10/2047 12:00:00 AM")]
        [InlineData("3/8/2048 12:00:00 AM")]
        [InlineData("3/14/2049 12:00:00 AM")]
        [InlineData("3/13/2050 12:00:00 AM")]
        [InlineData("3/12/2051 12:00:00 AM")]
        [InlineData("3/10/2052 12:00:00 AM")]
        [InlineData("3/9/2053 12:00:00 AM")]
        public void SecondSundayOfMarch_Valid(string dtStr)
        {
            // Arrange
            var expected = DateTime.Parse(dtStr);

            // Act
            var actual = DateTimeUtils.SecondSundayOfMarch(expected.Year);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3/1/2024 12:00:00 AM")]
        [InlineData("3/19/2025 12:00:00 AM")]
        [InlineData("11/8/2026 12:00:00 AM")]
        [InlineData("11/14/2027 12:00:00 AM")]
        [InlineData("11/12/2028 12:00:00 AM")]
        [InlineData("11/11/2029 12:00:00 AM")]
        [InlineData("11/10/2030 12:00:00 AM")]
        [InlineData("11/9/2031 12:00:00 AM")]
        [InlineData("11/14/2032 12:00:00 AM")]
        [InlineData("11/13/2033 12:00:00 AM")]
        [InlineData("11/12/2034 12:00:00 AM")]
        [InlineData("11/11/2035 12:00:00 AM")]
        [InlineData("11/9/2036 12:00:00 AM")]
        [InlineData("11/8/2037 12:00:00 AM")]
        [InlineData("10/14/2038 12:00:00 AM")]
        [InlineData("11/13/2039 12:00:00 AM")]
        [InlineData("11/11/2040 12:00:00 AM")]
        [InlineData("1/10/2041 12:00:00 AM")]
        [InlineData("11/9/2042 12:00:00 AM")]
        [InlineData("11/8/2043 12:00:00 AM")]
        [InlineData("11/13/2044 12:00:00 AM")]
        [InlineData("11/12/2045 12:00:00 AM")]
        [InlineData("11/11/2046 12:00:00 AM")]
        [InlineData("11/10/2047 12:00:00 AM")]
        [InlineData("11/8/2048 12:00:00 AM")]
        [InlineData("11/14/2049 12:00:00 AM")]
        [InlineData("11/13/2050 12:00:00 AM")]
        [InlineData("11/12/2051 12:00:00 AM")]
        [InlineData("11/10/2052 12:00:00 AM")]
        [InlineData("11/9/2053 12:00:00 AM")]
        public void SecondSundayOfMarch_Invalid(string dtStr)
        {
            // Arrange
            var expected = DateTime.Parse(dtStr);

            // Act
            var actual = DateTimeUtils.SecondSundayOfMarch(expected.Year);

            // Assert
            Assert.NotEqual(expected, actual);
        }

        [Theory]
        [InlineData("03/26/2023 12:00:00 AM")]
        [InlineData("03/31/2024 12:00:00 AM")]
        [InlineData("03/30/2025 12:00:00 AM")]
        [InlineData("03/29/2026 12:00:00 AM")]
        [InlineData("03/28/2027 12:00:00 AM")]
        [InlineData("03/26/2028 12:00:00 AM")]
        [InlineData("03/25/2029 12:00:00 AM")]
        [InlineData("03/31/2030 12:00:00 AM")]
        [InlineData("03/30/2031 12:00:00 AM")]
        [InlineData("03/28/2032 12:00:00 AM")]
        [InlineData("03/27/2033 12:00:00 AM")]
        [InlineData("03/26/2034 12:00:00 AM")]
        [InlineData("03/25/2035 12:00:00 AM")]
        [InlineData("03/30/2036 12:00:00 AM")]
        [InlineData("03/29/2037 12:00:00 AM")]
        [InlineData("03/28/2038 12:00:00 AM")]
        [InlineData("03/27/2039 12:00:00 AM")]
        [InlineData("03/25/2040 12:00:00 AM")]
        [InlineData("03/31/2041 12:00:00 AM")]
        [InlineData("03/30/2042 12:00:00 AM")]
        [InlineData("03/29/2043 12:00:00 AM")]
        [InlineData("03/27/2044 12:00:00 AM")]
        [InlineData("03/26/2045 12:00:00 AM")]
        [InlineData("03/25/2046 12:00:00 AM")]
        [InlineData("03/31/2047 12:00:00 AM")]
        [InlineData("03/29/2048 12:00:00 AM")]
        [InlineData("03/28/2049 12:00:00 AM")]
        [InlineData("03/27/2050 12:00:00 AM")]
        [InlineData("03/26/2051 12:00:00 AM")]
        [InlineData("03/31/2052 12:00:00 AM")]
        public void LastSundayOfMarch_Valid(string dtStr)
        {
            // Arrange
            var expected = DateTime.Parse(dtStr);

            // Act
            var actual = DateTimeUtils.LastSundayOfMarch(expected.Year);

            // Assert
            Assert.Equal(expected, actual);
        }
        [Theory]
        [InlineData("11/26/2023 12:00:00 AM")]
        [InlineData("11/30/2025 12:00:00 AM")]
        [InlineData("11/29/2026 12:00:00 AM")]
        [InlineData("11/28/2027 12:00:00 AM")]
        [InlineData("11/26/2028 12:00:00 AM")]
        [InlineData("11/25/2029 12:00:00 AM")]
        [InlineData("11/30/2031 12:00:00 AM")]
        [InlineData("11/28/2032 12:00:00 AM")]
        [InlineData("11/27/2033 12:00:00 AM")]
        [InlineData("11/26/2034 12:00:00 AM")]
        [InlineData("11/25/2035 12:00:00 AM")]
        [InlineData("11/30/2036 12:00:00 AM")]
        [InlineData("11/29/2037 12:00:00 AM")]
        [InlineData("11/28/2038 12:00:00 AM")]
        [InlineData("11/27/2039 12:00:00 AM")]
        [InlineData("11/25/2040 12:00:00 AM")]
        [InlineData("11/30/2042 12:00:00 AM")]
        [InlineData("11/29/2043 12:00:00 AM")]
        [InlineData("11/27/2044 12:00:00 AM")]
        [InlineData("11/26/2045 12:00:00 AM")]
        [InlineData("11/25/2046 12:00:00 AM")]
        [InlineData("11/29/2048 12:00:00 AM")]
        [InlineData("11/28/2049 12:00:00 AM")]
        [InlineData("11/27/2050 12:00:00 AM")]
        [InlineData("11/26/2051 12:00:00 AM")]
        public void LastSundayOfMarch_Invalid(string dtStr)
        {
            // Arrange
            var expected = DateTime.Parse(dtStr);

            // Act
            var actual = DateTimeUtils.LastSundayOfMarch(expected.Year);

            // Assert
            Assert.NotEqual(expected, actual);
        }

        [Theory]
        [InlineData("10/29/2023 12:00:00 AM")]
        [InlineData("10/27/2024 12:00:00 AM")]
        [InlineData("10/26/2025 12:00:00 AM")]
        [InlineData("10/25/2026 12:00:00 AM")]
        [InlineData("10/31/2027 12:00:00 AM")]
        [InlineData("10/29/2028 12:00:00 AM")]
        [InlineData("10/28/2029 12:00:00 AM")]
        [InlineData("10/27/2030 12:00:00 AM")]
        [InlineData("10/26/2031 12:00:00 AM")]
        [InlineData("10/31/2032 12:00:00 AM")]
        [InlineData("10/30/2033 12:00:00 AM")]
        [InlineData("10/29/2034 12:00:00 AM")]
        [InlineData("10/28/2035 12:00:00 AM")]
        [InlineData("10/26/2036 12:00:00 AM")]
        [InlineData("10/25/2037 12:00:00 AM")]
        [InlineData("10/31/2038 12:00:00 AM")]
        [InlineData("10/30/2039 12:00:00 AM")]
        [InlineData("10/28/2040 12:00:00 AM")]
        [InlineData("10/27/2041 12:00:00 AM")]
        [InlineData("10/26/2042 12:00:00 AM")]
        [InlineData("10/25/2043 12:00:00 AM")]
        [InlineData("10/30/2044 12:00:00 AM")]
        [InlineData("10/29/2045 12:00:00 AM")]
        [InlineData("10/28/2046 12:00:00 AM")]
        [InlineData("10/27/2047 12:00:00 AM")]
        [InlineData("10/25/2048 12:00:00 AM")]
        [InlineData("10/31/2049 12:00:00 AM")]
        [InlineData("10/30/2050 12:00:00 AM")]
        [InlineData("10/29/2051 12:00:00 AM")]
        [InlineData("10/27/2052 12:00:00 AM")]
        public void LastSundayOfOctober_Valid(string dtStr)
        {
            // Arrange
            var expected = DateTime.Parse(dtStr);

            // Act
            var actual = DateTimeUtils.LastSundayOfOctober(expected.Year);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3/29/2023 12:00:00 AM")]
        [InlineData("3/27/2024 12:00:00 AM")]
        [InlineData("3/26/2025 12:00:00 AM")]
        [InlineData("3/25/2026 12:00:00 AM")]
        [InlineData("3/31/2027 12:00:00 AM")]
        [InlineData("3/29/2028 12:00:00 AM")]
        [InlineData("3/28/2029 12:00:00 AM")]
        [InlineData("3/27/2030 12:00:00 AM")]
        [InlineData("3/26/2031 12:00:00 AM")]
        [InlineData("3/31/2032 12:00:00 AM")]
        [InlineData("3/30/2033 12:00:00 AM")]
        [InlineData("3/29/2034 12:00:00 AM")]
        [InlineData("3/28/2035 12:00:00 AM")]
        [InlineData("3/26/2036 12:00:00 AM")]
        [InlineData("3/25/2037 12:00:00 AM")]
        [InlineData("3/31/2038 12:00:00 AM")]
        [InlineData("3/30/2039 12:00:00 AM")]
        [InlineData("3/28/2040 12:00:00 AM")]
        [InlineData("3/27/2041 12:00:00 AM")]
        [InlineData("3/26/2042 12:00:00 AM")]
        [InlineData("3/25/2043 12:00:00 AM")]
        [InlineData("3/30/2044 12:00:00 AM")]
        [InlineData("3/29/2045 12:00:00 AM")]
        [InlineData("3/28/2046 12:00:00 AM")]
        [InlineData("3/27/2047 12:00:00 AM")]
        [InlineData("3/25/2048 12:00:00 AM")]
        [InlineData("3/31/2049 12:00:00 AM")]
        [InlineData("3/30/2050 12:00:00 AM")]
        [InlineData("3/29/2051 12:00:00 AM")]
        [InlineData("3/27/2052 12:00:00 AM")]
        public void LastSundayOfOctober_Invalid(string dtStr)
        {
            // Arrange
            var expected = DateTime.Parse(dtStr);

            // Act
            var actual = DateTimeUtils.LastSundayOfOctober(expected.Year);

            // Assert
            Assert.NotEqual(expected, actual);
        }

        [Theory]
        [InlineData("1/1/2024 00:00:02", "1/1/2024 00:01:00")]
        [InlineData("12/31/2023 23:59:02", "1/1/2024 00:00:00")]
        public void RoundUp_1Min(string dtStr, string expectedStr)
        {
            // Arrange
            var dt = DateTime.Parse(dtStr);
            var expected = DateTime.Parse(expectedStr);

            // Act
            var actual = DateTimeUtils.RoundUp(dt, TimeSpan.FromMinutes(1));

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("1/1/2024 00:00:02", "1/1/2024 00:00:00")]
        [InlineData("12/31/2023 23:59:02", "12/31/2023 23:59:00")]
        public void RoundDown_1Min(string dtStr, string expectedStr)
        {
            // Arrange
            var dt = DateTime.Parse(dtStr);
            var expected = DateTime.Parse(expectedStr);

            // Act
            var actual = DateTimeUtils.RoundDown(dt, TimeSpan.FromMinutes(1));

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("1/1/2024 00:00:00", "1/1/2024 00:00:00")]
        [InlineData("1/1/2024 00:01:00", "1/1/2024 00:00:00")]
        [InlineData("1/1/2024 00:02:00", "1/1/2024 00:00:00")]
        [InlineData("1/1/2024 00:03:00", "1/1/2024 00:05:00")]
        [InlineData("1/1/2024 00:04:00", "1/1/2024 00:05:00")]
        [InlineData("1/1/2024 00:05:00", "1/1/2024 00:05:00")]
        public void RoundToNearest_5Min(string dtStr, string expectedStr)
        {
            // Arrange
            var dt = DateTime.Parse(dtStr);
            var expected = DateTime.Parse(expectedStr);

            // Act
            var actual = DateTimeUtils.RoundToNearest(dt, TimeSpan.FromMinutes(5));

            // Assert
            Assert.Equal(expected, actual);
        }


    }
}
