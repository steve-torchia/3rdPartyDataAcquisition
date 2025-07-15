using DP.Base.DateTimeUtilities;
using System;
using Xunit.Sdk;

namespace DP.Base.Test
{
    public class DateTimeUtilsDSTTests
    {
        [Theory]
        [InlineData(2016)]
        [InlineData(2017)]
        [InlineData(2018)]
        [InlineData(2019)]
        [InlineData(2020)]
        [InlineData(2021)]
        [InlineData(2022)]
        [InlineData(2023)]
        [InlineData(2024)]
        public void GetDSTStart(int year)
        {
            // Arrange

            // Act
            var actual = DateTimeUtils.GetDSTStart(year, DSTRegion.NorthAmerica);

            // Assert
            Assert.Equal(DayOfWeek.Sunday, actual.DayOfWeek);
            Assert.True(actual.Day >= 8 && actual.Day <= 14);
            Assert.Equal(actual.Hour, 2);
        }

        [Theory]
        [InlineData(2016)]
        [InlineData(2017)]
        [InlineData(2018)]
        [InlineData(2019)]
        [InlineData(2020)]
        [InlineData(2021)]
        [InlineData(2022)]
        [InlineData(2023)]
        [InlineData(2024)]
        public void GetDSTEnd(int year)
        {
            // Arrange

            // Act
            var actual = DateTimeUtils.GetDSTEnd(year, DSTRegion.NorthAmerica);

            // Assert
            Assert.Equal(DayOfWeek.Sunday, actual.DayOfWeek);
            Assert.True(actual.Day >= 1 && actual.Day <= 7);
            Assert.Equal(actual.Hour, 2);
        }

        [Theory]
        [InlineData("1/3/2024 12:00:00 AM")]
        [InlineData("2/3/2024 12:00:00 AM")]
        [InlineData("3/3/2024 12:00:00 AM")]
        [InlineData("3/10/2024 12:00:00 AM")]   // before 2am, no conversion
        [InlineData("3/10/2024 1:00:00 AM")]    // before 2am, no conversion
        [InlineData("11/3/2024 3:00:00 AM")]    // after 2am, no conversion
        [InlineData("12/3/2024 12:00:00 AM")]
        public void ConvertToDaylightSavingTime_NoConversion(string dateTimeString)
        {
            // Arrange
            var dt = DateTime.Parse(dateTimeString);

            // Act
            var actual = DateTimeUtils.ConvertToDaylightSavingTime(dt, DSTRegion.NorthAmerica);

            // Assert
            Assert.Equal(dt, actual);
        }

        [Theory]
        [InlineData("3/10/2024 2:00:00 AM")]    // after 2am, conversion
        [InlineData("3/10/2024 3:00:00 AM")]    // after 2am, conversion
        [InlineData("4/3/2024 12:00:00 AM")]
        [InlineData("7/3/2024 12:00:00 AM")]
        [InlineData("9/3/2024 12:00:00 AM")]
        [InlineData("10/3/2024 12:00:00 AM")]
        [InlineData("11/3/2024 1:00:00 AM")]    // before 2am, conversion
        [InlineData("11/3/2024 2:00:00 AM")]    // before 2am, conversion
        public void ConvertToDaylightSavingTime_Conversion_NorthAmerica(string dateTimeString)
        {
            // Arrange
            var dt = DateTime.Parse(dateTimeString);

            // Act
            var actual = DateTimeUtils.ConvertToDaylightSavingTime(dt, DSTRegion.NorthAmerica);

            // Assert
            Assert.Equal(new TimeSpan(1, 0, 0), actual - dt);
        }

        [Theory]
        [InlineData("3/31/2024 1:00:00 AM")]    // after 1am, conversion
        [InlineData("3/31/2024 2:00:00 AM")]    // after 1am, conversion
        [InlineData("4/3/2024 12:00:00 AM")]
        [InlineData("7/3/2024 12:00:00 AM")]
        [InlineData("9/3/2024 12:00:00 AM")]
        [InlineData("10/3/2024 12:00:00 AM")]
        [InlineData("10/27/2024 12:00:00 AM")]    // before 1am, conversion
        [InlineData("10/27/2024 1:00:00 AM")]    // before 1am, conversion
        public void ConvertToDaylightSavingTime_Conversion_Europe(string dateTimeString)
        {
            // Arrange
            var dt = DateTime.Parse(dateTimeString);

            // Act
            var actual = DateTimeUtils.ConvertToDaylightSavingTime(dt, DSTRegion.Europe);

            // Assert
            Assert.Equal(new TimeSpan(1, 0, 0), actual - dt);
        }

