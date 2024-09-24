using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;

namespace RL.Backend.Commands.Handlers.Users;

public class AddUserToProcedureCommandHandler : IRequestHandler<AddUserToProcedureCommand, ApiResponse<Unit>>
{
    private readonly RLContext _context;

    public AddUserToProcedureCommandHandler(RLContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ApiResponse<Unit>> Handle(AddUserToProcedureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.PlanId < 1)
                return ApiResponse<Unit>.Fail(new BadRequestException($"Invalid {nameof(request.PlanId)}"));

            if (request.ProcedureId < 1)
                return ApiResponse<Unit>.Fail(new BadRequestException($"Invalid {nameof(request.ProcedureId)}"));

            if (request.UserId < 1)
                return ApiResponse<Unit>.Fail(new BadRequestException($"Invalid {nameof(request.UserId)}"));

            var plan = await _context.Plans
                                     .Include(plan => plan.PlanProcedures)
                                     .ThenInclude(planProc => planProc.Users)
                                     .FirstOrDefaultAsync(plan => plan.PlanId == request.PlanId, cancellationToken);

            if (plan is null)
                return ApiResponse<Unit>.Fail(new NotFoundException($"PlanId: {request.PlanId} not found"));

            var procedure = await _context.Procedures.FirstOrDefaultAsync(proc => proc.ProcedureId == request.ProcedureId, cancellationToken);

            if (procedure is null)
                return ApiResponse<Unit>.Fail(new NotFoundException($"ProcedureId: {request.ProcedureId} not found"));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

            if (user is null)
                return ApiResponse<Unit>.Fail(new NotFoundException($"UserId: {request.UserId} not found"));

            if (plan.PlanProcedures.Any(planProc => planProc.ProcedureId == procedure.ProcedureId &&
                                                    planProc.Users.Any(u => u.UserId == user.UserId)))
                return ApiResponse<Unit>.Succeed(new Unit());

            var planProc = plan.PlanProcedures.First(planProc => planProc.ProcedureId == procedure.ProcedureId);
            planProc.Users.Add(user);

            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<Unit>.Succeed(new Unit());
        }
        catch (Exception e)
        {
            return ApiResponse<Unit>.Fail(e);
        }
    }
}