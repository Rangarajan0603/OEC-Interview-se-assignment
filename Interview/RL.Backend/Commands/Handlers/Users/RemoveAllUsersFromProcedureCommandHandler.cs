using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;

namespace RL.Backend.Commands.Handlers.Users;

public class RemoveAllUsersFromProcedureCommandHandler : IRequestHandler<RemoveAllUsersFromProcedureCommand, ApiResponse<Unit>>
{
	private readonly RLContext _context;

	public RemoveAllUsersFromProcedureCommandHandler(RLContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task<ApiResponse<Unit>> Handle(RemoveAllUsersFromProcedureCommand request, CancellationToken cancellationToken)
	{
		try
		{
			if (request.PlanId < 1)
				return ApiResponse<Unit>.Fail(new BadRequestException($"Invalid {nameof(request.PlanId)}"));

			if (request.ProcedureId < 1)
				return ApiResponse<Unit>.Fail(new BadRequestException($"Invalid {nameof(request.ProcedureId)}"));

			var plan = await _context.Plans
			                         .Include(plan => plan.PlanProcedures)
			                         .ThenInclude(planProc => planProc.Users)
			                         .FirstOrDefaultAsync(plan => plan.PlanId == request.PlanId, cancellationToken);

			if (plan is null)
				return ApiResponse<Unit>.Fail(new NotFoundException($"PlanId: {request.PlanId} not found"));

			var procedure = await _context.Procedures.FirstOrDefaultAsync(proc => proc.ProcedureId == request.ProcedureId, cancellationToken);

			if (procedure is null)
				return ApiResponse<Unit>.Fail(new NotFoundException($"ProcedureId: {request.ProcedureId} not found"));

			var planProc = plan.PlanProcedures.First(planProc => planProc.ProcedureId == procedure.ProcedureId);

			if(!planProc.Users.Any())
				return ApiResponse<Unit>.Succeed(new Unit());

			planProc.Users.Clear(); 
			
			await _context.SaveChangesAsync(cancellationToken);

			return ApiResponse<Unit>.Succeed(new Unit());
		}
		catch (Exception e)
		{
			return ApiResponse<Unit>.Fail(e);
		}
	}
}