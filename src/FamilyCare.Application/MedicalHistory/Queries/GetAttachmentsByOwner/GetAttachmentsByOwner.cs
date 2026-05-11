using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Mappings;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Queries.GetAttachmentsByOwner;

public sealed record GetAttachmentsByOwnerQuery(
    AttachmentOwnerType OwnerType,
    Guid OwnerEntityId)
    : IQuery<IReadOnlyList<AttachmentDto>>;

public sealed class GetAttachmentsByOwnerQueryHandler(
    MedicalAccessGuard accessGuard,
    IAttachmentRepository attachmentRepository,
    IAppointmentRepository appointmentRepository,
    IExamRepository examRepository,
    IVaccineRepository vaccineRepository)
    : IRequestHandler<GetAttachmentsByOwnerQuery, IReadOnlyList<AttachmentDto>>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAttachmentRepository _attachmentRepository = attachmentRepository;
    private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;
    private readonly IExamRepository _examRepository = examRepository;
    private readonly IVaccineRepository _vaccineRepository = vaccineRepository;

    public async Task<IReadOnlyList<AttachmentDto>> Handle(
        GetAttachmentsByOwnerQuery request,
        CancellationToken cancellationToken)
    {
        var ownerMemberId = await ResolveOwnerMemberIdAsync(
            request.OwnerType, request.OwnerEntityId, cancellationToken);

        await _accessGuard.EnsureCanReadAsync(
            ownerMemberId, DataCategory.MedicalHistory, cancellationToken);

        var items = await _attachmentRepository.GetByOwnerAsync(
            request.OwnerType, request.OwnerEntityId, cancellationToken);

        return items.Select(a => a.ToDto()).ToList();
    }

    private async Task<FamilyMemberId> ResolveOwnerMemberIdAsync(
        AttachmentOwnerType ownerType,
        Guid ownerEntityId,
        CancellationToken cancellationToken)
    {
        return ownerType switch
        {
            AttachmentOwnerType.Appointment => (await _appointmentRepository
                .GetByIdAsync(AppointmentId.From(ownerEntityId), cancellationToken)
                ?? throw new NotFoundException(nameof(Appointment), ownerEntityId)).FamilyMemberId,

            AttachmentOwnerType.Exam => (await _examRepository
                .GetByIdAsync(ExamId.From(ownerEntityId), cancellationToken)
                ?? throw new NotFoundException(nameof(Exam), ownerEntityId)).FamilyMemberId,

            AttachmentOwnerType.Vaccine => (await _vaccineRepository
                .GetByIdAsync(VaccineId.From(ownerEntityId), cancellationToken)
                ?? throw new NotFoundException(nameof(Vaccine), ownerEntityId)).FamilyMemberId,

            _ => throw new ConflictException(
                "attachment.unsupported_owner",
                $"Unsupported attachment owner type: {ownerType}.")
        };
    }
}
