import React, { useState } from "react";
import ReactSelect from "react-select";

const PlanProcedureItem = ({
  procedure,
  users,
  handleAddUserToPlanProc,
  handleRemoveUserFromPlanProc,
  handleRemoveAllUsersFromPlanProc,
  selectedUsers = [],
}) => {

  
  const selectedUserOptions = selectedUsers.map((u) => ({
    label: u.name,
    value: u.userId,
  }));
     //decided to pass only user object to components and map the optinos that is particular to ReactSelect
    //this keeps the mapping contained only within this component. 

  const userOptions = users.map((u) => ({
    label: u.name,
    value: u.userId,
  }));   
 
  const handleUsersChanged = (procedure, userOptions) => {
    if(userOptions.length === 0)
      handleRemoveAllUsersFromPlanProc(procedure);
    else if(userOptions.length > selectedUserOptions.length)
      handleAssignUserToProcedure(procedure, userOptions);
    else
      handleRemoveUserFromProcedure(procedure, userOptions)
  };

  const handleAssignUserToProcedure = (procedure, userOptions) => {
    const addedUserOption = userOptions.find(x => !selectedUsers.some(s => s.userId === x.value));
    const addedUser = users.find(u => u.userId === addedUserOption.value);
    handleAddUserToPlanProc(procedure, addedUser);
  };

  const handleRemoveUserFromProcedure = (procedure, userOptions) => {
    const removedUser = selectedUsers.find(s => !userOptions.some(x => s.userId === x.value));
    handleRemoveUserFromPlanProc(procedure, removedUser);
  };

  return (
      <div className="py-2">
          <div>
              {procedure.procedureTitle}
          </div>

          <ReactSelect
              className="mt-2"
              placeholder="Select User to Assign"
              isMulti={true}
              options={userOptions}
              value={selectedUserOptions}
              onChange={(e) => handleUsersChanged(procedure, e)}
          />
      </div>
  );
};

export default PlanProcedureItem;
