class AppSidebar extends HTMLElement {
    connectedCallback() {
        const currentPath = window.location.pathname;
        const isDashboard = currentPath.includes('dashboard.html') || currentPath.endsWith('/') || currentPath.endsWith('wwwroot\\') || currentPath.endsWith('wwwroot/');
        const isSearch = currentPath.includes('search.html') || currentPath.includes('transaction-details.html');
        const isOffices = currentPath.includes('offices.html') || currentPath.includes('office-details.html');
        const isUsers = currentPath.includes('users.html')
            || currentPath.includes('user-details.html')
            || currentPath.includes('admin-user-add.html')
            || currentPath.includes('admin-roles.html');
        const isReports = currentPath.includes('reports.html');
        const isPerformance = currentPath.includes('performance.html');
        const activeClass = 'bg-primary/5 text-primary font-bold border border-primary/10';
        const inactiveClass = 'text-slate-600 hover:bg-slate-50 hover:text-primary border border-transparent';
        const activeIcon = 'text-primary';
        const inactiveIcon = 'text-slate-400 group-hover:text-primary transition-colors';

        this.innerHTML = `
        <div id="sidebar-overlay" class="fixed inset-0 z-40 hidden bg-slate-900/50 transition-opacity md:hidden"></div>

        <aside id="app-sidebar" class="fixed right-0 top-0 z-50 flex h-screen w-64 translate-x-full transform flex-col border-l border-slate-200 bg-white transition-transform md:sticky md:translate-x-0">
            <div class="flex h-16 items-center justify-between border-b border-slate-200 px-6">
                <div class="flex items-center gap-2">
                    <div class="flex h-8 w-8 items-center justify-center rounded-lg bg-primary font-bold text-white shadow-sm">A</div>
                    <span class="text-xl font-bold text-primary">الإدارة</span>
                </div>
                <button id="close-sidebar" class="text-slate-400 hover:text-slate-600 focus:outline-none md:hidden">
                    <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </button>
            </div>

            <div class="flex-1 overflow-y-auto p-4">
                <p class="mb-3 flex items-center gap-2 px-2 text-xs font-semibold tracking-wider text-slate-400">القائمة الرئيسية</p>
                <nav class="space-y-1.5">
                    <a href="dashboard.html" class="group flex items-center gap-3 rounded-lg px-3 py-2.5 transition-all duration-200 ${isDashboard ? activeClass : inactiveClass}">
                        <svg class="h-5 w-5 ${isDashboard ? activeIcon : inactiveIcon}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"></path>
                        </svg>
                        الرئيسية
                    </a>
                    <a href="search.html" class="group flex items-center gap-3 rounded-lg px-3 py-2.5 transition-all duration-200 ${isSearch ? activeClass : inactiveClass}">
                        <svg class="h-5 w-5 ${isSearch ? activeIcon : inactiveIcon}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-4.35-4.35m1.85-5.15a7 7 0 11-14 0 7 7 0 0114 0z"></path>
                        </svg>
                        البحث العام
                    </a>
                    <a href="offices.html" class="group flex items-center gap-3 rounded-lg px-3 py-2.5 transition-all duration-200 ${isOffices ? activeClass : inactiveClass}">
                        <svg class="h-5 w-5 ${isOffices ? activeIcon : inactiveIcon}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"></path>
                        </svg>
                        المكاتب
                    </a>
                    <a href="users.html" class="group flex items-center gap-3 rounded-lg px-3 py-2.5 transition-all duration-200 ${isUsers ? activeClass : inactiveClass}">
                        <svg class="h-5 w-5 ${isUsers ? activeIcon : inactiveIcon}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"></path>
                        </svg>
                        المستخدمين
                    </a>
                    <a href="reports.html" class="group flex items-center gap-3 rounded-lg px-3 py-2.5 transition-all duration-200 ${isReports ? activeClass : inactiveClass}">
                        <svg class="h-5 w-5 ${isReports ? activeIcon : inactiveIcon}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
                        </svg>
                        التقارير
                    </a>
                    <a href="performance.html" class="group flex items-center gap-3 rounded-lg px-3 py-2.5 transition-all duration-200 ${isPerformance ? activeClass : inactiveClass}">
                        <svg class="h-5 w-5 ${isPerformance ? activeIcon : inactiveIcon}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
                        </svg>
                        تحليل الأداء
                    </a>
                </nav>

                <p class="mb-3 mt-8 flex items-center gap-2 px-2 text-xs font-semibold tracking-wider text-slate-400">الإعدادات</p>
                <nav class="space-y-1.5">
                    <a href="#" class="group flex items-center gap-3 rounded-lg border border-transparent px-3 py-2.5 text-slate-600 transition-all duration-200 hover:bg-slate-50 hover:text-primary">
                        <svg class="h-5 w-5 text-slate-400 transition-colors group-hover:text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path>
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                        </svg>
                        إعدادات النظام
                    </a>
                </nav>
            </div>

            <div class="border-t border-slate-200 bg-slate-50/50 p-4">
                <button id="logout-btn" class="flex w-full items-center gap-3 rounded-lg border border-transparent px-3 py-2.5 font-medium text-red-600 transition-all duration-200 hover:border-red-100 hover:bg-red-50">
                    <svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path>
                    </svg>
                    تسجيل الخروج
                </button>
            </div>
        </aside>
        `;

        setTimeout(() => {
            const toggleBtn = document.getElementById('toggle-sidebar');
            const closeBtn = document.getElementById('close-sidebar');
            const sidebar = document.getElementById('app-sidebar');
            const overlay = document.getElementById('sidebar-overlay');

            if (toggleBtn && closeBtn && sidebar && overlay) {
                const closeSidebar = () => {
                    sidebar.classList.add('translate-x-full');
                    window.setTimeout(() => overlay.classList.add('hidden'), 300);
                };

                toggleBtn.addEventListener('click', () => {
                    sidebar.classList.remove('translate-x-full');
                    overlay.classList.remove('hidden');
                });

                closeBtn.addEventListener('click', closeSidebar);
                overlay.addEventListener('click', closeSidebar);
            }

            const logoutBtn = document.getElementById('logout-btn');
            if (logoutBtn && window.Auth) {
                logoutBtn.addEventListener('click', () => {
                    Auth.logout();
                });
            }
        }, 50);
    }
}

customElements.define('app-sidebar', AppSidebar);
