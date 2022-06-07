namespace bc.speedy
{
    public sealed class FieldData
    {
        public string Name { get; set; }
        public string DataType { get; set; }

        public bool IsRequired { get; set; } = true;
        public bool IsPrimaryKey { get; set; } = false;

        public string Default { get; set; } = "";
    }
}