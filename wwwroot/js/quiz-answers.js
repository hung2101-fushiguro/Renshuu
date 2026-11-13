(() => {
    const form = document.getElementById('quiz-form');
    const toggle = document.getElementById('toggle-answers');
    const status = document.getElementById('answer-visibility-status');
    if (!form || !toggle) return;

    const answers = Array.from(form.querySelectorAll('[data-answer-reveal]'));
    if (answers.length === 0) return;
    let visible = false;
    toggle.hidden = false;

    function toggleAnswers() {
        visible = !visible;
        answers.forEach(answer => { answer.hidden = !visible; });
        toggle.setAttribute('aria-pressed', String(visible));
        toggle.textContent = visible ? 'Ẩn đáp án (S)' : 'Hiện đáp án (S)';
        if (status) status.textContent = visible
            ? 'Đã hiện đáp án đúng bên dưới từng câu hỏi.'
            : 'Đã ẩn đáp án đúng.';
    }

    toggle.addEventListener('click', toggleAnswers);
    document.addEventListener('keydown', event => {
        if (event.defaultPrevented || event.repeat || event.isComposing
            || event.ctrlKey || event.altKey || event.metaKey
            || event.key.toLowerCase() !== 's') return;

        const target = event.target;
        if (target instanceof HTMLElement) {
            if (target.isContentEditable || target.closest('textarea, select, [role="textbox"]')) return;
            if (target instanceof HTMLInputElement
                && !['radio', 'checkbox', 'button', 'submit', 'reset'].includes(target.type)) return;
        }

        event.preventDefault();
        toggleAnswers();
    });
})();
