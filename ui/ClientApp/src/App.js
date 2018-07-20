import React, { Component } from "react";
import { Visualizer } from "./modules/visualizer";

const steps = [
  [
    [[0, 1, 0], [0, 1, 0], [0, 1, 0]],
    [[0, 0, 0], [0, 0, 0], [0, 0, 0]],
    [[0, 0, 0], [0, 0, 0], [0, 0, 0]]
  ],
  [
    [[0, 1, 0], [0, 1, 0], [0, 1, 0]],
    [[0, 1, 0], [1, 1, 1], [0, 1, 0]],
    [[0, 0, 0], [0, 0, 0], [0, 0, 0]]
  ],
  [
    [[0, 1, 0], [0, 1, 0], [0, 1, 0]],
    [[0, 1, 0], [1, 1, 1], [0, 1, 0]],
    [[0, 1, 0], [0, 1, 0], [0, 1, 0]]
  ]
];

export default class App extends Component {
  state = {
    step: 0
  };

  render() {
    return (
      <div style={{ maxWidth: "800px", margin: "auto" }}>
        <Visualizer data={steps[this.state.step]} />
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
