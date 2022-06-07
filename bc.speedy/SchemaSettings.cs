using System;
using System.Text;

namespace bc.speedy
{
    public sealed class SchemaSettings
    {
        public SchemaSettings()
        {
            // TODO: get valus from config file. 
        }
        public string TableOwner { get; set; } = "dbo";
        public bool AlwaysBracketIdentifiers { get; set; } = true;
        public Encoding DefaultEncoding = Encoding.UTF8;

        public override string ToString()
        {
            try
            {
                return $"Table Owner: {TableOwner}\nUse brackets: {AlwaysBracketIdentifiers}\nEncoding: {DefaultEncoding}";
            }
            catch(Exception)
            {
                return base.ToString();
            }
            
        }
    }
        

}
