using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerApplication
{
    public enum Status { Running, Suspended, Disabled, Unknown };

    [Serializable]
    public class Service
    {
        [XmlElement]
        public int Id { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public Status Status { get; set; }

        public Service()
        {
            Id = -1;
            Name = "unknown";
            Status = (Status)3;
        }

        public Service(int id, string name, Status status)
        {
            Id = id;
            Name = name;
            Status = status;
        }

        public override string ToString()
        {
            return $"{Id},{Name},{(int)Status}";
        }
    }
}
