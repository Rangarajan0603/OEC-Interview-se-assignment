const api_url = "http://localhost:10010";

export const startPlan = async () => {
    const url = `${api_url}/Plan`;
    const response = await postCommand(url, {});

    if (!response.ok) throw new Error("Failed to create plan");

    return await response.json();
};

export const addProcedureToPlan = async (planId, procedureId) => {
    const url = `${api_url}/Plan/AddProcedureToPlan`;
    var command = { planId: planId, procedureId: procedureId };
    const response = await postCommand(url, command);

    if (!response.ok) throw new Error("Failed to create plan");

    return true;
};

export const addUserToPlanProc = async (planId, procedureId, userId) => {
    const url = `${api_url}/Plan/AddUserToPlanProc`;
    const command = { planId, procedureId, userId };
    const response = await postCommand(url, command);

    if (!response.ok) throw new Error("Failed to add user");

    return true;
};

export const removeUserFromPlanProc = async (planId, procedureId, userId) => {
    const url = `${api_url}/Plan/RemoveUsersFromPlanProc`;
    const command = { planId, procedureId, userId };
    const response = await postCommand(url, command);

    if (!response.ok) throw new Error("Failed to remove user");

    return true;
};

export const removeAllUsersFromPlanProc = async (planId, procedureId) => {
    const url = `${api_url}/Plan/RemoveAllUsersFromPlanProc`;
    const command = { planId, procedureId };
    const response = await postCommand(url, command);

    if (!response.ok) throw new Error("Failed to remove users");

    return true;
};

const postCommand = async (url, command) => {
    const response = await fetch(url, {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(command),
    });
  
    return response;
}

export const getProcedures = async () => {
    const url = `${api_url}/Procedures`;
    const response = await fetch(url, {
        method: "GET",
    });

    if (!response.ok) throw new Error("Failed to get procedures");

    return await response.json();
};

export const getPlanProcedures = async (planId) => {
    const url = `${api_url}/PlanProcedure?$filter=planId eq ${planId}&$expand=procedure,users`;
    const response = await fetch(url, {
        method: "GET",
    });

    if (!response.ok) throw new Error("Failed to get plan procedures");

    return await response.json();
};

export const getUsers = async () => {
    const url = `${api_url}/Users`;
    const response = await fetch(url, {
        method: "GET",
    });

    if (!response.ok) throw new Error("Failed to get users");

    return await response.json();
};
