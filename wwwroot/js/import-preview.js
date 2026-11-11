document.querySelectorAll('[data-import-preview]').forEach(preview => {
    const filter = preview.querySelector('[data-preview-filter]');
    const rows = Array.from(preview.querySelectorAll('[data-preview-status]'));
    preview.querySelector('[data-preview-controls]').hidden = false;
    function update() {
        let visible = 0;
        rows.forEach(row => {
            row.hidden = filter.value !== 'all' && row.dataset.previewStatus !== filter.value;
            if (!row.hidden) visible++;
        });
        preview.querySelector('[data-preview-count]').textContent = `Hiển thị ${visible}/${rows.length} dòng. Bộ lọc chỉ thay đổi cách xem trước.`;
        preview.querySelector('[data-preview-empty]').hidden = visible !== 0;
    }
    filter.addEventListener('change', update);
    update();
});
