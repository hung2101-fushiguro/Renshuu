const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const { chromium } = require('playwright-core');
const JSZip = require('jszip');
const { fixture, tokenFrom, authenticateContext } = require('./test-auth.cjs');
const base = process.argv[2], session = fixture(base, process.argv[3]);
const output = path.resolve('artifacts/access-documents');
fs.mkdirSync(output, { recursive: true });
const checks = [], errors = [];
const pass = text => { checks.push(text); console.log('PASS ' + text); };
const observe = page => { page.on('pageerror', error => errors.push(error.message)); page.on('response', response => { if (response.status() >= 500) errors.push(response.status() + ' ' + response.url()); }); };
const password = () => 'Aa9!' + crypto.randomBytes(18).toString('base64');
const token = async page => page.locator('input[name="__RequestVerificationToken"]').first().inputValue();
const sameOrigin = value => new URL(value, base).origin === new URL(base).origin;
const decodeHtml = html => html.replace(/&#x([a-f0-9]+);/gi, (_, value) => String.fromCodePoint(parseInt(value, 16))).replace(/&#(\d+);/g, (_, value) => String.fromCodePoint(Number(value)));
function pdf() {
    const text = 'BT /F1 18 Tf 60 180 Td (Private study document) Tj ET';
    const objects = ['<< /Type /Catalog /Pages 2 0 R >>', '<< /Type /Pages /Kids [3 0 R] /Count 1 >>',
        '<< /Type /Page /Parent 2 0 R /MediaBox [0 0 360 240] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>',
        '<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>', `<< /Length ${Buffer.byteLength(text)} >>\nstream\n${text}\nendstream`];
    let data = '%PDF-1.4\n'; const offsets = [0];
    objects.forEach((object, index) => { offsets.push(Buffer.byteLength(data)); data += `${index + 1} 0 obj\n${object}\nendobj\n`; });
    const xref = Buffer.byteLength(data);
    data += `xref\n0 6\n0000000000 65535 f \n${offsets.slice(1).map(offset => String(offset).padStart(10, '0') + ' 00000 n ').join('\n')}\ntrailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n${xref}\n%%EOF\n`;
    return Buffer.from(data);
}
async function office(type) {
    const zip = new JSZip(); zip.file('[Content_Types].xml', '<Types/>');
    if (type === 'docx') zip.file('word/document.xml', '<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body><w:p><w:r><w:t>Tài liệu Word tiếng Việt</w:t></w:r></w:p></w:body></w:document>');
    else { zip.file('ppt/presentation.xml', '<p:presentation xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main"/>'); zip.file('ppt/slides/slide1.xml', '<p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"><a:p><a:r><a:t>Nội dung slide tiếng Việt</a:t></a:r></a:p></p:sld>'); }
    return zip.generateAsync({ type: 'nodebuffer' });
}
async function apiUpload(context, title, name, buffer) {
    const response = await context.request.get(base + '/Documents/Upload');
    const result = await context.request.post(base + '/Documents/Upload', { multipart: { __RequestVerificationToken: tokenFrom(await response.text()), 'Input.Title': title, Upload: { name, mimeType: 'application/octet-stream', buffer } } });
    assert.equal(result.status(), 200); assert.match(new URL(result.url()).pathname, /^\/Documents\/View\/\d+$/);
    return Number(new URL(result.url()).pathname.split('/').pop());
}
async function main() {
    const browser = await chromium.launch({ executablePath: 'C:/Program Files/Google/Chrome/Application/chrome.exe', headless: true });
    const contexts = [];
    async function context(account, options = {}) {
        const value = await browser.newContext({ viewport: { width: 1440, height: 900 }, ...options }); contexts.push(value);
        if (account) await authenticateContext(value, base, account);
        return value;
    }
    try {
        const guest = await context();
        for (const route of ['/', '/Documents', '/Admin', '/History', '/Administration/Users']) {
            const response = await guest.request.get(base + route, { maxRedirects: 0 });
            assert.equal(response.status(), 302); assert(response.headers().location.includes('/Account/Login'));
        }
        const invalidCsrf = await guest.request.post(base + '/Account/Login', { form: { 'Input.Email': session.Accounts.userA.Email, 'Input.Password': session.Accounts.userA.Password } });
        assert.equal(invalidCsrf.status(), 400);
        pass('Anonymous pages require login and account POSTs enforce antiforgery');

        const ownerContext = await context();
        const owner = await ownerContext.newPage(); observe(owner);
        await owner.goto(base + '/Account/Login');
        await owner.screenshot({ path: path.join(output, 'login-desktop.png'), fullPage: true });
        await owner.locator('#Input_Email').fill(session.Accounts.userA.Email);
        await owner.locator('#Input_Password').fill('WrongPassword9!');
        await owner.locator('[data-password-toggle]').click(); assert.equal(await owner.locator('#Input_Password').getAttribute('type'), 'text');
        await owner.locator('[data-password-toggle]').click();
        await owner.locator('button[type="submit"]').click();
        await owner.waitForLoadState('networkidle');
        assert.match(await owner.locator('.validation-summary-errors').innerText(), /Email hoặc mật khẩu/);
        await owner.locator('#Input_Password').fill(session.Accounts.userA.Password);
        await owner.locator('button[type="submit"]').click(); await owner.waitForURL(base + '/');
        assert((await ownerContext.cookies()).find(cookie => cookie.name === 'TracNghiem.Auth').httpOnly);
        assert.equal(await owner.locator('a[href="/Administration/Users"]').count(), 0);
        pass('Login validates credentials, supports password managers/show-password and uses an HttpOnly cookie');

        const signup = await context();
        const signupPage = await signup.newPage(); observe(signupPage); await signupPage.goto(base + '/Account/Register');
        await signupPage.setViewportSize({ width: 390, height: 844 }); await signupPage.screenshot({ path: path.join(output, 'register-mobile.png'), fullPage: true });
        const registrationToken = await token(signupPage);
        const registered = await signup.request.post(base + '/Account/Register', { form: { __RequestVerificationToken: registrationToken, 'Input.DisplayName': 'Người học mới', 'Input.Email': 'signup_' + Date.now() + '@access.example.test', 'Input.Password': 'SignupPassword9!', 'Input.ConfirmPassword': 'SignupPassword9!', 'Input.Role': 'Admin', Role: 'Admin', IsActive: true } });
        assert.equal(registered.status(), 200); assert.equal(new URL(registered.url()).pathname, '/');
        const blockedAdmin = await signup.request.get(base + '/Administration/Users'); assert.equal(blockedAdmin.status(), 403);
        assert.equal((await signup.request.get(base + '/Admin')).status(), 200);
        pass('Registration always creates User; both roles retain the shared bank, while user administration is restricted');

        await owner.goto(base + '/Documents/Upload');
        await owner.locator('#Upload').setInputFiles({ name: 'notes-userA.txt', mimeType: 'text/plain', buffer: Buffer.from('Nội dung riêng của userA\n<script>window.uploadXss=true</script>') });
        assert.equal(await owner.locator('#Input_Title').inputValue(), 'notes-userA');
        await owner.locator('#Input_Title').fill('PRN222 · Tài liệu riêng của userA');
        await owner.locator('#Input_Description').fill('Ghi chú học tập.');
        await owner.screenshot({ path: path.join(output, 'upload-desktop.png'), fullPage: true });
        await owner.locator('.document-form button[type="submit"]').click(); await owner.waitForURL('**/Documents/View/*');
        const documentId = Number(new URL(owner.url()).pathname.split('/').pop());
        assert.match(await owner.locator('.document-text').innerText(), /Nội dung riêng/); assert.equal(await owner.evaluate(() => window.uploadXss), undefined);
        await owner.screenshot({ path: path.join(output, 'document-text-desktop.png'), fullPage: true });
        pass('User upload, filename-based title, Unicode preview and escaped document text work');

        const otherContext = await context(session.Accounts.userB);
        const other = await otherContext.newPage(); observe(other); await other.goto(base + '/Documents');
        const otherToken = await token(other);
        await owner.goto(base + '/Documents/Edit/' + documentId); const firstVersion = await owner.locator('#Input_Version').inputValue();
        for (const route of ['/Documents/View/', '/Documents/Edit/', '/Documents/File/']) assert.equal((await otherContext.request.get(base + route + documentId)).status(), 404);
        const forbiddenEdit = await otherContext.request.post(base + '/Documents/Edit/' + documentId, { form: { __RequestVerificationToken: otherToken, 'Input.Title': 'Stolen', 'Input.Version': firstVersion } });
        assert.equal(forbiddenEdit.status(), 404);
        const forbiddenDelete = await otherContext.request.post(base + '/Documents/View/' + documentId + '?handler=Delete', { form: { __RequestVerificationToken: otherToken, version: firstVersion } }); assert.equal(forbiddenDelete.status(), 404);
        assert.equal((await otherContext.request.get(base + '/Documents?Scope=all')).status(), 403);
        assert.equal((await guest.request.get(base + '/Documents/File/' + documentId, { maxRedirects: 0 })).status(), 302);
        pass('A different user cannot list, view, edit, delete or download another user’s files');

        const pdfId = await apiUpload(ownerContext, 'Tài liệu PDF', 'study.pdf', pdf());
        const partial = await ownerContext.request.get(base + '/Documents/File/' + pdfId, { headers: { Range: 'bytes=0-4' } });
        assert.equal(partial.status(), 206); assert.equal((await partial.body()).toString(), '%PDF-');
        assert.match(partial.headers()['content-type'], /application\/pdf/); assert.match(partial.headers()['cache-control'], /no-store/);
        await owner.goto(base + '/Documents/View/' + pdfId); assert(await owner.locator('.pdf-preview').isVisible());
        await owner.waitForTimeout(1800); // The browser's native PDF viewer renders outside the page's DOM.
        await owner.screenshot({ path: path.join(output, 'document-pdf-desktop.png'), fullPage: true });
        const docxId = await apiUpload(ownerContext, 'Tài liệu Word', 'study.docx', await office('docx'));
        await owner.goto(base + '/Documents/View/' + docxId); assert.match(await owner.locator('.document-text').innerText(), /Word tiếng Việt/);
        const pptxId = await apiUpload(ownerContext, 'Bài trình chiếu', 'study.pptx', await office('pptx'));
        await owner.goto(base + '/Documents/View/' + pptxId); assert.match(await owner.locator('.document-text').innerText(), /Nội dung slide/);
        const download = await ownerContext.request.get(base + '/Documents/File/' + docxId + '?download=true'); assert.match(download.headers()['content-disposition'], /attachment/);
        pass('PDF range viewing and protected downloads work; DOCX/PPTX produce text previews');

        await owner.goto(base + '/Documents/Upload');
        const badFile = await ownerContext.request.post(base + '/Documents/Upload', { multipart: { __RequestVerificationToken: await token(owner), 'Input.Title': 'Invalid', Upload: { name: 'fake.pdf', mimeType: 'application/pdf', buffer: Buffer.from('not a PDF') } } });
        assert.equal(new URL(badFile.url()).pathname, '/Documents/Upload'); assert.match(await badFile.text(), /PDF/);
        const badCsrf = await ownerContext.request.post(base + '/Documents/Upload', { multipart: { 'Input.Title': 'No token', Upload: { name: 'notes.txt', mimeType: 'text/plain', buffer: Buffer.from('hello') } } }); assert.equal(badCsrf.status(), 400);
        pass('File signatures are checked on the server and upload mutations require antiforgery');

        const adminContext = await context(session.Accounts.admin);
        const admin = await adminContext.newPage(); observe(admin); await admin.goto(base + '/Documents?Scope=all');
        assert.match(await admin.locator('.document-rows').innerText(), /PRN222/);
        await admin.screenshot({ path: path.join(output, 'admin-documents-desktop.png'), fullPage: true });
        await admin.goto(base + '/Documents/Edit/' + documentId);
        await admin.locator('#Input_Title').fill('Tài liệu đã được admin chỉnh sửa');
        await admin.locator('#Input_TextContent').fill('Nội dung TXT do admin cập nhật.');
        await admin.locator('button[type="submit"]').filter({ hasText: 'Lưu thay đổi' }).click(); await admin.waitForURL('**/Documents/View/*');
        await owner.goto(base + '/Documents/View/' + documentId); assert.match(await owner.locator('.document-text').innerText(), /admin cập nhật/);
        const staleResult = await ownerContext.request.post(base + '/Documents/Edit/' + documentId, { form: { __RequestVerificationToken: await token(owner), 'Input.Title': 'Stale update', 'Input.Version': firstVersion } });
        assert.match(decodeHtml(await staleResult.text()), /vừa được cập nhật/);
        await admin.goto(base + '/Documents/Edit/' + documentId);
        await admin.locator('#Upload').setInputFiles({ name: 'replacement.txt', mimeType: 'text/plain', buffer: Buffer.from('Tệp thay thế bởi admin.') });
        await admin.locator('button[type="submit"]').filter({ hasText: 'Lưu thay đổi' }).click(); await admin.waitForURL('**/Documents/View/*');
        const replacement = await ownerContext.request.get(base + '/Documents/File/' + documentId + '?download=true'); assert.equal((await replacement.body()).toString(), 'Tệp thay thế bởi admin.');
        pass('Admin can edit user metadata/TXT and replace files; stale edits are rejected without changing ownership');

        await admin.locator('.document-delete button').click(); assert(await admin.locator('#action-dialog').isVisible());
        await admin.locator('#action-cancel').click(); assert(await admin.locator('h1').isVisible());
        await admin.locator('.document-delete button').click(); await admin.locator('#action-confirm').click(); await admin.waitForURL('**/Documents?scope=all');
        assert.equal((await ownerContext.request.get(base + '/Documents/File/' + documentId)).status(), 404);
        pass('Document deletion confirms, removes access immediately, and returns admin to all documents');

        const managed = { Email: 'managed_' + Date.now() + '@access.example.test', Password: password() };
        await admin.goto(base + '/Administration/Users/Edit');
        await admin.locator('#Input_DisplayName').fill('Nguyễn Test'); await admin.locator('#Input_Email').fill(managed.Email); await admin.locator('#Input_Password').fill(managed.Password);
        await admin.locator('button[type="submit"]').filter({ hasText: 'Tạo tài khoản' }).click(); await admin.waitForURL('**/Administration/Users');
        await admin.locator('#Q').fill(managed.Email); await admin.locator('.document-filters button').click(); await admin.waitForLoadState('networkidle');
        const managedPath = await admin.locator('.directory-action a').getAttribute('href');
        const managedContext = await context(managed);
        const retainedDoc = await apiUpload(managedContext, 'Giữ tài liệu khi xóa user', 'retained.txt', Buffer.from('Nội dung được giữ lại.'));
        await admin.goto(base + managedPath); await admin.locator('#Input_IsActive').uncheck();
        await admin.locator('button[type="submit"]').filter({ hasText: 'Lưu thay đổi' }).click(); await admin.waitForURL('**/Administration/Users');
        assert.equal((await managedContext.request.get(base, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })).status(), 401);
        pass('Admin can create and disable users; disabling revokes an existing session immediately');

        await admin.goto(base + managedPath); await admin.locator('#Input_IsActive').check(); await admin.locator('#Input_Role').selectOption('Admin');
        await admin.locator('button[type="submit"]').filter({ hasText: 'Lưu thay đổi' }).click(); await admin.waitForURL('**/Administration/Users');
        await authenticateContext(managedContext, base, managed);
        assert.equal((await managedContext.request.get(base + '/Administration/Users')).status(), 200);
        await admin.goto(base + managedPath); await admin.locator('#Input_Role').selectOption('User');
        await admin.locator('button[type="submit"]').filter({ hasText: 'Lưu thay đổi' }).click(); await admin.waitForURL('**/Administration/Users');
        assert.equal((await managedContext.request.get(base, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })).status(), 401);
        await authenticateContext(managedContext, base, managed); assert.equal((await managedContext.request.get(base + '/Administration/Users')).status(), 403);
        pass('Role changes take effect through session invalidation; admin access cannot remain after demotion');

        await admin.goto(base + managedPath); await admin.locator('.account-danger button').click(); await admin.locator('#action-confirm').click(); await admin.waitForURL('**/Administration/Users');
        assert.equal((await adminContext.request.get(base + '/Documents/View/' + retainedDoc)).status(), 200);
        assert.equal((await managedContext.request.get(base, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })).status(), 401);
        const home = await adminContext.request.get(base); const adminId = (await home.text()).match(/data-user-id="([^"]+)"/)[1];
        await admin.goto(base + '/Administration/Users/Edit/' + adminId); assert(await admin.locator('.account-danger button').isDisabled());
        await admin.locator('#Input_Role').selectOption('User'); await admin.locator('button[type="submit"]').filter({ hasText: 'Lưu thay đổi' }).click();
        await admin.waitForLoadState('networkidle'); assert.match(await admin.locator('.validation-summary-errors').innerText(), /không thể tự/);
        pass('Deleting a user preserves documents for admin; the current admin cannot remove their own access');

        const redirected = await context(); const login = await redirected.request.get(base + '/Account/Login');
        const safeReturn = await redirected.request.post(base + '/Account/Login', { maxRedirects: 0, form: { __RequestVerificationToken: tokenFrom(await login.text()), 'Input.Email': session.Accounts.userB.Email, 'Input.Password': session.Accounts.userB.Password, ReturnUrl: 'https://example.invalid' } });
        assert.equal(safeReturn.status(), 302); assert(sameOrigin(safeReturn.headers().location));
        await owner.goto(base); const scope = await owner.getAttribute('body', 'data-user-id');
        await owner.evaluate(scope => localStorage.setItem('quiz_draft_v3:' + scope + ':/Test/Take/999', '{}'), scope);
        await owner.locator('.user-menu summary').click(); await owner.locator('[data-logout] button').click(); await owner.waitForURL('**/Account/Login');
        assert.equal(await owner.evaluate(scope => Object.keys(localStorage).filter(key => key.startsWith('quiz_draft_v3:' + scope + ':')).length, scope), 0);
        assert.equal((await ownerContext.request.get(base, { maxRedirects: 0 })).status(), 302);
        pass('Return URLs stay local; logout is POST and clears only that account’s local drafts');

        await authenticateContext(ownerContext, base, session.Accounts.userA);
        await owner.goto(base);
        await owner.locator('#Input_QuestionCount').fill('1'); await owner.locator('#Input_TimeLimitMinutes').fill('10');
        await owner.locator('#start-practice').click(); await owner.waitForURL('**/Test/Take/*');
        const privateAttemptPath = new URL(owner.url()).pathname;
        const choices = owner.locator('.question input[type="radio"]');
        await choices.nth(0).check(); await owner.waitForFunction(() => document.getElementById('draft-status').textContent.includes('Đã lưu 1/'));
        assert.equal((await adminContext.request.get(base + privateAttemptPath)).status(), 404);
        await ownerContext.clearCookies(); await choices.nth(1).check();
        await owner.waitForFunction(() => document.getElementById('draft-status').textContent.includes('Phiên đăng nhập đã hết'));
        await owner.locator('#draft-status a').click(); await owner.waitForURL('**/Account/Login*');
        await owner.locator('#Input_Email').fill(session.Accounts.userA.Email); await owner.locator('#Input_Password').fill(session.Accounts.userA.Password);
        await owner.locator('button[type="submit"]').click(); await owner.waitForURL('**/Test/Take/*');
        assert(await owner.locator('.question input[type="radio"]').nth(1).isChecked());
        await owner.waitForFunction(() => document.getElementById('draft-status').textContent.includes('Đã lưu 1/'));
        const privateScope = await owner.locator('#quiz-form').getAttribute('data-draft-scope');
        await ownerContext.request.post(base + '/Account/Logout', { form: { __RequestVerificationToken: await token(owner) } });
        await authenticateContext(ownerContext, base, session.Accounts.userB); await owner.goto(base);
        assert.equal(await owner.evaluate(scope => Object.keys(localStorage).filter(key => key.startsWith('quiz_draft_v3:' + scope + ':')).length, privateScope), 0);
        assert.equal((await ownerContext.request.get(base + privateAttemptPath)).status(), 404);
        pass('Session expiry preserves unsynced answers for the same account; changing accounts removes previous local drafts');

        const oldPassword = session.Accounts.userB.Password, newPassword = password();
        await other.goto(base + '/Account/Profile');
        await other.locator('#Input_DisplayName').fill('Nguyễn Bình'); await other.locator('.account-password summary').click();
        await other.locator('#Input_CurrentPassword').fill(oldPassword); await other.locator('#Input_NewPassword').fill(newPassword); await other.locator('#Input_ConfirmPassword').fill(newPassword);
        await other.locator('button[type="submit"]').filter({ hasText: 'Lưu thay đổi' }).click(); await other.waitForLoadState('networkidle');
        assert.equal(await other.locator('#Input_DisplayName').inputValue(), 'Nguyễn Bình');
        assert.equal((await redirected.request.get(base, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })).status(), 401);
        await other.locator('.account-password summary').click();
        await other.locator('#Input_CurrentPassword').fill(newPassword); await other.locator('#Input_NewPassword').fill(oldPassword); await other.locator('#Input_ConfirmPassword').fill(oldPassword);
        await other.locator('button[type="submit"]').filter({ hasText: 'Lưu thay đổi' }).click(); await other.waitForLoadState('networkidle');
        pass('Profile and password updates keep the current session and revoke old sessions');
        for (const width of [1440, 1024, 768, 390]) {
            await admin.setViewportSize({ width, height: width === 390 ? 844 : 900 });
            for (const route of ['/Administration/Users', '/Documents?Scope=all', '/Documents/Upload', '/Administration/Users/Edit', '/Documents/Edit/' + docxId, '/Account/Profile', '/History/Analytics', '/Wrong/Flashcard', '/']) {
                await admin.goto(base + route);
                assert(await admin.evaluate(() => document.documentElement.scrollWidth <= innerWidth), `Overflow ${route} at ${width}`);
            }
        }
        await admin.goto(base + '/Administration/Users'); await admin.screenshot({ path: path.join(output, 'admin-users-mobile.png'), fullPage: true });
        await admin.goto(base + '/Documents?Scope=all'); await admin.screenshot({ path: path.join(output, 'admin-documents-mobile.png'), fullPage: true });
        await admin.goto(base + '/Documents/Upload'); await admin.screenshot({ path: path.join(output, 'upload-mobile.png'), fullPage: true });
        await admin.setViewportSize({ width: 1440, height: 900 }); await admin.goto(base + '/Administration/Users'); await admin.screenshot({ path: path.join(output, 'admin-users-desktop.png'), fullPage: true });
        await admin.emulateMedia({ reducedMotion: 'reduce' }); await admin.locator('.user-menu summary').focus(); await admin.keyboard.press('Enter'); assert(await admin.locator('.user-menu').getAttribute('open') !== null); await admin.keyboard.press('Escape'); assert.equal(await admin.locator('.user-menu').getAttribute('open'), null);
        assert.deepEqual(errors, []);
        pass('Account/document/admin layouts fit four widths, support keyboard/reduced motion and have no browser errors');
        fs.writeFileSync(path.join(output, 'results.json'), JSON.stringify({ checks, errors }, null, 2));
    } finally { for (const context of contexts) await context.close(); await browser.close(); }
}
main().catch(error => { console.error(error); process.exitCode = 1; });
