// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Sidebar toggle functionality for mobile
document.addEventListener('DOMContentLoaded', function() {
    const sidebarToggleBtn = document.querySelector('.sidebar-toggle-btn');
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    
    if (sidebarToggleBtn && sidebar) {
        // Toggle sidebar
        sidebarToggleBtn.addEventListener('click', function(e) {
            e.stopPropagation();
            sidebar.classList.toggle('open');
            if (overlay) {
                overlay.classList.toggle('active');
            }
        });

        // Fechar sidebar ao clicar no overlay
        if (overlay) {
            overlay.addEventListener('click', function() {
                sidebar.classList.remove('open');
                overlay.classList.remove('active');
            });
        }

        // Fechar sidebar ao clicar fora (em mobile)
        document.addEventListener('click', function(e) {
            if (window.innerWidth <= 992) {
                if (sidebar && sidebar.classList.contains('open')) {
                    if (!sidebar.contains(e.target) && !sidebarToggleBtn.contains(e.target)) {
                        sidebar.classList.remove('open');
                        if (overlay) {
                            overlay.classList.remove('active');
                        }
                    }
                }
            }
        });
    }
});