using SpeedySchema.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SpeedySchema.Engine
{
    public sealed class QueryParser
    {
        #region internal bits
        public static class QueryTokens
        {
            public const string TableIndicator = "::";
            public const string FieldsIndicator = ">";
            public const string PrimaryKeyIndicator = "*";
            public const string FieldSeparator = ",";
            public const string FieldTypeDefStartIndicator = "(";
            public const string FieldTypeDefEndIndicator = ")";
            public const string FieldPrecisionStartIndicator = "[";
            public const string FieldPrecisionEndIndicator = "]";
            public const string FieldNullableIndicator = "?";
        }

        internal sealed class FieldReference
        {
            private string _dataType;
            public FieldReference()
            {
                this.Name = "Field1";
                this.DataType = "NVARCHAR";
                this.Precision = 100;
                this.IsNullable = false;
            }
            public string Name { get; set; }
            public string DataType 
            {
                get { return _dataType; }
                set { _dataType = value; __setPrecisionDefaults(value); }
            }

            private void __setPrecisionDefaults(string datatype)
            {
                switch (datatype.ToLower())
                {
                    case "date":
                    case "datetime":
                    case "double":
                    case "int":
                    case "integer":
                    case "float":
                    case "bool":
                    case "bit":
                    case "image":
                        this.Precision = -1;
                        break;
                }
            }
            public int Precision { get; set; }
            public int Precision2 { get; set; }
            public bool IsNullable { get; set; }
            public string DefaultExpression { get; set; }

            public override string ToString()
            {
                var expr = string.Format("[{0}] {1}", this.Name, this.DataType);
                if (this.Precision > 0)
                {
                    expr += string.Format("({0})", this.Precision);
                }

                expr += string.Format(" {0}", this.IsNullable ? "NULL" : "NOT NULL");

                if (!string.IsNullOrWhiteSpace(this.DefaultExpression))
                {
                    expr += (" " + this.DefaultExpression);
                }

                return expr;
            }
        }
        #endregion

        public string Parse(string request)
        {
            try
            {
                // control logic
                var parts = request.Split(
                    new[] { QueryTokens.TableIndicator }, 
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                {
                    return __CreateTablesSql(parts);
                }
            }
            catch(Exception ex)
            {
                return string.Format("error parsing: {0}", ex);
            }
            return "Unable to parse";
        }

        private string __CreateTablesSql(string[] parts)
        {
            var sb = new StringBuilder();
            foreach (var phrase in parts)
            {
                if (phrase.Contains(QueryTokens.FieldsIndicator))
                {
                    sb.AppendLine(__CreateTableSql(phrase));
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendFormat("Failed to parse. Expected: '{0}' after tablename.  Cannot locate fields.", QueryTokens.FieldsIndicator);
                    sb.AppendLine();
                }
                
            }
            return sb.ToString();
        }

        private string __CreateTableSql(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase)) throw new ArgumentNullException("phrase");

            var sb = new StringBuilder();

            var tablename = phrase.Substring(0, phrase.IndexOf(QueryTokens.FieldsIndicator));
            var fields = new Dictionary<string, FieldReference>();

            // trim the table name from the content.
            phrase = phrase.Substring(phrase.IndexOf(QueryTokens.FieldsIndicator)+1);
            
            sb.Append("CREATE TABLE");
            sb.AppendFormat(" [{0}] ", tablename);
            sb.Append("( \r\n\t");


            //// TESTING CODE -- REMOVE LATER
            //sb.AppendLine();
            //sb.Append("=============================");
            //sb.AppendLine();
            //sb.Append(phrase);

            // add LSS defaults
            fields.AddQuick("Id", "uniqueidentifier", defaultexpr: "(new())");
            fields.AddMultiple("datetime", false, "(getdate())", "CreatedOn", "UpdatedOn");
            fields.AddMultiple("nvarchar", false, "", "CreatedBy", "UpdatedBy");

            // take the remaining text and expect that it represents a comma-delimited list
            // containing definitions for field
            phrase.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(q => (q ?? "").Trim())
                .ToList()
                .ForEach(q => fields.ParseFieldReference(q));

            var fieldsSql = string.Join(", \r\n\t", fields.Select(q => q.Value.ToString()).ToArray());
            
            sb.Append(fieldsSql);
            sb.AppendLine();

            sb.Append(") ");
            sb.AppendLine();
                   
            return sb.ToString();
        }
    }

    internal static class FieldReferenceDictionaryExtensions
    {
        internal static void ParseFieldReference(this Dictionary<string, QueryParser.FieldReference> list, string phrase)
        {
            
            var fld = new QueryParser.FieldReference();
            ParseFieldReference_recur(list, phrase, fld);
        }

        private static void ParseFieldReference_recur(this Dictionary<string, QueryParser.FieldReference> list, string phrase, QueryParser.FieldReference state)
        {
            if (phrase.StartsWith(QueryParser.QueryTokens.PrimaryKeyIndicator))
            {
                // This is a hack to 'skip over' any field denoted as a PK.  
                // We will use the default 'Id' column for now and will track the PK in future versions.
                phrase = phrase.Substring(1);
                return;
            }

            if (phrase.Contains(QueryParser.QueryTokens.FieldNullableIndicator))
            {
                state.IsNullable = true;
                phrase = phrase.Replace(QueryParser.QueryTokens.FieldNullableIndicator, string.Empty);
            }

            if (phrase.Contains(QueryParser.QueryTokens.FieldTypeDefStartIndicator) && phrase.Contains(QueryParser.QueryTokens.FieldTypeDefEndIndicator))
            {
                Func<string, string> _parsefieldtypealias = q => {
                    var result = q;
                    var section = ConfigurationManager.GetSection("SpeedySchema/TypeMapper") as DataTypeMappingConfigurationSection;
                    foreach (var dt in section.DataTypes.Cast<DataTypeMappingElement>())
                    {
                        var lowres = result.ToLower();
                        var target_lowres = dt.UserType.ToLower();
                        if (lowres.Equals(target_lowres))
                        {
                            result = dt.SQLType;
                        }
                    }
                    return result;
                };

                // find parens -> (field)
                var formattedtypename = "NVARCHAR";
                
                var startpos = phrase.IndexOf(QueryParser.QueryTokens.FieldTypeDefStartIndicator);
                var endpos = phrase.IndexOf(QueryParser.QueryTokens.FieldTypeDefEndIndicator);
                var length = endpos - startpos + 1; // including parens
                if (startpos > -1 && endpos > -1)
                {
                    var typedef = phrase.Substring(startpos, length);
                    phrase = phrase.Replace(typedef, string.Empty);
                    formattedtypename = typedef.Replace(QueryParser.QueryTokens.FieldTypeDefStartIndicator, string.Empty).Replace(QueryParser.QueryTokens.FieldTypeDefEndIndicator, string.Empty);
                    // find precision indicator
                    var p_startpos = formattedtypename.IndexOf(QueryParser.QueryTokens.FieldPrecisionStartIndicator);
                    var p_endpos = formattedtypename.IndexOf(QueryParser.QueryTokens.FieldPrecisionEndIndicator);
                    if (p_startpos > -1 && p_endpos > -1)
                    {
                        var p_length = p_endpos - p_startpos + 1; // including brackets surrounding precision. 
                        var prec = formattedtypename.Substring(p_startpos, p_length);
                        formattedtypename = formattedtypename.Replace(prec, string.Empty);
                        var trimmed_prec = prec.Replace(QueryParser.QueryTokens.FieldPrecisionStartIndicator, string.Empty).Replace(QueryParser.QueryTokens.FieldPrecisionEndIndicator, string.Empty);
                        int outval;
                        if (int.TryParse(trimmed_prec, out outval))
                        {
                            state.Precision = outval;
                        }
                    }

                    // deal with field aliases
                    formattedtypename = _parsefieldtypealias(formattedtypename);

                    if (string.IsNullOrWhiteSpace(formattedtypename))
                    {
                        formattedtypename = "NVARCHAR";
                    }
                }
                
                state.DataType = formattedtypename;
            }

            state.Name = phrase;
            list.Add(state.Name, state);
        }

        internal static void AddQuick(this Dictionary<string, QueryParser.FieldReference> list, string name, string datatype = "string", bool canbenull = false, string defaultexpr = null)
        {
            var item = new QueryParser.FieldReference { Name = name, IsNullable = canbenull, DataType = datatype, DefaultExpression = defaultexpr };
            list.Add(name, item);
        }

        internal static void AddMultiple(this Dictionary<string, QueryParser.FieldReference> list, string datatype, bool canbenull, string defaultexpr, params string[] fields)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (fields == null) throw new ArgumentNullException("fields");
            foreach (var item in fields)
            {
                list.AddQuick(item, datatype, canbenull, defaultexpr);
            }
        }
    }
}
