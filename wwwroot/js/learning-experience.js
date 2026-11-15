/* Product behaviour around the unmodified Scroll Craft engine. */
(() => {
    const root = document.getElementById('learning-experience');
    if (!root) return;
    const html = document.documentElement;
    const reduce = matchMedia('(prefers-reduced-motion: reduce)');
    const subjectSelect = document.getElementById('subject-select');
    const countInput = document.getElementById('Input_QuestionCount');
    const timeInput = document.getElementById('Input_TimeLimitMinutes');
    const subjects = JSON.parse(document.getElementById('session-subjects').textContent);
    const modeButtons = [...root.querySelectorAll('[data-session-mode]')];
    const action = document.getElementById('session-action');
    let mode = 'practice';
    let engine;
    const setText = (id, value) => { document.getElementById(id).textContent = String(value); };
    const selectedSubject = () => subjects.find(subject => subject.Id === Number(subjectSelect?.value));
    function syncSession(announce = false) {
        const subject = selectedSubject();
        if (!subject) return;
        const review = mode === 'review';
        const count = review ? Math.min(subject.WrongCount, 200) : Math.max(0, Math.min(Number(countInput.value) || 0, subject.QuestionCount, 200));
        setText('ticket-subject', subject.Name);
        setText('ticket-count', count);
        setText('session-bank-count', subject.QuestionCount);
        setText('ticket-wrong', subject.WrongCount);
        setText('ticket-kind', review ? 'Phiên ôn câu sai' : 'Phiên luyện tập');
        setText('ticket-time', !review && Number(timeInput.value) > 0 ? timeInput.value + ' phút luyện tập' : 'Theo nhịp của bạn');
        setText('session-note', review ? (subject.WrongCount ? subject.WrongCount + ' câu cần ôn lại.' : 'Chưa có câu cần ôn.') : 'Đồng bộ theo tùy chọn bài.');
        action.querySelector('span').textContent = review ? 'Xem câu sai' : 'Điều chỉnh';
        action.href = review ? '/Wrong?subjectId=' + subject.Id : '#practice';
        root.dataset.sessionMode = mode;
        modeButtons.forEach(button => button.setAttribute('aria-pressed', String(button.dataset.sessionMode === mode)));
        if (announce) setText('session-announcement', subject.Name + ', ' + count + ' câu hỏi, ' + (review ? 'ôn câu sai.' : 'luyện tập.'));
        engine?.layout();
    }
    modeButtons.forEach(button => button.addEventListener('click', () => { mode = button.dataset.sessionMode; syncSession(true); }));
    subjectSelect?.addEventListener('change', () => syncSession(true));
    countInput?.addEventListener('input', () => syncSession());
    timeInput?.addEventListener('input', () => syncSession());
    root.querySelectorAll('.preset').forEach(button => button.addEventListener('click', () => syncSession()));
    action.addEventListener('click', () => {
        if (mode !== 'practice' || !subjectSelect) return;
        // The anchor handles native scrolling; focus makes keyboard continuation useful.
        setTimeout(() => subjectSelect.focus({ preventScroll: true }), 0);
    });
    syncSession();

    function restoreSurface() {
        root.querySelectorAll('[data-sc-parallax], [data-sc-tilt]').forEach(element => element.style.removeProperty('transform'));
        root.querySelectorAll('[data-sc-reveal]').forEach(element => element.style.removeProperty('clip-path'));
        root.querySelectorAll('[data-sc-act]').forEach(element => element.style.removeProperty('--sc-p'));
    }
    function syncMotion() {
        const enabled = html.dataset.motion !== 'off' && !reduce.matches && !document.hidden;
        if (!enabled) {
            engine?.destroy(); engine = undefined;
            root.classList.remove('experience-active');
            restoreSurface();
            return;
        }
        if (engine || !window.ScrollCraft) return;
        // Study statistics stay readable immediately; motion belongs to the preview.
        root.classList.add('experience-active');
        engine = ScrollCraft.mount(root);
    }
    document.addEventListener('renshuu:motionchange', syncMotion);
    document.addEventListener('visibilitychange', syncMotion);
    reduce.addEventListener('change', syncMotion);
    window.addEventListener('pagehide', () => { engine?.destroy(); engine = undefined; });
    window.addEventListener('pageshow', () => { syncSession(); syncMotion(); });
    syncMotion();
})();
