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
            aggregate: (values, rows) => {
              const best = minBy(x => x.energy, rows);
              return best.solverName + " (best)";
            }
          },
          {
            Header: "Energy",
            accessor: d => [d.energy, d.rate],
            id: "energy",
            aggregate: (values, rows) => {
              const best = minBy(x => x.energy[0], rows);
              return best.energy;
            },
            Cell: EnergyCell
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
            id: "leaderDiff",
            Cell: ({ value }) => (
              <div
                style={{
                  background: isFinite(value)
                    ? value < 0
                      ? "rgb(0, 255, 0)"
                      : "orange"
                    : "white"
                }}
              >
                {value}
              </div>
            )
          },
          {
            Header: "Leader Energy",
            id: "leaderEnergy",
            accessor: d => [d.leaderEnergy, d.leaderRate],
            aggregate: ([valuee]) => valuee,
            Cell: EnergyCell
          }
        ]}
        pivotBy={["taskName"]}
        defaultPageSize={100}
      />
    );
  }
}

const EnergyCell = ({ value: [value, rate] }) => (
  <div
    style={{
      background: isFinite(value) ? getColor(rate) : "black",
      color: isFinite(value) ? "black" : "white"
    }}
  >
    {value}
  </div>
);

function getColor(value) {
  //value from 0 to 1
  var hue = ((1 - value) * 120).toString(10);
  return ["hsl(", hue, ",100%,50%)"].join("");
}
