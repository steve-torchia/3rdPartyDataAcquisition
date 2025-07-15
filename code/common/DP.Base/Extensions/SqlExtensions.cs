using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DP.Base.Extensions
{
    public static class SqlExtensions
    {
        /// <summary>
        /// Format of the contents of the table as a json string.
        /// If the table has multiple rows, only the first is retrieved and converted to a json string.
        /// </summary>
        public static string ToJsonObjectString(this DataTable table)
        {
            if (table.Rows.Count <= 0)
            {
                throw new Exception($"There was no data to read");
            }

            return JArray.FromObject(table).First().ToString(Newtonsoft.Json.Formatting.None);
        }

        public static DataTable ConvertToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                var colType = prop.PropertyType.IsEnum ? typeof(string) : prop.PropertyType;
                var colTypeNullable = Nullable.GetUnderlyingType(colType) ?? colType;
                table.Columns.Add(prop.Name, colTypeNullable);
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }
    }
}
