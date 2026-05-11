using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.UpdateExamResults;

public sealed record UpdateExamResultsCommand(ExamId ExamId, string NewResults) : ICommand;

public sealed class UpdateExamResultsCommandValidator : AbstractValidator<UpdateExamResultsCommand>
{
    public UpdateExamResultsCommandValidator()
    {
        RuleFor(x => x.NewResults).NotEmpty().MaximumLength(8000);
    }
}

public sealed class UpdateExamResultsCommandHandler(
    MedicalAccessGuard accessGuard,
    IExamRepository examRepository)
    : IRequestHandler<UpdateExamResultsCommand>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IExamRepository _examRepository = examRepository;

    public async Task Handle(UpdateExamResultsCommand request, CancellationToken cancellationToken)
    {
        var exam = await _examRepository.GetByIdAsync(request.ExamId, cancellationToken)
            ?? throw new NotFoundException(nameof(Exam), request.ExamId);

        await _accessGuard.EnsureCanWriteAsync(
            exam.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        exam.UpdateResults(request.NewResults);
        _examRepository.Update(exam);
    }
}
