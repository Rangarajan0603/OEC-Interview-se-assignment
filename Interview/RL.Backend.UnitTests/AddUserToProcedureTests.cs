using FluentAssertions;
using Moq;
using RL.Backend.Commands;
using RL.Backend.Exceptions;
using RL.Data;
using RL.Data.DataModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Commands.Handlers.Users;

namespace RL.Backend.UnitTests;

[TestClass]
public class AddUserToProcedureTests
{
	[TestMethod]
	[DataRow(-1)]
	[DataRow(0)]
	[DataRow(int.MinValue)]
	public async Task HandlerReturnsBadRequestOnInvalidUserId(int userId)
	{
		var context = new Mock<RLContext>();
		var sut = new AddUserToProcedureCommandHandler(context.Object);
		var request = new AddUserToProcedureCommand()
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
	[DataRow(-1)]
	[DataRow(0)]
	[DataRow(int.MinValue)]
	public async Task HandlerBadRequestOnInvalidPlanId(int planId)
	{
		var context = new Mock<RLContext>();
		var sut = new AddUserToProcedureCommandHandler(context.Object);
		var request = new AddUserToProcedureCommand()
		{
			UserId = 1,
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
		var sut = new AddUserToProcedureCommandHandler(context.Object);
		var request = new AddUserToProcedureCommand()
		{
			UserId = 1,
			PlanId = 1,
			ProcedureId = procedureId
		};

		var result = await sut.Handle(request, new CancellationToken());

		result.Exception.Should().BeOfType(typeof(BadRequestException));
		result.Succeeded.Should().BeFalse();
	}

	[TestMethod]
	[DataRow(1, 1, 1)]
	[DataRow(19, 1010, 123)]
	[DataRow(35, 69, 456)]
	public async Task HandlerReturnSuccessOnAddingUserToPlanProcedure(int planId, int procedureId, int userId)
	{
		var context = DbContextHelper.CreateContext();
		var sut = new AddUserToProcedureCommandHandler(context);
		var request = new AddUserToProcedureCommand()
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
			PlanId = planId
		});

		context.Users.Add(new User
		{
			UserId = userId
		});
		
		await context.SaveChangesAsync();

		var result = await sut.Handle(request, new CancellationToken());

		result.Value.Should().BeOfType(typeof(Unit));
		result.Succeeded.Should().BeTrue();
	}

	[TestMethod]
	[DataRow(1, 1, 1)]
	[DataRow(19, 1010, 123)]
	[DataRow(35, 69, 456)]
	public async Task HandlerReturnSuccessOnUserExistingForPlanProcedure(int planId, int procedureId, int userId)
	{
		var context = DbContextHelper.CreateContext();
		var sut = new AddUserToProcedureCommandHandler(context);
		var request = new AddUserToProcedureCommand()
		{
			PlanId = planId,
			ProcedureId = procedureId,
			UserId = userId
		};

		var user = new User
		{
			UserId = userId
		};

		context.Users.Add(user);

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
			Users = new List<User>{user}
		});

		await context.SaveChangesAsync();

		var result = await sut.Handle(request, new CancellationToken());

		result.Value.Should().BeOfType(typeof(Unit));
		result.Succeeded.Should().BeTrue();
	}


	[TestMethod]
	[DataRow(1, 1, 1)]
	[DataRow(19, 1010, 123)]
	[DataRow(35, 69, 456)]
	public async Task HandlerAddsUserToPlanProcedure(int planId, int procedureId, int userId)
	{
		var context = DbContextHelper.CreateContext();
		var sut = new AddUserToProcedureCommandHandler(context);
		var request = new AddUserToProcedureCommand()
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
			PlanId = planId
		});

		context.Users.Add(new User
		{
			UserId = userId
		});

		await context.SaveChangesAsync();

		var result = await sut.Handle(request, new CancellationToken());
		var planProc = context.Plans.Include(plan => plan.PlanProcedures)
		                      .ThenInclude(planProcedure => planProcedure.Users)
		                      .First(p => p.PlanId == planId)
		                      .PlanProcedures.First(pp => pp.ProcedureId == procedureId);

		planProc.Users.Count.Should().Be(1);
		result.Value.Should().BeOfType(typeof(Unit));
		result.Succeeded.Should().BeTrue();
	}

}