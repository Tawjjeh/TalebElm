using System;
using System.Collections.Generic;
using System.Text;

namespace TalebElm.Domain.Entities
{
    public  class Track :BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
