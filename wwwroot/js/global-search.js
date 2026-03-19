(() => {
    if (!window.Auth || !Auth.requireAuth()) {
        return;
    }

    const FALLBACK_TEXT = '—';
    const CACHE_LIMIT = 200;
    const VIEW_LIMIT = 10;
    const state = {
        token: 0,
        operationsToken: 0,
        cache: {
            admins: null,
            accounts: null,
            devices: null,
            transactions: null
        }
    };

    const initialState = document.getElementById('initial-state');
    const loadingState = document.getElementById('loading-state');
    const resultsContainer = document.getElementById('results-container');
    const loadingTitle = document.getElementById('loading-title');
    const loadingSubtitle = document.getElementById('loading-subtitle');
    const pageNotice = document.getElementById('page-notice');
    const searchForm = document.getElementById('search-form');
    const searchInput = document.getElementById('search-input');
    const operationsModal = document.getElementById('operations-modal');
    const operationsModalTitle = document.getElementById('operations-modal-title');
    const operationsModalSubtitle = document.getElementById('operations-modal-subtitle');
    const operationsLoading = document.getElementById('operations-loading');
    const operationsError = document.getElementById('operations-error');
    const operationsContent = document.getElementById('operations-content');
    const operationsTransactions = document.getElementById('operations-transactions');
    const operationsTransactionsCount = document.getElementById('operations-transactions-count');
    const operationsEmpty = document.getElementById('operations-empty');
    let currentOperationsLibraryId = null;
    let currentOperationsLibraryName = '';

    document.getElementById('header-username').textContent = Auth.getUsername();
    document.getElementById('header-fullname').textContent = Auth.getDisplayName();

    function byId(id) {
        return document.getElementById(id);
    }

    function escapeHtml(value) {
        return String(value ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function displayText(value) {
        return value === undefined || value === null || String(value).trim() === '' ? FALLBACK_TEXT : String(value);
    }

    function normalize(value) {
        return String(value ?? '').toLowerCase().trim();
    }

    function formatMoney(amount) {
        return Number(amount || 0).toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function formatDate(value) {
        if (!value) {
            return FALLBACK_TEXT;
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return FALLBACK_TEXT;
        }

        return date.toLocaleString('en-GB', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    function getFinancialStatusLabel(status) {
        if (status === 'Open') {
            return 'مفتوحة';
        }

        if (status === 'PartiallyPaid') {
            return 'مدفوعة جزئياً';
        }

        if (status === 'Paid') {
            return 'مدفوعة';
        }

        if (status === 'Cancelled') {
            return 'ملغية';
        }

        return displayText(status);
    }

    function getTransactionTypeLabel(type) {
        if (type === 'OpenInvoice') {
            return 'فاتورة مفتوحة';
        }

        return displayText(type);
    }

    function canSettleTransaction(remainingAmount, status) {
        return Number(remainingAmount || 0) > 0 && status !== 'Cancelled' && status !== 'Paid';
    }

    function buildSettlementButtons(transactionId, remainingAmount, status, libraryId, libraryName) {
        if (!canSettleTransaction(remainingAmount, status)) {
            return '';
        }

        const safeTransactionId = escapeHtml(String(transactionId || ''));
        const safeRemainingAmount = escapeHtml(String(remainingAmount || 0));
        const safeLibraryId = escapeHtml(String(libraryId || ''));
        const safeLibraryName = escapeHtml(displayText(libraryName));

        return `
            <button type="button" data-ui-action="settle-transaction-full" data-transaction-id="${safeTransactionId}" data-remaining-amount="${safeRemainingAmount}" data-library-id="${safeLibraryId}" data-library-name="${safeLibraryName}" class="inline-flex items-center gap-2 rounded-lg bg-emerald-600 px-3 py-2 text-xs font-semibold text-white transition hover:bg-emerald-700">
                دفع كامل
            </button>
            <button type="button" data-ui-action="settle-transaction-partial" data-transaction-id="${safeTransactionId}" data-remaining-amount="${safeRemainingAmount}" data-library-id="${safeLibraryId}" data-library-name="${safeLibraryName}" class="inline-flex items-center gap-2 rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-xs font-semibold text-amber-700 transition hover:bg-amber-100">
                دفع جزئي
            </button>
        `;
    }

    function dateTokens(value) {
        if (!value) {
            return [];
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return [];
        }

        return [
            date.toISOString().slice(0, 10),
            date.toLocaleDateString('en-GB'),
            formatDate(date.toISOString())
        ];
    }

    function parseSearchDate(term) {
        const value = normalize(term);
        if (!value) {
            return null;
        }

        const date = new Date(value);
        return Number.isNaN(date.getTime()) ? null : date;
    }

    function sameDay(left, right) {
        if (!left || !right) {
            return false;
        }

        const leftDate = new Date(left);
        if (Number.isNaN(leftDate.getTime())) {
            return false;
        }

        return leftDate.getFullYear() === right.getFullYear()
            && leftDate.getMonth() === right.getMonth()
            && leftDate.getDate() === right.getDate();
    }

    function matchesSearch(term, fields, dates) {
        const normalizedTerm = normalize(term);
        const haystack = normalize([
            ...(fields || []),
            ...((dates || []).flatMap((value) => dateTokens(value)))
        ].join(' '));

        if (haystack.includes(normalizedTerm)) {
            return true;
        }

        const searchDate = parseSearchDate(term);
        return !!searchDate && (dates || []).some((value) => sameDay(value, searchDate));
    }

    function setNotice(messages, tone) {
        const text = Array.isArray(messages) ? messages.filter(Boolean).join(' ') : messages;
        if (!text) {
            pageNotice.className = 'mb-6 hidden rounded-xl border px-4 py-3 text-sm font-medium';
            pageNotice.textContent = '';
            return;
        }

        pageNotice.className = tone === 'error'
            ? 'mb-6 rounded-xl border border-rose-100 bg-rose-50 px-4 py-3 text-sm font-medium text-rose-700'
            : 'mb-6 rounded-xl border border-amber-100 bg-amber-50 px-4 py-3 text-sm font-medium text-amber-800';
        pageNotice.textContent = text;
    }

    function showInitial() {
        initialState.classList.remove('hidden');
        loadingState.classList.add('hidden');
        resultsContainer.classList.add('hidden');
    }

    function showLoading(title, subtitle) {
        loadingTitle.textContent = title;
        loadingSubtitle.textContent = subtitle;
        initialState.classList.add('hidden');
        resultsContainer.classList.add('hidden');
        loadingState.classList.remove('hidden');
    }

    function showResults() {
        initialState.classList.add('hidden');
        loadingState.classList.add('hidden');
        resultsContainer.classList.remove('hidden');
    }

    async function fetchJson(url, fallbackMessage, options) {
        const response = await Auth.apiFetch(url, options);
        if (!response) {
            throw new Error('انتهت الجلسة الحالية. سجل الدخول مرة أخرى.');
        }

        if (response.status === 404) {
            return null;
        }

        if (!response.ok) {
            let message = fallbackMessage;
            try {
                const text = await response.text();
                if (text) {
                    message = text;
                }
            } catch {
                message = fallbackMessage;
            }

            throw new Error(message);
        }

        return response.json();
    }

    async function loadCached(key, url, fallbackMessage) {
        if (state.cache[key]) {
            return state.cache[key];
        }

        const data = await fetchJson(url, fallbackMessage);
        state.cache[key] = Array.isArray(data) ? data : [];
        return state.cache[key];
    }

    function syncUrl(term) {
        const params = new URLSearchParams();
        if (term) {
            params.set('q', term);
        }

        window.history.replaceState({}, '', window.location.pathname + (params.toString() ? '?' + params.toString() : ''));
    }

    function renderEmpty(text) {
        return `<div class="rounded-xl border border-dashed border-slate-200 bg-slate-50 p-6 text-center text-sm text-slate-400">${escapeHtml(text)}</div>`;
    }

    function renderList(id, countId, items, renderer, emptyText) {
        byId(countId).textContent = String(items.length);
        byId(id).innerHTML = items.length ? items.map(renderer).join('') : renderEmpty(emptyText);
    }

    function renderMetrics(term, results) {
        const total = results.libraries.length
            + results.users.length
            + results.devices.length
            + results.transactions.length
            + results.audit.length
            + (results.qr ? 1 : 0);

        const sections = [
            results.libraries.length,
            results.users.length,
            results.devices.length,
            results.transactions.length,
            results.audit.length,
            results.qr ? 1 : 0
        ].filter((count) => count > 0).length;

        byId('metric-total-results').textContent = String(total);
        byId('metric-active-sections').textContent = String(sections);
        byId('metric-query-text').textContent = term || FALLBACK_TEXT;
        byId('metric-qr-state').textContent = results.qr ? 'موجود' : 'لا يوجد';
    }

    function renderLibraries(items) {
        renderList('libraries-results', 'libraries-count', items, (item) => `
            <article class="rounded-xl border border-slate-100 bg-white p-4 transition hover:border-slate-200 hover:shadow-sm">
                <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                        <a href="office-details.html?id=${encodeURIComponent(item.id)}" class="font-bold text-slate-800 transition hover:text-primary">${escapeHtml(item.libraryName)}</a>
                        <p class="mt-1 text-xs text-slate-400">${escapeHtml(item.libraryCode)} / ${escapeHtml(displayText(item.ownerName))}</p>
                    </div>
                    <div class="text-left text-xs text-slate-500">${escapeHtml(displayText(item.city))}</div>
                </div>
            </article>
        `, 'لا توجد مكاتب مطابقة.');
    }

    function renderUsers(items) {
        renderList('users-results', 'users-count', items, (item) => `
            <article class="rounded-xl border border-slate-100 bg-white p-4 transition hover:border-slate-200 hover:shadow-sm">
                <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                        <a href="user-details.html?type=${encodeURIComponent(item.type)}&id=${encodeURIComponent(item.id)}&tab=${encodeURIComponent(item.type)}" class="font-bold text-slate-800 transition hover:text-primary">${escapeHtml(item.title)}</a>
                        <p class="mt-1 text-xs text-slate-400">${escapeHtml(item.subtitle)}</p>
                    </div>
                    <span class="rounded-full border px-2.5 py-1 text-[11px] font-bold ${item.type === 'admin' ? 'border-slate-200 bg-slate-100 text-slate-700' : 'border-blue-100 bg-blue-50 text-blue-700'}">${item.type === 'admin' ? 'أدمن' : 'حساب مكتب'}</span>
                </div>
            </article>
        `, 'لا توجد حسابات أو مستخدمون مطابقون.');
    }

    function renderDevices(items) {
        renderList('devices-results', 'devices-count', items, (item) => `
            <article class="rounded-xl border border-slate-100 bg-white p-4 transition hover:border-slate-200 hover:shadow-sm">
                <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                        <a href="office-details.html?id=${encodeURIComponent(item.libraryId || '')}" class="font-bold text-slate-800 transition hover:text-primary">${escapeHtml(item.posCode)}</a>
                        <p class="mt-1 text-xs text-slate-400">${escapeHtml(displayText(item.serialNumber))} / ${escapeHtml(displayText(item.libraryName))}</p>
                    </div>
                    <div class="text-left text-xs text-slate-500">${escapeHtml(displayText(item.deviceVendor))}</div>
                </div>
            </article>
        `, 'لا توجد أجهزة مطابقة.');
    }

    function renderTransactions(items) {
        renderList('transactions-results', 'transactions-count', items, (item) => `
            <article class="rounded-xl border border-slate-100 bg-white p-4 transition hover:border-slate-200 hover:shadow-sm">
                <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                        <a href="office-details.html?id=${encodeURIComponent(item.libraryId)}" class="font-bold text-slate-800 transition hover:text-primary">${escapeHtml(displayText(item.libraryName))}</a>
                        <p class="mt-1 text-xs text-slate-400">${escapeHtml(displayText(item.description))}</p>
                    </div>
                    <div class="text-left">
                        <div class="text-sm font-extrabold text-slate-900">${escapeHtml(formatMoney(item.amount))}</div>
                        <div class="mt-1 text-[11px] text-slate-400">${escapeHtml(formatDate(item.transactionDate || item.createdAt))}</div>
                    </div>
                </div>
                <div class="mt-4 flex flex-wrap justify-end gap-2">
                    <a href="transaction-details.html?id=${encodeURIComponent(item.id)}" class="inline-flex items-center gap-2 rounded-lg border border-primary/20 bg-primary/5 px-3 py-2 text-xs font-semibold text-primary transition hover:bg-primary/10">
                        استعراض العملية
                    </a>
                </div>
            </article>
        `, 'لا توجد معاملات مالية مطابقة.');
    }

    function renderQr(item) {
        renderList('qr-results', 'qr-count', item ? [item] : [], (value) => `
            <article class="rounded-xl border border-slate-100 bg-white p-4 transition hover:border-slate-200 hover:shadow-sm">
                <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                        <a href="office-details.html?id=${encodeURIComponent(value.libraryId)}" class="font-bold text-slate-800 transition hover:text-primary">${escapeHtml(value.qrReference)}</a>
                        <p class="mt-1 text-xs text-slate-400">${escapeHtml(value.libraryName)} / ${escapeHtml(value.studentName)}</p>
                    </div>
                    <div class="text-left">
                        <div class="text-sm font-extrabold text-slate-900">${escapeHtml(formatMoney(value.remainingAmountIqd))}</div>
                        <div class="mt-1 text-[11px] text-slate-400">المتبقي</div>
                    </div>
                </div>
                <div class="mt-4 flex flex-wrap justify-end gap-2">
                    <a href="transaction-details.html?id=${encodeURIComponent(value.financialTransactionId)}" class="inline-flex items-center gap-2 rounded-lg border border-primary/20 bg-primary/5 px-3 py-2 text-xs font-semibold text-primary transition hover:bg-primary/10">
                        استعراض العملية
                    </a>
                </div>
            </article>
        `, 'لا توجد مطابقة مباشرة لرمز QR المرجعي.');
    }

    function renderAudit(items) {
        renderList('audit-results', 'audit-count', items, (item) => `
            <article class="rounded-xl border border-slate-100 bg-white p-4 transition hover:border-slate-200 hover:shadow-sm">
                <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                        <a href="reports.html?search=${encodeURIComponent(item.traceIdentifier || item.accountUsername || '')}" class="font-bold text-slate-800 transition hover:text-primary">${escapeHtml(displayText(item.actionName))}</a>
                        <p class="mt-1 text-xs text-slate-400">${escapeHtml(displayText(item.endpoint))}</p>
                    </div>
                    <div class="text-left">
                        <div class="text-xs font-semibold text-slate-600">${escapeHtml(displayText(item.accountUsername))}</div>
                        <div class="mt-1 text-[11px] text-slate-400">${escapeHtml(formatDate(item.operationDate))}</div>
                    </div>
                </div>
            </article>
        `, 'لا توجد سجلات نظام مطابقة.');
    }

    function openOperationsModal() {
        if (!operationsModal) {
            window.alert('تعذر فتح نافذة استعراض العمليات في هذه الصفحة.');
            return;
        }

        operationsModal.classList.remove('hidden');
        operationsModal.classList.add('flex');
        document.body.classList.add('overflow-hidden');
    }

    function closeOperationsModal() {
        operationsModal.classList.add('hidden');
        operationsModal.classList.remove('flex');
        document.body.classList.remove('overflow-hidden');
    }

    function renderOperationsTransactions(items) {
        operationsTransactionsCount.textContent = String(items.length);

        if (!items.length) {
            operationsTransactions.innerHTML = '';
            operationsEmpty.classList.remove('hidden');
            return;
        }

        operationsEmpty.classList.add('hidden');
        operationsTransactions.innerHTML = items.map((item) => `
            <article class="rounded-xl border border-slate-100 bg-white p-4 shadow-sm">
                <div class="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                    <div class="min-w-0 flex-1">
                        <div class="flex flex-wrap items-center gap-2">
                            <h5 class="text-sm font-bold text-slate-800">${escapeHtml(displayText(item.description))}</h5>
                            <span class="rounded-full border border-slate-200 bg-slate-50 px-2.5 py-1 text-[11px] font-semibold text-slate-600">${escapeHtml(getTransactionTypeLabel(item.transactionType))}</span>
                            <span class="rounded-full border border-blue-100 bg-blue-50 px-2.5 py-1 text-[11px] font-semibold text-blue-700">${escapeHtml(getFinancialStatusLabel(item.status))}</span>
                        </div>
                        <div class="mt-2 text-xs text-slate-400">${escapeHtml(formatDate(item.transactionDate || item.createdAt))}</div>
                    </div>
                    <div class="grid gap-3 text-left sm:grid-cols-3 lg:min-w-[320px]">
                        <div class="rounded-lg border border-slate-100 bg-slate-50 px-3 py-2">
                            <div class="text-[11px] font-semibold text-slate-400">المبلغ</div>
                            <div class="mt-1 text-sm font-extrabold text-slate-900">${escapeHtml(formatMoney(item.amount))}</div>
                        </div>
                        <div class="rounded-lg border border-emerald-100 bg-emerald-50 px-3 py-2">
                            <div class="text-[11px] font-semibold text-emerald-700">المدفوع</div>
                            <div class="mt-1 text-sm font-extrabold text-emerald-800">${escapeHtml(formatMoney(item.paidAmount))}</div>
                        </div>
                        <div class="rounded-lg border border-amber-100 bg-amber-50 px-3 py-2">
                            <div class="text-[11px] font-semibold text-amber-700">المتبقي</div>
                            <div class="mt-1 text-sm font-extrabold text-amber-800">${escapeHtml(formatMoney(item.remainingAmount))}</div>
                        </div>
                    </div>
                </div>
                <div class="mt-4 flex flex-wrap justify-end gap-2">
                    ${buildSettlementButtons(item.id, item.remainingAmount, item.status, currentOperationsLibraryId, currentOperationsLibraryName)}
                </div>
            </article>
        `).join('');
    }

    async function openOperationsPreview(libraryId, libraryName) {
        const id = String(libraryId || '').trim();
        if (!id) {
            return;
        }

        currentOperationsLibraryId = id;
        currentOperationsLibraryName = displayText(libraryName);
        const token = ++state.operationsToken;
        operationsModalTitle.textContent = 'استعراض العمليات';
        operationsModalSubtitle.textContent = displayText(libraryName);
        operationsError.classList.add('hidden');
        operationsError.textContent = '';
        operationsContent.classList.add('hidden');
        operationsTransactions.innerHTML = '';
        operationsTransactionsCount.textContent = '0';
        operationsEmpty.classList.add('hidden');
        operationsLoading.classList.remove('hidden');
        openOperationsModal();

        try {
            const statement = await fetchJson('/api/financialtransactions/library/' + encodeURIComponent(id) + '/statement', 'تعذر تحميل عمليات المكتب.');
            if (token !== state.operationsToken) {
                return;
            }

            const transactions = Array.isArray(statement && statement.transactions) ? statement.transactions : [];
            operationsModalSubtitle.textContent = displayText(libraryName);
            renderOperationsTransactions(transactions);
            operationsLoading.classList.add('hidden');
            operationsContent.classList.remove('hidden');
        } catch (error) {
            if (token !== state.operationsToken) {
                return;
            }

        operationsLoading.classList.add('hidden');
        operationsContent.classList.add('hidden');
        operationsError.textContent = error instanceof Error ? error.message : 'تعذر تحميل عمليات المكتب.';
        operationsError.classList.remove('hidden');
        }
    }

    function handleActionClick(event) {
        if (event.target === operationsModal) {
            closeOperationsModal();
            return;
        }

        const actionTrigger = event.target.closest('[data-ui-action]');
        if (!actionTrigger) {
            return;
        }

        event.preventDefault();
        const action = actionTrigger.getAttribute('data-ui-action');

        if (action === 'open-operations-preview') {
            openOperationsPreview(actionTrigger.getAttribute('data-library-id'), actionTrigger.getAttribute('data-library-name'));
            return;
        }

        if (action === 'close-operations-modal') {
            closeOperationsModal();
            return;
        }

        if (action === 'settle-transaction-full') {
            settleTransaction(
                actionTrigger.getAttribute('data-transaction-id'),
                'full',
                actionTrigger.getAttribute('data-remaining-amount'),
                actionTrigger.getAttribute('data-library-id'),
                actionTrigger.getAttribute('data-library-name')
            );
            return;
        }

        if (action === 'settle-transaction-partial') {
            settleTransaction(
                actionTrigger.getAttribute('data-transaction-id'),
                'partial',
                actionTrigger.getAttribute('data-remaining-amount'),
                actionTrigger.getAttribute('data-library-id'),
                actionTrigger.getAttribute('data-library-name')
            );
        }
    }

    async function settleTransaction(transactionId, mode, remainingAmount, libraryId, libraryName) {
        const id = String(transactionId || '').trim();
        if (!id) {
            return;
        }

        const remaining = Number(remainingAmount || 0);
        if (!(remaining > 0)) {
            window.alert('هذه الحركة مسددة بالكامل بالفعل.');
            return;
        }

        const payload = {
            settlementMode: mode === 'partial' ? 'PartialAmount' : 'Full',
            notes: mode === 'partial' ? 'تسديد جزئي من البحث العام' : 'تسديد كامل من البحث العام'
        };

        if (mode === 'partial') {
            const input = window.prompt('أدخل مبلغ التسديد الجزئي', String(remaining));
            if (input === null) {
                return;
            }

            const amount = Number(input);
            if (!Number.isFinite(amount) || amount <= 0) {
                window.alert('مبلغ التسديد الجزئي غير صالح.');
                return;
            }

            if (amount > remaining) {
                window.alert('المبلغ المدخل أكبر من المتبقي على الحركة.');
                return;
            }

            payload.amount = amount;
        } else {
            const confirmed = window.confirm(`تأكيد تسديد كامل المبلغ المتبقي ${formatMoney(remaining)} ؟`);
            if (!confirmed) {
                return;
            }
        }

        try {
            await fetchJson('/api/financialtransactions/' + encodeURIComponent(id) + '/settlements', 'تعذر تنفيذ التسديد.', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (operationsModal.classList.contains('flex') && libraryId) {
                await openOperationsPreview(libraryId, libraryName);
            }

            if (String(searchInput.value || '').trim().length >= 2) {
                await executeSearch(searchInput.value);
            }

            window.alert(mode === 'partial' ? 'تم تنفيذ التسديد الجزئي بنجاح.' : 'تم تنفيذ التسديد الكامل بنجاح.');
        } catch (error) {
            window.alert(error instanceof Error ? error.message : 'تعذر تنفيذ التسديد.');
        }
    }

    async function searchLibraries(term) {
        const data = await fetchJson('/api/libraries/search?q=' + encodeURIComponent(term) + '&limit=' + VIEW_LIMIT, 'تعذر البحث في المكاتب.');
        return Array.isArray(data) ? data.slice(0, VIEW_LIMIT) : [];
    }

    async function searchUsers(term) {
        const [admins, accounts] = await Promise.all([
            loadCached('admins', '/api/adminusers?limit=' + CACHE_LIMIT, 'تعذر تحميل حسابات الأدمن.'),
            loadCached('accounts', '/api/libraryaccounts?limit=' + CACHE_LIMIT, 'تعذر تحميل حسابات المكاتب.')
        ]);

        const adminResults = admins
            .filter((item) => matchesSearch(term, [item.id, item.fullName, item.username, item.email, item.phoneNumber, item.roleName, item.roleCode], [item.createdAt, item.updatedAt, item.lastLoginAt]))
            .map((item) => ({
                type: 'admin',
                id: item.id,
                title: item.fullName,
                subtitle: [item.username, item.email || item.phoneNumber, item.roleName].filter(Boolean).join(' / ')
            }));

        const accountResults = accounts
            .filter((item) => matchesSearch(term, [item.id, item.fullName, item.username, item.phoneNumber, item.libraryName, item.roleName], [item.createdAt, item.updatedAt, item.lastLoginAt]))
            .map((item) => ({
                type: 'account',
                id: item.id,
                title: item.fullName,
                subtitle: [item.username, item.libraryName, item.phoneNumber].filter(Boolean).join(' / ')
            }));

        return adminResults.concat(accountResults).slice(0, VIEW_LIMIT * 2);
    }

    async function searchDevices(term) {
        const devices = await loadCached('devices', '/api/posdevices?limit=' + CACHE_LIMIT, 'تعذر تحميل الأجهزة.');
        return devices.filter((item) => matchesSearch(term, [
            item.id,
            item.posCode,
            item.serialNumber,
            item.deviceModel,
            item.deviceVendor,
            item.libraryName,
            item.activatedByUsername
        ], [item.activatedAt, item.lastAuthenticatedAt, item.createdAt, item.updatedAt])).slice(0, VIEW_LIMIT);
    }

    async function searchTransactions(term) {
        const transactions = await loadCached('transactions', '/api/financialtransactions', 'تعذر تحميل المعاملات المالية.');
        return transactions.filter((item) => matchesSearch(term, [
            item.id,
            item.libraryName,
            item.description,
            item.createdByAdminUsername,
            item.createdByLibraryUsername,
            item.status,
            item.transactionType,
            item.amount,
            item.paidAmount,
            item.remainingAmount
        ], [item.transactionDate, item.dueDate, item.createdAt, item.updatedAt])).slice(0, VIEW_LIMIT);
    }

    async function searchQr(term) {
        if (!term || term.length < 3) {
            return null;
        }

        return fetchJson('/api/adminqrcodes/by-reference?reference=' + encodeURIComponent(term), 'تعذر البحث عن رمز QR.');
    }

    async function searchAudit(term) {
        const params = new URLSearchParams({ page: '1', pageSize: String(VIEW_LIMIT) });
        const parsedDate = parseSearchDate(term);
        if (parsedDate) {
            const day = parsedDate.toISOString().slice(0, 10);
            params.set('fromDate', day + 'T00:00:00Z');
            params.set('toDate', day + 'T23:59:59.999Z');
        } else {
            params.set('search', term);
        }

        const data = await fetchJson('/api/auditlogs?' + params.toString(), 'تعذر البحث في سجلات النظام.');
        return data && Array.isArray(data.items) ? data.items : [];
    }

    async function executeSearch(term) {
        const trimmed = String(term || '').trim();
        const token = ++state.token;
        syncUrl(trimmed);

        if (trimmed.length < 2) {
            setNotice('', '');
            showInitial();
            return;
        }

        showLoading('جاري تنفيذ البحث...', 'يتم جلب النتائج من أكثر من مصدر داخل النظام');
        const warnings = [];

        const [libraries, users, devices, transactions, qr, audit] = await Promise.allSettled([
            searchLibraries(trimmed),
            searchUsers(trimmed),
            searchDevices(trimmed),
            searchTransactions(trimmed),
            searchQr(trimmed),
            searchAudit(trimmed)
        ]);

        if (token !== state.token) {
            return;
        }

        const results = {
            libraries: libraries.status === 'fulfilled' ? libraries.value : [],
            users: users.status === 'fulfilled' ? users.value : [],
            devices: devices.status === 'fulfilled' ? devices.value : [],
            transactions: transactions.status === 'fulfilled' ? transactions.value : [],
            qr: qr.status === 'fulfilled' ? qr.value : null,
            audit: audit.status === 'fulfilled' ? audit.value : []
        };

        [libraries, users, devices, transactions, qr, audit].forEach((result) => {
            if (result.status === 'rejected') {
                warnings.push(result.reason instanceof Error ? result.reason.message : 'تعذر إكمال بعض أجزاء البحث.');
            }
        });

        renderMetrics(trimmed, results);
        renderLibraries(results.libraries);
        renderUsers(results.users);
        renderDevices(results.devices);
        renderTransactions(results.transactions);
        renderQr(results.qr);
        renderAudit(results.audit);

        setNotice(warnings, warnings.length ? 'warning' : '');
        showResults();
    }

    let debounceId = null;
    searchInput.addEventListener('input', () => {
        window.clearTimeout(debounceId);
        debounceId = window.setTimeout(() => executeSearch(searchInput.value), 350);
    });

    searchForm.addEventListener('submit', (event) => {
        event.preventDefault();
        window.clearTimeout(debounceId);
        executeSearch(searchInput.value);
    });

    resultsContainer.addEventListener('click', handleActionClick);
    operationsModal.addEventListener('click', handleActionClick);

    document.addEventListener('click', (event) => {
        const suggestion = event.target.closest('[data-suggestion]');
        if (!suggestion) {
            return;
        }

        const value = suggestion.getAttribute('data-suggestion') || '';
        searchInput.value = value;
        executeSearch(value);
    });

    const initialQuery = new URLSearchParams(window.location.search).get('q') || '';
    if (initialQuery) {
        searchInput.value = initialQuery;
        executeSearch(initialQuery);
    } else {
        showInitial();
    }

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape' && operationsModal.classList.contains('flex')) {
            closeOperationsModal();
        }
    });
})();
