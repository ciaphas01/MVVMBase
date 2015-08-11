using System;
using System.Collections.Generic;
using System.Linq;

namespace MVVMBase
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class INPCDependsAttribute : Attribute
    {
        public List<string> TriggerProperties;
        
        public INPCDependsAttribute(params string[] properties)
        {
            TriggerProperties = properties.ToList();
        }
    }
}
