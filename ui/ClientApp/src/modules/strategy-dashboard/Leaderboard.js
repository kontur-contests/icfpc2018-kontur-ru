import React from "react";
import { getTotals, KONTUR_NAME } from "./StrategyDashboardApi";

import happy from "./happy.wav";
import sad from "./sad.wav";

export class Leaderboard extends React.Component {
  state = {
    records: [],
    diffs: {}
  };

  componentDidMount() {
    this.fetchData();

    this.interval = setInterval(this.fetchData, 15000);
  }

  componentWillUnmount() {
    clearInterval(this.interval);
  }

  fetchData = async () => {
    const records = (await getTotals()).sort((a, b) => b.score - a.score);

    const diffs = getDiffs(records, this.state.records, this.state.diffs);

    const curKonturPos = this.state.records.findIndex(
      x => x.name === KONTUR_NAME
    );
    const newKonturPos = records.findIndex(x => x.name === KONTUR_NAME);

    if (newKonturPos < curKonturPos) {
      this.happy && this.happy.play();
    }

    if (newKonturPos > curKonturPos) {
      this.sad && this.sad.play();
    }

    this.setState({ records, diffs });
  };

  render() {
    return (
      <React.Fragment>
        <h1>Leaderboard</h1>
        <table style={{ width: "100%" }}>
          <thead>{this.renderHead()}</thead>
          <tbody>{this.state.records.map(this.renderRow)}</tbody>
        </table>
        <audio src={happy} ref={a => (this.happy = a)} />
        <audio src={sad} ref={a => (this.sad = a)} />
      </React.Fragment>
    );
  }

  renderHead = () => {
    return (
      <tr>
        <th>Place</th>
        <th>Name</th>
        <th>Energy</th>
        <th>Score</th>
        <th>Score Change</th>
      </tr>
    );
  };

  renderRow = (record, index) => {
    const diff = this.state.diffs[record.name];

    const diffStyles = {};
    if (diff) {
      if (diff > 0) {
        diffStyles.background = "rgb(0, 255, 0)";
      } else {
        diffStyles.background = "rgb(255, 0, 0)";
      }
    }

    return (
      <tr
        key={record.name}
        style={{ background: record.name === KONTUR_NAME ? "orange" : "white" }}
      >
        <td>{index + 1}</td>
        <td>{record.name}</td>
        <td>{record.energy}</td>
        <td>{record.score}</td>
        <td styles={diffStyles}>{diff}</td>
      </tr>
    );
  };
}

function getDiffs(newRecords, curRecords, curDiffs) {
  const diffs = { ...curDiffs };

  console.log(newRecords);

  for (const record of newRecords) {
    const curMatchingRecord = curRecords.find(x => x.name === record.name);
    if (curMatchingRecord) {
      const diff = record.score - curMatchingRecord.score;
      if (diff) {
        diffs[record.name] = diff;
      }
    }
  }

  return diffs;
}
