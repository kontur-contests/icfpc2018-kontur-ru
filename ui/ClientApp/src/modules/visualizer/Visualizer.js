import React from "react";
import { initVisualizer } from "./initVisualizer";
import PropTypes from "prop-types";

import "./Visualizer.css";

export class Visualizer extends React.Component {
  static propTypes = {
    model: PropTypes.arrayOf(
      PropTypes.arrayOf(PropTypes.arrayOf(PropTypes.number))
    ),
    bots: PropTypes.arrayOf(PropTypes.arrayOf(PropTypes.number))
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

  botsCache = [];

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
    const { model, bots } = this.props;
    if (model) {
      this.visualizer.setSize(
        model.length,
        model[0].length,
        model[0][0].length
      );
      this.visualizer.setMatrixFn(([x, y, z]) => model[x][y][z]);
    }
    if (bots) {
      this.botsCache.forEach(x => this.visualizer.botRem(x));
      this.botsCache.length = 0;
      this.botsCache = bots.map(x => this.visualizer.botAdd(...x));
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
