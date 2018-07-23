import React from "react";
import { getStrategyTraces } from "./StrategyDashboardApi";

import ReactTable from "react-table";
import "react-table/react-table.css";

import tada from "./tada.wav";

const columns = [
  { Header: "Task Name", accessor: "taskName" },
  { Header: "Old Best Solver Name", accessor: "oldBestSolverName" },
  { Header: "Old Best Solver Energy", accessor: "oldBestSolverEnergy" },
  {
    Header: "Energy Diff",
    id: "energyDiff",
    accessor: x =>
      x.bestSolverEnergy
        ? x.bestSolverEnergy - x.oldBestSolverEnergy
        : -Infinity
  },
  { Header: "Cur Best Solver Name", accessor: "bestSolverName" },
  { Header: "Cur Best Solver Energy", accessor: "bestSolverEnergy" }
];

export class StrategyTrace extends React.Component {
  state = {
    data: [],
    forTada: []
  };

  componentDidMount() {
    this.fetchData();

    this.interval = setInterval(this.fetchData, 10000);
  }

  componentWillUnmount() {
    clearInterval(this.interval);
  }

  fetchData = async () => {
    const data = (await getStrategyTraces()).filter(
      x => !x.taskName.includes("_tgt")
    );

    const forTada = [];

    const normalized = this.state.data.reduce((acc, x) => {
      acc[x.taskName] = x;
      return acc;
    }, {});

    if (this.state.data.length) {
      for (const rec of data) {
        const cur = normalized[rec.taskName];
        if (cur) {
          const curEnergyDiff = cur.bestSolverEnergy - cur.oldBestSolverEnergy;
          const newEnergyDiff = rec.bestSolverEnergy - rec.oldBestSolverEnergy;

          if (
            newEnergyDiff < 0 &&
            (newEnergyDiff < curEnergyDiff ||
              (isNaN(curEnergyDiff) && !isNaN(newEnergyDiff)))
          ) {
            forTada.push(rec);
          }
        }
      }
    }

    this.setState(
      state => ({ data, forTada: state.forTada.concat(forTada) }),
      () => forTada.length && this.tada()
    );
  };

  tada = () => {
    this.audio && this.audio.play();
  };

  render() {
    return (
      <div>
        <ReactTable data={this.state.data} columns={columns} />
        <audio src={tada} ref={n => (this.audio = n)} />
        <hr />
        <h2>Enhanced</h2>
        <ul>
          {this.state.forTada.map(x => {
            return (
              <li key={x.taskName}>
                {x.taskName}: {x.bestSolverName}
              </li>
            );
          })}
        </ul>
      </div>
    );
  }
}
