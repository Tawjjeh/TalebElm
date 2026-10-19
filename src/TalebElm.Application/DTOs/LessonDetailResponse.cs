using System;
using System.Collections.Generic;
using System.Text;

namespace TalebElm.Application.DTOs
{
    public record LessonDetailResponse(
     Guid Id,
     Guid ModuleId,
     string Title,
     string Content,    
     int Order,IReadOnlyList<LessonResourceResponse> Resources);
}
