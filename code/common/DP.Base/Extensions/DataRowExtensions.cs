using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Extensions
{
    public static class DataRowExtensions
    {
        public static string GetString(this DataRow row, int index, bool allowNull = false)
        {
            if (row.ItemArray[index] == DBNull.Value)
            {
                return (allowNull) ? (string)null : throw new Exception($"value cannot be null (index = {index})");
            }

            return (string)row.ItemArray[index].ToString();
        }

        public static long? GetLong(this DataRow row, int index, bool allowNull = false)
        {
            var rowVal = row.ItemArray[index];
            if (rowVal == DBNull.Value || string.IsNullOrEmpty(rowVal.ToString()))
            {
                return (allowNull) ? (long?)null : throw new Exception($"value cannot be null (index = {index})");
            }

            return long.Parse(rowVal.ToString());
        }

        public static int? GetInt(this DataRow row, int index, bool allowNull = false)
        {
            var rowVal = row.ItemArray[index];
            if (rowVal == DBNull.Value || string.IsNullOrEmpty(rowVal.ToString()))
            {
                return (allowNull) ? (int?)null : throw new Exception($"value cannot be null (index = {index})");
            }

            return int.Parse(rowVal.ToString());
        }

        public static bool? GetBool(this DataRow row, int index, bool allowNull = false)
        {
            var rowVal = row.ItemArray[index];
            if (rowVal == DBNull.Value || string.IsNullOrEmpty(rowVal.ToString()))
            {
                return (allowNull) ? (bool?)null : throw new Exception($"value cannot be null (index = {index})");
            }

            return bool.Parse(rowVal.ToString());
        }

        public static DateTime? GetDateTime(this DataRow row, int index, bool allowNull = false)
        {
            var rowVal = row.ItemArray[index];
            if (rowVal == DBNull.Value || string.IsNullOrEmpty(rowVal.ToString()))
            {
                return (allowNull) ? (DateTime?)null : throw new Exception($"value cannot be null (index = {index})");
            }

            return DateTime.Parse(rowVal.ToString());
        }

        public static decimal? GetDecimal(this DataRow row, int index, bool allowNull = false)
        {
            var rowVal = row.ItemArray[index];
            if (rowVal == DBNull.Value || string.IsNullOrEmpty(rowVal.ToString()))
            {
                return (allowNull) ? (decimal?)null : throw new Exception($"value cannot be null (index = {index})");
            }

            return (rowVal is decimal) ? (decimal)rowVal : decimal.Parse(rowVal.ToString());
        }

        public static float? GetSingle(this DataRow row, int index, bool allowNull = false)
        {
            var rowVal = row.ItemArray[index];
            if (rowVal == DBNull.Value || string.IsNullOrEmpty(rowVal.ToString()))
            {
                return (allowNull) ? (float?)null : throw new Exception($"value cannot be null (index = {index})");
            }

            return (rowVal is float) ? (float)rowVal : float.Parse(rowVal.ToString());
        }

        public static double? GetDouble(this DataRow row, int index, bool allowNull = false)
        {
            var rowVal = row.ItemArray[index];
            if (rowVal == DBNull.Value || string.IsNullOrEmpty(rowVal.ToString()))
            {
                return (allowNull) ? (float?)null : throw new Exception($"value cannot be null (index = {index})");
            }

            return (rowVal is double) ? (double)rowVal : double.Parse(rowVal.ToString());
        }

        public static Guid? GetGuid(this DataRow row, int index, bool allowNull = false)
        {
            var rowVal = row.ItemArray[index];
            if (rowVal == DBNull.Value || string.IsNullOrEmpty(rowVal.ToString()))
            {
                return (allowNull) ? (Guid?)null : throw new Exception($"value cannot be null (index = {index})");
            }

            return (rowVal is Guid) ? (Guid)rowVal : Guid.Parse(rowVal.ToString());
        }
    }
}
