(() => {
    if (!window.Auth || !Auth.requireAuth()) {
        return;
    }

    const FALLBACK_TEXT = '—';
    const MAX_LIBRARIES = 200;
    const BATCH_SIZE = 8;
    const state = {
        offices: [],
        search: '',
        warnings: []
    };

    const loadingState = document.getElementById('loading-state');
    const contentContainer = document.getElementById('content-container');
    const loadingTitle = document.getElementById('loading-title');
    const loadingSubtitle = document.getElementById('loading-subtitle');
    const pageNotice = document.getElementById('page-notice');
    const refreshButton = document.getElementById('refresh-button');
    const searchInput = document.getElementById('search-input');
    const searchCount = document.getElementById('search-count');
    const officesTbody = document.getElementById('offices-tbody');
    const officesEmpty = document.getElementById('offices-empty');
    const monthlyList = document.getElementById('monthly-list');
    const monthlyEmpty = document.getElementById('monthly-empty');

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

    function formatNumber(value) {
        return Number(value || 0).toLocaleString('en-US');
    }

    function formatPercent(value) {
        return Number(value || 0).toLocaleString('en-US', {
            minimumFractionDigits: 1,
            maximumFractionDigits: 1
        }) + '%';
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

    function formatMonthLabel(value) {
        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return 'شهر غير معروف';
        }

        return date.toLocaleDateString('ar-IQ', {
            month: 'long',
            year: 'numeric'
        });
    }

    function updateMetric(id, value) {
        const element = byId(id);
        if (element) {
            element.textContent = value;
        }
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

    function showLoading(title, subtitle) {
        loadingTitle.textContent = title;
        loadingSubtitle.textContent = subtitle;
        loadingState.classList.remove('hidden');
        contentContainer.classList.add('hidden');
        refreshButton.disabled = true;
    }

    function showContent() {
        loadingState.classList.add('hidden');
        contentContainer.classList.remove('hidden');
        refreshButton.disabled = false;
    }

    async function fetchJson(url, fallbackMessage) {
        const response = await Auth.apiFetch(url);
        if (!response) {
            throw new Error('انتهت الجلسة الحالية. سجل الدخول مرة أخرى.');
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

    function getMonthKey(dateValue) {
        const date = new Date(dateValue);
        if (Number.isNaN(date.getTime())) {
            return null;
        }

        return String(date.getFullYear()) + '-' + String(date.getMonth() + 1).padStart(2, '0');
    }

    function addMonthValue(map, dateValue, charges, settled, transactionCount, settlementCount) {
        const key = getMonthKey(dateValue);
        if (!key) {
            return;
        }

        if (!map.has(key)) {
            map.set(key, {
                key,
                label: formatMonthLabel(dateValue),
                charges: 0,
                settled: 0,
                transactionCount: 0,
                settlementCount: 0
            });
        }

        const item = map.get(key);
        item.charges += Number(charges || 0);
        item.settled += Number(settled || 0);
        item.transactionCount += Number(transactionCount || 0);
        item.settlementCount += Number(settlementCount || 0);
    }

    function renderMonthly(months) {
        if (!months.length) {
            monthlyList.innerHTML = '';
            monthlyEmpty.classList.remove('hidden');
            return;
        }

        monthlyEmpty.classList.add('hidden');
        const maxValue = Math.max(1, ...months.flatMap((item) => [item.charges, item.settled]));
        monthlyList.innerHTML = months.map((item) => {
            const chargesWidth = Math.max(4, (item.charges / maxValue) * 100);
            const settledWidth = Math.max(4, (item.settled / maxValue) * 100);

            return `
                <article class="rounded-xl border border-slate-100 bg-slate-50/70 p-4">
                    <div class="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                        <div>
                            <h3 class="font-bold text-slate-800">${escapeHtml(item.label)}</h3>
                            <p class="mt-1 text-xs text-slate-400">${formatNumber(item.transactionCount)} حركة / ${formatNumber(item.settlementCount)} تسوية</p>
                        </div>
                        <div class="flex flex-wrap gap-2 text-xs font-semibold">
                            <span class="rounded-full border border-blue-100 bg-blue-50 px-2.5 py-1 text-blue-700">مشتريات ${formatMoney(item.charges)}</span>
                            <span class="rounded-full border border-emerald-100 bg-emerald-50 px-2.5 py-1 text-emerald-700">مدفوع ${formatMoney(item.settled)}</span>
                        </div>
                    </div>
                    <div class="mt-4 space-y-3">
                        <div>
                            <div class="mb-1 flex items-center justify-between text-xs text-slate-500"><span>المشتريات</span><span>${formatMoney(item.charges)}</span></div>
                            <div class="h-2.5 rounded-full bg-slate-200"><div class="h-2.5 rounded-full bg-blue-600" style="width:${chargesWidth}%"></div></div>
                        </div>
                        <div>
                            <div class="mb-1 flex items-center justify-between text-xs text-slate-500"><span>المدفوع</span><span>${formatMoney(item.settled)}</span></div>
                            <div class="h-2.5 rounded-full bg-slate-200"><div class="h-2.5 rounded-full bg-emerald-600" style="width:${settledWidth}%"></div></div>
                        </div>
                    </div>
                </article>
            `;
        }).join('');
    }

    function renderRanking(containerId, items, formatter) {
        const container = byId(containerId);
        if (!container) {
            return;
        }

        if (!items.length) {
            container.innerHTML = '<div class="rounded-xl border border-dashed border-slate-200 bg-slate-50 p-8 text-center text-sm text-slate-400">لا توجد بيانات كافية لعرض هذا الترتيب.</div>';
            return;
        }

        const maxValue = Math.max(1, ...items.map((item) => formatter(item).value));
        container.innerHTML = items.map((item, index) => {
            const meta = formatter(item);
            const width = Math.max(6, (meta.value / maxValue) * 100);
            return `
                <article class="rounded-xl border border-slate-100 bg-white p-4 transition hover:border-slate-200 hover:shadow-sm">
                    <div class="flex items-start justify-between gap-3">
                        <div class="min-w-0">
                            <div class="flex items-center gap-2">
                                <span class="inline-flex h-7 w-7 items-center justify-center rounded-full bg-slate-100 text-xs font-bold text-slate-600">${index + 1}</span>
                                <a href="office-details.html?id=${encodeURIComponent(item.id)}" class="truncate font-bold text-slate-800 transition hover:text-primary">${escapeHtml(item.name)}</a>
                            </div>
                            <p class="mt-1 text-xs text-slate-400">${escapeHtml(item.code)} / ${escapeHtml(displayText(item.ownerName))}</p>
                        </div>
                        <div class="text-left">
                            <div class="metric-value text-sm font-extrabold text-slate-900">${escapeHtml(meta.label)}</div>
                            <div class="mt-1 text-[11px] text-slate-400">${escapeHtml(meta.note)}</div>
                        </div>
                    </div>
                    <div class="mt-4 h-2.5 rounded-full bg-slate-100">
                        <div class="${meta.barClass} h-2.5 rounded-full" style="width:${width}%"></div>
                    </div>
                </article>
            `;
        }).join('');
    }

    function renderStatusMetrics(statusCounts) {
        updateMetric('metric-open', formatNumber(statusCounts.Open || 0));
        updateMetric('metric-partially-paid', formatNumber(statusCounts.PartiallyPaid || 0));
        updateMetric('metric-paid', formatNumber(statusCounts.Paid || 0));
        updateMetric('metric-cancelled', formatNumber(statusCounts.Cancelled || 0));
    }

    function renderSummary(result) {
        updateMetric('metric-total-charges', formatMoney(result.totalCharges));
        updateMetric('metric-total-settled', formatMoney(result.totalSettled));
        updateMetric('metric-total-due', formatMoney(result.totalDue));
        updateMetric('metric-collection-rate', formatPercent(result.collectionRate));
        updateMetric('metric-offices', formatNumber(result.processedOffices));
        updateMetric('metric-transactions', formatNumber(result.transactionsCount));
        updateMetric('metric-settlements', formatNumber(result.settlementsCount));
        updateMetric('metric-average-due', formatMoney(result.averageDue));
    }

    function getFilteredOffices() {
        const term = normalize(state.search);
        if (!term) {
            return state.offices;
        }

        return state.offices.filter((item) => normalize([
            item.code,
            item.name,
            item.ownerName,
            item.province,
            item.city
        ].join(' ')).includes(term));
    }

    function renderOfficesTable() {
        const rows = getFilteredOffices();
        byId('offices-count').textContent = formatNumber(rows.length) + ' مكتب';

        if (state.search) {
            searchCount.textContent = 'نتائج البحث: ' + formatNumber(rows.length);
            searchCount.classList.remove('hidden');
        } else {
            searchCount.classList.add('hidden');
            searchCount.textContent = '';
        }

        if (!rows.length) {
            officesTbody.innerHTML = '';
            officesEmpty.classList.remove('hidden');
            return;
        }

        officesEmpty.classList.add('hidden');
        officesTbody.innerHTML = rows.map((item) => `
            <tr class="transition-colors hover:bg-slate-50/70">
                <td class="px-5 py-4 font-mono text-xs font-semibold text-slate-700">${escapeHtml(item.code)}</td>
                <td class="px-5 py-4">
                    <div class="font-bold text-slate-900">${escapeHtml(item.name)}</div>
                    <div class="mt-1 text-xs text-slate-400">${escapeHtml(displayText(item.province))} / ${escapeHtml(displayText(item.city))}</div>
                </td>
                <td class="px-5 py-4 text-slate-600">${escapeHtml(displayText(item.ownerName))}</td>
                <td class="px-5 py-4 font-bold text-slate-800">${escapeHtml(formatMoney(item.totalCharges))}</td>
                <td class="px-5 py-4 font-bold text-emerald-700">${escapeHtml(formatMoney(item.totalSettled))}</td>
                <td class="px-5 py-4 font-bold ${item.totalDue > 0 ? 'text-rose-600' : 'text-slate-700'}">${escapeHtml(formatMoney(item.totalDue))}</td>
                <td class="px-5 py-4">
                    <div class="font-bold text-slate-800">${escapeHtml(formatPercent(item.collectionRate))}</div>
                    <div class="mt-1 h-2 rounded-full bg-slate-100">
                        <div class="h-2 rounded-full ${item.collectionRate >= 80 ? 'bg-emerald-500' : item.collectionRate >= 50 ? 'bg-amber-500' : 'bg-rose-500'}" style="width:${Math.max(4, Math.min(100, item.collectionRate))}%"></div>
                    </div>
                </td>
                <td class="px-5 py-4 text-slate-600">${escapeHtml(formatNumber(item.transactionsCount))}</td>
                <td class="px-5 py-4 text-xs font-medium text-slate-500">${escapeHtml(formatDate(item.lastPaymentDate))}</td>
                <td class="px-5 py-4 text-center">
                    <a href="office-details.html?id=${encodeURIComponent(item.id)}" class="inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-3 py-2 text-xs font-semibold text-slate-700 transition hover:border-primary hover:text-primary">عرض</a>
                </td>
            </tr>
        `).join('');
    }

    function computeAnalytics(statements) {
        const months = new Map();
        const statusCounts = { Open: 0, PartiallyPaid: 0, Paid: 0, Cancelled: 0 };
        const offices = [];
        let totalCharges = 0;
        let totalSettled = 0;
        let totalDue = 0;
        let transactionsCount = 0;
        let settlementsCount = 0;

        statements.forEach(({ library, statement }) => {
            const summary = statement && statement.summary ? statement.summary : {};
            const officeCharges = Number(summary.totalCharges || 0);
            const officeSettled = Number(summary.totalSettled || summary.totalPaid || 0);
            const officeDue = Number(summary.totalDue || summary.totalBalance || 0);
            const officeTransactionsCount = Number(summary.transactionsCount || 0);
            const officeSettlementsCount = Number(summary.settlementsCount || 0);
            const officeCollectionRate = officeCharges > 0 ? (officeSettled / officeCharges) * 100 : (officeDue <= 0 ? 100 : 0);

            totalCharges += officeCharges;
            totalSettled += officeSettled;
            totalDue += officeDue;
            transactionsCount += officeTransactionsCount;
            settlementsCount += officeSettlementsCount;

            offices.push({
                id: library.id,
                code: library.libraryCode,
                name: library.libraryName,
                ownerName: library.ownerName,
                province: library.province,
                city: library.city,
                totalCharges: officeCharges,
                totalSettled: officeSettled,
                totalDue: officeDue,
                transactionsCount: officeTransactionsCount,
                settlementsCount: officeSettlementsCount,
                collectionRate: Number(officeCollectionRate.toFixed(1)),
                lastPaymentDate: summary.lastPaymentDate || null
            });

            (Array.isArray(statement.transactions) ? statement.transactions : []).forEach((tx) => {
                addMonthValue(months, tx.transactionDate || tx.createdAt, tx.amount, 0, 1, 0);
                const key = tx.status || 'Open';
                statusCounts[key] = (statusCounts[key] || 0) + 1;
            });

            (Array.isArray(statement.settlements) ? statement.settlements : []).forEach((settlement) => {
                addMonthValue(months, settlement.settlementDate || settlement.createdAt, 0, settlement.amount, 0, 1);
            });
        });

        offices.sort((a, b) => b.totalCharges - a.totalCharges || b.totalSettled - a.totalSettled || a.name.localeCompare(b.name));

        return {
            offices,
            totalCharges,
            totalSettled,
            totalDue,
            transactionsCount,
            settlementsCount,
            collectionRate: totalCharges > 0 ? (totalSettled / totalCharges) * 100 : 0,
            averageDue: offices.length ? totalDue / offices.length : 0,
            processedOffices: offices.length,
            monthlySeries: Array.from(months.values()).sort((a, b) => a.key.localeCompare(b.key)).slice(-8),
            topDebtors: offices.filter((item) => item.totalDue > 0).sort((a, b) => b.totalDue - a.totalDue || b.totalCharges - a.totalCharges).slice(0, 8),
            topCollectors: offices.filter((item) => item.totalCharges > 0).sort((a, b) => b.collectionRate - a.collectionRate || b.totalSettled - a.totalSettled).slice(0, 8),
            statusCounts
        };
    }

    async function loadFinancialAnalysis() {
        state.warnings = [];
        setNotice('', '');
        showLoading('جاري تحليل الأداء المالي...', 'يتم تحميل المكاتب والبيانات المالية وتجميع المؤشرات');

        try {
            const [statsResult, librariesResult] = await Promise.allSettled([
                fetchJson('/api/libraries/stats', 'تعذر تحميل إحصاءات المكاتب.'),
                fetchJson('/api/libraries?limit=' + MAX_LIBRARIES, 'تعذر تحميل قائمة المكاتب.')
            ]);

            const stats = statsResult.status === 'fulfilled' ? statsResult.value : null;
            const libraries = librariesResult.status === 'fulfilled' && Array.isArray(librariesResult.value) ? librariesResult.value : [];

            if (statsResult.status === 'rejected') {
                state.warnings.push(statsResult.reason instanceof Error ? statsResult.reason.message : 'تعذر تحميل إحصاءات المكاتب.');
            }

            if (librariesResult.status === 'rejected') {
                throw librariesResult.reason instanceof Error ? librariesResult.reason : new Error('تعذر تحميل قائمة المكاتب.');
            }

            if (!libraries.length) {
                throw new Error('لا توجد مكاتب متاحة للتحليل المالي.');
            }

            if (stats && Number(stats.total || 0) > libraries.length) {
                state.warnings.push('تم تحليل أول ' + formatNumber(libraries.length) + ' مكتب فقط لأن الحد الحالي للواجهة هو ' + formatNumber(MAX_LIBRARIES) + ' مكتب.');
            }

            const statementResults = [];
            let failedStatements = 0;

            for (let index = 0; index < libraries.length; index += BATCH_SIZE) {
                const processed = Math.min(index, libraries.length);
                showLoading('جاري تحليل الأداء المالي...', 'تمت معالجة ' + formatNumber(processed) + ' من ' + formatNumber(libraries.length) + ' مكتب');
                const batch = libraries.slice(index, index + BATCH_SIZE);
                const batchResults = await Promise.allSettled(
                    batch.map((library) => fetchJson('/api/financialtransactions/library/' + encodeURIComponent(library.id) + '/statement', 'تعذر تحميل البيان المالي.'))
                );

                batchResults.forEach((result, batchIndex) => {
                    if (result.status === 'fulfilled') {
                        statementResults.push({
                            library: batch[batchIndex],
                            statement: result.value || {}
                        });
                    } else {
                        failedStatements += 1;
                    }
                });
            }

            if (!statementResults.length) {
                throw new Error('تعذر تحميل أي بيان مالي للمكاتب.');
            }

            if (failedStatements > 0) {
                state.warnings.push('تعذر تحميل البيان المالي لـ ' + formatNumber(failedStatements) + ' مكتب. تم عرض التحليل بناءً على البيانات المتاحة فقط.');
            }

            const analytics = computeAnalytics(statementResults);
            state.offices = analytics.offices;
            renderSummary(analytics);
            renderStatusMetrics(analytics.statusCounts);
            renderMonthly(analytics.monthlySeries);
            renderRanking('top-debtors-list', analytics.topDebtors, (item) => ({
                value: item.totalDue,
                label: formatMoney(item.totalDue),
                note: 'المتبقي الحالي',
                barClass: 'bg-rose-500'
            }));
            renderRanking('top-collectors-list', analytics.topCollectors, (item) => ({
                value: item.collectionRate,
                label: formatPercent(item.collectionRate),
                note: 'المحصّل: ' + formatMoney(item.totalSettled),
                barClass: 'bg-emerald-500'
            }));
            renderOfficesTable();

            if (state.warnings.length) {
                setNotice(state.warnings, 'warning');
            }

            showContent();
        } catch (error) {
            const message = error instanceof Error ? error.message : 'تعذر تحميل التحليل المالي.';
            state.offices = [];
            monthlyList.innerHTML = '';
            monthlyEmpty.classList.remove('hidden');
            byId('top-debtors-list').innerHTML = '<div class="rounded-xl border border-dashed border-slate-200 bg-slate-50 p-8 text-center text-sm text-slate-400">تعذر عرض القائمة حالياً.</div>';
            byId('top-collectors-list').innerHTML = '<div class="rounded-xl border border-dashed border-slate-200 bg-slate-50 p-8 text-center text-sm text-slate-400">تعذر عرض القائمة حالياً.</div>';
            setNotice(message, 'error');
            renderOfficesTable();
            showContent();
        }
    }

    searchInput.addEventListener('input', (event) => {
        state.search = event.target.value || '';
        renderOfficesTable();
    });

    refreshButton.addEventListener('click', () => {
        loadFinancialAnalysis();
    });

    loadFinancialAnalysis();
})();
