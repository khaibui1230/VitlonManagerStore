// Sidebar Toggle
window.addEventListener('DOMContentLoaded', event => {
    // Toggle the side navigation
    const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sb-sidenav-toggled');
            localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sb-sidenav-toggled'));
        });
    }

    // Load saved sidebar state
    const savedSidebarState = localStorage.getItem('sb|sidebar-toggle');
    if (savedSidebarState === 'true' && window.innerWidth <= 768) {
        document.body.classList.add('sb-sidenav-toggled');
    }

    // Close sidebar on mobile when clicking outside
    document.addEventListener('click', event => {
        if (window.innerWidth <= 768 && !event.target.closest('#layoutSidenav_nav') && !event.target.closest('#sidebarToggle')) {
            document.body.classList.remove('sb-sidenav-toggled');
        }
    });

    // Handle window resize
    window.addEventListener('resize', () => {
        if (window.innerWidth > 768) {
            document.body.classList.remove('sb-sidenav-toggled');
        }
    });
});

// Initialize Simple DataTables
window.addEventListener('DOMContentLoaded', event => {
    const datatablesSimple = document.getElementsByClassName('datatable-table');
    if (datatablesSimple.length > 0) {
        Array.from(datatablesSimple).forEach(table => {
            new simpleDatatables.DataTable(table, {
                searchable: true,
                sortable: true,
                perPage: 10,
                perPageSelect: [10, 25, 50, 100],
                labels: {
                    placeholder: "Tìm kiếm...",
                    perPage: "{select} dòng mỗi trang",
                    noRows: "Không tìm thấy dữ liệu",
                    info: "Hiển thị {start} đến {end} của {rows} dòng",
                }
            });
        });
    }
}); 