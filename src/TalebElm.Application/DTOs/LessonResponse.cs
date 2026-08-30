using System;
using System.Collections.Generic;
using System.Text;

namespace TalebElm.Application.DTOs
{
    public record LessonResponse(Guid Id, string Title, Guid ModuleId);
}
