import "./index.css";
import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter, Route, NavLink } from "react-router-dom";
import Traces from "./screens/Traces";
import { StrategyDashboard } from "./modules/strategy-dashboard";
import { Leaderboard } from "./modules/strategy-dashboard/Leaderboard";
import { StrategyTrace } from "./modules/strategy-dashboard/StrategyTrace";

const baseUrl = document.getElementsByTagName("base")[0].getAttribute("href");
const rootElement = document.getElementById("root");

ReactDOM.render(
  <BrowserRouter basename={baseUrl}>
    <div>
      <div>
        <NavLink to="/">Traces</NavLink>{"\t"}
        <NavLink to="/dashboard">Dashboard</NavLink>{"\t"}
        <NavLink to="/dashboard-old">Dashboard Old</NavLink>{"\t"}
        <NavLink to="/leaderboard">Leaderboard</NavLink>{"\t"}
        <NavLink to="/strategy-trace">Strategy Trace</NavLink>
      </div>
      <Route exact path="/" component={Traces} />
      <Route path="/dashboard" component={StrategyDashboard} />
      <Route path="/dashboard-old" component={() => <StrategyDashboard old />} />
      <Route path="/leaderboard" component={Leaderboard} />
      <Route path="/strategy-trace" component={StrategyTrace} />
    </div>
  </BrowserRouter>,
  rootElement
);
