const fs = require('fs');
const opn = require('opn');

const input = './models/problemsL.txt';
const output = './models.html';

const data = fs
    .readFileSync(input)
    .toString()
    .split('\n')
    .filter(x => x !== '')
    .map(x => x.split(':: '))
    .map(x => ([
        x[0],
        ...x[1].split('; ')
    ]))
    .map(x => ([
        x[0],
        x[1],
        ...x[2].split(' (')
    ]))
    .map(x => ([
        x[0],
        x[1],
        x[2].split(' & '),
        x[3].replace(')', '')
    ]))
    .map(x => `<li>${x[2].map((y, i) => `${i !== 0 ? ', ' : ''}<a target='_blank' href='${y}'>${x[3]}</a>`)}</li>`)
    .join('<br/>')

fs.writeFileSync(output, `<ul>${data}</ul>`)

opn(output)

process.exit(0)