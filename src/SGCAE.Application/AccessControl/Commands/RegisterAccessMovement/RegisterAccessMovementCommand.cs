using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SGCAE.Application.AccessControl.DTOs;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Application.Common.Interfaces;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.Application.AccessControl.Commands.RegisterAccessMovement;

public record RegisterAccessMovementCommand(
    string CredentialCode,
    Guid AccessPointId,
    MovementType MovementType) : IRequest<AccessResultDto>;

public class RegisterAccessMovementCommandValidator : AbstractValidator<RegisterAccessMovementCommand>
{
    public RegisterAccessMovementCommandValidator()
    {
        RuleFor(v => v.CredentialCode)
            .NotEmpty().WithMessage("Credential code is required.");

        RuleFor(v => v.AccessPointId)
            .NotEmpty().WithMessage("Access point ID is required.");

        RuleFor(v => v.MovementType)
            .IsInEnum().WithMessage("Movement type must be Entry (1) or Exit (2).");
    }
}

public class RegisterAccessMovementCommandHandler : IRequestHandler<RegisterAccessMovementCommand, AccessResultDto>
{
    private readonly IApplicationDbContext _context;

    public RegisterAccessMovementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccessResultDto> Handle(RegisterAccessMovementCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.CredentialCode.Trim().ToUpperInvariant();

        // 1. Validate Access Point
        var accessPoint = await _context.AccessPoints
            .FirstOrDefaultAsync(p => p.Id == request.AccessPointId, cancellationToken);

        if (accessPoint is null)
            throw new NotFoundException(nameof(AccessPoint), request.AccessPointId);

        if (!accessPoint.IsEnabled)
        {
            var deniedLog = AccessLog.CreateDenied(
                employeeId: null,
                credentialId: null,
                accessPoint.Id,
                normalizedCode,
                request.MovementType,
                $"Access point '{accessPoint.Name}' is currently disabled.");

            _context.AccessLogs.Add(deniedLog);
            await _context.SaveChangesAsync(cancellationToken);

            return new AccessResultDto
            {
                LogId = deniedLog.Id,
                Result = AccessResult.Denied,
                RejectionReason = deniedLog.RejectionReason,
                CredentialCode = normalizedCode,
                AccessPointName = accessPoint.Name,
                MovementType = request.MovementType,
                Timestamp = deniedLog.Timestamp
            };
        }

        // 2. Validate Credential
        var credential = await _context.Credentials
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.Code == normalizedCode, cancellationToken);

        if (credential is null)
        {
            var deniedLog = AccessLog.CreateDenied(
                employeeId: null,
                credentialId: null,
                accessPoint.Id,
                normalizedCode,
                request.MovementType,
                $"Credential '{normalizedCode}' does not exist in the system.");

            _context.AccessLogs.Add(deniedLog);
            await _context.SaveChangesAsync(cancellationToken);

            return new AccessResultDto
            {
                LogId = deniedLog.Id,
                Result = AccessResult.Denied,
                RejectionReason = deniedLog.RejectionReason,
                CredentialCode = normalizedCode,
                AccessPointName = accessPoint.Name,
                MovementType = request.MovementType,
                Timestamp = deniedLog.Timestamp
            };
        }

        // 3. Validate Credential Status
        if (!credential.IsValidForAccess())
        {
            var reason = credential.Status switch
            {
                CredentialStatus.Suspended => $"Credential suspended. {credential.StatusChangeReason}".Trim(),
                CredentialStatus.Revoked => $"Credential permanently revoked. {credential.StatusChangeReason}".Trim(),
                _ => "Credential is not valid for access."
            };

            var deniedLog = AccessLog.CreateDenied(
                credential.EmployeeId,
                credential.Id,
                accessPoint.Id,
                normalizedCode,
                request.MovementType,
                reason);

            _context.AccessLogs.Add(deniedLog);
            await _context.SaveChangesAsync(cancellationToken);

            return new AccessResultDto
            {
                LogId = deniedLog.Id,
                Result = AccessResult.Denied,
                RejectionReason = reason,
                CredentialCode = normalizedCode,
                EmployeeName = credential.Employee?.FullName,
                EmployeeIdentificationNumber = credential.Employee?.IdentificationNumber,
                AccessPointName = accessPoint.Name,
                MovementType = request.MovementType,
                Timestamp = deniedLog.Timestamp
            };
        }

        // 4. Validate Employee Status
        var employee = credential.Employee;
        if (employee is null || !employee.CanAccess())
        {
            var deniedLog = AccessLog.CreateDenied(
                credential.EmployeeId,
                credential.Id,
                accessPoint.Id,
                normalizedCode,
                request.MovementType,
                "Access denied: Employee employment status is inactive.");

            _context.AccessLogs.Add(deniedLog);
            await _context.SaveChangesAsync(cancellationToken);

            return new AccessResultDto
            {
                LogId = deniedLog.Id,
                Result = AccessResult.Denied,
                RejectionReason = deniedLog.RejectionReason,
                CredentialCode = normalizedCode,
                EmployeeName = employee?.FullName,
                EmployeeIdentificationNumber = employee?.IdentificationNumber,
                AccessPointName = accessPoint.Name,
                MovementType = request.MovementType,
                Timestamp = deniedLog.Timestamp
            };
        }

        // 5. Authorized Access
        var grantedLog = AccessLog.CreateGranted(
            employee.Id,
            credential.Id,
            accessPoint.Id,
            normalizedCode,
            request.MovementType);

        _context.AccessLogs.Add(grantedLog);
        await _context.SaveChangesAsync(cancellationToken);

        return new AccessResultDto
        {
            LogId = grantedLog.Id,
            Result = AccessResult.Granted,
            RejectionReason = null,
            CredentialCode = normalizedCode,
            EmployeeName = employee.FullName,
            EmployeeIdentificationNumber = employee.IdentificationNumber,
            AccessPointName = accessPoint.Name,
            MovementType = request.MovementType,
            Timestamp = grantedLog.Timestamp
        };
    }
}

