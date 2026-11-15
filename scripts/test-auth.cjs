const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');

function fixture(base, database) {
    const data = JSON.parse(fs.readFileSync(process.env.TRACNGHIEM_TEST_SESSION || path.resolve('artifacts/access-test.json'), 'utf8').replace(/^\uFEFF/, ''));
    assert(/^TracNghiemWeb_AccessTest_[a-f0-9]{32}$/.test(data.Database), 'Disposable database required');
    assert.equal(data.Database, database, 'The requested database must match the fixture');
    const url = new URL(base);
    assert(['localhost', '127.0.0.1'].includes(url.hostname) && url.port === String(data.Port), 'Fixture URL required');
    return data;
}
const tokenFrom = html => {
    const token = html.match(/name="__RequestVerificationToken"[^>]*value="([^"]+)"/)?.[1];
    assert(token, 'Antiforgery token missing'); return token;
};
async function authenticateContext(context, base, account) {
    const login = await context.request.get(base + '/Account/Login');
    const result = await context.request.post(base + '/Account/Login', { maxRedirects: 0, form: {
        __RequestVerificationToken: tokenFrom(await login.text()), 'Input.Email': account.Email, 'Input.Password': account.Password, ReturnUrl: '/'
    } });
    assert([302, 303].includes(result.status()), 'Test sign-in failed');
    assert(result.headers().location.startsWith('/') && !result.headers().location.startsWith('//'), 'Sign-in returned an external redirect');
}
module.exports = { fixture, tokenFrom, authenticateContext };
