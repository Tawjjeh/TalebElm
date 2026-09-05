using System;
using System.Collections.Generic;
using System.Text;

namespace TalebElm.Domain.Entities
{
    public class Exam : BaseEntity
    {
        public string Title { get; set; }= string.Empty;
        public int PassThreshold { get; set; }
        public Guid ModuleId { get; set; }
    }
}
