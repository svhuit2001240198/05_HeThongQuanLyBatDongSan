document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-password-toggle]').forEach(button => {
        button.addEventListener('click', () => {
            const input = document.getElementById(button.dataset.passwordToggle);
            if (!input) return;
            const visible = input.type === 'password';
            input.type = visible ? 'text' : 'password';
            button.setAttribute('aria-label', visible ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');
            button.setAttribute('aria-pressed', String(visible));
        });
    });
    document.querySelectorAll('form[data-confirm]').forEach(form => {
        form.addEventListener('submit', event => { if (!window.confirm(form.dataset.confirm)) event.preventDefault(); });
    });
    document.querySelectorAll('[data-dismiss-alert]').forEach(button => {
        button.addEventListener('click', () => button.closest('.alert')?.remove());
    });
    const navButton = document.querySelector('[data-nav-toggle]');
    const nav = document.getElementById('top-nav');
    navButton?.addEventListener('click', () => {
        const open = nav?.classList.toggle('is-open');
        navButton.setAttribute('aria-expanded', String(Boolean(open)));
    });
    document.addEventListener('click', event => {
        document.querySelectorAll('.account-menu[open]').forEach(menu => { if (!menu.contains(event.target)) menu.removeAttribute('open'); });
    });
});
