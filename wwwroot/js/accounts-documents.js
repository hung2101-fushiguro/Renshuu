(() => {
    'use strict';
    const userId = document.body.dataset.userId || '';
    const marker = 'tracnghiem_account';
    const clearOtherDrafts = () => {
        try {
            Object.keys(localStorage).filter(key => key.startsWith('quiz_draft_v2:') || key.startsWith('quiz_draft_v3:') && !key.startsWith('quiz_draft_v3:' + userId + ':')).forEach(key => localStorage.removeItem(key));
            localStorage.setItem(marker, userId);
        } catch (_) { /* Server ownership is authoritative even if browser storage is disabled. */ }
    };
    if (userId) clearOtherDrafts();
    window.addEventListener('storage', event => { if (event.key === marker && event.newValue !== userId) { window.dispatchEvent(new Event('account:changed')); location.reload(); } });
    window.addEventListener('pageshow', event => { if (event.persisted) location.reload(); });
    document.querySelectorAll('[data-logout]').forEach(form => form.addEventListener('submit', () => {
        try { Object.keys(localStorage).filter(key => key.startsWith('quiz_draft_v3:' + userId + ':')).forEach(key => localStorage.removeItem(key)); localStorage.setItem(marker, ''); } catch (_) { }
    }));
    document.querySelectorAll('[data-password-toggle]').forEach(button => button.addEventListener('click', () => {
        const input = document.getElementById(button.dataset.passwordToggle);
        const show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        button.textContent = show ? 'Ẩn' : 'Hiện';
        button.setAttribute('aria-pressed', String(show));
        button.setAttribute('aria-label', show ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');
    }));
    const menu = document.querySelector('.user-menu');
    document.addEventListener('click', event => { if (menu && !menu.contains(event.target)) menu.open = false; });
    document.addEventListener('keydown', event => { if (event.key === 'Escape' && menu?.open) { menu.open = false; menu.querySelector('summary').focus(); } });
    const dialog = document.getElementById('action-dialog');
    let pending, submitter;
    document.querySelectorAll('form[data-confirm]').forEach(form => form.addEventListener('submit', event => {
        if (form.dataset.confirmed === 'true') { delete form.dataset.confirmed; return; }
        event.preventDefault();
        pending = form; submitter = event.submitter;
        document.getElementById('action-description').textContent = form.dataset.confirm;
        dialog.showModal();
        document.getElementById('action-cancel').focus();
    }));
    document.getElementById('action-cancel')?.addEventListener('click', () => dialog.close());
    document.getElementById('action-confirm')?.addEventListener('click', () => {
        if (!pending) return;
        const form = pending, button = submitter;
        form.dataset.confirmed = 'true';
        pending = null;
        dialog.close();
        form.requestSubmit(button);
    });
    document.querySelectorAll('form[data-busy]').forEach(form => form.addEventListener('submit', event => {
        if (event.defaultPrevented) return;
        form.setAttribute('aria-busy', 'true');
        form.querySelectorAll('button[type="submit"]').forEach(button => { button.disabled = true; button.textContent = form.dataset.busy; });
    }));
    const upload = document.querySelector('.document-form input[type="file"]');
    if (upload) {
        const selection = document.getElementById('file-selection');
        const refreshFile = () => {
            const file = upload.files?.[0];
            let error = '';
            if (file && !/\.(pdf|docx|pptx|txt)$/i.test(file.name)) error = 'Chỉ nhận PDF, DOCX, PPTX hoặc TXT.';
            if (file && file.size > 20 * 1024 * 1024) error = 'Mỗi tệp tối đa 20 MB.';
            upload.setCustomValidity(error);
            selection.textContent = error || (file ? file.name + ' · ' + (file.size / 1024 / 1024 < 1 ? Math.max(1, Math.round(file.size / 1024)) + ' KB' : (file.size / 1024 / 1024).toFixed(1) + ' MB') : '');
            selection.classList.toggle('text-danger', !!error);
            const title = document.getElementById('Input_Title');
            if (file && !error && title && !title.value.trim()) title.value = file.name.replace(/\.[^.]+$/, '').slice(0, 200);
        };
        upload.addEventListener('change', refreshFile);
        const zone = document.querySelector('[data-drop-zone]');
        zone?.addEventListener('dragover', event => { event.preventDefault(); zone.classList.add('dragging'); });
        zone?.addEventListener('dragleave', () => zone.classList.remove('dragging'));
        zone?.addEventListener('drop', event => {
            event.preventDefault(); zone.classList.remove('dragging');
            if (event.dataTransfer.files.length !== 1) { selection.textContent = 'Chọn một tệp mỗi lần tải lên.'; return; }
            upload.files = event.dataTransfer.files; refreshFile();
        });
    }
})();
