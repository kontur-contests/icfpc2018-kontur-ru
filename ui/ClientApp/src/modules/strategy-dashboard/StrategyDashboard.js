import React from "react";
import { getSolutionResults, maxBy, minBy } from "./StrategyDashboardApi";

export class StrategyDashboard extends React.Component {
  state = {
    result: {},
    taskNames: [],
    solverNames: []
  };

  componentDidMount() {
    getSolutionResults().then(x => {
      console.log(x);
      this.setState({
        result: x.result,
        taskNames: x.taskNames,
        solverNames: x.solverNames
      });
    });
  }

  render() {
    return (
      <div>
        <h2>Solutions</h2>
        <table style={{ width: "100%" }}>
          <thead>{this.renderHeader()}</thead>
          <tbody>{this.state.taskNames.map(this.renderSolutionRow)}</tbody>
        </table>
      </div>
    );
  }

  renderHeader() {
    return (
      <tr>
        <th>Task Name</th>
        {this.state.solverNames.map(x => <th key={x}>{x}</th>)}
      </tr>
    );
  }

  renderSolutionRow = taskName => {
    const solverSolutions = this.state.result[taskName];

    const getEnergySpent = solverName =>
      solverSolutions[solverName] === 0 ||
      solverSolutions[solverName] === undefined
        ? Infinity
        : solverSolutions[solverName];

    const isFiniteValue = solverName => {
      return (
        solverSolutions[solverName] !== 0 &&
        solverSolutions[solverName] !== undefined
      );
    };

    const bestSolver = getEnergySpent(
      minBy(getEnergySpent, this.state.solverNames.filter(isFiniteValue))
    );

    const worstSolver = getEnergySpent(
      maxBy(getEnergySpent, this.state.solverNames.filter(isFiniteValue))
    );

    const isSolved = this.state.solverNames.some(isFiniteValue);
    
    if ((!isSolved)) {
      console.log(taskName)
    } 

    return (
      <tr key={taskName}>
        <td style={{ background: isSolved ? "" : "orange" }}>{taskName}</td>
        {this.state.solverNames.map(x => {
          const energySpent = getEnergySpent(x);

          const pct = (energySpent - bestSolver) / (worstSolver - bestSolver);

          const color = isFinite(energySpent) ? getColor(pct) : "black";

          return (
            <td
              key={x}
              style={{
                background: color,
                color: isFinite(energySpent) ? "black" : "white"
              }}
            >
              {energySpent}
            </td>
          );
        })}
      </tr>
    );
  };
}

function getColor(value) {
  //value from 0 to 1
  var hue = ((1 - value) * 120).toString(10);
  return ["hsl(", hue, ",100%,50%)"].join("");
}
