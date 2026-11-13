(() => {
 const form = document.getElementById('quiz-form');
 if (!form) return;
 const status = document.getElementById('draft-status');
 let dirty = false, saving = false, submitting = false;
 async function save() {
  if (!dirty || saving || submitting) return;
  saving = true; dirty = false; status.textContent = 'Đang lưu…';
  try {
   const response = await fetch(`${location.pathname}?handler=Draft`, { method:'POST', body:new FormData(form) });
   if (!response.ok || !response.headers.get('content-type')?.includes('application/json')) throw new Error();
   status.textContent = 'Đã lưu trên máy chủ.';
  } catch { dirty = true; status.textContent = 'Chưa lưu được. Giữ trang mở, kiểm tra kết nối hoặc nộp bài.'; }
  finally { saving = false; }
 }
 form.addEventListener('change', event => { if (event.target.matches('input[type=radio]')) { dirty = true; save(); } });
 form.addEventListener('submit', () => { submitting = true; });
 setInterval(save, 3000);
 window.addEventListener('beforeunload', event => { if (!submitting && (dirty || saving)) { event.preventDefault(); event.returnValue = ''; } });
})();
