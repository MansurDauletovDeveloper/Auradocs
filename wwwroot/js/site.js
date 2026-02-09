/**
 * DocFlow - Modern Document Management System
 * Client-side JavaScript enhancements
 */

(function() {
    'use strict';

    // ==========================================
    // DOM Ready Handler
    // ==========================================
    document.addEventListener('DOMContentLoaded', function() {
        initAnimations();
        initAlertDismiss();
        initFormEnhancements();
        initTableRowClick();
        initTooltips();
        initDropdownAnimations();
    });

    // ==========================================
    // Smooth page element animations
    // ==========================================
    function initAnimations() {
        // Add staggered animation to cards
        const cards = document.querySelectorAll('.card, .stat-card');
        cards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';
            setTimeout(() => {
                card.style.transition = 'opacity 0.4s ease, transform 0.4s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 50 + (index * 50));
        });

        // Animate table rows
        const tableRows = document.querySelectorAll('tbody tr');
        tableRows.forEach((row, index) => {
            row.style.opacity = '0';
            setTimeout(() => {
                row.style.transition = 'opacity 0.3s ease';
                row.style.opacity = '1';
            }, 100 + (index * 30));
        });
    }

    // ==========================================
    // Auto-dismiss alerts after 5 seconds
    // ==========================================
    function initAlertDismiss() {
        const alerts = document.querySelectorAll('.alert-dismissible');
        alerts.forEach(alert => {
            setTimeout(() => {
                const closeBtn = alert.querySelector('.btn-close');
                if (closeBtn) {
                    closeBtn.click();
                }
            }, 5000);
        });
    }

    // ==========================================
    // Form enhancements
    // ==========================================
    function initFormEnhancements() {
        // Add floating label effect
        const inputs = document.querySelectorAll('.form-control, .form-select');
        inputs.forEach(input => {
            // Add focus ring animation
            input.addEventListener('focus', function() {
                this.parentElement.classList.add('input-focused');
            });
            input.addEventListener('blur', function() {
                this.parentElement.classList.remove('input-focused');
            });
        });

        // Enhance file input display
        const fileInputs = document.querySelectorAll('input[type="file"]');
        fileInputs.forEach(input => {
            input.addEventListener('change', function() {
                if (this.files.length > 0) {
                    const fileName = this.files[0].name;
                    const fileSize = formatFileSize(this.files[0].size);
                    
                    // Find or create status element
                    let status = this.parentElement.querySelector('.file-status');
                    if (!status) {
                        status = document.createElement('div');
                        status.className = 'file-status mt-2 text-success small';
                        this.parentElement.appendChild(status);
                    }
                    status.innerHTML = `<i class="bi bi-check-circle me-1"></i>Выбран: ${fileName} (${fileSize})`;
                }
            });
        });

        // Form submission loading state
        const forms = document.querySelectorAll('form');
        forms.forEach(form => {
            form.addEventListener('submit', function(e) {
                const submitBtn = this.querySelector('button[type="submit"]');
                if (submitBtn && !submitBtn.disabled) {
                    const originalText = submitBtn.innerHTML;
                    submitBtn.disabled = true;
                    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Подождите...';
                    
                    // Re-enable after 10 seconds (fallback)
                    setTimeout(() => {
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = originalText;
                    }, 10000);
                }
            });
        });
    }

    // ==========================================
    // Make table rows clickable
    // ==========================================
    function initTableRowClick() {
        const clickableRows = document.querySelectorAll('tr[onclick], tr.cursor-pointer');
        clickableRows.forEach(row => {
            row.style.cursor = 'pointer';
            row.addEventListener('mouseenter', function() {
                this.style.backgroundColor = 'var(--gray-50)';
            });
            row.addEventListener('mouseleave', function() {
                this.style.backgroundColor = '';
            });
        });
    }

    // ==========================================
    // Initialize Bootstrap tooltips
    // ==========================================
    function initTooltips() {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"], [title]');
        tooltipTriggerList.forEach(el => {
            if (el.title && !el.getAttribute('data-bs-toggle')) {
                el.setAttribute('data-bs-toggle', 'tooltip');
                el.setAttribute('data-bs-placement', 'top');
            }
        });
        
        if (typeof bootstrap !== 'undefined') {
            const tooltipList = [...tooltipTriggerList].map(el => new bootstrap.Tooltip(el, {
                trigger: 'hover'
            }));
        }
    }

    // ==========================================
    // Dropdown animation enhancement
    // ==========================================
    function initDropdownAnimations() {
        const dropdowns = document.querySelectorAll('.dropdown-menu');
        dropdowns.forEach(dropdown => {
            dropdown.addEventListener('show.bs.dropdown', function() {
                this.classList.add('animate-fade-in');
            });
        });
    }

    // ==========================================
    // Utility Functions
    // ==========================================
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    // ==========================================
    // Smooth scroll for anchor links
    // ==========================================
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                e.preventDefault();
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // ==========================================
    // Keyboard shortcuts
    // ==========================================
    document.addEventListener('keydown', function(e) {
        // Ctrl + / or Cmd + / - Focus search input
        if ((e.ctrlKey || e.metaKey) && e.key === '/') {
            e.preventDefault();
            const searchInput = document.querySelector('input[name="SearchTerm"], input[type="search"]');
            if (searchInput) {
                searchInput.focus();
            }
        }
    });

})();
