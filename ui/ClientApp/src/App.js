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
    step: 0
  };

  render() {
    const stepData = steps[this.state.step];
    return (
      <div style={{ maxWidth: "800px", margin: "auto" }}>
        <Visualizer model={stepData.model} bots={stepData.bots} />
        <hr />
        <input
          style={{ width: "100%" }}
          type="range"
          onChange={this.handleStepChange}
          min={0}
          value={this.state.step}
          max={steps.length - 1}
        />
        <center>Step: {this.state.step}</center>
      </div>
    );
  }

  handleStepChange = event => {
    const step = parseInt(event.target.value);
    this.setState({ step });
  };
}
