using Moq;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Services;

namespace TalebElm.Tests.UnitTests;

public class ExamServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ExamService _sut;

    public ExamServiceTests()
    {
        _sut = new ExamService(_unitOfWork.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExamExists_ReturnsExamResponse()
    {
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            Title = "C# Basics",
            PassThreshold = 60,
            ModuleId = Guid.NewGuid()
        };
        _unitOfWork.Setup(u => u.Exams.GetByIdAsync(exam.Id)).ReturnsAsync(exam);

        var result = await _sut.GetByIdAsync(exam.Id);

        Assert.Equal(exam.Id, result.Id);
        Assert.Equal(exam.Title, result.Title);
        Assert.Equal(exam.PassThreshold, result.PassThreshold);
        Assert.Equal(exam.ModuleId, result.ModuleId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExamMissing_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _unitOfWork.Setup(u => u.Exams.GetByIdAsync(id)).ReturnsAsync((Exam?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(id));

        Assert.Contains(id.ToString(), ex.Message);
    }
}