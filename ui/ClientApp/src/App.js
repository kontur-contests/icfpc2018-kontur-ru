import React, { Component } from "react";
import { Visualizer } from "./modules/visualizer";

const steps = [
  {
    model: [
      [[0, 1, 0], [0, 1, 0], [0, 1, 0]],
      [[0, 0, 0], [0, 0, 0], [0, 0, 0]],
      [[0, 0, 0], [0, 0, 0], [0, 0, 0]]
    ],
    bots: [[1, 1, 1]]
  },
  {
    model: [
      [[0, 1, 0], [0, 1, 0], [0, 1, 0]],
      [[0, 1, 0], [1, 1, 1], [0, 1, 0]],
      [[0, 0, 0], [0, 0, 0], [0, 0, 0]]
    ],
    bots: [[0, 0, 0], [2, 2, 2]]
  },
  {
    model: [
      [[0, 1, 0], [0, 1, 0], [0, 1, 0]],
      [[0, 1, 0], [1, 1, 1], [0, 1, 0]],
      [[0, 1, 0], [0, 1, 0], [0, 1, 0]]
    ],
    bots: [[1, 1, 1], [2, 2, 2]]
  }
];

export default class App extends Component {
  state = {
    step: 0,
    modelId: 0,
    steps: [],
    solutions: []
  };

  componentDidMount() {
    this.requestMatrix007();
  }

  render() {
    const stepData = this.state.steps[this.state.step];

    return (
      <div style={{ maxWidth: "1200px", margin: "auto" }}>
        {!stepData && <h1>Loading...</h1>}
        {stepData && (
          <Visualizer model={stepData.model} bots={stepData.bots} />
        )}
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
        <hr/>
        <h2>Solutions</h2>
        <ul>
          {this.state.solutions.map(x => (
            <li>
              <a href={`/?file=${x}`} key={x}>
                {x}
              </a>
            </li>
          ))}
        </ul>
      </div>
    );
  }

  requestMatrix007 = () => {
    const search = document.location.search;
    if (!search.includes("file")) {
      alert('Specify parameter "file" in url query');
    }

    fetch(`/api/matrix/trace${search}`)
      .then(x => x.json())
      .then(processData)
      .then(x => this.setState({ steps: x, step: 0 }));

    fetch(`/api/matrix/solutions`)
      .then(x => x.json())
      .then(x => this.setState({ solutions: x }));
  };

  requestModel = id => {
    fetch(`/api/matrix/index?i=${id}`)
      .then(x => x.json())
      .then(model => {
        this.setState({ steps: [{ model, bots: [] }], step: 0 });
      });
  };

  handleStepChange = event => {
    const step = parseInt(event.target.value);
    this.setState({ step });
  };

  handleModelChange = event => {
    const modelId = parseInt(event.target.value);
    this.setState({ modelId, steps: [] }, () => this.requestModel(modelId));
  };
}

function processData(data) {
  if (data.length === 0) {
    return 0;
  }

  const normalized = data.map(y => ({
    changes: y.change,
    bots: y.bots.map(x => [x.item1, x.item2, x.item3]),
    r: y.r
  }));

  const r = normalized[0].r;

  const steps = [];
  const model = Array.from({ length: r }, () =>
    Array.from({ length: r }, () => Array(r).fill(0))
  );

  for (let i = 0; i < data.length; i++) {
    const { changes, bots } = normalized[i];

    changes.forEach(({ x, y, z }) => {
      model[x][y][z] = 1;
    });

    steps.push({
      model: model.map(x => x.map(y => y.slice(0))),
      bots
    });
  }

  return steps;
}