        [Theory]
        [InlineData("3/10/2024 1:00:00 AM", "3/10/2024 1:00:00 AM")]    // before change, should match
        [InlineData("3/10/2024 1:59:59 AM", "3/10/2024 1:59:59 AM")]    // before change, should match
        [InlineData("3/10/2024 3:00:00 AM", "3/10/2024 2:00:00 AM")]    // after change, DST is 1hr ahead
        [InlineData("3/10/2024 3:00:01 AM", "3/10/2024 2:00:01 AM")]    // after change, DST is 1hr ahead
        public void DST_to_ST_MarchBoundaries(string inputDstString, string expectedStandardTimeString)
        {
            // Arrange
            var inputDst = DateTime.Parse(inputDstString);
            var expectedStandardTime = DateTime.Parse(expectedStandardTimeString);

            // Act
            var actualStandardTime = DateTimeUtils.ConvertToStandardTime(inputDst, DSTRegion.NorthAmerica);

            // Assert
            Assert.Equal(expectedStandardTime, actualStandardTime);
        }

        [Theory]
        [InlineData("3/10/2024 2:00:00 AM")]
        [InlineData("3/10/2024 2:01:00 AM")]
        [InlineData("3/10/2024 2:59:59 AM")]
        public void DSTMarch_StartHour_DoesNotExist_NorthAmerica(string dateTimeString)
        {
            var dt = DateTime.Parse(dateTimeString);
            Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeUtils.ConvertToStandardTime(dt, DSTRegion.NorthAmerica));
        }

        [Theory]
        [InlineData("3/31/2024 1:00:00 AM")]
        [InlineData("3/31/2024 1:01:00 AM")]
        [InlineData("3/31/2024 1:59:59 AM")]
        public void DSTMarch_StartHour_DoesNotExist_Europe(string dateTimeString)
        {
            var dt = DateTime.Parse(dateTimeString);
            Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeUtils.ConvertToStandardTime(dt, DSTRegion.Europe));
        }

        [Theory]
        [InlineData("11/3/2024 1:00:00 AM", "11/3/2024 12:00:00 AM")]
        [InlineData("11/3/2024 1:00:01 AM", "11/3/2024 12:00:01 AM")]
        [InlineData("11/3/2024 1:59:59 AM", "11/3/2024 12:59:59 AM")]
        public void DSTNovember_DuplicatedHour_IsSecondDuplicateHour_False(string inputDstString, string expectedStandardTimeString)
        {
            // Arrange
            var inputDst = DateTime.Parse(inputDstString);
            var expectedStandardTime = DateTime.Parse(expectedStandardTimeString);

            // Act
            var actualStandardTime = DateTimeUtils.ConvertToStandardTime(inputDst, DSTRegion.NorthAmerica, isSecondDuplicateHour: false);

            // Assert
            Assert.Equal(expectedStandardTime, actualStandardTime);
        }

        [Theory]
        [InlineData("11/3/2024 1:00:00 AM", "11/3/2024 1:00:00 AM")]
        [InlineData("11/3/2024 1:00:01 AM", "11/3/2024 1:00:01 AM")]
        [InlineData("11/3/2024 1:59:59 AM", "11/3/2024 1:59:59 AM")]
        public void DSTNovember_DuplicatedHour_IsSecondDuplicateHour_True(string inputDstString, string expectedStandardTimeString)
        {
            // Arrange
            var inputDst = DateTime.Parse(inputDstString);
            var expectedStandardTime = DateTime.Parse(expectedStandardTimeString);

            // Act
            var actualStandardTime = DateTimeUtils.ConvertToStandardTime(inputDst, DSTRegion.NorthAmerica, isSecondDuplicateHour: true);

            // Assert
            Assert.Equal(expectedStandardTime, actualStandardTime);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DSTNovember_DuplicatedHour_IsSecondDuplicateHour_DoesNotMatterAtTimeChange(bool isSecondDuplicateHour)
        {
            // The only duplicated hour is the one with Hour = 1.  In other words, 1:59:59 would be duplicated but 2:00:00 would not
            // Verify that 2:00 is not converted no matter what isSecondDuplicateHour is 

            // Arrange
            var inputDst = DateTime.Parse("11/3/2024 2:00:00 AM");
            var expectedStandardTime = DateTime.Parse("11/3/2024 2:00:00 AM");

            // Act
            var actualStandardTime = DateTimeUtils.ConvertToStandardTime(inputDst, DSTRegion.NorthAmerica, isSecondDuplicateHour);

            // Assert
            Assert.Equal(expectedStandardTime, actualStandardTime);
        }
    }
}
