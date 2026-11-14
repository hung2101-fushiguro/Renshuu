(() => {
    'use strict';
    const form = document.getElementById('quiz-form');
    if (!form) return;
    const key = 'quiz_attempt_' + form.dataset.attemptId;
    const radios = Array.from(form.querySelectorAll('input[type="radio"]'));
    const status = document.getElementById('draft-status');
    const total = Number(form.dataset.total);
    let version = 0, savedVersion = 0, busy = false, submitting = false, closed = false, debounce;
    let syncing = false, autoSubmitted = false;
    let initialRemaining = form.dataset.remainingSeconds === '' ? null : Number(form.dataset.remainingSeconds);
    let syncMono = performance.now(), syncWall = Date.now();

    const snapshot = () => Object.fromEntries(radios.filter(r => r.checked).map(r => [r.name.match(/\d+/)[0], r.value]));
    function cache(pending) {
        try { sessionStorage.setItem(key, JSON.stringify({ answers: snapshot(), pending })); } catch (_) { /* Storage may be disabled. */ }
    }
    function progress() {
        const count = radios.filter(r => r.checked).length;
        document.getElementById('progress-text').textContent = `${count}/${total} câu`;
        document.getElementById('progress').setAttribute('aria-valuenow', count);
        document.getElementById('progress-bar').style.width = `${total ? count / total * 100 : 0}%`;
    }
    function closeAttempt(text) {
        closed = true;
        clearTimeout(debounce);
        radios.forEach(radio => { radio.disabled = true; });
        document.querySelectorAll('#quiz-form button, button[form="quiz-form"]').forEach(button => { button.disabled = true; });
        status.textContent = text || 'Bài đã kết thúc.';
        const link = document.createElement('a');
        link.href = form.dataset.resultUrl;
        link.textContent = 'Xem kết quả';
        status.append(' ', link);
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
                closeAttempt(result.error);
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
    function tick() {
        if (!timer || initialRemaining === null || closed || submitting) return;
        const elapsed = Math.max(performance.now() - syncMono, Date.now() - syncWall) / 1000;
        const remaining = Math.max(0, initialRemaining - elapsed);
        const seconds = Math.ceil(remaining);
        document.getElementById('timer-text').textContent = `${String(Math.floor(seconds / 60)).padStart(2, '0')}:${String(seconds % 60).padStart(2, '0')}`;
        timer.classList.toggle('is-warning', remaining <= 300);
        timer.classList.toggle('is-danger', remaining <= 60);
        if (!remaining && !autoSubmitted) {
            autoSubmitted = true;
            form.requestSubmit();
        }
    }
    async function syncState() {
        if (closed || submitting || syncing || document.hidden) return;
        syncing = true;
        const url = new URL(form.action);
        url.searchParams.set('handler', 'State');
        try {
            const response = await fetch(url, { cache: 'no-store', signal: AbortSignal.timeout(8000) });
            if (!response.ok) return;
            const state = await response.json();
            if (submitting || closed) return;
            if (state.closed) {
                closeAttempt('Bài đã kết thúc.');
                return;
            }
            if (typeof state.remainingSeconds === 'number') {
                initialRemaining = state.remainingSeconds;
                syncMono = performance.now();
                syncWall = Date.now();
                tick();
            }
        } catch (_) { /* Keep the local countdown while offline. */ }
        finally { syncing = false; }
    }
    if (timer) {
        tick();
        setInterval(tick, 250);
    }
    document.addEventListener('visibilitychange', () => { if (!document.hidden) { tick(); syncState(); } });
    window.addEventListener('focus', syncState);
    setInterval(syncState, 30000);
})();
