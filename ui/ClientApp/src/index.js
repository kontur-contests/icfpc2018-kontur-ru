import "./index.css";
import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter, Route } from "react-router-dom";
import Traces from "./screens/Traces";

const baseUrl = document.getElementsByTagName("base")[0].getAttribute("href");
const rootElement = document.getElementById("root");

ReactDOM.render(
  <BrowserRouter basename={baseUrl}>
    <div>
      <Route exact path="/" component={Traces} />
    </div>
  </BrowserRouter>,
  rootElement
);
