const ELASTIC_URL =
  "/api/elastic";

const makeRequest = query => {
  return fetch(`${ELASTIC_URL}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(query)
  });
};

const getAllSolutionsWithMinimalEnergy = () => {
  return makeRequest({
    size: 0,
    aggs: {
      task_name: {
        terms: {
          field: "taskName.keyword",
          size: 1000
        },
        aggs: {
          min_energy: {
            min: {
              field: "energySpent"
            }
          }
        }
      }
    }
  }).then(x => x.json());
};

const searchSolutionsForProblem = problemName => {
  return makeRequest({
    size: 1000,
    query: {
      bool: {
        should: [{ match: { "taskName.keyword": problemName } }]
      }
    },
    _source: ["energySpent", "taskName", "solverName"]
  }).then(x => x.json());
};

export const getSolutionResults = async () => {
  const solutionsWithMinimalEnergy = await getAllSolutionsWithMinimalEnergy();
  const problemNames = solutionsWithMinimalEnergy.aggregations.task_name.buckets.map(
    x => x.key
  );

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

  const nameGroup = {};
  for (const group of solutions) {
    for (const solution of group) {
      if (!nameGroup[solution.name]) {
        nameGroup[solution.name] = {};
      }

      if (!nameGroup[solution.name][solution.solverName]) {
        nameGroup[solution.name][solution.solverName] = [];
      }

      nameGroup[solution.name][solution.solverName].push(solution);
    }
  }

  const result = {};
  const taskNames = new Set();
  const solverNames = new Set();
  for (const taskName in nameGroup) {
    const solverGroup = nameGroup[taskName];
    result[taskName] = {};
    taskNames.add(taskName);
    for (const solverName in solverGroup) {
      const solverResults = solverGroup[solverName];
      result[taskName][solverName] = minBy(x => x.energySpent, solverResults).energySpent;
      solverNames.add(solverName);
    }
  }

  return { result, taskNames: [...taskNames].sort(), solverNames: [...solverNames] };
};

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