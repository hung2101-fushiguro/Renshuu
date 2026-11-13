(() => {
    'use strict';
    const form = document.getElementById('quiz-form');
    if (!form) return;
    const key = 'quiz_attempt_' + form.dataset.attemptId;
    const radios = Array.from(form.querySelectorAll('input[type="radio"]'));
    const status = document.getElementById('draft-status');
    const total = Number(form.dataset.total);
    let version = 0, savedVersion = 0, busy = false, submitting = false, closed = false, debounce;

    const snapshot = () => Object.fromEntries(radios.filter(r => r.checked).map(r => [r.name.match(/\d+/)[0], r.value]));
    function cache(pending) {
        try { sessionStorage.setItem(key, JSON.stringify({ answers: snapshot(), pending })); } catch (_) { /* Storage may be disabled. */ }
    }
    function progress() {
        const count = radios.filter(r => r.checked).length;
        document.getElementById('progress-text').textContent = `${count}/${total} câu`;
        document.getElementById('progress').setAttribute('aria-valuenow', count);
        document.getElementById('progress-bar').style.width = `${total ? count / total * 100 : 0}%`;
        form.querySelectorAll('.question').forEach(question => {
            const complete = !!question.querySelector('input:checked');
            const link = document.querySelector(`.question-jump a[data-question-number="${question.dataset.questionNumber}"]`);
            link?.classList.toggle('answered', complete);
            link?.setAttribute('aria-label', `Đến câu ${question.dataset.questionNumber}, ${complete ? 'đã' : 'chưa'} trả lời`);
        });
    }
    async function save() {
        if (busy || submitting || closed || savedVersion === version) return;
        busy = true;
        const sendingVersion = version;
        const body = new FormData(form);
        const url = new URL(form.action);
        url.searchParams.set('handler', 'SaveDraft');
        status.textContent = 'Đang lưu đáp án…';
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 12000);
        let success = false;
        try {
            const response = await fetch(url, { method: 'POST', body, signal: controller.signal, credentials: 'same-origin' });
            const result = await response.json();
            if (response.status === 409 && result.closed) {
                closed = true;
                status.textContent = result.error;
                const link = document.createElement('a');
                link.href = form.dataset.resultUrl;
                link.textContent = 'Xem kết quả';
                status.append(' ', link);
                radios.forEach(radio => { radio.disabled = true; });
                document.querySelectorAll('#quiz-form button[type="submit"], button[form="quiz-form"]').forEach(button => { button.disabled = true; });
                return;
            }
            if (!response.ok || !result.success) throw new Error('save failed');
            savedVersion = sendingVersion;
            cache(savedVersion !== version);
            status.textContent = `Đã lưu ${result.savedCount}/${total} câu trên máy chủ.`;
            success = true;
        } catch (_) {
            status.textContent = 'Chưa lưu được lên máy chủ. Đáp án đang giữ trong tab này; sẽ thử lại khi có mạng.';
        } finally {
            clearTimeout(timeout);
            busy = false;
            if (success && savedVersion !== version) save();
        }
    }
    try {
        const local = JSON.parse(sessionStorage.getItem(key) || 'null');
        // Only replay unsynced local changes; a synced cache must not replace newer server data.
        if (local && (local.pending || !local.answers)) {
            const answers = local.answers || local; // Upgrade the earlier per-tab draft format.
            radios.forEach(r => { if (answers[r.name.match(/\d+/)[0]] === r.value) r.checked = true; });
            version++;
            cache(true);
            debounce = setTimeout(save, 500);
        }
    } catch (_) { /* Corrupt storage must not prevent the quiz. */ }
    form.addEventListener('change', event => {
        if (closed) return;
        if (!event.target.matches('input[type="radio"]')) return;
        version++;
        cache(true);
        progress();
        status.textContent = 'Đang chờ lưu đáp án…';
        clearTimeout(debounce);
        debounce = setTimeout(save, 600);
    });
    form.addEventListener('submit', () => { submitting = true; clearTimeout(debounce); });
    window.addEventListener('beforeunload', event => {
        if (!submitting && !closed && (busy || version !== savedVersion)) {
            event.preventDefault();
            event.returnValue = '';
        }
    });
    window.addEventListener('online', save);
    window.addEventListener('pageshow', event => { if (event.persisted) location.reload(); });
    setInterval(save, 15000);
    progress();

    const timer = document.getElementById('timer');
    if (timer) {
        const initialRemaining = Number(form.dataset.remainingSeconds);
        const opened = performance.now();
        function tick() {
            if (submitting || closed) return;
            const remaining = Math.max(0, initialRemaining - (performance.now() - opened) / 1000);
            const seconds = Math.ceil(remaining);
            document.getElementById('timer-text').textContent = `${String(Math.floor(seconds / 60)).padStart(2, '0')}:${String(seconds % 60).padStart(2, '0')}`;
            timer.classList.toggle('is-warning', remaining <= 300);
            timer.classList.toggle('is-danger', remaining <= 60);
            if (!remaining) form.requestSubmit();
        }
        tick();
        setInterval(tick, 1000);
        document.addEventListener('visibilitychange', tick);
    }
})();
