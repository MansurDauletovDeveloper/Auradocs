/**
 * DocFlow — Client-side interactions
 * Sidebar, ripple effects, smart enhancements
 */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        initSidebar();
        initRipple();
        initAlertDismiss();
        initFormEnhancements();
        initTableRowClick();
        initTooltips();
        initGreeting();
        initCollapsibleHints();
    });


    // ==========================================
    // Sidebar Toggle (mobile)
    // ==========================================
    function initSidebar() {
        const sidebar = document.getElementById('sidebar');
        const toggle = document.getElementById('sidebarToggle');
        const overlay = document.getElementById('sidebarOverlay');

        if (!sidebar || !toggle) return;

        function openSidebar() {
            sidebar.classList.add('show');
            overlay.classList.add('show');
            document.body.style.overflow = 'hidden';
        }

        function closeSidebar() {
            sidebar.classList.remove('show');
            overlay.classList.remove('show');
            document.body.style.overflow = '';
        }

        toggle.addEventListener('click', function (e) {
            e.stopPropagation();
            if (sidebar.classList.contains('show')) {
                closeSidebar();
            } else {
                openSidebar();
            }
        });

        if (overlay) {
            overlay.addEventListener('click', closeSidebar);
        }

        // Close on Escape
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && sidebar.classList.contains('show')) {
                closeSidebar();
            }
        });

        // Close on nav click (mobile)
        sidebar.querySelectorAll('.sidebar-link').forEach(function (link) {
            link.addEventListener('click', function () {
                if (window.innerWidth < 992) {
                    closeSidebar();
                }
            });
        });
    }


    // ==========================================
    // Material-like Ripple on Buttons
    // ==========================================
    function initRipple() {
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.btn');
            if (!btn) return;

            var existing = btn.querySelectorAll('.ripple');
            existing.forEach(function (r) { r.remove(); });

            var circle = document.createElement('span');
            circle.classList.add('ripple');

            var rect = btn.getBoundingClientRect();
            var size = Math.max(rect.width, rect.height);
            circle.style.width = circle.style.height = size + 'px';
            circle.style.left = (e.clientX - rect.left - size / 2) + 'px';
            circle.style.top = (e.clientY - rect.top - size / 2) + 'px';

            btn.appendChild(circle);

            setTimeout(function () {
                circle.remove();
            }, 600);
        });
    }


    // ==========================================
    // Auto-dismiss alerts
    // ==========================================
    function initAlertDismiss() {
        var alerts = document.querySelectorAll('.alert-dismissible');
        alerts.forEach(function (alert) {
            setTimeout(function () {
                var closeBtn = alert.querySelector('.btn-close');
                if (closeBtn) closeBtn.click();
            }, 6000);
        });
    }


    // ==========================================
    // Form enhancements
    // ==========================================
    function initFormEnhancements() {
        // Focus ring
        var inputs = document.querySelectorAll('.form-control, .form-select');
        inputs.forEach(function (input) {
            input.addEventListener('focus', function () {
                this.parentElement.classList.add('input-focused');
            });
            input.addEventListener('blur', function () {
                this.parentElement.classList.remove('input-focused');
            });
        });

        // File input display
        var fileInputs = document.querySelectorAll('input[type="file"]');
        fileInputs.forEach(function (input) {
            input.addEventListener('change', function () {
                if (this.files.length > 0) {
                    var fileName = this.files[0].name;
                    var fileSize = formatFileSize(this.files[0].size);

                    var status = this.parentElement.querySelector('.file-status');
                    if (!status) {
                        status = document.createElement('div');
                        status.className = 'file-status mt-2 small';
                        status.style.color = 'var(--emerald-600)';
                        this.parentElement.appendChild(status);
                    }
                    status.innerHTML = '<i class="bi bi-check-circle me-1"></i>Выбран: ' + fileName + ' (' + fileSize + ')';
                }
            });
        });

        // Form submit loading state
        var forms = document.querySelectorAll('form');
        forms.forEach(function (form) {
            form.addEventListener('submit', function () {
                var submitBtn = this.querySelector('button[type="submit"]');
                if (submitBtn && !submitBtn.disabled) {
                    var originalText = submitBtn.innerHTML;
                    submitBtn.disabled = true;
                    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Подождите...';

                    setTimeout(function () {
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = originalText;
                    }, 10000);
                }
            });
        });

        // Drag-and-drop for upload areas
        var uploadAreas = document.querySelectorAll('.upload-area');
        uploadAreas.forEach(function (area) {
            ['dragenter', 'dragover'].forEach(function (evName) {
                area.addEventListener(evName, function (e) {
                    e.preventDefault();
                    area.classList.add('dragover');
                });
            });
            ['dragleave', 'drop'].forEach(function (evName) {
                area.addEventListener(evName, function (e) {
                    e.preventDefault();
                    area.classList.remove('dragover');
                });
            });
        });
    }


    // ==========================================
    // Clickable table rows
    // ==========================================
    function initTableRowClick() {
        var rows = document.querySelectorAll('tr[onclick], tr.cursor-pointer');
        rows.forEach(function (row) {
            row.style.cursor = 'pointer';
        });
    }


    // ==========================================
    // Bootstrap tooltips
    // ==========================================
    function initTooltips() {
        if (typeof bootstrap === 'undefined') return;

        var els = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        els.forEach(function (el) {
            new bootstrap.Tooltip(el, { trigger: 'hover' });
        });
    }


    // ==========================================
    // Time-of-day greeting on Dashboard
    // ==========================================
    function initGreeting() {
        var el = document.getElementById('greetingText');
        if (!el) return;

        var hour = new Date().getHours();
        var greeting;

        if (hour >= 5 && hour < 12) greeting = 'Доброе утро';
        else if (hour >= 12 && hour < 17) greeting = 'Добрый день';
        else if (hour >= 17 && hour < 22) greeting = 'Добрый вечер';
        else greeting = 'Доброй ночи';

        el.textContent = greeting;
    }


    // ==========================================
    // Collapsible hint blocks
    // ==========================================
    function initCollapsibleHints() {
        var hints = document.querySelectorAll('.auth-hint[data-collapsible]');
        hints.forEach(function (hint) {
            var title = hint.querySelector('h6');
            var content = hint.querySelector('ul');
            if (!title || !content) return;

            content.style.overflow = 'hidden';
            content.style.maxHeight = '0';
            content.style.transition = 'max-height 0.3s ease';
            content.style.marginTop = '0';

            title.style.cursor = 'pointer';
            title.innerHTML += ' <i class="bi bi-chevron-down" style="font-size: 0.7rem; transition: transform 0.2s ease;"></i>';

            title.addEventListener('click', function () {
                var chevron = title.querySelector('.bi-chevron-down');
                if (content.style.maxHeight === '0px' || content.style.maxHeight === '0') {
                    content.style.maxHeight = content.scrollHeight + 'px';
                    content.style.marginTop = '0.5rem';
                    if (chevron) chevron.style.transform = 'rotate(180deg)';
                } else {
                    content.style.maxHeight = '0';
                    content.style.marginTop = '0';
                    if (chevron) chevron.style.transform = 'rotate(0)';
                }
            });
        });
    }


    // ==========================================
    // Utilities
    // ==========================================
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        var k = 1024;
        var sizes = ['Bytes', 'KB', 'MB', 'GB'];
        var i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }


    // ==========================================
    // Smooth scroll for anchors
    // ==========================================
    document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
        anchor.addEventListener('click', function (e) {
            var target = document.querySelector(this.getAttribute('href'));
            if (target) {
                e.preventDefault();
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });


    // ==========================================
    // Keyboard shortcut: Ctrl+/ for search
    // ==========================================
    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key === '/') {
            e.preventDefault();
            var searchInput = document.querySelector('input[name="SearchTerm"], input[type="search"]');
            if (searchInput) searchInput.focus();
        }
    });

})();
