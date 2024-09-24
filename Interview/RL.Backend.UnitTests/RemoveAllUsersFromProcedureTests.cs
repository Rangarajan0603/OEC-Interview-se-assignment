using FluentAssertions;
using MediatR;
using Moq;
using RL.Backend.Commands.Handlers.Users;
using RL.Backend.Commands;
using RL.Backend.Exceptions;
using RL.Data;
using RL.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace RL.Backend.UnitTests;

[TestClass]
public class RemoveAllUsersFromProcedureTests
{
	[TestMethod]
	[DataRow(-1)]
	[DataRow(0)]
	[DataRow(int.MinValue)]
	public async Task HandlerBadRequestOnInvalidPlanId(int planId)
	{
		var context = new Mock<RLContext>();
		var sut = new RemoveAllUsersFromProcedureCommandHandler(context.Object);
		var request = new RemoveAllUsersFromProcedureCommand()
		{
			PlanId = planId,
			ProcedureId = 1
		};

		var result = await sut.Handle(request, new CancellationToken());

		result.Exception.Should().BeOfType(typeof(BadRequestException));
		result.Succeeded.Should().BeFalse();
	}

	[TestMethod]
	[DataRow(-1)]
	[DataRow(0)]
	[DataRow(int.MinValue)]
	public async Task HandlerBadRequestOnInvalidProcedureId(int procedureId)
	{
		var context = new Mock<RLContext>();
		var sut = new RemoveAllUsersFromProcedureCommandHandler(context.Object);
		var request = new RemoveAllUsersFromProcedureCommand()
		{
			PlanId = 1,
			ProcedureId = procedureId
		};

		var result = await sut.Handle(request, new CancellationToken());

		result.Exception.Should().BeOfType(typeof(BadRequestException));
		result.Succeeded.Should().BeFalse();
	}

	[TestMethod]
	[DataRow(1, 2)]
	[DataRow(3, 4)]
	public async Task HandlerReturnsSuccessOnNoUsers(int planId, int procedureId)
	{
		var context = DbContextHelper.CreateContext();
		var sut = new RemoveAllUsersFromProcedureCommandHandler(context);
		var request = new RemoveAllUsersFromProcedureCommand()
		{
			PlanId = planId,
			ProcedureId = procedureId
		};

		context.Plans.Add(new Plan
		{
			PlanId = planId
		});

		context.Procedures.Add(new Procedure
		{
			ProcedureId = procedureId,
			ProcedureTitle = "Test Procedure"
		});

		context.PlanProcedures.Add(new PlanProcedure
		{
			ProcedureId = procedureId,
			PlanId = planId,
			Users = new List<User>()
		});

		await context.SaveChangesAsync();
		
		var result = await sut.Handle(request, new CancellationToken());

		result.Value.Should().BeOfType(typeof(Unit));
		result.Succeeded.Should().BeTrue();
	}


	[TestMethod]
	[DataRow(1, 2)]
	[DataRow(3, 4)]
	public async Task HandlerReturnsSuccessOnRemovingUsers(int planId, int procedureId)
	{
		var context = DbContextHelper.CreateContext();
		var sut = new RemoveAllUsersFromProcedureCommandHandler(context);
		var request = new RemoveAllUsersFromProcedureCommand()
		{
			PlanId = planId,
			ProcedureId = procedureId
		};

		context.Plans.Add(new Plan
		{
			PlanId = planId
		});

		context.Procedures.Add(new Procedure
		{
			ProcedureId = procedureId,
			ProcedureTitle = "Test Procedure"
		});

		context.PlanProcedures.Add(new PlanProcedure
		{
			ProcedureId = procedureId,
			PlanId = planId,
			Users = new List<User>{new() {Name = "Test user", UserId = 1}}
		});

		await context.SaveChangesAsync();

		var result = await sut.Handle(request, new CancellationToken());
		var planProc = context.Plans.Include(plan => plan.PlanProcedures)
								.ThenInclude(planProcedure => planProcedure.Users)
								.First(p => p.PlanId == planId)
								.PlanProcedures.First(pp => pp.ProcedureId == procedureId);

		planProc.Users.Count.Should().Be(0);
		result.Value.Should().BeOfType(typeof(Unit));
		result.Succeeded.Should().BeTrue();
	}
}