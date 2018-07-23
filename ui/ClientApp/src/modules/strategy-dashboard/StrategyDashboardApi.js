const ELASTIC_URL = "/api/elastic";

const MAX_RETRIES = 5;

const LEADERS_NAME = "ZZZ ICFP Leaders";
export const KONTUR_NAME = "kontur.ru";

const makeRequest = (query, retries = 0) => {
  return fetch(`${ELASTIC_URL}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(query)
  })
    .then(x => (x.status > 399 ? Promise.reject(x) : x))
    .catch(err => {
      if (retries >= MAX_RETRIES) {
        throw err;
      }
      return makeRequest(query, retries + 1);
    });
};

function bestSolutionsMorpher({
  aggregations: {
    taskName: { buckets }
  }
}) {
  return buckets.map(x => ({
    taskName: x.key,
    solverName: x.solverName.buckets[0].key,
    energySpent: x.energySpent.value
  }));
}

const getBestSolutions = (start, end) => {
  return makeRequest({
    size: 0,
    _source: {
      excludes: []
    },
    aggs: {
      taskName: {
        terms: {
          field: "taskName.keyword",
          size: 10000,
          order: {
            energySpent: "desc"
          }
        },
        aggs: {
          energySpent: {
            min: {
              field: "energySpent"
            }
          },
          solverName: {
            terms: {
              field: "solverName.keyword",
              size: 1,
              order: {
                energySpent: "asc"
              }
            },
            aggs: {
              energySpent: {
                min: {
                  field: "energySpent"
                }
              }
            }
          }
        }
      }
    },
    stored_fields: ["*"],
    script_fields: {},
    docvalue_fields: ["startedAt"],
    query: {
      bool: {
        must: [
          {
            match_all: {}
          },
          {
            match_phrase: {
              isSuccess: {
                query: true
              }
            }
          },
          {
            range: {
              startedAt: {
                gte: start,
                lte: end,
                format: "epoch_millis"
              }
            }
          }
        ],
        filter: [],
        should: [],
        must_not: []
      }
    }
  })
    .then(x => x.json())
    .then(bestSolutionsMorpher);
};

const getLastBestSolutions = () => {
  const tenMinsBefore = Date.now() - 10 * 60 * 1000;
  return getBestSolutions(tenMinsBefore, Date.now());
};

const getOldBestSolutions = () => {
  const tenMinsBefore = Date.now() - 10 * 60 * 1000;
  const weekAgo = Date.now() - 4 * 24 * 60 * 60 * 1000;
  return getBestSolutions(weekAgo, tenMinsBefore);
};

export const getStrategyTraces = async () => {
  const [oldSolutions, curSolutions] = await Promise.all([
    getOldBestSolutions(),
    getBestSolutions()
  ]);

  console.log('oldSolutions', oldSolutions)
  console.log('curSolutions', curSolutions)

  const curSolutionsMap = curSolutions.reduce((acc, x) => {
    acc[x.taskName] = x;
    return acc;
  }, {});

  return oldSolutions.map(oldSolution => {
    const curSolution = curSolutionsMap[oldSolution.taskName] || {};
    return {
      taskName: curSolution.taskName,
      oldBestSolverName: oldSolution.solverName,
      oldBestSolverEnergy: oldSolution.energySpent,
      bestSolverName: curSolution.solverName,
      bestSolverEnergy: curSolution.energySpent
    };
  });
};

const getAllSolutionsWithMinimalEnergy = () => {
  return makeRequest({
    size: "0",
    aggs: {
      task_name: {
        terms: {
          field: "taskName.keyword",
          size: 100000
        }
      }
    }
  }).then(x => x.json());
};

const searchSolutionsForProblem = async problemName => {
  const result = await makeRequest({
    size: 1000,
    query: {
      bool: {
        filter: [{ term: { "taskName.keyword": problemName } }]
      }
    },
    _source: ["energySpent", "taskName", "solverName"]
  }).then(x => x.json());

  return result;
};

const getLeaderBoard = () => fetch(`/api/leaderboard`).then(x => x.json());

export const getTotals = () => {
  return getLeaderBoard().then(x => x.filter(y => y.probNum === "total"));
};

function selectData(taskNameGroup, leadersGroupped) {
  const result = {};
  const taskNames = new Set();
  const solverNames = new Set();

  for (const taskName in taskNameGroup) {
    const solverGroup = taskNameGroup[taskName];
    result[taskName] = {};
    taskNames.add(taskName);

    for (const solverName in solverGroup) {
      const solverResults = solverGroup[solverName];
      result[taskName][solverName] = minBy(
        x => (x.energySpent ? x.energySpent : Infinity),
        solverResults
      ).energySpent;
      solverNames.add(solverName);
    }

    solverNames.add(LEADERS_NAME);
    if (taskName in leadersGroupped) {
      result[taskName][LEADERS_NAME] = minBy(
        x => x.energy,
        leadersGroupped[taskName]
      ).energy;
    }
  }

  return { result, taskNames, solverNames, leadersGroupped };
}

function groupTasks(solutions) {
  const taskNameGroup = {};
  for (const group of solutions) {
    for (const solution of group) {
      if (!taskNameGroup[solution.name]) {
        taskNameGroup[solution.name] = {};
      }

      if (!taskNameGroup[solution.name][solution.solverName]) {
        taskNameGroup[solution.name][solution.solverName] = [];
      }

      taskNameGroup[solution.name][solution.solverName].push(solution);
    }
  }
  return taskNameGroup;
}

function groupLeaders(leaderboard) {
  const leadersGroup = {};
  for (const record of leaderboard) {
    if (!leadersGroup[record.probNum]) {
      leadersGroup[record.probNum] = [];
    }
    leadersGroup[record.probNum].push(record);
  }
  return leadersGroup;
}

export const getTasksBestSolutions = async () => {
  const solutionsWithMinimalEnergy = await getAllSolutionsWithMinimalEnergy();

  const result = {};
  for (let bucket of solutionsWithMinimalEnergy.aggregations.task_name
    .buckets) {
    result[bucket.key] = bucket.min_energy.value;
  }

  return result;
};

export const getNotSolvedProblems = async () => {
  const solved = await makeRequest({
    size: "0",
    query: {
      bool: {
        filter: {
          term: {
            isSuccess: "true"
          }
        }
      }
    },
    aggs: {
      task_name: {
        terms: {
          field: "taskName.keyword",
          size: 100000
        }
      }
    }
  })
    .then(x => x.json())
    .then(x => x.aggregations.task_name.buckets)
    .then(x => x.map(x => x.key));

  const solutionsWithMinimalEnergy = await getAllSolutionsWithMinimalEnergy();

  const problemNames = solutionsWithMinimalEnergy.aggregations.task_name.buckets
    .map(x => x.key)
    .filter(x => !x.startsWith("LA") && !x.includes("_tgt"));

  return problemNames.filter(x => !solved.includes(x));
};

export const getSolutionResults = async () => {
  const solutionsWithMinimalEnergy = await getAllSolutionsWithMinimalEnergy();

  const problemNames = solutionsWithMinimalEnergy.aggregations.task_name.buckets
    .map(x => x.key)
    .filter(x => !x.startsWith("LA") && !x.includes("_tgt"));

  const searchResults = await Promise.all(
    problemNames.map(searchSolutionsForProblem)
  );

  const solutions = searchResults.map(result => {
    return result.hits.hits.map(({ _source: x }) => ({
      name: x.taskName,
      solverName: x.solverName,
      energySpent: x.energySpent
    }));
  });

  const leaderboard = await getLeaderBoard();

  const { result, taskNames, solverNames, leadersGroupped } = selectData(
    groupTasks(solutions),
    groupLeaders(leaderboard)
  );

  return {
    result,
    taskNames: [...taskNames].sort(),
    solverNames: [...solverNames].sort(
      (a, b) => (a.toLowerCase() > b.toLowerCase() ? 1 : -1)
    ),
    leadersGroupped
  };
};

function raterFactory(solverSolutions, solverNames) {
  const getEnergySpent = solverName =>
    solverSolutions[solverName] === 0 ||
    solverSolutions[solverName] === undefined
      ? Infinity
      : solverSolutions[solverName];

  const isFiniteValue = solverName => {
    return (
      solverSolutions[solverName] !== 0 &&
      solverSolutions[solverName] !== undefined
    );
  };

  const rates = solverNames
    .filter(isFiniteValue)
    .sort((a, b) => getEnergySpent(a) - getEnergySpent(b));
  // console.log(rates);
  return solverName =>
    rates.indexOf(solverName) / Math.max(rates.length - 1, 1);
}

export function denormalizeData({
  result,
  taskNames,
  solverNames,
  leadersGroupped
}) {
  const data = [];

  for (const taskName of taskNames) {
    const rater = raterFactory(result[taskName], solverNames);

    for (const solverName of solverNames) {
      if (solverName === LEADERS_NAME) {
        continue;
      }

      const energy = result[taskName][solverName];
      const isSolved = energy !== 0 && energy !== undefined;
      const leaderEnergy = result[taskName][LEADERS_NAME] || Infinity;

      let konturScore;
      let bestScore;
      if (leadersGroupped[taskName]) {
        konturScore = leadersGroupped[taskName].find(
          x => x.name === KONTUR_NAME
        ).score;
        bestScore = maxBy(x => x.score, leadersGroupped[taskName]).score;
      }

      const record = {
        energy: isSolved ? energy : Infinity,
        taskName,
        solverName,
        leaderEnergy,
        rate: rater(solverName),
        leaderRate: isSolved ? rater(LEADERS_NAME) : 0,
        konturScore,
        bestScore
      };

      data.push(record);
    }
  }

  return data;
}

export function minBy(fn, xs) {
  let min;
  for (const x of xs) {
    if (min === undefined) {
      min = x;
      continue;
    }

    if (fn(min) > fn(x)) {
      min = x;
    }
  }
  return min;
}

export function maxBy(fn, xs) {
  let max;
  for (const x of xs) {
    if (max === undefined) {
      max = x;
      continue;
    }

    if (fn(max) < fn(x)) {
      max = x;
    }
  }
  return max;
}
