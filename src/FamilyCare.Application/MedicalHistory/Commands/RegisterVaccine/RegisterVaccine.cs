using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.RegisterVaccine;

public sealed record RegisterVaccineCommand(
    FamilyMemberId FamilyMemberId,
    string Name,
    DateOnly AppliedAt,
    string? Manufacturer = null,
    string? BatchNumber = null,
    int? DoseNumber = null,
    DateOnly? NextDoseDue = null,
    string? Notes = null)
    : ICommand<VaccineId>;

public sealed class RegisterVaccineCommandValidator : AbstractValidator<RegisterVaccineCommand>
{
    public RegisterVaccineCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Manufacturer).MaximumLength(120);
        RuleFor(x => x.BatchNumber).MaximumLength(80);
        RuleFor(x => x.Notes).MaximumLength(2000);

        RuleFor(x => x.DoseNumber)
            .GreaterThanOrEqualTo(1).When(x => x.DoseNumber.HasValue);

        RuleFor(x => x.AppliedAt)
            .LessThanOrEqualTo(_ => dateTimeProvider.TodayUtc)
            .WithMessage("Applied date cannot be in the future.");

        RuleFor(x => x.NextDoseDue)
            .GreaterThanOrEqualTo(x => x.AppliedAt)
            .When(x => x.NextDoseDue.HasValue)
            .WithMessage("Next dose date cannot be before applied date.");
    }
}

public sealed class RegisterVaccineCommandHandler(
    MedicalAccessGuard accessGuard,
    IVaccineRepository vaccineRepository)
    : IRequestHandler<RegisterVaccineCommand, VaccineId>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IVaccineRepository _vaccineRepository = vaccineRepository;

    public async Task<VaccineId> Handle(RegisterVaccineCommand request, CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureCanWriteAsync(
            request.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        var vaccine = Vaccine.Register(
            request.FamilyMemberId,
            request.Name,
            request.AppliedAt,
            request.Manufacturer,
            request.BatchNumber,
            request.DoseNumber,
            request.NextDoseDue,
            request.Notes);

        await _vaccineRepository.AddAsync(vaccine, cancellationToken);
        return vaccine.Id;
    }
}
