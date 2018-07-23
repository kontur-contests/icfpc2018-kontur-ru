import React from "react";
import { getStrategyTraces } from "./StrategyDashboardApi";

import ReactTable from "react-table";
import "react-table/react-table.css";

const columns = [
  { Header: "Task Name", accessor: "taskName" },
  { Header: "Old Best Solver Name", accessor: "oldBestSolverName" },
  { Header: "Old Best Solver Energy", accessor: "oldBestSolverEnergy" },
  {
    Header: "Energy Diff",
    id: "energyDiff",
    accessor: x => x.bestSolverEnergy - x.oldBestSolverEnergy
  },
  { Header: "Cur Best Solver Name", accessor: "bestSolverName" },
  { Header: "Cur Best Solver Energy", accessor: "bestSolverEnergy" }
];

export class StrategyTrace extends React.Component {
  state = {
    data: []
  };

  componentDidMount() {
    this.fetchData();
  }

  fetchData = async () => {
    const data = await getStrategyTraces();
    console.log(data);
    this.setState({ data: data.filter(x => !x.taskName.includes("_tgt")) });
  };

  render() {
    return (
      <div>
        <ReactTable data={this.state.data} columns={columns} />
      </div>
    );
  }
}
