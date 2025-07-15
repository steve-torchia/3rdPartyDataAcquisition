using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.ComponentModel.Serialization
{
    public class CsvSerializer<T> where T : class, new()
    {
        public char Separator { get; set; }
        
        private List<PropertyInfo> properties;

        public CsvSerializer(char separator)
        {
            var type = typeof(T);

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance
                 | BindingFlags.GetProperty | BindingFlags.SetProperty);

            this.properties = (from a in props
                               where a.GetCustomAttribute<CsvIgnoreAttribute>() == null
                           orderby a.Name
                           select a).ToList();

            this.Separator = separator;
        }

        public void Serialize(Stream stream, IList<T> data)
        {
            var sb = new StringBuilder();
            var values = new List<string>();

            sb.AppendLine(this.GetHeader());
            
            foreach (var item in data)
            {
                values.Clear();

                foreach (var p in this.properties)
                {
                    var raw = p.GetValue(item);
                    var value = raw == null ?
                                string.Empty :
                                raw.ToString();
                    values.Add(value);
                }

                sb.AppendLine(string.Join(this.Separator.ToString(), values.ToArray()));
            }

            using (var sw = new StreamWriter(stream))
            {
                sw.Write(sb.ToString().Trim());
            }
        }

        public IList<T> Deserialize(Stream stream)
        {
            string[] columns;
            string[] rows;

            try
            {
                using (var sr = new StreamReader(stream))
                {
                    columns = sr.ReadLine().Split(this.Separator);
                    rows = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("The CSV File is Invalid. See Inner Exception for more inoformation.", ex);
            }

            var data = new List<T>();
            for (int row = 0; row < rows.Length; row++)
            {
                var line = rows[row];
                if (string.IsNullOrWhiteSpace(line))
                {
                    //throw new Exception(string.Format(@"Error: Empty line at line number: {0}", row));
                    continue;
                }

                var parts = line.Split(this.Separator);

                var datum = new T();
                for (int i = 0; i < parts.Length; i++)
                {
                    var value = parts[i];
                    var column = columns[i];
                    
                    var p = this.properties.First(a => a.Name == column);

                    var converter = TypeDescriptor.GetConverter(p.PropertyType);
                    var convertedvalue = converter.ConvertFrom(value);

                    p.SetValue(datum, convertedvalue);
                }

                data.Add(datum);
            }

            return data;
        }

        private string GetHeader()
        {
            var columns = this.properties.Select(a => a.Name).ToArray();
            var header = string.Join(this.Separator.ToString(), columns);
            return header;
        }
    }

    public class CsvIgnoreAttribute : Attribute
    {
    }
}
