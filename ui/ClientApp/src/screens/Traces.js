import React, { Component } from "react";
import { Visualizer } from "../modules/visualizer";
import { Plot2D } from "../modules/plot-2d/Plot2D";

export default class App extends Component {
  state = {
    step: 0,
    modelId: 0,
    steps: [],
    solutions: [],
    energy: [],
    index: false
  };

  componentDidMount() {
    this.requestMatrix007();
  }

  render() {
    const stepData = this.state.steps[this.state.step];

    return (
      <div style={{ maxWidth: "1200px", margin: "auto" }}>
        {!this.state.index && !stepData && <h2>Loading...</h2>}
        {stepData && (
          <React.Fragment>
            <Visualizer model={stepData.model} bots={stepData.bots} />
            <hr />
            <input
              style={{ width: "100%" }}
              type="range"
              onChange={this.handleStepChange}
              min={0}
              value={this.state.step}
              max={this.state.steps.length - 1}
            />
            <center>Step: {this.state.step}</center>
            <hr />
            <h2>Energy</h2>
            <Plot2D data={this.state.energy} />
          </React.Fragment>
        )}
        <hr />
        <h2>Solutions</h2>
        <ul>
          {this.state.solutions.map(x => (
            <li key={x}>
              <a href={`/?file=${x}`}>{x}</a>
            </li>
          ))}
        </ul>
      </div>
    );
  }

  requestMatrix007 = () => {
    const search = document.location.search;
    if (!search.includes("file")) {
      this.setState({ index: true });
    } else {
      fetch(`/api/matrix/trace${search}`)
        .then(x => x.json())
        .then(processData)
        .then(x =>
          this.setState({ steps: x.steps, step: 0, energy: x.energy })
        );
    }

    fetch(`/api/matrix/solutions`)
      .then(x => x.json())
      .then(x => this.setState({ solutions: x }));
  };

  handleStepChange = event => {
    const step = parseInt(event.target.value);
    this.setState({ step });
  };
}

function processData(data) {
  const r = data.r;

  const steps = [];
  const model = Array.from({ length: r }, () =>
    Array.from({ length: r }, () => Array(r).fill(0))
  );

  for (let i = 0; i < data.ticks.length; i++) {
      const { changes, bots, energy } = data.ticks[i];

    changes.forEach(([ x, y, z ]) => {
      model[x][y][z] = 1;
    });

    steps.push({
      model: model.map(x => x.map(y => y.slice(0))),
      bots
    });
  }

    const energy = data.ticks.map((step, x) => ({
    y: step.energy,
    x
  }));

  return { steps, energy };
}
