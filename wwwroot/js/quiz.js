(() => {
    'use strict';
    const form = document.getElementById('quiz-form');
    if (!form) return;
    const key = 'quiz_draft_v2:' + location.pathname;
    const radios = Array.from(form.querySelectorAll('input[type="radio"]'));
    const questions = Array.from(form.querySelectorAll('.question'));
    const jumps = Array.from(document.querySelectorAll('.question-jump a'));
    const status = document.getElementById('draft-status');
    const total = Number(form.dataset.total);
    const dialog = document.getElementById('submit-dialog');
    let version = 0, savedVersion = 0, saving = false, submitting = false, closed = false, expired = false;
    let debounce, retry, storageWorks = true, autoSubmitted = false;
    let initialRemaining = form.dataset.remainingSeconds === '' ? null : Number(form.dataset.remainingSeconds);
    let syncMono = performance.now(), syncWall = Date.now();
    const questionId = radio => radio.name.match(/\d+/)[0];
    const snapshot = () => Object.fromEntries(radios.filter(r => r.checked).map(r => [questionId(r), r.value]));
    function message(text, state = '') { status.textContent = text; status.dataset.state = state; }
    function cache(pending) {
        try { localStorage.setItem(key, JSON.stringify({ answers: snapshot(), pending })); storageWorks = true; }
        catch (_) { storageWorks = false; }
    }
    function body() {
        const data = new FormData(form);
        // Disabled radios still belong to the final snapshot when time has expired.
        radios.forEach(radio => data.delete(radio.name));
        Object.entries(snapshot()).forEach(([id, value]) => data.set(`Answers[${id}]`, value));
        return data;
    }
    function progress() {
        const count = radios.filter(radio => radio.checked).length;
        document.getElementById('progress-text').textContent = `${count}/${total}`;
        document.getElementById('progress').setAttribute('aria-valuenow', String(count));
        document.getElementById('progress-bar').style.width = `${total ? count / total * 100 : 0}%`;
        questions.forEach(question => {
            const answered = !!question.querySelector('input:checked');
            const jump = jumps.find(link => link.dataset.questionId === question.dataset.questionId);
            jump?.classList.toggle('answered', answered);
            jump?.setAttribute('aria-label', `Câu ${question.dataset.questionNumber}, ${answered ? 'đã trả lời' : 'chưa trả lời'}`);
            const clear = question.querySelector('[data-clear-question]');
            if (clear) clear.disabled = !answered || expired || closed;
        });
    }
    function closeAttempt() {
        closed = true;
        clearTimeout(debounce);
        clearTimeout(retry);
        radios.forEach(radio => { radio.disabled = true; });
        document.querySelectorAll('button[form="quiz-form"], #quiz-form button').forEach(button => { button.disabled = true; });
        message('Bài đã kết thúc. ');
        const link = document.createElement('a');
        link.href = form.dataset.resultUrl;
        link.textContent = 'Xem kết quả';
        status.append(link);
        progress();
    }
    async function save() {
        if (saving || submitting || closed || expired || savedVersion === version) return;
        saving = true;
        const sentVersion = version;
        const url = new URL(form.action);
        url.searchParams.set('handler', 'SaveDraft');
        message('Đang lưu…');
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 10000);
        let success = false;
        try {
            const response = await fetch(url, { method: 'POST', body: body(), credentials: 'same-origin', signal: controller.signal });
            const result = await response.json();
            if (submitting) return;
            if (response.status === 409 && result.closed) { closeAttempt(); return; }
            if (!response.ok || !result.success) throw new Error('Save failed');
            savedVersion = sentVersion;
            cache(savedVersion !== version);
            message(`Đã lưu ${result.savedCount}/${total} trên máy chủ`, 'saved');
            success = true;
        } catch (_) {
            if (!submitting && !closed) {
                cache(true);
                message(storageWorks ? 'Chưa đồng bộ · đã giữ tạm trên máy.' : 'Chưa lưu được. Giữ trang này để thử lại.', 'error');
                retry = setTimeout(save, 5000);
            }
        } finally {
            clearTimeout(timeout);
            saving = false;
            if (success && savedVersion !== version) save();
        }
    }
    function changed() {
        if (closed || submitting || expired) return;
        version++;
        cache(true);
        progress();
        message('Chờ lưu…');
        clearTimeout(debounce);
        debounce = setTimeout(save, 600);
    }
    try {
        const local = JSON.parse(localStorage.getItem(key) || 'null');
        if (local?.pending && local.answers && typeof local.answers === 'object') {
            radios.forEach(radio => { radio.checked = local.answers[questionId(radio)] === radio.value; });
            changed();
        }
    } catch (_) { /* Unavailable or corrupt storage must not block answering. */ }
    form.addEventListener('change', event => { if (event.target.matches('input[type="radio"]')) changed(); });
    form.querySelectorAll('[data-clear-question]').forEach(button => button.addEventListener('click', () => {
        if (closed || submitting || expired) return;
        button.closest('.question').querySelectorAll('input[type="radio"]').forEach(radio => { radio.checked = false; });
        changed();
    }));
    function focusQuestion(question) {
        if (!question) return;
        question.scrollIntoView({ block: 'start', behavior: matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth' });
        question.focus({ preventScroll: true });
    }
    document.getElementById('next-unanswered').addEventListener('click', () => {
        const question = questions.find(item => !item.querySelector('input:checked'));
        if (question) focusQuestion(question);
        else message('Bạn đã trả lời đủ các câu. Có thể nộp bài.');
    });
    jumps.forEach(link => link.addEventListener('click', event => {
        event.preventDefault();
        focusQuestion(document.getElementById('question-' + link.dataset.questionId));
    }));
    const submitButtons = Array.from(document.querySelectorAll('button[form="quiz-form"], #quiz-form button[type="submit"]'));
    const originalLabels = new Map(submitButtons.map(button => [button, button.innerHTML]));
    async function submit(leaving = false) {
        if (submitting || closed) return;
        dialog.close();
        submitting = true;
        clearTimeout(debounce);
        clearTimeout(retry);
        cache(true);
        const payload = body();
        const url = new URL(form.action);
        if (leaving) url.searchParams.set('handler', 'SaveAndExit');
        radios.forEach(radio => { radio.disabled = true; });
        submitButtons.forEach(button => { button.disabled = true; button.textContent = leaving ? 'Đang lưu…' : 'Đang chấm…'; });
        message(leaving ? 'Đang lưu bài…' : expired ? 'Hết giờ · đang nộp bài…' : 'Đang nộp bài…');
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 15000);
        try {
            const response = await fetch(url, { method: 'POST', body: payload, credentials: 'same-origin', signal: controller.signal });
            const target = new URL(response.url);
            const resultPath = new URL(form.dataset.resultUrl, location.origin).pathname;
            const homePath = new URL(form.dataset.homeUrl, location.origin).pathname;
            if (!response.ok || !response.redirected || target.origin !== location.origin || (target.pathname !== resultPath && !(leaving && target.pathname === homePath))) throw new Error('Submit failed');
            if (leaving && target.pathname === homePath) cache(false);
            location.assign(target.href);
        } catch (_) {
            submitting = false;
            radios.forEach(radio => { radio.disabled = expired; });
            submitButtons.forEach(button => { button.disabled = false; button.innerHTML = originalLabels.get(button); });
            message(storageWorks ? 'Chưa gửi được. Lựa chọn đã giữ tạm; hãy thử nộp lại.' : 'Chưa gửi được. Giữ trang này và thử nộp lại.', 'error');
            progress();
        } finally { clearTimeout(timeout); }
    }
    form.addEventListener('submit', event => {
        event.preventDefault();
        if (submitting || closed) return;
        if (event.submitter?.id === 'leave-quiz') { submit(true); return; }
        const count = radios.filter(radio => radio.checked).length;
        if (!expired && count < total) {
            document.getElementById('submit-description').textContent = count ? `Bạn đã trả lời ${count}/${total} câu. ${total - count} câu còn trống sẽ không có điểm.` : `Bạn chưa trả lời câu nào. Nộp ngay sẽ nhận 0/${total} điểm.`;
            dialog.showModal();
            document.getElementById('cancel-submit').focus();
        } else submit();
    });
    document.getElementById('cancel-submit').addEventListener('click', () => dialog.close());
    document.getElementById('confirm-submit').addEventListener('click', () => submit());
    window.addEventListener('beforeunload', event => {
        if (!submitting && !closed && (saving || version !== savedVersion)) { event.preventDefault(); event.returnValue = ''; }
    });
    window.addEventListener('online', save);
    window.addEventListener('pageshow', event => { if (event.persisted) location.reload(); });
    setInterval(save, 15000);
    function tick() {
        if (initialRemaining === null || closed || submitting) return;
        const elapsed = Math.max(performance.now() - syncMono, Date.now() - syncWall) / 1000;
        const remaining = Math.max(0, initialRemaining - elapsed);
        const seconds = Math.ceil(remaining);
        document.getElementById('timer-text').textContent = `${String(Math.floor(seconds / 60)).padStart(2, '0')}:${String(seconds % 60).padStart(2, '0')}`;
        const timer = document.getElementById('timer');
        timer.classList.toggle('is-warning', remaining <= 300);
        timer.classList.toggle('is-danger', remaining <= 60);
        if (!remaining && !autoSubmitted) {
            autoSubmitted = true;
            expired = true;
            submit();
        }
    }
    let syncing = false;
    async function syncState() {
        if (closed || submitting || syncing || document.hidden) return;
        syncing = true;
        const url = new URL(form.action); url.searchParams.set('handler', 'State');
        try {
            const response = await fetch(url, { cache: 'no-store', signal: AbortSignal.timeout(8000) });
            if (!response.ok) return;
            const state = await response.json();
            if (submitting || closed) return;
            if (state.closed) { closeAttempt(); return; }
            initialRemaining = state.remainingSeconds;
            syncMono = performance.now(); syncWall = Date.now();
            tick();
        } catch (_) { /* Keep the current countdown and the local draft while offline. */ }
        finally { syncing = false; }
    }
    if (document.getElementById('timer')) { tick(); setInterval(tick, 250); }
    document.addEventListener('visibilitychange', () => { if (!document.hidden) { tick(); syncState(); } });
    window.addEventListener('focus', syncState);
    setInterval(syncState, 30000);
    document.querySelectorAll('[data-enhanced]').forEach(element => { element.hidden = false; });
    if (matchMedia('(max-width: 700px)').matches) document.querySelector('.question-overview').open = false;
    if ('ResizeObserver' in window) {
        const resize = new ResizeObserver(() => {
            const header = document.querySelector('.site-header');
            document.documentElement.style.setProperty('--header-height', `${getComputedStyle(header).position === 'sticky' ? header.offsetHeight : 0}px`);
            document.documentElement.style.setProperty('--quizbar-height', `${document.getElementById('quiz-toolbar').offsetHeight}px`);
        });
        resize.observe(document.querySelector('.site-header')); resize.observe(document.getElementById('quiz-toolbar'));
    }
    if ('IntersectionObserver' in window) {
        const observer = new IntersectionObserver(entries => {
            const visible = entries.find(entry => entry.isIntersecting);
            if (!visible) return;
            jumps.forEach(link => link.removeAttribute('aria-current'));
            jumps.find(link => link.dataset.questionId === visible.target.dataset.questionId)?.setAttribute('aria-current', 'true');
        }, { rootMargin: '-160px 0px -45% 0px' });
        questions.forEach(question => observer.observe(question));
    }
    progress();
})();
