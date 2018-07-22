import React from "react";

import ReactTable from "react-table";
import "react-table/react-table.css";
import { minBy } from "./StrategyDashboardApi";

export class DataTable extends React.Component {
  render() {
    return (
      <ReactTable
        data={this.props.data}
        loading={this.props.loading}
        columns={[
          { Header: "Task Name", accessor: "taskName" },
          {
            Header: "Solver",
            accessor: "solverName",
            sortable: false,
            aggregate: (values, rows) => {
              const best = minBy(x => x.energy, rows);
              return best.solverName + " (best)";
            }
          },
          {
            Header: "Energy",
            accessor: "energy",
            aggregate: (values, rows) => {
              const best = minBy(x => x.energy, rows);
              return best.energy;
            }
          },
          {
            Header: "Leader Diff",
            accessor: d => {
              if (isFinite(d.leaderEnergy) && isFinite(d.energy)) {
                return d.energy - d.leaderEnergy;
              }
              return Infinity;
            },
            aggregate: (values, rows) => {
              const min = Math.min(...values);
              return min;
            },
            id: "leaderDiff"
          },
          {
            Header: "Leader Energy",
            accessor: "leaderEnergy",
            aggregate: ([valuee]) => valuee
          }
        ]}
        pivotBy={["taskName"]}
        defaultPageSize={100}
      />
    );
  }
}
