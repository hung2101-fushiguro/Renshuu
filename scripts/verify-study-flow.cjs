// Writes one attempt, then removes it. Run only against an isolated, seeded test database.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright-core');
const base = process.argv[2];
if (!base || !['localhost', '127.0.0.1'].includes(new URL(base).hostname) || process.argv[3] !== '--allow-test-writes') {
    throw new Error('Usage: node scripts/verify-study-flow.cjs http://localhost:5187 --allow-test-writes (isolated database only)');
}
const output = path.resolve('artifacts/study-flow');
fs.mkdirSync(output, { recursive: true });
const noNativePointer = () => {
    Element.prototype.requestPointerLock = () => Promise.reject(new Error('Disabled in test'));
    Element.prototype.setPointerCapture = () => {};
};
(async () => {
    const browser = await chromium.launch({ executablePath: process.env.SCROLLCRAFT_CHROME || 'C:/Program Files/Google/Chrome/Application/chrome.exe', headless: true });
    const errors = [], checks = [];
    let context, id, token, takePath;
    async function fresh(options = {}) {
        const result = await browser.newContext({ viewport: { width: 1440, height: 1000 }, ...options });
        await result.addInitScript(noNativePointer);
        return result;
    }
    function observe(page) { page.on('pageerror', e => errors.push(e.message)); page.on('dialog', dialog => dialog.accept()); }
    const getToken = page => page.locator('input[name="__RequestVerificationToken"]').first().inputValue();
    try {
        context = await fresh();
        let page = await context.newPage(); observe(page);
        await page.goto(base);
        await page.locator('#Input_QuestionCount').fill('2');
        await page.locator('#Input_TimeLimitMinutes').fill('10');
        await Promise.all([page.waitForURL('**/Test/Take/*'), page.locator('#start-practice').click()]);
        takePath = new URL(page.url()).pathname;
        id = Number(takePath.split('/').pop());
        token = await getToken(page);
        const questions = await page.locator('.question input[value="A"]').evaluateAll(inputs => inputs.map(input => input.name));
        assert(questions.length >= 2);
        const radio = (name, value) => page.locator(`input[name="${name}"][value="${value}"]`);
        await radio(questions[0], 'A').check();
        await page.waitForFunction(() => document.getElementById('draft-status').textContent.includes('Đã lưu 1/'));
        assert.equal(await page.locator('#progress').getAttribute('aria-valuenow'), '1');
        assert.notEqual(await page.locator('#timer-text').textContent(), '--:--');
        await page.screenshot({ path: path.join(output, 'draft-desktop.png') });
        await page.goto(base);
        assert.equal(await page.locator(`.pending-item[href="${takePath}"]`).count(), 1);
        await page.locator(`.pending-item[href="${takePath}"]`).click();
        assert(await radio(questions[0], 'A').isChecked());
        checks.push('Debounced draft saves, updates progress, and resumes from the dashboard');

        await context.close(); // Empty storage/cookies in another browser context.
        context = await fresh();
        page = await context.newPage(); observe(page);
        await page.goto(base + takePath);
        token = await getToken(page);
        assert(await radio(questions[0], 'A').isChecked());
        assert.equal(await page.evaluate(() => sessionStorage.length), 0);
        checks.push('Saved answers restore in a completely fresh browser context');
        await radio(questions[0], 'C').check();
        await radio(questions[1], 'B').check();
        await radio(questions[0], 'D').check();
        await page.waitForFunction(() => document.getElementById('draft-status').textContent.includes('Đã lưu 2/'));
        await page.reload();
        assert(await radio(questions[0], 'D').isChecked());
        assert(await radio(questions[1], 'B').isChecked());
        checks.push('Rapid edits retain the latest choices after a server reload');

        await page.route('**/*handler=SaveDraft*', route => route.abort());
        await radio(questions[0], 'B').check();
        await page.waitForFunction(() => document.getElementById('draft-status').textContent.includes('Chưa lưu được'));
        await page.reload();
        assert(await radio(questions[0], 'B').isChecked());
        await page.unroute('**/*handler=SaveDraft*');
        await page.evaluate(() => window.dispatchEvent(new Event('online')));
        await page.waitForFunction(() => document.getElementById('draft-status').textContent.includes('Đã lưu 2/'));
        checks.push('Failed saves keep the local draft and retry successfully after reconnecting');

        const nojs = await fresh({ javaScriptEnabled: false, viewport: { width: 390, height: 844 } });
        const nojsPage = await nojs.newPage();
        await nojsPage.goto(base + takePath);
        assert(await nojsPage.locator(`input[name="${questions[0]}"][value="B"]`).isChecked());
        assert.equal(await nojsPage.locator('#progress').getAttribute('aria-valuenow'), String(questions.length));
        await nojs.close();
        await page.setViewportSize({ width: 390, height: 844 });
        await page.waitForFunction(() => document.querySelector('.sidebar').getBoundingClientRect().right <= 1);
        await page.screenshot({ path: path.join(output, 'draft-mobile.png'), fullPage: true });
        assert(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth));
        await page.goto(base);
        await page.locator('.pending-panel').scrollIntoViewIfNeeded();
        await page.screenshot({ path: path.join(output, 'pending-mobile.png') });
        assert(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth));
        checks.push('Server-restored choices work without JavaScript; draft and resume views fit mobile');

        await page.goto(base + takePath);
        token = await getToken(page);
        const rejected = await context.request.post(base + takePath + '?handler=SaveDraft', { form: { id, [questions[0]]: 'A' } });
        assert.equal(rejected.status(), 400);
        const stale = await context.newPage(); observe(stale);
        await stale.goto(base + takePath);
        await Promise.all([page.waitForURL('**/Test/Result/*'), page.locator('button[form="quiz-form"]').click()]);
        await stale.locator(`input[name="${questions[0]}"][value="A"]`).check();
        await stale.waitForFunction(() => !!document.querySelector('#draft-status a'));
        assert(await stale.locator(`input[name="${questions[0]}"][value="A"]`).isDisabled());
        await stale.locator('#draft-status a').click();
        assert(new URL(stale.url()).pathname.startsWith('/Test/Result/'));
        await stale.close();
        const resultText = await page.locator('.result-summary').innerText();
        const closed = await context.request.post(base + takePath + '?handler=SaveDraft', { form: { id, __RequestVerificationToken: token, [questions[0]]: 'A' } });
        assert.equal(closed.status(), 409);
        assert.equal((await closed.json()).closed, true);
        await context.request.post(base + takePath, { form: { id, __RequestVerificationToken: token } });
        await page.reload();
        assert.equal(await page.locator('.result-summary').innerText(), resultText);
        checks.push('Antiforgery is enforced; saved drafts close after submission and repeated POSTs preserve the grade');

        await page.goto(base + '/Admin/Import');
        const item = { Subject: 'Browser preview only', Content: 'Preview duplicate', A: 'A', B: 'B', C: 'C', D: 'D', Correct: 'A' };
        await page.locator('input[type="file"]').setInputFiles({ name: 'preview.json', mimeType: 'application/json', buffer: Buffer.from(JSON.stringify([item, item, { ...item, Content: '', Correct: '' }])) });
        await Promise.all([page.waitForNavigation(), page.locator('button[type="submit"]').first().click()]);
        assert.equal(await page.locator('[data-preview-status="valid"]').count(), 1);
        assert.equal(await page.locator('[data-preview-status="duplicate"]').count(), 1);
        assert.equal(await page.locator('[data-preview-status="invalid"]').count(), 1);
        const filter = page.locator('[data-preview-filter]');
        await filter.selectOption('invalid');
        assert.equal(await page.locator('[data-preview-status]:visible').count(), 1);
        assert(await page.locator('[data-preview-status="invalid"]').isVisible());
        await page.screenshot({ path: path.join(output, 'import-mobile.png'), fullPage: true });
        await filter.selectOption('all');
        assert.equal(await page.locator('[data-preview-status]:visible').count(), 3);
        assert(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth));
        await page.setViewportSize({ width: 1440, height: 1000 });
        await page.screenshot({ path: path.join(output, 'import-desktop.png'), fullPage: true });
        await page.locator('input[type="file"]').setInputFiles({ name: 'invalid.json', mimeType: 'application/json', buffer: Buffer.from(JSON.stringify([{ ...item, Content: '', Correct: '' }])) });
        await Promise.all([page.waitForNavigation(), page.locator('button[type="submit"]').first().click()]);
        assert(await page.locator('form[action*="handler=Save"] button').isDisabled());
        await page.locator('[data-preview-filter]').selectOption('valid');
        assert.equal(await page.locator('[data-preview-status]:visible').count(), 0);
        assert(await page.locator('[data-preview-empty]').isVisible());
        checks.push('Real JSON upload previews within-file duplicates, filters invalid rows and preserves all rows for saving');
        assert.deepEqual(errors, []);
        checks.push('No browser JavaScript errors');
        console.log(checks.map(c => 'PASS ' + c).join('\n'));
    } finally {
        if (id) {
            if (!context || !context.pages().length) context = await fresh();
            // Submit if necessary so the existing history deletion workflow can remove it.
            const take = await context.request.get(base + takePath);
            const html = await take.text();
            const currentToken = html.match(/name="__RequestVerificationToken"[^>]*value="([^"]+)"/)?.[1] || token;
            await context.request.post(base + takePath, { form: { id, __RequestVerificationToken: currentToken } });
            const history = await context.request.get(base + '/History');
            const historyToken = (await history.text()).match(/name="__RequestVerificationToken"[^>]*value="([^"]+)"/)?.[1];
            assert(historyToken, 'Cleanup needs the history antiforgery token');
            await context.request.post(base + '/History?handler=Delete', { form: { id, __RequestVerificationToken: historyToken } });
            console.log('Cleaned up test attempt ' + id);
        }
        await browser.close();
    }
})().catch(error => { console.error(error); process.exitCode = 1; });
