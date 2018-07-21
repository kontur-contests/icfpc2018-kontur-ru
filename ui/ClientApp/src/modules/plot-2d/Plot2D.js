import React from "react";
import PropTypes from "prop-types";

import { Line } from "react-chartjs-2";

export class Plot2D extends React.Component {
  static propTypes = {
    data: PropTypes.arrayOf(
      PropTypes.shape({ x: PropTypes.number, y: PropTypes.number })
    ).isRequired
  };

  render() {
    return (
      <Line
        data={{
          labels: this.props.data.map(p => p.x),
          datasets: [
            {
              data: this.props.data,
              label: "energy",
              lineTension: 0.1,
              backgroundColor: "rgba(75,192,192,0.4)",
              borderColor: "rgba(75,192,192,1)",
              borderCapStyle: "butt",
              borderDash: [],
              borderDashOffset: 0.0,
              borderJoinStyle: "miter",
              pointBorderColor: "rgba(75,192,192,1)",
              pointBackgroundColor: "#fff",
              pointBorderWidth: 1,
              pointHoverRadius: 5,
              pointHoverBackgroundColor: "rgba(75,192,192,1)",
              pointHoverBorderColor: "rgba(220,220,220,1)",
              pointHoverBorderWidth: 2,
              pointRadius: 1,
              pointHitRadius: 10
            }
          ]
        }}
      />
    );
  }
}
