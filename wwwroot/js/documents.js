(() => {
    'use strict';

    document.querySelectorAll('form[data-confirm]').forEach(form => {
        form.addEventListener('submit', event => {
            if (form.dataset.confirmed === 'true') { delete form.dataset.confirmed; return; }
            if (!window.confirm(form.dataset.confirm)) event.preventDefault();
            else form.dataset.confirmed = 'true';
        });
    });

    document.querySelectorAll('form[data-busy]').forEach(form => {
        form.addEventListener('submit', event => {
            if (event.defaultPrevented) return;
            form.setAttribute('aria-busy', 'true');
            form.querySelectorAll('button[type="submit"]').forEach(button => {
                button.disabled = true;
                button.textContent = form.dataset.busy;
            });
        });
    });

    const upload = document.querySelector('.document-form input[type="file"]');
    if (!upload) return;
    const selection = document.getElementById('file-selection');
    const refreshFile = () => {
        const file = upload.files?.[0];
        let error = '';
        if (file && !/\.(pdf|docx|pptx|txt)$/i.test(file.name)) error = 'Chỉ nhận PDF, DOCX, PPTX hoặc TXT.';
        if (file && file.size > 20 * 1024 * 1024) error = 'Mỗi tệp tối đa 20 MB.';
        upload.setCustomValidity(error);
        if (selection) {
            selection.textContent = error || (file ? file.name + ' · ' + (file.size / 1024 / 1024 < 1 ? Math.max(1, Math.round(file.size / 1024)) + ' KB' : (file.size / 1024 / 1024).toFixed(1) + ' MB') : '');
            selection.classList.toggle('text-danger', !!error);
        }
        const title = document.getElementById('Input_Title');
        if (file && !error && title && !title.value.trim()) title.value = file.name.replace(/\.[^.]+$/, '').slice(0, 200);
    };
    upload.addEventListener('change', refreshFile);
    const zone = document.querySelector('[data-drop-zone]');
    zone?.addEventListener('dragover', event => { event.preventDefault(); zone.classList.add('dragging'); });
    zone?.addEventListener('dragleave', () => zone.classList.remove('dragging'));
    zone?.addEventListener('drop', event => {
        event.preventDefault();
        zone.classList.remove('dragging');
        if (event.dataTransfer.files.length !== 1) {
            if (selection) selection.textContent = 'Chọn một tệp mỗi lần tải lên.';
            return;
        }
        upload.files = event.dataTransfer.files;
        refreshFile();
    });
})();
