// Account form behaviour: password visibility toggle and a single-submit guard.
(function () {
    'use strict';

    document.querySelectorAll('[data-password-toggle]').forEach(function (button) {
        var input = document.getElementById(button.getAttribute('aria-controls'));
        if (!input) { return; }

        button.addEventListener('click', function () {
            var show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            button.setAttribute('aria-pressed', show ? 'true' : 'false');
            button.textContent = show ? 'Ẩn' : 'Hiện';
            input.focus();
        });
    });

    var form = document.querySelector('.auth-form');
    var submit = form && form.querySelector('[data-auth-submit]');
    var status = form && form.querySelector('[data-auth-status]');
    if (!form || !submit) { return; }

    form.addEventListener('submit', function () {
        if (submit.getAttribute('aria-busy') === 'true') {
            return;
        }

        submit.setAttribute('aria-busy', 'true');
        submit.disabled = true;
        submit.textContent = 'Đang đăng nhập…';
        if (status) { status.textContent = 'Đang kiểm tra thông tin đăng nhập.'; }
    });
})();
