using System;
using System.Collections.Generic;
using System.Text;

namespace TalebElm.Domain.Entities
{
    public class Module
    {
        public Guid Id { get; set; }
        public string Title{ get; set; }= string.Empty;
        public string Summary { get; set; }=string.Empty;
        public int Order { get; set; }
        public Guid TrackId { get; set; }

    }
}
