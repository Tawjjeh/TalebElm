using System;
using System.Collections.Generic;
using System.Text;

namespace TalebElm.Application.Interfaces
{
    public interface IApplicationDbContext { Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); }
}
