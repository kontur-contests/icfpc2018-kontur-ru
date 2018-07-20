import React from "react";
import { initVisualizer } from "./initVisualizer";
import PropTypes from "prop-types";

import "./Visualizer.css";

export class Visualizer extends React.Component {
  static propTypes = {
    data: PropTypes.arrayOf(
      PropTypes.arrayOf(PropTypes.arrayOf(PropTypes.number))
    )
  };

  /**
   * @type {HTMLCanvasElement | null}
   */
  canvas = null;

  /**
   * @type {HTMLElement | null}
   */
  canvasContainer = null;

  visualizer = null;

  componentDidMount() {
    this.visualizer = initVisualizer(
      {
        screenshot: true,
        controls: true
      },
      this.canvas,
      this.canvasContainer
    );

    this.doImperativeStuff();
  }

  componentWillUnmount() {
    if (this.visualizer) {
      this.visualizer.removeSubscriptions();
    }
  }

  componentDidUpdate() {
    this.doImperativeStuff();
  }

  doImperativeStuff() {
    const { data } = this.props;
    if (data) {
      this.visualizer.setSize(data.length, data[0].length, data[0][0].length);
      this.visualizer.setMatrixFn(([x, y, z]) => data[x][y][z]);
    }
  }

  render() {
    return (
      <div
        ref={cntr => (this.canvasContainer = cntr)}
        style={{ position: "relative" }}
        className="Visualizer"
      >
        <canvas
          className="Visualizer__canvas"
          ref={cnvs => (this.canvas = cnvs)}
          tabIndex="0"
        />
      </div>
    );
  }
}
