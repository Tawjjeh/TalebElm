using System;
using System.Collections.Generic;
using System.Text;

namespace TalebElm.Domain.Entities
{
    public class UserProgress : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid ModuleId { get; set; }
        public bool IsUnlocked { get; set; }
        public bool PassedExam { get; set; }
        public int Score { get; set; }

    }
}
