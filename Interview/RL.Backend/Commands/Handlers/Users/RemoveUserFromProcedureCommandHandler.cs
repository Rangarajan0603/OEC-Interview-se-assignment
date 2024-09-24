using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;

namespace RL.Backend.Commands.Handlers.Users;

public class RemoveUserFromProcedureCommandHandler: IRequestHandler<RemoveUserFromProcedureCommand, ApiResponse<Unit>>
{
	private readonly RLContext _context;

	public RemoveUserFromProcedureCommandHandler(RLContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task<ApiResponse<Unit>> Handle(RemoveUserFromProcedureCommand request, CancellationToken cancellationToken)
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

			var user = await _context.Users.FirstOrDefaultAsync(u => request.UserId == u.UserId, cancellationToken);

			if (user is null)
				return ApiResponse<Unit>.Fail(new NotFoundException("No user found for the given Ids"));

			var planProc = plan.PlanProcedures.First(planProc => planProc.ProcedureId == procedure.ProcedureId);
			
			planProc.Users.Remove(user); 
			
			await _context.SaveChangesAsync(cancellationToken);

			return ApiResponse<Unit>.Succeed(new Unit());
		}
		catch (Exception e)
		{
			return ApiResponse<Unit>.Fail(e);
		}
	}
}