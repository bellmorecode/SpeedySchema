using System;

namespace bc.speedy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
        }
    }

    public sealed class SchemaGenerator
    {
        public SchemaSettings Settings { get; set; } = null;
        public SchemaGenerator()
        {
            Settings = new SchemaSettings();
        }
    }
    
    public sealed class SchemaSettings
    {
        public SchemaSettings()
        {
            // TODO: get valus from config file. 
        }

        public string TableOwner { get; set; } = "dbo";
        public bool AlwaysBracketIdentifiers { get; set; } = true;
    }
}
