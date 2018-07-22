import React, { Component } from "react";
import { Visualizer } from "../modules/visualizer";

const ChunkSize = 250;

export default class App extends Component {
  state = {
    step: 0,
    modelId: 0,
    steps: [],
    solutions: [],
    energy: [],
    index: false,
    page: -1,
    loading: false,
    totalSteps: 0
  };

  componentDidMount() {
    this.requestSolutions();
    this.loadNextPage();
  }

  componentDidUpdate(prevProps, prevState) {
    if (this.state.step > (this.state.page + 1) * ChunkSize) {
      this.loadNextPage();
    }

    if (this.state.step < this.state.page * ChunkSize) {
      this.loadPrevPage();
    }
  }

  render() {
    const stepData = this.state.steps[
      this.state.page
        ? this.state.step - (this.state.page - 1) * ChunkSize
        : this.state.step
    ];

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
              max={this.state.totalSteps}
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

  async loadNextPage() {
    const data = await this.requestPage(this.state.page + 1);
    if (data) this.applyNextPageData(data);
  }

  applyNextPageData(data) {
    const anchorStep = this.state.steps[this.state.page * ChunkSize];
    const calculatedModels = applyChanges(
      data,
      anchorStep ? anchorStep.model : null
    );
    this.setState({
      steps: calculatedModels.steps,
      totalSteps: calculatedModels.totalCount
    });
  }

  async loadPrevPage() {
    const data = await this.requestPage(this.state.page - 1);
    if (data) {
    }
  }

  requestPage(page) {
    const search = document.location.search;
    if (!search.includes("file=")) {
      return null;
    }
    const startTick = Math.max(0, (page - 1) * ChunkSize);
    const q = `${search}&count=${ChunkSize * 3}&startTick=${startTick}`;
    try {
      this.setState({ page, loading: true });
      return fetch(`/api/matrix/trace${q}`).then(x => x.json());
    } catch (e) {
    } finally {
      this.setState({ loading: false });
    }
  }

  requestSolutions() {
    fetch(`/api/matrix/solutions`)
      .then(x => x.json())
      .then(x => this.setState({ solutions: x }));
  }

  handleStepChange = event => {
    if (this.state.loading) {
      return
    }
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

  return { steps, totalCount: data.totalTicks };
}
