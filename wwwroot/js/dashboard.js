(() => {
    const select = document.getElementById('subject-select');
    const count = document.getElementById('Input_QuestionCount');
    const hint = document.getElementById('count-hint');
    if (select && count) {
        const presets = [...document.querySelectorAll('.preset')];
        const submit = document.getElementById('start-practice');
        const available = () => Number(select.selectedOptions[0]?.dataset.count || 0);
        const refresh = (clamp) => {
            const max = Math.min(200, available());
            if (clamp && Number(count.value) > max) count.value = Math.max(1, max);
            count.max = Math.max(1, max);
            hint.textContent = max ? 'Có ' + available() + ' câu hỏi' : 'Môn này chưa có câu hỏi';
            submit.disabled = max === 0;
            presets.forEach(button => {
                button.disabled = Number(button.dataset.value) > max;
                button.setAttribute('aria-pressed', String(Number(button.dataset.value) === Number(count.value)));
            });
        };
        select.addEventListener('change', () => refresh(true));
        count.addEventListener('input', () => refresh(false));
        presets.forEach(button => button.addEventListener('click', () => { count.value = button.dataset.value; refresh(false); }));
        refresh(!document.querySelector('.validation-summary-errors, .field-validation-error'));
    }
    const search = document.getElementById('library-search');
    const items = [...document.querySelectorAll('.library-item')];
    const normalize = value => value.toLocaleLowerCase('vi').normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd');
    search?.addEventListener('input', () => {
        const term = normalize(search.value.trim());
        let visible = 0;
        items.forEach(item => { item.hidden = !normalize(item.dataset.search || '').includes(term); if (!item.hidden) visible++; });
        document.getElementById('search-empty').hidden = visible > 0 || !term;
        document.getElementById('search-status').textContent = visible + ' môn học hoặc thư mục phù hợp.';
        document.querySelectorAll('.library-subheading').forEach(heading => { heading.hidden = !!term; });
    });
})();
