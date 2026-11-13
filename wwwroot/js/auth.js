(() => {
    const form = document.querySelector('[data-auth-form]');
    if (!form) return;

    const password = form.querySelector('#Password');
    const toggle = form.querySelector('[data-password-toggle]');
    const submit = form.querySelector('[data-auth-submit]');
    const status = form.querySelector('[data-auth-status]');
    const originalText = submit.textContent;
    let submitting = false;

    toggle.addEventListener('click', () => {
        const visible = password.type === 'password';
        password.type = visible ? 'text' : 'password';
        toggle.setAttribute('aria-pressed', String(visible));
        toggle.textContent = visible ? 'Ẩn mật khẩu' : 'Hiện mật khẩu';
    });

    form.addEventListener('submit', event => {
        if (submitting) {
            event.preventDefault();
            return;
        }
        if (event.defaultPrevented || !form.checkValidity()) return;
        submitting = true;
        submit.disabled = true;
        submit.textContent = 'Đang xử lý…';
        form.setAttribute('aria-busy', 'true');
        status.textContent = 'Đang gửi thông tin, vui lòng chờ.';
    });

    // Back/forward navigation can restore the form from the browser's page cache.
    window.addEventListener('pageshow', () => {
        submitting = false;
        submit.disabled = false;
        submit.textContent = originalText;
        form.removeAttribute('aria-busy');
        status.textContent = '';
        password.type = 'password';
        toggle.setAttribute('aria-pressed', 'false');
        toggle.textContent = 'Hiện mật khẩu';
    });
})();
