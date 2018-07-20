(async () => {
    const fs = require('fs');
    const opn = require('opn');
    const puppeteer = require('puppeteer');

    const input = './models/problemsL.txt';
    const output = './images/';

    const expression = /[-a-zA-Z0-9@:%_\+.~#?&//=]{2,256}\.[a-z]{2,4}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]*)?/gi
    const regex = new RegExp(expression)

    const links = fs
        .readFileSync(input)
        .toString()
        .split('\n')
        .filter(x => x !== '')
        .map(x => x.match(regex).slice(1))
        .reduce((acc, cur) => [...acc, ...cur], [])

    const browser = await puppeteer.launch();

    Promise.all(links.map(async (x, i) => {
        const page = await browser.newPage();
        await page.goto(x);
        await page.click('button._cookie-banner-btn').catch(() => {})
        await page
            .$('img.thing-img')
            .then(x => x.boundingBox())
            .then(x => page.screenshot({
                path: `${output}${i}.png`,
                clip: x
            }));
    })).then(async data => {
        await browser.close();
        opn(output)
        process.exit(0)
    })
})();