namespace bc.speedy
{
    public static class Segments
    {
        public const string CreateTableIndicator = "::";
        public const string DropTableIndicator = "^^";
        public const string NullableIndicator = "?";

        public const string FieldDelimiter = ",";

        public const string PrimaryKeyFieldIndicator = "*";

        public const string PrimaryKey = "PRIMARY KEY";
        public const string Null = "NULL";
        public const string NotNull = "NOT NULL";

        public const string DropTableSql = "Drop Table ";
        public const string CreateTableSql = "Create Table ";

        public const string PropsBegin = "(";
        public const string PropsEnd = ")";

        public const string TextFieldSizeBegin = "[";
        public const string TextFieldSizeEnd = "]";

        public const string UnicodePrefix = "n";
    }
}
