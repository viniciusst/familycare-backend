using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.RegisterExam;

public sealed record RegisterExamCommand(
    FamilyMemberId FamilyMemberId,
    DateOnly ExamDate,
    string ExamType,
    string? Laboratory = null,
    string? Results = null,
    string? RequestedBy = null)
    : ICommand<ExamId>;

public sealed class RegisterExamCommandValidator : AbstractValidator<RegisterExamCommand>
{
    public RegisterExamCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.ExamType).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Laboratory).MaximumLength(120);
        RuleFor(x => x.Results).MaximumLength(8000);
        RuleFor(x => x.RequestedBy).MaximumLength(120);

        RuleFor(x => x.ExamDate)
            .LessThanOrEqualTo(_ => dateTimeProvider.TodayUtc)
            .WithMessage("Exam date cannot be in the future.");
    }
}

public sealed class RegisterExamCommandHandler(
    MedicalAccessGuard accessGuard,
    IExamRepository examRepository)
    : IRequestHandler<RegisterExamCommand, ExamId>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IExamRepository _examRepository = examRepository;

    public async Task<ExamId> Handle(RegisterExamCommand request, CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureCanWriteAsync(
            request.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        var exam = Exam.Register(
            request.FamilyMemberId,
            request.ExamDate,
            request.ExamType,
            request.Laboratory,
            request.Results,
            request.RequestedBy);

        await _examRepository.AddAsync(exam, cancellationToken);
        return exam.Id;
    }
}
