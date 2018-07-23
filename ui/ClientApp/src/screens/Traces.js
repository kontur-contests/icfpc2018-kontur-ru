import React, { Component } from "react";
import { Visualizer } from "../modules/visualizer";

const CHUNK_SIZE = 500;

export default class App extends Component {
  state = {
    step: 0,
    steps: [],
    solutions: [],
    index: false,
    loading: false,
    maxSteps: 2500
  };

  componentDidMount() {
    this.requestSolutions();
    this.fetchTrace();
  }

  componentDidUpdate(prevProps, prevState) {
    if (this.state.maxSteps !== prevState.maxSteps) {
      this.fetchTrace();
    }
  }

  async fetchTrace() {
    const search = document.location.search;
    if (!search.includes("file=")) {
      return;
    }
    const tickRanges = range(0, this.state.maxSteps, CHUNK_SIZE);
    const promises = tickRanges.map(x => {
      const q = `${search}&count=${CHUNK_SIZE}&startTick=${x}`;
      return fetch(`/api/matrix/trace${q}`).then(x => x.json());
    });
    const results = await Promise.all(promises);
    const flattened = results.reduce(
      (acc, x) => {
        acc.r = x.r;
        acc.ticks = acc.ticks.concat(x.ticks);
      },
      { r: 0, ticks: [] }
    );

    const { steps } = applyChanges(flattened);
    this.setState({ steps });
  }

  render() {
    const stepData = this.state.steps[this.state.step];

    return (
      <div style={{ maxWidth: "1200px", margin: "auto" }}>
        <label>
          Max tick{" "}
          <input
            type="number"
            value={this.state.maxSteps}
            onChange={({ target }) => this.setState({ maxSteps: target.value })}
            min={0}
          />
        </label>
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
              max={this.state.steps.length}
              disabled={this.state.loading}
            />
            <center>Step: {this.state.step}</center>
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

  requestSolutions() {
    fetch(`/api/matrix/solutions`)
      .then(x => x.json())
      .then(x => this.setState({ solutions: x }));
  }

  handleStepChange = event => {
    const step = parseInt(event.target.value);
    this.setState({ step });
  };
}

function applyChanges(data, anchorModel) {
  const model =
    anchorModel ||
    Array.from({ length: data.r }, () =>
      Array.from({ length: data.r }, () => Array(data.r).fill(0))
    );

  const steps = [];

  for (let i = 0; i < data.ticks.length; i++) {
    const { filled, cleared, bots, tickIndex } = data.ticks[i];

    filled.forEach(([x, y, z]) => {
      model[x][y][z] = 1;
    });

    cleared.forEach(([x, y, z]) => {
      model[x][y][z] = 0;
    });

    steps.push({
      model: model.map(x => x.map(y => y.slice(0))),
      bots,
      stepIndex: tickIndex
    });
  }

  return { steps };
}

function range(min, max, step) {
  if (step < 0 && min < max) {
    throw new TypeError("step < 0 && min < max");
  }

  if (step < 0) {
    return range(max, min, -step).reverse();
  }

  const result = [min];

  let current = min;
  while (current < max - step) {
    current += step;
    result.push(current);
  }

  return result;
}
