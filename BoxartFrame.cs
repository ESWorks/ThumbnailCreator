using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WPF_Thumbnails
{
    [DataContract(Name = "BoxArtFrame")]
    public struct BoxArtFrame
    {
        [DataMember(Name = "Width", IsRequired = true)]
        public int Width { get; set; }
        [DataMember(Name = "Height", IsRequired = true)]
        public int Height { get; set; }
        [DataMember(Name = "Name", IsRequired = true)]
        public string Name { get; private set; }

        public BoxArtFrame(int width, int height, string name)
        {
            Name = name;
            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            return Name + " - " + Width + " x " + Height;
        }
    }

    public static class DictionaryExtension
    {
        public static string GetXmlString(this IDictionary dict)
        {
            DataContractSerializer serializer = new DataContractSerializer(dict.GetType());

            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter writer = new XmlTextWriter(sw))
                {
                    // add formatting so the XML is easy to read in the log
                    writer.Formatting = Formatting.Indented;

                    serializer.WriteObject(writer, dict);

                    writer.Flush();

                    return sw.ToString();

                }
            }
        }

        public static IDictionary GetXmlDictionary(this Type type, string xml)
        {
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Dictionary<,>))
                throw new InvalidCastException("Can't cast type to generic dictionary!");

            DataContractSerializer serializer = new DataContractSerializer(type);

            using (StringReader sw = new StringReader(xml))
            {
                using (XmlTextReader writer = new XmlTextReader(sw))
                {
                    return (IDictionary)serializer.ReadObject(writer);
                }
            }
        }
        public static string ReadXmlToString(string location)
        {
            try
            {
                return File.ReadAllText(location);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public static void SaveXmlToFile(this string xml, string location)
        {
            try
            {
                File.WriteAllText(location, xml);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

}
