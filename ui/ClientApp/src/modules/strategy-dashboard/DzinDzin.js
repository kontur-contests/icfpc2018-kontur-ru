import React from "react";
import { getTasksBestSolutions } from "./StrategyDashboardApi";

import dzin from "./dzin.mp3";

const INTERVAL = 10000;

export class DzinDzin extends React.Component {
  state = {
    solutions: {},
    lastSolved: []
  };

  componentDidMount() {
    this.fetchData();

    this.interval = setInterval(this.fetchData, INTERVAL);
  }

  componentWillUnmount() {
    clearInterval(this.this.interval);
  }

  fetchData = async () => {
    const solutions = await getTasksBestSolutions();

    const lastSolved = [];

    if (Object.keys(this.state.solutions).length) {
      for (const taskName in solutions) {
        if (solutions[taskName] && !this.state.solutions[taskName]) {
          lastSolved.push(taskName);
        }
      }
    }

    this.setState(
      { solutions, lastSolved },
      () => lastSolved.length && this.dzinDzin()
    );
  };

  dzinDzin() {
    if (this.player) {
      this.player.play();
    }
  }

  render() {
    return (
      <div>
        <audio src={dzin} ref={p => (this.player = p)} />
        <h2>Last Solved</h2>
        <ul>
          {this.state.lastSolved.map(x => (
            <li key={x}>
              {x}: {this.state.solutions[x]}
            </li>
          ))}
        </ul>
        <hr />
        <h2>Non solved</h2>
        <ul>
          {Object.keys(this.state.solutions)
            .filter(x => this.state.solutions[x] === 0)
            .map(x => (
              <li key={x}>
                {x}
              </li>
            ))}
        </ul>
      </div>
    );
  }
}
