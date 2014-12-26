using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SpeedySchema.Config
{
    public sealed class DataTypeMappingConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("DataTypes", IsRequired = true)]
        public DataTypeMappingElementCollection DataTypes
        {
            get { return (DataTypeMappingElementCollection)this["DataTypes"]; }
            set { this["DataTypes"] = value; }
        }

        [ConfigurationProperty("ConnectionInfo", IsRequired = true)]
        public ConnectionInfoElement ConnectionInfo
        {
            get { return (ConnectionInfoElement)this["ConnectionInfo"]; }
            set { this["ConnectionInfo"] = value; }
        }
    }

    public class ConnectionInfoElement : ConfigurationElement
    {
        [ConfigurationProperty("Source", IsRequired = true)]
        public string Source
        {
            get { return (string)this["Source"]; }
        }

    }

    [ConfigurationCollection(typeof(DataTypeMappingElement), AddItemName = "DataType")]
    public sealed class DataTypeMappingElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataTypeMappingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as DataTypeMappingElement).UserType;
        }
    }

    public sealed class DataTypeMappingElement : ConfigurationElement
    {
        [ConfigurationProperty("UserType", IsRequired = true, IsKey = true)]
        public string UserType
        {
            get { return (string)this["UserType"]; }
        }

        [ConfigurationProperty("SQLType", IsRequired = true)]
        public string SQLType
        {
            get { return (string)this["SQLType"]; }
        }

        [ConfigurationProperty("FieldLength", IsRequired = true)]
        public int FieldLength
        {
            get { return (int)this["FieldLength"]; }
        }

    }
}
