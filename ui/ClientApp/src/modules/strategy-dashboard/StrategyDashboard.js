import React from "react";
import { getSolutionResults, denormalizeData } from "./StrategyDashboardApi";
import { DataTable } from "./DataTable";

export class StrategyDashboard extends React.Component {
  state = {
    result: {},
    taskNames: [],
    solverNames: [],
    loading: true,
    denormalizedData: [],
    selectedSolver: "all"
  };

  componentDidMount() {
    this.fetchData();

    this.interval = setInterval(this.fetchData, 30000)
  }

  componentWillUnmount() {
    clearInterval(this.interval)
  }

  fetchData = async () => {
    try {
      console.log('FETCHING DASHBOARD')
      this.setState({ loading: true });
      const data = await getSolutionResults();
      console.log(denormalizeData(data));
      this.setState({
        result: data.result,
        taskNames: data.taskNames,
        solverNames: data.solverNames,
        denormalizedData: denormalizeData(data)
      });
      this.setState({ loading: false });
    } catch (e) {}

    // await new Promise(resolve => setTimeout(resolve, 15000))

    // this.fetchData()
  };

  render() {
    return (
      <div>
        <h2>Solutions</h2>
        <label>
          Solver:{' '}
          <select
            value={this.state.selectedSolver}
            onChange={({ target }) =>
              this.setState({ selectedSolver: target.value })
            }
          >
            <option value="all">All</option>
            {this.state.solverNames.map(x => (
              <option value={x} key={x}>
                {x}
              </option>
            ))}
          </select>
        </label>
        <hr/>
        {!this.props.old && (
          <DataTable
            data={this.state.denormalizedData.filter(this.filterData)}
            // loading={this.state.loading}
          />
        )}
        {this.props.old && this.renderOldTable()}
      </div>
    );
  }

  filterData = row => {
    if (this.state.selectedSolver === "all") {
      return true;
    }

    return row.solverName === this.state.selectedSolver;
  };

  renderOldTable() {
    return (
      <table style={{ width: "100%" }}>
        <thead>{this.renderHeader()}</thead>
        <tbody>{this.state.taskNames.map(this.renderSolutionRow)}</tbody>
      </table>
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

    const isSolved = this.state.solverNames.some(isFiniteValue);

    const sorted = this.state.solverNames
      .slice(0)
      .filter(isFiniteValue)
      .sort((a, b) => getEnergySpent(a) - getEnergySpent(b));

    const getRating = solverName => sorted.indexOf(solverName);

    return (
      <tr key={taskName}>
        <td style={{ background: isSolved ? "" : "orange" }}>{taskName}</td>
        {this.state.solverNames.map(x => {
          const energySpent = getEnergySpent(x);

          const rating = getRating(x) / (sorted.length - 1);

          const color = isFinite(energySpent) ? getColor(rating) : "black";

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
