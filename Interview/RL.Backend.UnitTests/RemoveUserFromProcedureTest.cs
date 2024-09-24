using MediatR;
using Moq;
using RL.Backend.Commands.Handlers.Users;
using RL.Backend.Commands;
using RL.Backend.Exceptions;
using RL.Data.DataModels;
using RL.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace RL.Backend.UnitTests;

[TestClass]
public class RemoveUserFromProcedureTest
{
	[TestMethod]
	[DataRow(-1)]
	[DataRow(0)]
	[DataRow(int.MinValue)]
	public async Task HandlerBadRequestOnInvalidPlanId(int planId)
	{
		var context = new Mock<RLContext>();
		var sut = new RemoveUserFromProcedureCommandHandler(context.Object);
		var request = new RemoveUserFromProcedureCommand
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
		var sut = new RemoveUserFromProcedureCommandHandler(context.Object);
		var request = new RemoveUserFromProcedureCommand
		{
			PlanId = 1,
			ProcedureId = procedureId
		};

		var result = await sut.Handle(request, new CancellationToken());

		result.Exception.Should().BeOfType(typeof(BadRequestException));
		result.Succeeded.Should().BeFalse();
	}

	[TestMethod]
	[DataRow(-1)]
	[DataRow(0)]
	[DataRow(int.MinValue)]
	public async Task HandlerReturnsBadRequestOnInvalidUserId(int userId)
	{
		var context = new Mock<RLContext>();
		var sut = new RemoveUserFromProcedureCommandHandler(context.Object);
		var request = new RemoveUserFromProcedureCommand
		{
			UserId = userId,
			PlanId = 1,
			ProcedureId = 1
		};

		var result = await sut.Handle(request, new CancellationToken());

		result.Exception.Should().BeOfType(typeof(BadRequestException));
		result.Succeeded.Should().BeFalse();
	}

	[TestMethod]
	[DataRow(1, 2, 3)]
	[DataRow(3, 4, 5)]
	public async Task HandleThrowsExceptionOnNoUsers(int planId, int procedureId, int userId)
	{
		var context = DbContextHelper.CreateContext();
		var sut = new RemoveUserFromProcedureCommandHandler(context);
		var request = new RemoveUserFromProcedureCommand
		{
			PlanId = planId,
			ProcedureId = procedureId,
			UserId = userId
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

		result.Exception.Should().BeOfType(typeof(NotFoundException));
		result.Succeeded.Should().BeFalse();
	}

	[TestMethod]
	[DataRow(1, 2, 3)]
	[DataRow(3, 4, 5)]
	public async Task HandlerReturnsSuccessOnRemovingUser(int planId, int procedureId, int userId)
	{
		var context = DbContextHelper.CreateContext();
		var sut = new RemoveUserFromProcedureCommandHandler(context);
		var request = new RemoveUserFromProcedureCommand
		{
			PlanId = planId,
			ProcedureId = procedureId,
			UserId = userId
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
			Users = new List<User>
			{
				new() { Name = "Test user", UserId = userId },
				new() { Name = "Test user", UserId = userId + 1 },
				new() { Name = "Test user", UserId = userId + 2 }
			}
		});

		await context.SaveChangesAsync();

		var result = await sut.Handle(request, new CancellationToken());
		var planProc = context.Plans.Include(plan => plan.PlanProcedures)
								.ThenInclude(planProcedure => planProcedure.Users)
								.First(p => p.PlanId == planId)
								.PlanProcedures.First(pp => pp.ProcedureId == procedureId);

		planProc.Users.Count.Should().Be(2);
		result.Value.Should().BeOfType(typeof(Unit));
		result.Succeeded.Should().BeTrue();
	}
}