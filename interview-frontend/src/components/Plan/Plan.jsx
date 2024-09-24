import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import {
  addProcedureToPlan,
  addUserToPlanProc,
  getPlanProcedures,
  getProcedures,
  getUsers,
  removeUserFromPlanProc,
  removeAllUsersFromPlanProc
} from "../../api/api";
import Layout from '../Layout/Layout';
import ProcedureItem from "./ProcedureItem/ProcedureItem";
import PlanProcedureItem from "./PlanProcedureItem/PlanProcedureItem";

const Plan = () => {
  let { id } = useParams();
  const planId = parseInt(id); // changed it to int and update in all places for type consistency
  const [procedures, setProcedures] = useState([]);
  const [planProcedures, setPlanProcedures] = useState([]);
  const [users, setUsers] = useState([]);

  useEffect(() => {
    (async () => {
      var procedures = await getProcedures();
      var planProcedures = await getPlanProcedures(planId);
      var users = await getUsers();
      
      setUsers(users);
      setProcedures(procedures);
      setPlanProcedures(planProcedures);
    })();
  }, [planId]);

  const handleAddProcedureToPlan = async (procedure) => {
    const hasProcedureInPlan = planProcedures.some((p) => p.procedureId === procedure.procedureId);
    if (hasProcedureInPlan) return;

    await addProcedureToPlan(planId, procedure.procedureId);
    setPlanProcedures((prevState) => {
      return [
        ...prevState,
        {
          planId, //since id was string here, it caused failure handleAddUserToPlanProc. So kept id check as == instead of ===
          // now fixed it by using planId and updated the check in handleAddUserToPlanProc to === 
          procedureId: procedure.procedureId,
          procedure: {
            procedureId: procedure.procedureId,
            procedureTitle: procedure.procedureTitle,
          },
          users: []
        },
      ];
    });
  };

  const handleAddUserToPlanProc = async (procedure, newUser) => {
    let planProc = planProcedures.find(pp => pp.procedureId === procedure.procedureId && pp.planId === planId); 
    let userExist = planProc.users?.some(u => u.userId === newUser.userId);

    if(userExist) return;

    try{
      const userAdded = await addUserToPlanProc(planId, procedure.procedureId, newUser.userId);
      if(userAdded)
        console.log(`User ${newUser.userId} added to procedure ${procedure.procedureId} and plan ${planId}.`)
    } catch(error) {
      console.error(`Failed to add user ${newUser.userId} to procedure ${procedure.procedureId} and plan ${planId}. Error: ${error}`);
      return;
    }

    setPlanProcedures(prevState => {
      let newState = [...prevState];
      let procIndex = newState.findIndex(
        p => p.procedureId === procedure.procedureId && p.planId === planId
      );

      newState[procIndex].users = [...planProc.users, newUser]

      return newState;
    });
  };

  const handleRemoveUserFromPlanProc = async (procedure, removedUser) => {
    let planProc = planProcedures.find(pp => pp.procedureId === procedure.procedureId && pp.planId === planId); 
    let userExist = planProc.users?.some(u => u.userId === removedUser.userId);

    if(!userExist) return;

    try{
      //used a different end point for removing all users as its simpler, reads better and consistent.
      //thought of having one endpoint and passing as array of users to be removed. 
      //In this way one endpoint could handle removing one/many/all users. 
      //But for adding users we doing it one by one, so went ahead with same approach for consistency
      const userRemoved = await removeUserFromPlanProc(planId, procedure.procedureId, removedUser.userId);
      if(userRemoved)
        console.log(`User ${removedUser.userId} removed from procedure ${procedure.procedureId} and plan ${planId}.`)
    } catch(error) {
      console.error(`Failed to remove user ${removedUser.userId} from procedure ${procedure.procedureId} and plan ${planId}. Error: ${error}`);
      return;
      //could use alert to notify user or add proper notification with library/custom code.
      //For the sake of interview opted to use just console log.
    }

    setPlanProcedures(prevState => {
      let newState = [...prevState];
      let procIndex = newState.findIndex(
        p => p.procedureId === procedure.procedureId && p.planId === planId
      );
      let updatedUsers = planProc.users.filter(u => u.userId !== removedUser.userId)

      newState[procIndex].users = updatedUsers

      return newState;
    });
  }

  const handleRemoveAllUsersFromPlanProc = async (procedure) => {
    try{
      const usersRemoved = await removeAllUsersFromPlanProc(planId, procedure.procedureId);
      if(usersRemoved)
        console.log(`All user removed from procedure ${procedure.procedureId} and plan ${planId}.`)
    } catch(error) {
      console.error(`Failed to remove all users from procedure ${procedure.procedureId} and plan ${planId}. Error: ${error}`);
      return;
    }

    setPlanProcedures(prevState => {
      let newState = [...prevState];
      let procIndex = newState.findIndex(
        p => p.procedureId === procedure.procedureId && p.planId === planId
      );

      newState[procIndex].users = []

      return newState;
    });
  }

  return (
    <Layout>
      <div className="container pt-4">
        <div className="d-flex justify-content-center">
          <h2>OEC Interview Frontend</h2>
        </div>
        <div className="row mt-4">
          <div className="col">
            <div className="card shadow">
              <h5 className="card-header">Repair Plan</h5>
              <div className="card-body">
                <div className="row">
                  <div className="col">
                    <h4>Procedures</h4>
                    <div>
                      {procedures.map((p) => (
                        <ProcedureItem
                          key={p.procedureId}
                          procedure={p}
                          handleAddProcedureToPlan={handleAddProcedureToPlan}
                          planProcedures={planProcedures}
                        />
                      ))}
                    </div>
                  </div>
                  <div className="col">
                    <h4>Added to Plan</h4>
                    <div>
                      {planProcedures.map((p) => (
                        <PlanProcedureItem
                          key={p.procedure.procedureId}
                          procedure={p.procedure}
                          selectedUsers={p.users}
                          users={users}
                          handleAddUserToPlanProc={handleAddUserToPlanProc}
                          handleRemoveUserFromPlanProc={handleRemoveUserFromPlanProc}
                          handleRemoveAllUsersFromPlanProc={handleRemoveAllUsersFromPlanProc}
                        />
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default Plan;
