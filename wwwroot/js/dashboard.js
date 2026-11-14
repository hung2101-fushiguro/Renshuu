(() => {
    'use strict';
    const form = document.getElementById('start-form');
    if (!form) return;
    const subject = document.getElementById('subject-select');
    const count = document.getElementById('Input_QuestionCount');
    const time = document.getElementById('Input_TimeLimitMinutes');
    const start = document.getElementById('start-practice');
    const available = () => Number(subject.selectedOptions[0]?.dataset.count || 0);
    const countButtons = Array.from(form.querySelectorAll('[data-count-preset]'));
    const timeButtons = Array.from(form.querySelectorAll('[data-time-preset]'));
    function refresh() {
        const max = Math.min(200, available());
        count.max = String(Math.max(1, max));
        start.disabled = !max;
        document.getElementById('count-hint').textContent = max ? `${available()} câu trong ngân hàng` : 'Môn này chưa có câu hỏi.';
        countButtons.forEach(button => {
            button.disabled = Number(button.dataset.countPreset) > max;
            button.setAttribute('aria-pressed', String(button.dataset.countPreset === count.value));
        });
        timeButtons.forEach(button => button.setAttribute('aria-pressed', String(button.dataset.timePreset === time.value)));
        document.getElementById('setup-summary').textContent = `${count.value || '—'} câu · ${time.value ? time.value + ' phút' : 'Không giới hạn'}`;
    }
    subject.addEventListener('change', () => {
        count.value = String(Math.max(1, Math.min(Number(count.value) || 10, available(), 200)));
        refresh();
    });
    countButtons.forEach(button => button.addEventListener('click', () => { count.value = button.dataset.countPreset; refresh(); }));
    timeButtons.forEach(button => button.addEventListener('click', () => { time.value = button.dataset.timePreset; refresh(); }));
    form.addEventListener('input', refresh);
    form.addEventListener('submit', () => { start.disabled = true; start.textContent = 'Đang tạo đề…'; });
    window.addEventListener('pageshow', () => { start.innerHTML = 'Tạo đề &amp; làm bài <span aria-hidden="true">→</span>'; refresh(); });
    document.querySelectorAll('[data-enhanced]').forEach(element => { element.hidden = false; });
    const search = document.getElementById('library-search');
    const items = Array.from(document.querySelectorAll('.library-item'));
    const normalize = value => value.toLocaleLowerCase('vi').normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd');
    search?.addEventListener('input', () => {
        const query = normalize(search.value.trim());
        items.forEach(item => { item.hidden = !normalize(item.dataset.search).includes(query); });
        document.getElementById('library-empty').hidden = items.some(item => !item.hidden);
    });
    refresh();
})();
