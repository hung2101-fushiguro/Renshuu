(() => {
    const root = document.documentElement;
    root.classList.remove('no-js');
    const motionButton = document.getElementById('motion-toggle');
    const scenePause = document.getElementById('scene-pause');
    const reduced = matchMedia('(prefers-reduced-motion: reduce)');
    let paused = false;
    try { paused = localStorage.getItem('renshuu-motion') === 'paused'; } catch { }
    const updateMotion = () => {
        const stopped = paused || reduced.matches;
        root.dataset.motion = stopped ? 'off' : '';
        motionButton.setAttribute('aria-pressed', String(stopped));
        motionButton.setAttribute('aria-label', stopped ? 'Bật chuyển động' : 'Tạm dừng chuyển động');
        motionButton.disabled = reduced.matches;
        motionButton.title = reduced.matches ? 'Đang giảm chuyển động theo cài đặt thiết bị' : motionButton.getAttribute('aria-label');
        if (scenePause) {
            scenePause.setAttribute('aria-pressed', String(stopped));
            scenePause.setAttribute('aria-label', stopped ? 'Bật chuyển động thẻ ôn tập' : 'Tạm dừng chuyển động thẻ ôn tập');
            scenePause.textContent = stopped ? '▷' : 'Ⅱ';
            scenePause.disabled = reduced.matches;
        }
        document.dispatchEvent(new CustomEvent('renshuu:motionchange', { detail: { paused: stopped } }));
    };
    updateMotion();
    motionButton.addEventListener('click', () => {
        paused = !paused;
        try { localStorage.setItem('renshuu-motion', paused ? 'paused' : 'active'); } catch { }
        updateMotion();
    });
    reduced.addEventListener('change', updateMotion);
    scenePause?.addEventListener('click', () => motionButton.click());
    document.addEventListener('visibilitychange', () => { root.classList.toggle('document-hidden', document.hidden); });
    const scene = document.querySelector('.study-scene');
    if (scene && 'IntersectionObserver' in window) {
        new IntersectionObserver(entries => { scene.classList.toggle('offscreen', !entries[0].isIntersecting); }).observe(scene);
    }
    const menu = document.getElementById('menu-toggle');
    const sidebar = document.getElementById('sidebar');
    const scrim = document.getElementById('mobile-scrim');
    const narrow = matchMedia('(max-width: 760px)');
    const setMenu = open => {
        root.classList.toggle('menu-open', open);
        menu.setAttribute('aria-expanded', String(open));
        menu.setAttribute('aria-label', open ? 'Đóng điều hướng' : 'Mở điều hướng');
        scrim.hidden = !open;
        sidebar.inert = narrow.matches && !open;
        document.querySelector('.app-shell').inert = open;
        if (open) {
            sidebar.setAttribute('role', 'dialog');
            sidebar.setAttribute('aria-modal', 'true');
        } else {
            sidebar.removeAttribute('role');
            sidebar.removeAttribute('aria-modal');
        }
        if (open) sidebar.querySelector('a').focus();
    };
    setMenu(false);
    menu.addEventListener('click', () => setMenu(!root.classList.contains('menu-open')));
    document.getElementById('menu-close').addEventListener('click', () => { setMenu(false); menu.focus(); });
    scrim.addEventListener('click', () => { setMenu(false); menu.focus(); });
    narrow.addEventListener('change', () => setMenu(false));
    document.addEventListener('keydown', event => {
        if (!root.classList.contains('menu-open')) return;
        if (event.key === 'Escape') { setMenu(false); menu.focus(); }
        if (event.key === 'Tab') {
            const links = [...sidebar.querySelectorAll('a, button')].filter(element => element.getClientRects().length);
            if (event.shiftKey && document.activeElement === links[0]) { event.preventDefault(); links.at(-1).focus(); }
            else if (!event.shiftKey && document.activeElement === links.at(-1)) { event.preventDefault(); links[0].focus(); }
        }
    });
    const errors = document.querySelector('.validation-summary-errors');
    if (errors) { errors.tabIndex = -1; errors.focus(); }
    const invalidFields = [...document.querySelectorAll('.input-validation-error')];
    invalidFields.forEach(field => field.setAttribute('aria-invalid', 'true'));
    if (!errors && invalidFields.length) invalidFields[0].focus();
})();
