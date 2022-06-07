using System;
using System.Collections.Generic;

namespace bc.speedy
{
    public sealed class SchemaGenerator
    {

        public SchemaSettings Settings { get; set; } = null;
        public SchemaGenerator()
        {
            Settings = new SchemaSettings();
        }

        public string Parse(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException("input");
            }
            if (input.StartsWith(Segments.CreateTableIndicator))
            {
                return ParseCreateTable(input.Substring(Segments.CreateTableIndicator.Length));
            }
            if (input.StartsWith(Segments.DropTableIndicator))
            {
                return ParseDropTable(input.Substring(Segments.DropTableIndicator.Length));
            }
            throw new InvalidOperationException("Unknown table operation. Please see the README file.");
        }

        private string ParseCreateTable(string input)
        {
            if (input.IndexOf(">") > -1)
            {
                var tablename = input.Substring(0, input.IndexOf(">"));
                var tableExpr = (Settings.AlwaysBracketIdentifiers) ? $"[{Settings.TableOwner}].[{tablename}] " : $"{Settings.TableOwner}.${tablename} ";
                var fields = ParseFieldList(input.Substring(tablename.Length + 1));
                return $"{Segments.CreateTableSql}{tableExpr} ({fields})";
            }
            throw new InvalidOperationException("Unable to identify table name, missing '>'.");
        }

        private string ParseDropTable(string input)
        {
            if (input.IndexOf(">") > -1)
            {
                var tablename = input.Substring(0, input.IndexOf(">"));
                return (Settings.AlwaysBracketIdentifiers) ? $"{Segments.DropTableSql}[{Settings.TableOwner}].[{tablename}]" : $"{Segments.DropTableSql}{Settings.TableOwner}.{tablename}";
            } 
            else
            {
                return (Settings.AlwaysBracketIdentifiers) ? $"{Segments.DropTableSql}[{input.Trim()}]" : $"{Segments.DropTableSql}{input.Trim()}";
            }
                
        }

        private string ParseFieldList(string input)
        {
            var field_list = new List<string>();
            var fields = input.Split(Segments.FieldDelimiter);
            foreach(var field in fields)
            {
                var parsed = ParseField(field.Trim());
                //Console.WriteLine($"{field}, {parsed}");
                field_list.Add(parsed);
            }
            return String.Join(", ", field_list);
        }
        private string ParseField(string input)
        {
            //Console.WriteLine(input);

            // field parse
            var field_data = GetFieldData(input);

            // field t-sql formatter.
            var output = FormatField(field_data);

            return output;
        }

        private string FormatField(FieldData data)
        {
            var isRequired = data.IsPrimaryKey ? Segments.PrimaryKey : (data.IsRequired ? Segments.NotNull : Segments.Null);
            var defaultExpr = string.IsNullOrWhiteSpace(data.Default) ? string.Empty : $" {data.Default}";
            var nameExpr = Settings.AlwaysBracketIdentifiers ? $"[{data.Name}]" : $"{data.Name}";
            return  $"{nameExpr} {data.DataType} {isRequired}{defaultExpr}";
        }

        private string GetEncodingPrefix ()
        {
            return Settings.DefaultEncoding == System.Text.Encoding.UTF8 ? Segments.UnicodePrefix : string.Empty;
        }

        private string TextFieldInfo(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var prefix = GetEncodingPrefix();
            input = input.Replace("text", $"{prefix}varchar");
            input = input.Replace(Segments.TextFieldSizeBegin, Segments.PropsBegin);
            input = input.Replace(Segments.TextFieldSizeEnd, Segments.PropsEnd);
            return input;
        }

        private FieldData GetFieldData(string input)
        {
            var data = new FieldData();
            data.Name = "Field1";
            data.DataType = "varchar(500)";
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                var fieldExpr = input;
                if (fieldExpr.StartsWith(Segments.PrimaryKeyFieldIndicator))
                {
                    data.IsPrimaryKey = true;
                    fieldExpr = fieldExpr.Substring(Segments.PrimaryKeyFieldIndicator.Length);
                }
                string field_props = "";
                if (fieldExpr.IndexOf(Segments.PropsBegin) > -1 
                    && fieldExpr.IndexOf(Segments.PropsBegin) < fieldExpr.IndexOf(Segments.PropsEnd))
                {
                    var start = fieldExpr.IndexOf(Segments.PropsBegin) + 1;
                    var end = fieldExpr.LastIndexOf(Segments.PropsEnd);
                    field_props = fieldExpr.Substring(start, end-start);
                    
                    // parse props
                    if (field_props.IndexOf(Segments.NullableIndicator) > -1)
                    {
                        data.IsRequired = false;
                        data.DataType = TextFieldInfo(field_props.Replace(Segments.NullableIndicator, String.Empty));
                    } 
                    else
                    {
                        data.DataType = TextFieldInfo(field_props);
                    }

                }
                data.Name = field_props.Length == 0 ? fieldExpr : fieldExpr.Substring(0, fieldExpr.IndexOf(Segments.PropsBegin));
                return data;
            }

            return data;
        }
    }
}
