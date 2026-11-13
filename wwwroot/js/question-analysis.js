(() => {
    const button = document.getElementById('analyze-question');
    if (!button) return;
    const form = button.closest('form');
    const status = document.getElementById('analysis-status');
    const preview = document.getElementById('analysis-preview');
    const apply = document.getElementById('apply-analysis');
    let suggestion;
    let analyzedInput;
    const snapshot = () => JSON.stringify(Array.from(new FormData(form).entries()).filter(([key]) => key.startsWith('Input.')));
    button.addEventListener('click', async () => {
        button.disabled = true;
        preview.hidden = true;
        suggestion = null;
        status.textContent = 'Đang phân tích câu hỏi…';
        const original = snapshot();
        try {
            const response = await fetch('?handler=Analyze', { method: 'POST', body: new FormData(form) });
            const data = await response.json();
            if (!response.ok) throw new Error(data.error || 'Không thể phân tích câu hỏi.');
            if (original !== snapshot()) throw new Error('Nội dung đã thay đổi. Hãy phân tích lại.');
            suggestion = data;
            analyzedInput = original;
            document.getElementById('analysis-summary').textContent = `${data.chapter} / ${data.topic} · ${{Easy:'Dễ',Medium:'Trung bình',Hard:'Khó'}[data.difficulty]}`;
            document.getElementById('analysis-reason').textContent = data.reason;
            preview.hidden = false;
            status.textContent = 'Đề xuất đã sẵn sàng. Kiểm tra trước khi áp dụng.';
        } catch (error) { status.textContent = error instanceof SyntaxError ? 'Không nhận được kết quả. Kiểm tra phiên đăng nhập và thử lại.' : error.message; }
        finally { button.disabled = false; }
    });
    apply.addEventListener('click', () => {
        if (!suggestion) return;
        if (snapshot() !== analyzedInput) { preview.hidden = true; status.textContent = 'Nội dung đã thay đổi. Hãy phân tích lại.'; return; }
        for (const key of ['Chapter', 'Topic', 'Difficulty']) {
            document.getElementById(`Input_${key}`).value = suggestion[key.toLowerCase()];
        }
        preview.hidden = true;
        status.textContent = 'Đã điền phân loại. Bấm lưu câu hỏi để ghi nhận.';
    });
})();
