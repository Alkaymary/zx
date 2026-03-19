(() => {
    if (!window.Auth || !Auth.requireAuth()) {
        return;
    }

    const STANDARD_TEMPLATES = [
        {
            key: 'general-manager',
            name: 'مدير عام',
            code: 'admin-general-manager',
            tone: 'blue',
            note: 'صلاحية شاملة لإدارة الحسابات والوصول إلى كامل النظام.',
            capabilities: [
                'إنشاء جميع الحسابات الإدارية وربطها بأي دور متاح.',
                'الوصول إلى المكاتب والمالية والتقارير والبحث العام وكل الشاشات.',
                'إدارة الأدوار ومتابعة العمليات والأجهزة دون قيود تشغيلية.'
            ]
        },
        {
            key: 'sales-manager',
            name: 'مدير المبيعات',
            code: 'admin-sales-manager',
            tone: 'emerald',
            note: 'يركز على الاطلاع والتحليل المالي والميداني للمكاتب.',
            capabilities: [
                'استعراض جميع بيانات المكاتب والحسابات المرتبطة بها.',
                'الوصول إلى التقارير والتحليل المالي والمعاملات والتسويات.',
                'متابعة الأداء المالي دون إدارة الأجهزة أو إنشاء أدوار جديدة.'
            ]
        },
        {
            key: 'technical-manager',
            name: 'مدير التقنيات',
            code: 'admin-technical-manager',
            tone: 'amber',
            note: 'مسؤول عن دورة حياة أجهزة الـ POS وتجهيزها وتعطيلها.',
            capabilities: [
                'إضافة أجهزة POS جديدة وربطها بالمكاتب والحسابات المناسبة.',
                'تعديل بيانات الأجهزة وتحديث حالتها أو تعطيلها عند الحاجة.',
                'التركيز على الجانب التقني للأجهزة دون التوسع في الإدارة المالية.'
            ]
        },
        {
            key: 'viewer',
            name: 'العرض',
            code: 'admin-viewer',
            tone: 'slate',
            note: 'دور للقراءة فقط للاطلاع على البيانات بدون أي تعديل.',
            capabilities: [
                'الوصول إلى الصفحات والمعلومات المسموح قراءتها فقط.',
                'عدم إنشاء أو تعديل أو حذف أي عنصر داخل النظام.',
                'مناسب للمراجعة والرقابة والمتابعة الإدارية غير التنفيذية.'
            ]
        }
    ];

    const state = {
        roles: [],
        search: '',
        editingRoleId: null,
        isSaving: false,
        isApplyingTemplates: false
    };

    const loadingOverlay = document.getElementById('loading-overlay');
    const errorState = document.getElementById('error-state');
    const errorMessage = document.getElementById('error-message');
    const contentContainer = document.getElementById('content-container');
    const notice = document.getElementById('notice');
    const rolesList = document.getElementById('roles-list');
    const rolesEmpty = document.getElementById('roles-empty');
    const rolesEmptyDescription = document.getElementById('roles-empty-description');
    const rolesCount = document.getElementById('roles-count');
    const templatesGrid = document.getElementById('templates-grid');
    const searchInput = document.getElementById('search-input');
    const templateSelect = document.getElementById('template-select');
    const roleForm = document.getElementById('role-form');
    const roleNameInput = document.getElementById('role-name');
    const roleCodeInput = document.getElementById('role-code');
    const formTitle = document.getElementById('form-title');
    const saveRoleLabel = document.getElementById('save-role-label');
    const cancelEditButton = document.getElementById('cancel-edit-button');
    const saveRoleButton = document.getElementById('save-role-button');
    const applyTemplatesButton = document.getElementById('apply-templates-button');
    const applyTemplatesLabel = document.getElementById('apply-templates-label');
    const profilePreviewNote = document.getElementById('profile-preview-note');
    const profilePreviewCapabilities = document.getElementById('profile-preview-capabilities');

    document.getElementById('header-username').textContent = Auth.getUsername();
    document.getElementById('header-fullname').textContent = Auth.getDisplayName();

    function escapeHtml(value) {
        return String(value ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function displayText(value) {
        return value === undefined || value === null || String(value).trim() === '' ? '—' : String(value);
    }

    function normalize(value) {
        return String(value ?? '').trim().toLowerCase();
    }

    function formatDate(value) {
        if (!value) {
            return '—';
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return '—';
        }

        return date.toLocaleString('en-GB', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    function isAdminRole(role) {
        const guardName = String(role && role.guardName !== undefined ? role.guardName : '').toLowerCase();
        return guardName === 'admin' || guardName === '0';
    }

    function getAdminRoles() {
        return (Array.isArray(state.roles) ? state.roles : []).filter(isAdminRole);
    }

    function findTemplateByCode(code) {
        const normalizedCode = normalize(code);
        return STANDARD_TEMPLATES.find((template) => normalize(template.code) === normalizedCode) || null;
    }

    function getTemplateByKey(templateKey) {
        return STANDARD_TEMPLATES.find((template) => template.key === templateKey) || null;
    }

    function getRoleById(roleId) {
        return getAdminRoles().find((role) => String(role.id) === String(roleId)) || null;
    }

    function getRoleProfile(role) {
        const matchedTemplate = findTemplateByCode(role && role.code);
        if (matchedTemplate) {
            return matchedTemplate;
        }

        return {
            key: 'custom',
            name: displayText(role && role.name),
            code: displayText(role && role.code),
            tone: 'slate',
            note: 'دور مخصص للأدمن يحتاج إلى ضبط تشغيلي حسب سياسة الإدارة.',
            capabilities: [
                'يظهر هنا كدور إداري مخصص خارج القوالب القياسية.',
                'يمكن تعديل الاسم أو الكود أو إعادة ربطه بأحد القوالب الجاهزة.',
                'تأكد من توثيق الغرض من هذا الدور قبل استخدامه على نطاق واسع.'
            ]
        };
    }

    function getToneClasses(tone) {
        if (tone === 'blue') {
            return { card: 'border-blue-100 bg-blue-50/70', badge: 'bg-blue-100 text-blue-700', icon: 'text-blue-600' };
        }

        if (tone === 'emerald') {
            return { card: 'border-emerald-100 bg-emerald-50/70', badge: 'bg-emerald-100 text-emerald-700', icon: 'text-emerald-600' };
        }

        if (tone === 'amber') {
            return { card: 'border-amber-100 bg-amber-50/70', badge: 'bg-amber-100 text-amber-700', icon: 'text-amber-600' };
        }

        return { card: 'border-slate-200 bg-slate-50/70', badge: 'bg-slate-100 text-slate-700', icon: 'text-slate-500' };
    }

    function showContent() {
        loadingOverlay.classList.add('hidden');
        errorState.classList.add('hidden');
        errorState.classList.remove('flex');
        contentContainer.classList.remove('hidden');
    }

    function showError(message) {
        loadingOverlay.classList.add('hidden');
        contentContainer.classList.add('hidden');
        errorState.classList.remove('hidden');
        errorState.classList.add('flex');
        errorMessage.textContent = message || 'تعذر تحميل صفحة أدوار المدراء.';
    }

    function showNotice(message, tone) {
        const tones = {
            success: 'border-emerald-200 bg-emerald-50 text-emerald-800',
            error: 'border-red-200 bg-red-50 text-red-800',
            info: 'border-blue-200 bg-blue-50 text-blue-800'
        };

        notice.className = 'rounded-xl border px-5 py-4 text-sm font-semibold ' + (tones[tone] || tones.info);
        notice.textContent = message;
        notice.classList.remove('hidden');
    }

    function hideNotice() {
        notice.classList.add('hidden');
        notice.textContent = '';
    }

    async function parseJsonResponse(response, fallbackMessage) {
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

        if (response.status === 204) {
            return null;
        }

        return response.json();
    }

    async function fetchJsonWithTimeout(url, fallbackMessage, timeoutMs = 12000) {
        const controller = new AbortController();
        const timeoutId = window.setTimeout(() => controller.abort(), timeoutMs);

        try {
            const response = await Auth.apiFetch(url, { signal: controller.signal });
            return await parseJsonResponse(response, fallbackMessage);
        } catch (error) {
            if (error && error.name === 'AbortError') {
                throw new Error('انتهت مهلة الاتصال أثناء تحميل البيانات.');
            }

            throw error;
        } finally {
            window.clearTimeout(timeoutId);
        }
    }

    function renderSummary() {
        const adminRoles = getAdminRoles();
        const totalAssignedAdmins = adminRoles.reduce((sum, role) => sum + Number(role.adminUsersCount || 0), 0);
        const standardCount = adminRoles.filter((role) => !!findTemplateByCode(role.code)).length;
        const deletableCount = adminRoles.filter((role) => Number(role.adminUsersCount || 0) === 0 && Number(role.libraryAccountsCount || 0) === 0).length;

        document.getElementById('summary-roles-count').textContent = String(adminRoles.length);
        document.getElementById('summary-standard-count').textContent = String(standardCount);
        document.getElementById('summary-admin-users-count').textContent = String(totalAssignedAdmins);
        document.getElementById('summary-deletable-count').textContent = String(deletableCount);
    }

    function renderTemplateOptions() {
        templateSelect.innerHTML = `
            <option value="">دور مخصص</option>
            ${STANDARD_TEMPLATES.map((template) => `
                <option value="${escapeHtml(template.key)}">${escapeHtml(template.name)} - ${escapeHtml(template.code)}</option>
            `).join('')}
        `;
    }

    function renderProfilePreview(template) {
        if (!template) {
            profilePreviewNote.textContent = 'اختر قالباً جاهزاً لعرض وصف الصلاحيات المستهدفة لهذا الدور.';
            profilePreviewCapabilities.innerHTML = '';
            return;
        }

        profilePreviewNote.textContent = template.note;
        profilePreviewCapabilities.innerHTML = template.capabilities.map((capability) => `
            <div class="flex items-start gap-2 text-sm text-slate-600">
                <span class="mt-1 inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary">
                    <svg class="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                    </svg>
                </span>
                <span>${escapeHtml(capability)}</span>
            </div>
        `).join('');
    }

    function setEditingRole(role) {
        state.editingRoleId = role ? Number(role.id) : null;
        formTitle.textContent = state.editingRoleId ? 'تعديل الدور' : 'إضافة دور جديد';
        cancelEditButton.classList.toggle('hidden', !state.editingRoleId);

        if (!role) {
            roleForm.reset();
            templateSelect.value = '';
            renderProfilePreview(null);
            setSavingState(false);
            return;
        }

        const matchedTemplate = findTemplateByCode(role.code);
        templateSelect.value = matchedTemplate ? matchedTemplate.key : '';
        roleNameInput.value = role.name || '';
        roleCodeInput.value = role.code || '';
        renderProfilePreview(matchedTemplate);
        setSavingState(false);
    }

    function setSavingState(saving) {
        state.isSaving = saving;
        saveRoleButton.disabled = saving;

        if (saving) {
            saveRoleLabel.textContent = state.editingRoleId ? 'جاري حفظ التعديلات...' : 'جاري إنشاء الدور...';
            return;
        }

        saveRoleLabel.textContent = state.editingRoleId ? 'حفظ التعديلات' : 'إنشاء الدور';
    }

    function setApplyTemplatesState(running) {
        state.isApplyingTemplates = running;
        applyTemplatesButton.disabled = running;
        applyTemplatesLabel.textContent = running ? 'جاري تطبيق القوالب...' : 'تطبيق القوالب القياسية';
    }

    function getFilteredRoles() {
        const query = normalize(state.search);
        const roles = getAdminRoles();
        if (!query) {
            return roles;
        }

        return roles.filter((role) => normalize(role.name + ' ' + role.code).includes(query));
    }

    function renderTemplates() {
        const existingCodes = new Set(getAdminRoles().map((role) => normalize(role.code)));

        templatesGrid.innerHTML = STANDARD_TEMPLATES.map((template) => {
            const tone = getToneClasses(template.tone);
            const exists = existingCodes.has(normalize(template.code));

            return `
                <article class="rounded-xl border p-4 ${tone.card}">
                    <div class="flex items-start justify-between gap-3">
                        <div>
                            <h3 class="text-base font-bold text-slate-900">${escapeHtml(template.name)}</h3>
                            <p class="mt-1 text-xs font-semibold text-slate-500" dir="ltr">${escapeHtml(template.code)}</p>
                        </div>
                        <span class="inline-flex items-center rounded-full px-3 py-1 text-xs font-bold ${exists ? 'bg-emerald-100 text-emerald-700' : tone.badge}">
                            ${exists ? 'موجود' : 'غير موجود'}
                        </span>
                    </div>
                    <p class="mt-3 text-sm leading-7 text-slate-600">${escapeHtml(template.note)}</p>
                    <div class="mt-3 space-y-2">
                        ${template.capabilities.map((capability) => `
                            <div class="flex items-start gap-2 text-sm text-slate-600">
                                <span class="mt-1 inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-white/80 ${tone.icon}">
                                    <svg class="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                                    </svg>
                                </span>
                                <span>${escapeHtml(capability)}</span>
                            </div>
                        `).join('')}
                    </div>
                    <div class="mt-4">
                        <button type="button" data-template-key="${escapeHtml(template.key)}" class="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-semibold text-slate-700 transition hover:border-primary hover:text-primary">
                            استخدام القالب
                        </button>
                    </div>
                </article>
            `;
        }).join('');
    }

    function renderRoles() {
        const roles = getFilteredRoles();
        rolesCount.textContent = roles.length + ' دور';

        if (!roles.length) {
            rolesList.innerHTML = '';
            rolesEmpty.classList.remove('hidden');
            rolesEmptyDescription.textContent = state.search
                ? 'لم يتم العثور على دور يطابق عبارة البحث الحالية.'
                : 'يمكنك تطبيق القوالب القياسية أو إنشاء دور جديد من النموذج الجانبي.';
            return;
        }

        rolesEmpty.classList.add('hidden');
        rolesList.innerHTML = roles.map((role) => {
            const profile = getRoleProfile(role);
            const tone = getToneClasses(profile.tone);
            const assignedAdmins = Number(role.adminUsersCount || 0);
            const assignedLibraries = Number(role.libraryAccountsCount || 0);
            const canDelete = assignedAdmins === 0 && assignedLibraries === 0;

            return `
                <article class="rounded-xl border border-slate-100 bg-white shadow-sm transition-all hover:border-slate-200 hover:shadow-md">
                    <div class="p-5">
                        <div class="flex flex-col gap-4 xl:flex-row xl:items-start xl:justify-between">
                            <div class="min-w-0 flex-1">
                                <div class="flex flex-wrap items-center gap-2">
                                    <h3 class="text-lg font-bold text-slate-900">${escapeHtml(displayText(role.name))}</h3>
                                    <span class="inline-flex items-center rounded-full px-3 py-1 text-xs font-bold ${tone.badge}">${escapeHtml(profile.name)}</span>
                                    <span class="inline-flex items-center rounded-full bg-slate-100 px-3 py-1 text-xs font-bold text-slate-700">Admin Guard</span>
                                </div>
                                <div class="mt-3 flex flex-wrap items-center gap-2 text-sm text-slate-500">
                                    <span class="rounded-lg border border-slate-200 bg-slate-50 px-3 py-1 font-mono text-xs font-semibold text-slate-700" dir="ltr">${escapeHtml(displayText(role.code))}</span>
                                    <span>تم الإنشاء: ${escapeHtml(formatDate(role.createdAt))}</span>
                                </div>
                                <div class="mt-4 grid gap-3 sm:grid-cols-3">
                                    <div class="rounded-lg border border-slate-100 bg-slate-50/80 px-4 py-3">
                                        <p class="text-xs font-semibold text-slate-400">عدد المدراء المرتبطين</p>
                                        <p class="mt-1 text-lg font-extrabold text-slate-800">${assignedAdmins}</p>
                                    </div>
                                    <div class="rounded-lg border border-slate-100 bg-slate-50/80 px-4 py-3">
                                        <p class="text-xs font-semibold text-slate-400">حسابات مكاتب مرتبطة</p>
                                        <p class="mt-1 text-lg font-extrabold text-slate-800">${assignedLibraries}</p>
                                    </div>
                                    <div class="rounded-lg border border-slate-100 bg-slate-50/80 px-4 py-3">
                                        <p class="text-xs font-semibold text-slate-400">حالة الحذف</p>
                                        <p class="mt-1 text-sm font-bold ${canDelete ? 'text-emerald-700' : 'text-amber-700'}">${canDelete ? 'متاح' : 'مقيد'}</p>
                                    </div>
                                </div>
                                <div class="mt-4 rounded-xl border ${tone.card} p-4">
                                    <p class="text-sm font-bold text-slate-800">${escapeHtml(profile.note)}</p>
                                    <div class="mt-3 space-y-2">
                                        ${profile.capabilities.map((capability) => `
                                            <div class="flex items-start gap-2 text-sm text-slate-600">
                                                <span class="mt-1 inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-white ${tone.icon}">
                                                    <svg class="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                                                    </svg>
                                                </span>
                                                <span>${escapeHtml(capability)}</span>
                                            </div>
                                        `).join('')}
                                    </div>
                                </div>
                            </div>
                            <div class="flex shrink-0 flex-wrap items-center gap-3">
                                <button type="button" data-role-action="edit" data-role-id="${escapeHtml(String(role.id))}" class="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 transition hover:border-primary hover:text-primary">
                                    <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
                                    </svg>
                                    تعديل
                                </button>
                                <button type="button" data-role-action="delete" data-role-id="${escapeHtml(String(role.id))}" ${canDelete ? '' : 'disabled'} class="inline-flex items-center gap-2 rounded-lg border px-4 py-2.5 text-sm font-semibold transition ${canDelete ? 'border-red-200 bg-red-50 text-red-700 hover:bg-red-100' : 'cursor-not-allowed border-slate-200 bg-slate-100 text-slate-400'}">
                                    <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6M9 7V4a1 1 0 011-1h4a1 1 0 011 1v3m-7 0h8"></path>
                                    </svg>
                                    حذف
                                </button>
                            </div>
                        </div>
                    </div>
                </article>
            `;
        }).join('');
    }

    function renderPage() {
        renderSummary();
        renderTemplates();
        renderRoles();
    }

    async function loadRoles() {
        const roles = await fetchJsonWithTimeout('/api/roles', 'تعذر تحميل الأدوار.');
        state.roles = Array.isArray(roles) ? roles : [];
        renderPage();
    }

    function buildRolePayload() {
        return {
            name: roleNameInput.value.trim(),
            code: roleCodeInput.value.trim(),
            guardName: 0
        };
    }

    function applyTemplateToForm(templateKey) {
        const template = getTemplateByKey(templateKey);
        templateSelect.value = template ? template.key : '';

        if (!template) {
            renderProfilePreview(null);
            return;
        }

        roleNameInput.value = template.name;
        roleCodeInput.value = template.code;
        renderProfilePreview(template);
    }

    async function saveRole(event) {
        event.preventDefault();
        if (state.isSaving || !roleForm.reportValidity()) {
            return;
        }

        hideNotice();
        setSavingState(true);

        try {
            const roleId = state.editingRoleId;
            const url = roleId ? '/api/roles/' + encodeURIComponent(roleId) : '/api/roles';
            const method = roleId ? 'PUT' : 'POST';

            await parseJsonResponse(await Auth.apiFetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(buildRolePayload())
            }), roleId ? 'تعذر تحديث الدور.' : 'تعذر إنشاء الدور.');

            await loadRoles();
            setEditingRole(null);
            showNotice(roleId ? 'تم تحديث الدور بنجاح.' : 'تم إنشاء الدور بنجاح.', 'success');
        } catch (error) {
            console.error('Failed to save role', error);
            showNotice(error instanceof Error ? error.message : 'تعذر حفظ الدور.', 'error');
        } finally {
            setSavingState(false);
        }
    }

    async function applyStandardTemplates() {
        if (state.isApplyingTemplates) {
            return;
        }

        const existingCodes = new Set(getAdminRoles().map((role) => normalize(role.code)));
        const missingTemplates = STANDARD_TEMPLATES.filter((template) => !existingCodes.has(normalize(template.code)));

        if (!missingTemplates.length) {
            showNotice('جميع القوالب القياسية موجودة بالفعل.', 'info');
            return;
        }

        hideNotice();
        setApplyTemplatesState(true);

        try {
            for (const template of missingTemplates) {
                await parseJsonResponse(await Auth.apiFetch('/api/roles', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        name: template.name,
                        code: template.code,
                        guardName: 0
                    })
                }), 'تعذر إنشاء القالب القياسي ' + template.name + '.');
            }

            await loadRoles();
            showNotice('تمت إضافة القوالب القياسية الناقصة بنجاح.', 'success');
        } catch (error) {
            console.error('Failed to apply standard templates', error);
            showNotice(error instanceof Error ? error.message : 'تعذر تطبيق القوالب القياسية.', 'error');
        } finally {
            setApplyTemplatesState(false);
        }
    }

    async function deleteRole(roleId) {
        const role = getRoleById(roleId);
        if (!role) {
            showNotice('تعذر العثور على الدور المطلوب.', 'error');
            return;
        }

        if (Number(role.adminUsersCount || 0) > 0 || Number(role.libraryAccountsCount || 0) > 0) {
            showNotice('لا يمكن حذف دور مرتبط بمستخدمين أو حسابات حالياً.', 'error');
            return;
        }

        if (!window.confirm('تأكيد حذف الدور "' + role.name + '"؟')) {
            return;
        }

        hideNotice();

        try {
            await parseJsonResponse(await Auth.apiFetch('/api/roles/' + encodeURIComponent(role.id), {
                method: 'DELETE'
            }), 'تعذر حذف الدور.');

            await loadRoles();
            if (state.editingRoleId === Number(role.id)) {
                setEditingRole(null);
            }
            showNotice('تم حذف الدور بنجاح.', 'success');
        } catch (error) {
            console.error('Failed to delete role', error);
            showNotice(error instanceof Error ? error.message : 'تعذر حذف الدور.', 'error');
        }
    }

    function handleRoleListClick(event) {
        const trigger = event.target.closest('[data-role-action]');
        if (!trigger) {
            return;
        }

        const roleId = trigger.getAttribute('data-role-id');
        const action = trigger.getAttribute('data-role-action');

        if (!roleId || !action) {
            return;
        }

        if (action === 'edit') {
            const role = getRoleById(roleId);
            if (!role) {
                return;
            }

            hideNotice();
            setEditingRole(role);
            window.scrollTo({ top: 0, behavior: 'smooth' });
            return;
        }

        if (action === 'delete') {
            deleteRole(roleId);
        }
    }

    function handleTemplatesClick(event) {
        const trigger = event.target.closest('[data-template-key]');
        if (!trigger) {
            return;
        }

        const templateKey = trigger.getAttribute('data-template-key');
        if (!templateKey) {
            return;
        }

        hideNotice();
        applyTemplateToForm(templateKey);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    async function loadPageData() {
        try {
            renderTemplateOptions();
            await loadRoles();
            setEditingRole(null);
            showContent();
        } catch (error) {
            console.error('Failed to initialize admin roles page', error);
            showError(error instanceof Error ? error.message : 'تعذر تحميل الصفحة.');
        }
    }

    searchInput.addEventListener('input', (event) => {
        state.search = event.target.value || '';
        renderRoles();
    });

    templateSelect.addEventListener('change', () => {
        applyTemplateToForm(templateSelect.value);
    });

    roleForm.addEventListener('submit', saveRole);
    rolesList.addEventListener('click', handleRoleListClick);
    templatesGrid.addEventListener('click', handleTemplatesClick);
    cancelEditButton.addEventListener('click', () => {
        hideNotice();
        setEditingRole(null);
    });
    applyTemplatesButton.addEventListener('click', applyStandardTemplates);

    loadPageData();
})();
