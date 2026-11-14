// Run against the isolated database recorded by the local test session only.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { execFileSync } = require('node:child_process');
const { chromium } = require('playwright-core');
const base = process.argv[2];
const database = process.argv[3];
if (!base || !['localhost', '127.0.0.1'].includes(new URL(base).hostname) || !/^TracNghiemWeb_UiTest_[a-f0-9]{32}$/.test(database || ''))
    throw new Error('Usage: node scripts/verify-study-flow.cjs http://localhost:5081 TracNghiemWeb_UiTest_<32 hex chars>');
const checks = [], errors = [];
const sql = query => execFileSync('sqlcmd', ['-S', 'localhost', '-d', database, '-E', '-C', '-b', '-h', '-1', '-W', '-Q', 'SET NOCOUNT ON; ' + query], { encoding: 'utf8' }).trim();
const correct = id => sql(`SELECT CorrectOption FROM dbo.Questions WHERE Id=${Number(id)}`);
const observe = page => {
    page.on('pageerror', error => errors.push(error.message));
    page.on('dialog', dialog => dialog.accept());
};
const pass = text => { checks.push(text); console.log('PASS ' + text); };
const waitSaved = (page, count) => page.waitForFunction(value => document.getElementById('draft-status').textContent.includes(`Đã lưu ${value}/`), count);
const radios = page => page.locator('.question input[value="A"]').evaluateAll(inputs => inputs.map(input => ({ name: input.name, id: Number(input.name.match(/\d+/)[0]) })));
const radio = (page, item, value) => page.locator(`input[name="${item.name}"][value="${value}"]`);
async function start(page, count = 3, minutes = '') {
    await page.goto(base);
    await page.locator('#Input_QuestionCount').fill(String(count));
    await page.locator('#Input_TimeLimitMinutes').fill(String(minutes));
    await page.locator('#start-practice').click();
    await page.waitForURL('**/Test/Take/*');
    return { path: new URL(page.url()).pathname, id: Number(new URL(page.url()).pathname.split('/').pop()) };
}
async function main() {
    const browser = await chromium.launch({ executablePath: process.env.CHROME_PATH || 'C:/Program Files/Google/Chrome/Application/chrome.exe', headless: true });
    try {
        let context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
        let page = await context.newPage(); observe(page);
        await page.goto(base);
        assert(await page.locator('[data-count-preset="20"]').isDisabled());
        await page.locator('[data-time-preset="30"]').click();
        assert.equal(await page.locator('#Input_TimeLimitMinutes').inputValue(), '30');
        await page.locator('#library-search').fill('LAP TRINH');
        assert.equal(await page.locator('.library-item:visible').count(), 1);
        await page.locator('#library-search').fill('does not exist');
        assert(await page.locator('#library-empty').isVisible());
        pass('Setup presets respect the bank size; library search ignores Vietnamese accents');

        const attempt = await start(page, 3, 10);
        let items = await radios(page);
        assert.equal(items.length, 3);
        assert(!await page.content().then(html => html.includes('CorrectOption')));
        const right = correct(items[0].id);
        await radio(page, items[0], right).check(); await waitSaved(page, 1);
        assert.equal(await page.locator('#progress').getAttribute('aria-valuenow'), '1');
        assert.match(await page.locator('#timer-text').textContent(), /^\d{2}:\d{2}$/);
        await page.reload();
        assert(await radio(page, items[0], right).isChecked());
        await page.locator('#leave-quiz').click();
        await page.waitForURL(base + '/');
        assert.equal(await page.locator(`.pending-item[href="${attempt.path}"]`).count(), 1);
        await page.locator(`.pending-item[href="${attempt.path}"]`).click();
        assert(await radio(page, items[0], right).isChecked());
        pass('Random exam, live progress, server draft, reload, save-and-exit and resume work');

        await context.close();
        context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
        page = await context.newPage(); observe(page);
        await page.goto(base + attempt.path);
        assert(await radio(page, items[0], right).isChecked());
        await radio(page, items[0], 'C').check();
        await radio(page, items[0], 'B').check();
        await radio(page, items[0], 'D').check();
        await waitSaved(page, 1);
        await page.reload(); assert(await radio(page, items[0], 'D').isChecked());
        pass('Draft restores without browser storage; rapid edits retain the latest choice');

        await page.route('**/*handler=SaveDraft*', route => route.abort());
        await radio(page, items[0], 'A').check();
        await page.waitForFunction(() => document.getElementById('draft-status').textContent.includes('Chưa đồng bộ'));
        await page.reload(); assert(await radio(page, items[0], 'A').isChecked());
        await page.unroute('**/*handler=SaveDraft*');
        await page.evaluate(() => window.dispatchEvent(new Event('online')));
        await waitSaved(page, 1);
        await page.locator(`[data-clear-question="${items[0].id}"]`).click();
        await waitSaved(page, 0);
        await page.reload(); assert.equal(await page.locator('.question input:checked').count(), 0);
        pass('Offline choices survive reload, retry on reconnect, and can be cleared completely');

        const nojs = await browser.newContext({ javaScriptEnabled: false, viewport: { width: 390, height: 844 } });
        const plain = await nojs.newPage(); observe(plain);
        await plain.goto(base + attempt.path);
        await radio(plain, items[0], right).check();
        await plain.locator('#leave-quiz').click(); await plain.waitForURL(base + '/');
        await plain.goto(base + attempt.path);
        assert(await radio(plain, items[0], right).isChecked());
        assert.equal(await plain.locator('#progress').getAttribute('aria-valuenow'), '1');
        await nojs.close();
        pass('Without JavaScript, save-and-exit and server-restored answers still work');

        await page.reload();
        for (const width of [390, 768, 1024, 1440]) {
            await page.setViewportSize({ width, height: width === 390 ? 844 : 900 });
            assert(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), `Overflow at ${width}`);
        }
        await page.emulateMedia({ reducedMotion: 'reduce' });
        await page.locator('.question-overview').evaluate(element => { element.open = true; });
        await page.locator('#next-unanswered').click();
        assert.equal(await page.locator('.question:focus').getAttribute('data-question-number'), '2');
        pass('Question navigation, focus, reduced motion and four viewport widths work');

        const stale = await context.newPage(); observe(stale); await stale.goto(base + attempt.path);
        await page.locator('button[form="quiz-form"]').filter({ hasText: 'Nộp bài' }).click();
        assert(await page.locator('#submit-dialog').isVisible());
        assert.match(await page.locator('#submit-description').innerText(), /2 câu còn trống/);
        await page.locator('#cancel-submit').click();
        assert(!await page.locator('#submit-dialog').isVisible());
        await page.locator('button[form="quiz-form"]').filter({ hasText: 'Nộp bài' }).click();
        await page.locator('#confirm-submit').click(); await page.waitForURL('**/Test/Result/*');
        assert.match(await page.locator('.result-summary').innerText(), /1\/3/);
        assert.equal(await page.evaluate(key => localStorage.getItem(key), 'quiz_draft_v2:' + attempt.path), null);
        const summary = await page.locator('.result-summary').innerText();
        if (!await radio(stale, items[0], 'B').isDisabled()) await radio(stale, items[0], 'B').check();
        await stale.waitForFunction(() => !!document.querySelector('#draft-status a'));
        assert(await radio(stale, items[0], 'B').isDisabled());
        const token = await stale.locator('input[name="__RequestVerificationToken"]').inputValue();
        const blocked = await context.request.post(base + attempt.path + '?handler=SaveDraft', { form: { id: attempt.id, [items[0].name]: 'B' } });
        assert.equal(blocked.status(), 400);
        const closed = await context.request.post(base + attempt.path + '?handler=SaveDraft', { form: { id: attempt.id, __RequestVerificationToken: token, [items[0].name]: 'B' } });
        assert.equal(closed.status(), 409);
        await context.request.post(base + attempt.path, { form: { id: attempt.id, __RequestVerificationToken: token } });
        await page.reload(); assert.equal(await page.locator('.result-summary').innerText(), summary);
        await stale.close();
        pass('Incomplete submission confirms; grading is correct; stale tabs and duplicate POSTs cannot alter it');

        const failureAttempt = await start(page, 2);
        items = await radios(page);
        for (const item of items) await radio(page, item, correct(item.id)).check();
        await waitSaved(page, 2);
        await page.route('**' + failureAttempt.path, route => route.request().method() === 'POST' ? route.abort() : route.continue());
        await page.locator('button[form="quiz-form"]').filter({ hasText: 'Nộp bài' }).click();
        await page.waitForFunction(() => document.getElementById('draft-status').textContent.includes('Chưa gửi được'));
        assert.equal(await page.locator('.question input:checked').count(), 2);
        assert(await page.evaluate(key => !!localStorage.getItem(key), 'quiz_draft_v2:' + failureAttempt.path));
        await page.unroute('**' + failureAttempt.path);
        await page.locator('button[form="quiz-form"]').filter({ hasText: 'Nộp bài' }).click();
        await page.waitForURL('**/Test/Result/*');
        assert.match(await page.locator('.result-summary').innerText(), /2\/2/);
        pass('A failed submission keeps all choices and can be retried successfully');

        await start(page, 1);
        await page.locator('button[form="quiz-form"]').filter({ hasText: 'Nộp bài' }).click();
        await page.locator('#confirm-submit').click(); await page.waitForURL('**/Test/Result/*');
        assert.match(await page.locator('.result-summary').innerText(), /0\/1/);
        pass('Submitting a completely blank exam grades zero correctly');

        const timed = await start(page, 1, 1);
        items = await radios(page);
        await radio(page, items[0], correct(items[0].id)).check(); await waitSaved(page, 1);
        sql(`UPDATE dbo.Attempts SET StartedAt=DATEADD(second,-59,GETDATE()) WHERE Id=${timed.id}`);
        await page.reload(); await page.waitForURL('**/Test/Result/*', { timeout: 10000 });
        assert.match(await page.locator('.result-summary').innerText(), /1\/1/);
        pass('The countdown expires and automatically submits the saved exam');

        for (const route of ['/', '/Admin', '/Admin/Questions', '/Admin/Paste', '/Admin/AddMany', '/Admin/Readings', '/History', '/Wrong', '/Folder/1']) {
            const response = await page.goto(base + route);
            assert.equal(response.status(), 200, route);
        }
        for (const width of [390, 768, 1024, 1440]) {
            await page.setViewportSize({ width, height: 900 }); await page.goto(base);
            assert(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), `Home overflow at ${width}`);
        }
        assert.deepEqual(errors, []);
        pass('Existing management, import, reading, folder, wrong-answer and history routes work without JavaScript errors');
        fs.mkdirSync('artifacts/study-flow', { recursive: true });
        fs.writeFileSync(path.resolve('artifacts/study-flow/results.json'), JSON.stringify({ checks, errors }, null, 2));
    } finally { await browser.close(); }
}
main().catch(error => { console.error(error); process.exitCode = 1; });
