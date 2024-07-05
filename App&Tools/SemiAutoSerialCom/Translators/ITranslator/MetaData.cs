using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.App.Translators
{
    [AttributeUsage(AttributeTargets.Class)]
    [MetadataAttribute]
    public class MetaData : Attribute
    {
        public string? Name { get; set; }
        /// <summary>
        /// year month day hour minuite, yyyyMMddHHmm
        /// </summary>
        public long Version { get; set; }
        public string? UserLanguage { get; set; }
        public string? MachineLanguage { get; set; }
    }
    public class MetaDataContainer
    {
        [ImportMany]
        public IEnumerable<Lazy<ITranslator, MetaData>>? Container { get; set; }
    }
}
