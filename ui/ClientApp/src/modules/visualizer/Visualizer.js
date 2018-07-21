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
    
    if (this.props.model) {
      const sizes = modelToSizes(this.props.model)
      this.visualizer.setSize(sizes.x, sizes.y, sizes.z);
    } 
    
    this.doImperativeStuff();
  }

  componentWillUnmount() {
    if (this.visualizer) {
      this.visualizer.removeSubscriptions();
    }
  }

  componentDidUpdate(prevProps) {
    this.updateSizeIfNeeded(prevProps.model, this.props.model);
    this.doImperativeStuff();
  }

  updateSizeIfNeeded(prevModel, curModel) {
    const prevSizes = prevModel && modelToSizes(prevModel)
    const curSizes = curModel && modelToSizes(curModel);

    if (
      (!prevModel && curModel) ||
      prevSizes.x !== curSizes.x ||
      prevSizes.y !== curSizes.y ||
      prevSizes.z !== curSizes.z
    ) {
      this.visualizer.setSize(curSizes.x, curSizes.y, curSizes.z);
    }
  }

  doImperativeStuff() {
    const { model, bots } = this.props;
    if (model) {
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

function modelToSizes(model) {
  return {
    x: model.length,
    y: model[0].length,
    z: model[0][0].length
  }
}