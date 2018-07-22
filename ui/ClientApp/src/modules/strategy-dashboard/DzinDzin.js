import React from "react";
import { getNotSolvedProblems } from "./StrategyDashboardApi";

import dzin from "./dzin.mp3";

const INTERVAL = 10000;

export class DzinDzin extends React.Component {
  state = {
    problems: [],
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
    const problems = (await getNotSolvedProblems()).filter(
      x => !x.includes("_tgt")
    ).sort();

    const lastSolved = [];

    if (problems.length) {
      for (const taskName of this.state.problems) {
        if (!problems.includes(taskName)) {
          lastSolved.push(taskName);
        }
      }
    }

    this.setState(state =>
      ({ problems, lastSolved: lastSolved.length ? lastSolved : state.lastSolved }),
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
              {x}
            </li>
          ))}
        </ul>
        <hr />
        <h2>Non solved ({this.state.problems.length})</h2>
        <ul>{this.state.problems.map(x => <li key={x}>{x}</li>)}</ul>
      </div>
    );
  }
}
