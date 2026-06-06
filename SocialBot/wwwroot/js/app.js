let config = {};
let refreshInterval = null;

async function api(url, opts = {}) {
    try {
        const res = await fetch(url, {
            method: opts.method || 'GET',
            headers: { 'Accept': 'application/json' },
            ...opts
        });
        const data = await res.json();
        if (!res.ok) throw new Error(data.error || 'Gabim ne server');
        return data;
    } catch (e) {
        showToast(e.message, 'error');
        throw e;
    }
}

function showToast(msg, type = 'success') {
    const t = document.getElementById('toast');
    t.textContent = msg;
    t.className = 'toast ' + type + ' show';
    setTimeout(() => t.classList.remove('show'), 3500);
}

document.getElementById('modalOverlay').addEventListener('click', (e) => {
    if (e.target === e.currentTarget) hideModal();
});

function showModal(html) {
    document.getElementById('modalContent').innerHTML = html;
    document.getElementById('modalOverlay').classList.add('show');
}
function hideModal() {
    document.getElementById('modalOverlay').classList.remove('show');
}

// Navigation
document.querySelectorAll('.sidebar nav a').forEach(a => {
    a.addEventListener('click', (e) => {
        e.preventDefault();
        document.querySelectorAll('.sidebar nav a, .page').forEach(el => el.classList.remove('active'));
        a.classList.add('active');
        document.getElementById('page-' + a.dataset.page).classList.add('active');
        loadPageData(a.dataset.page);
    });
});

async function loadPageData(page) {
    if (page === 'dashboard') loadDashboard();
    else if (page === 'pages') loadPages();
    else if (page === 'posts') loadPosts();
    else if (page === 'comments') loadComments();
    else if (page === 'instagram') loadInstagramStatus();
    else if (page === 'aisettings') loadAiSettings();
}

// Settings
document.getElementById('settingsForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    document.getElementById('btnSaveSettings').disabled = true;
    document.getElementById('btnSaveSettings').textContent = 'Duke ruajtur...';
    try {
        const appId = document.getElementById('settingsAppId').value.trim();
        const appSecret = document.getElementById('settingsAppSecret').value.trim();
        await api(`/api/auth/set-credentials?appId=${encodeURIComponent(appId)}&appSecret=${encodeURIComponent(appSecret)}`, { method: 'POST' });
        showToast('Kredencialet u ruajten! Tani kliko "Lidhu me Facebook"');
        loadSettings();
    } catch (e) {}
    document.getElementById('btnSaveSettings').disabled = false;
    document.getElementById('btnSaveSettings').textContent = '\ud83d\udd17 Ruaj';
    document.getElementById('authLink').style.display = 'inline-flex';
    showToast('Kredencialet u ruajten! Tani kliko "Lidhu me Facebook" ne hapin 3');
});

async function loadSettings() {
    try {
        const cfg = await api('/api/config');
        config = cfg;
        document.getElementById('settingsAppId').value = cfg.appId || '';
        const redirect = window.location.origin + '/auth/callback';
        document.getElementById('settingsRedirectUri').value = redirect;

        const authStatus = document.getElementById('authStatus');
        const authLink = document.getElementById('authLink');
        const btnStatus = document.getElementById('btnAuthStatus');
        const btnRefresh = document.getElementById('btnAuthRefresh');
        const statusCard = document.getElementById('authStatusCard');

        // Update login button everywhere
        const loginBtn = document.getElementById('loginBtn');

        if (cfg.appId) {
            const authUrl = await api(`/api/auth/url?appId=${cfg.appId}&redirectUri=${encodeURIComponent(redirect)}`);
            authLink.href = authUrl.url;
            authLink.style.display = 'inline-flex';
            if (loginBtn) {
                loginBtn.href = authUrl.url;
                loginBtn.style.display = 'inline-flex';
            }
        } else {
            authLink.style.display = 'none';
            if (loginBtn) loginBtn.style.display = 'none';
        }

        if (cfg.hasToken) {
            document.getElementById('setupSteps').style.display = 'none';
            statusCard.style.display = 'block';
            authStatus.className = 'alert alert-info';
            authStatus.innerHTML = '\u2705 Je i lidhur me Facebook! Token-i eshte aktiv.';
            authLink.textContent = '\u26a0\ufe0f Riautorizo';
            authLink.style.display = 'inline-flex';
            btnStatus.style.display = 'inline-flex';
            btnRefresh.style.display = 'inline-flex';
            document.getElementById('btnDebug').style.display = 'inline-flex';
            btnStatus.onclick = async () => {
                try { showToast('Token aktiv', 'success'); } catch (e) { showToast('Token problem', 'error'); }
            };
            btnRefresh.onclick = () => { loadPages(); showToast('Faqet u rifreskuan'); };

            const loginSection = document.getElementById('loginSection');
            const dashMain = document.getElementById('dashboardMain');
            if (loginSection) loginSection.style.display = 'none';
            if (dashMain) dashMain.style.display = 'block';
        } else {
            document.getElementById('setupSteps').style.display = 'block';
            statusCard.style.display = cfg.appId ? 'block' : 'none';
            authStatus.className = 'alert alert-warning';
            authStatus.innerHTML = 'Nuk je i lidhur. Vendos App ID dhe App Secret, pastaj kliko "Lidhu me Facebook".';
            authLink.textContent = '\ud83d\udd17 Lidhu me Facebook';
            btnStatus.style.display = 'none';
            btnRefresh.style.display = 'none';
            document.getElementById('btnDebug').style.display = 'none';

            if (cfg.appId) {
                authLink.style.display = 'inline-flex';
            } else {
                authLink.style.display = 'none';
            }

            const loginSection = document.getElementById('loginSection');
            const dashMain = document.getElementById('dashboardMain');
            if (loginSection) loginSection.style.display = 'block';
            if (dashMain) dashMain.style.display = 'none';
        }

        if (cfg.pageId) {
            const name = cfg.pages?.find(p => p.id === cfg.pageId)?.name || cfg.pageId;
            document.getElementById('selectedPageInfo').innerHTML = `Faqja aktive: <strong>${name}</strong>`;
        } else if (cfg.hasToken) {
            document.getElementById('selectedPageInfo').innerHTML = 'Asnje faqe e zgjedhur. Shko te <a href="#" onclick="navigateTo(\'pages\')">Faqet</a>.';
        }
    } catch (e) { console.error(e); }
}

function navigateTo(page) {
    document.querySelectorAll('.sidebar nav a, .page').forEach(el => el.classList.remove('active'));
    document.querySelector(`[data-page="${page}"]`).classList.add('active');
    document.getElementById('page-' + page).classList.add('active');
    loadPageData(page);
}

// Pages
async function loadPages() {
    try {
        const data = await api('/api/pages');
        const pages = data.pages || data || [];
        const error = data.error || null;
        const container = document.getElementById('pagesList');
        if (error && (!Array.isArray(pages) || pages.length === 0)) {
            container.innerHTML = `<div class="alert alert-warning" style="white-space:pre-line;">${escapeHtml(error)}</div>`;
            document.getElementById('statPages').textContent = '0';
            return;
        }
        if (!Array.isArray(pages) || pages.length === 0) {
            container.innerHTML = `<div class="alert alert-warning">Nuk u gjet asnje faqe. Sigurohu qe je autorizuar ne Facebook.</div>`;
            document.getElementById('statPages').textContent = '0';
            return;
        }
        let html = '<div class="grid grid-2">';
        const selectedPageId = config.pageId;
        pages.forEach(p => {
            const isSelected = p.id === selectedPageId;
            html += `<div class="card page-card" onclick="selectPage('${p.id}','${p.accessToken}','${p.name.replace(/'/g,"\\'")}')" style="${isSelected ? 'border-color:var(--primary);border-width:2px;' : ''}">
                <div style="display:flex;align-items:center;gap:12px;">
                    ${p.pictureUrl ? `<img src="${p.pictureUrl}" style="width:48px;height:48px;border-radius:8px;">` : `<div style="width:48px;height:48px;border-radius:8px;background:var(--surface2);display:flex;align-items:center;justify-content:center;font-size:24px;">&#9679;</div>`}
                    <div style="flex:1;">
                        <div class="name">${p.name} ${isSelected ? '<span style="color:var(--primary);font-size:11px;">(aktive)</span>' : ''}</div>
                        <small style="color:var(--text2);">${p.category || ''} ${p.followerCount ? '| ' + p.followerCount.toLocaleString() + ' ndjekes' : ''}</small>
                    </div>
                </div>
            </div>`;
        });
        html += '</div>';
        container.innerHTML = html;

        // Update post form page selector
        const sel = document.getElementById('postPage');
        sel.innerHTML = '';
        pages.forEach(p => {
            const opt = document.createElement('option');
            opt.value = p.id;
            opt.textContent = p.name + (p.id === config.pageId ? ' (aktive)' : '');
            if (p.id === config.pageId) opt.selected = true;
            sel.appendChild(opt);
        });

        document.getElementById('statPages').textContent = pages.length;

        if (error) {
            showToast(error, 'error');
        }
    } catch (e) { console.error(e); }
}
        let html = '<div class="grid grid-2">';
        pages.forEach(p => {
            const isSelected = p.id === config.pageId;
            html += `<div class="card page-card" onclick="selectPage('${p.id}','${p.accessToken}','${p.name.replace(/'/g,"\\'")}')" style="${isSelected ? 'border-color:var(--primary);border-width:2px;' : ''}">
                <div style="display:flex;align-items:center;gap:12px;">
                    ${p.pictureUrl ? `<img src="${p.pictureUrl}" style="width:48px;height:48px;border-radius:8px;">` : `<div style="width:48px;height:48px;border-radius:8px;background:var(--surface2);display:flex;align-items:center;justify-content:center;font-size:24px;">&#9679;</div>`}
                    <div style="flex:1;">
                        <div class="name">${p.name} ${isSelected ? '<span style="color:var(--primary);font-size:11px;">(aktive)</span>' : ''}</div>
                        <small style="color:var(--text2);">${p.category || ''} ${p.followerCount ? '| ' + p.followerCount.toLocaleString() + ' ndjekes' : ''}</small>
                    </div>
                </div>
            </div>`;
        });
        html += '</div>';
        container.innerHTML = html;

        // Update post form page selector
        const sel = document.getElementById('postPage');
        sel.innerHTML = '';
        pages.forEach(p => {
            const opt = document.createElement('option');
            opt.value = p.id;
            opt.textContent = p.name + (p.id === config.pageId ? ' (aktive)' : '');
            if (p.id === config.pageId) opt.selected = true;
            sel.appendChild(opt);
        });

        document.getElementById('statPages').textContent = pages.length;
    } catch (e) { console.error(e); }
}

async function selectPage(id, token, name) {
    try {
        await api(`/api/pages/select?pageId=${id}&pageToken=${encodeURIComponent(token)}&pageName=${encodeURIComponent(name)}`, { method: 'POST' });
        showToast('Faqja u zgjodh: ' + name);
        config.pageId = id;
        await loadPages();
        await loadSettings();
    } catch (e) { console.error(e); }
}

// Posts
document.getElementById('postForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const btn = document.getElementById('btnPublish');
    btn.disabled = true;
    btn.textContent = 'Duke publikuar...';
    try {
        const params = new URLSearchParams();
        params.set('message', document.getElementById('postMessage').value);
        const mediaUrl = document.getElementById('postMedia').value.trim();
        if (mediaUrl) params.set('mediaUrl', mediaUrl);
        params.set('mediaType', document.getElementById('postMediaType').value);
        const schedule = document.getElementById('postSchedule').value;
        if (schedule) params.set('scheduleTime', schedule);

        const result = await api(`/api/posts/publish?${params.toString()}`, { method: 'POST' });
        showToast(result.message);
        document.getElementById('postForm').reset();
        loadPosts();
    } catch (e) { console.error(e); }
    btn.disabled = false;
    btn.textContent = 'Publiko / Planifiko';
});

async function loadPosts() {
    try {
        const postsDiv = document.getElementById('postsContainer');
        if (!config.pageId) {
            postsDiv.innerHTML = '<div class="alert alert-warning">Zgjidh nje faqe fillimisht te <a href="#" onclick="navigateTo(\'pages\')">Faqet</a>.</div>';
            return;
        }
        const posts = await api(`/api/posts?pageId=${config.pageId}`);
        document.getElementById('statPosts').textContent = posts ? posts.length : 0;

        const tbody = document.getElementById('postsTable');
        if (!posts || posts.filter(p => p.status === 'published').length === 0) {
            tbody.innerHTML = '<tr><td colspan="6"><div class="empty-state"><p>Nuk ka postime ne kete faqe.</p></div></td></tr>';
        } else {
            tbody.innerHTML = posts.filter(p => p.status === 'published').slice(0, 20).map(p => `
                <tr>
                    <td style="white-space:nowrap;font-size:12px;">${p.createdTime ? new Date(p.createdTime).toLocaleDateString('sq') : '-'}</td>
                    <td style="max-width:300px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;">${escapeHtml((p.message || '').substring(0, 80))}</td>
                    <td>${p.likesCount}</td>
                    <td>${p.commentsCount}</td>
                    <td><span class="badge badge-success">Publikuar</span></td>
                    <td>
                        <button class="btn btn-sm btn-outline" onclick="viewComments('${p.id}')">Komente</button>
                    </td>
                </tr>
            `).join('');
        }

        const scheduledDiv = document.getElementById('scheduledPosts');
        const scheduled = posts.filter(p => p.status === 'scheduled');
        if (scheduled.length === 0) {
            scheduledDiv.innerHTML = '<div class="empty-state"><p>Nuk ka postime te skeduluara.</p></div>';
        } else {
            scheduledDiv.innerHTML = scheduled.map(p => `
                <div style="padding:8px 0;border-bottom:1px solid var(--border);font-size:13px;">
                    <div style="display:flex;justify-content:space-between;align-items:center;">
                        <span class="badge badge-warning">Skeduluar</span>
                        <small style="color:var(--text2);">${p.scheduledTime ? new Date(p.scheduledTime).toLocaleString('sq') : '?'}</small>
                    </div>
                    <p style="color:var(--text2);margin-top:4px;">${escapeHtml((p.message || '').substring(0, 100))}</p>
                </div>
            `).join('');
        }

        const recentDiv = document.getElementById('recentPosts');
        const recent = posts.filter(p => p.status === 'published').slice(0, 5);
        if (recent.length === 0) {
            recentDiv.innerHTML = '<div class="empty-state"><p>Nuk ka postime te fundit.</p></div>';
        } else {
            recentDiv.innerHTML = recent.map(p => `
                <div style="padding:10px 0;border-bottom:1px solid var(--border);">
                    <div style="color:var(--text2);font-size:11px;">${p.createdTime ? new Date(p.createdTime).toLocaleString('sq') : ''}</div>
                    <div style="margin:4px 0;">${escapeHtml((p.message || '').substring(0, 150))}</div>
                    <div style="display:flex;gap:16px;color:var(--text2);font-size:12px;">
                        <span>&#9829; ${p.likesCount}</span>
                        <span>&#9993; ${p.commentsCount}</span>
                        <span>&#8635; ${p.sharesCount}</span>
                    </div>
                </div>
            `).join('');
        }
    } catch (e) { console.error(e); }
}

// Comments
async function loadComments() {
    try {
        const section = document.getElementById('commentsSection');
        if (!config.pageId) {
            section.innerHTML = '<div class="alert alert-warning">Zgjidh nje faqe fillimisht.</div>';
            return;
        }
        const posts = await api(`/api/posts?pageId=${config.pageId}`);
        if (!posts || posts.filter(p => p.status === 'published').length === 0) {
            section.innerHTML = '<div class="empty-state"><p>Nuk ka postime per te pare komentet.</p></div>';
            return;
        }
        const publishedPosts = posts.filter(p => p.status === 'published');
        let html = '<div class="form-group"><label>Zgjidh nje postim</label><select id="commentPostSelect" onchange="loadPostComments(this.value)" style="max-width:100%;">';
        publishedPosts.forEach(p => {
            html += `<option value="${p.id}">${escapeHtml((p.message || '').substring(0, 60))}${p.commentsCount ? ' (' + p.commentsCount + ' komente)' : ''}</option>`;
        });
        html += '</select></div><div id="postComments"></div>';
        section.innerHTML = html;
        loadPostComments(publishedPosts[0].id);
    } catch (e) { console.error(e); }
}

async function loadPostComments(postId) {
    try {
        const comments = await api(`/api/comments?postId=${postId}&pageId=${config.pageId}`);
        const container = document.getElementById('postComments');
        document.getElementById('statComments').textContent = comments ? comments.length : 0;
        if (!comments || comments.length === 0) {
            container.innerHTML = '<div class="empty-state"><p>Nuk ka komente ne kete postim.</p></div>';
            return;
        }
        container.innerHTML = comments.map(c => `
            <div class="comment-item">
                <div style="display:flex;justify-content:space-between;">
                    <div>
                        <span class="author">${escapeHtml(c.fromName)}</span>
                        <span class="time">${c.createdTime ? new Date(c.createdTime).toLocaleString('sq') : ''}</span>
                    </div>
                    <span style="color:var(--text2);font-size:11px;">&#9829; ${c.likeCount}</span>
                </div>
                <div class="text">${escapeHtml(c.message)}</div>
                <button class="btn btn-sm btn-outline" onclick="showReplyBox('${c.id}')">Pergjigju</button>
                <div class="reply-box" id="reply-${c.id}">
                    <textarea id="replyMsg-${c.id}" rows="2" placeholder="Shkruaj pergjigjen..." style="margin-bottom:4px;"></textarea>
                    <button class="btn btn-sm btn-primary" onclick="replyToComment('${c.id}')">Dergo</button>
                </div>
            </div>
        `).join('');
    } catch (e) { console.error(e); }
}

function showReplyBox(commentId) {
    const box = document.getElementById('reply-' + commentId);
    box.classList.toggle('show');
    if (box.classList.contains('show')) {
        document.getElementById('replyMsg-' + commentId).focus();
    }
}

async function replyToComment(commentId) {
    const msg = document.getElementById('replyMsg-' + commentId).value.trim();
    if (!msg) { showToast('Shkruaj nje pergjigje', 'error'); return; }
    try {
        await api(`/api/comments/reply?commentId=${commentId}&message=${encodeURIComponent(msg)}`, { method: 'POST' });
        showToast('Pergjigja u dergua!');
        document.getElementById('reply-' + commentId).classList.remove('show');
        document.getElementById('replyMsg-' + commentId).value = '';
    } catch (e) {}
}

function viewComments(postId) {
    navigateTo('comments');
    setTimeout(() => {
        const sel = document.getElementById('commentPostSelect');
        if (sel) { sel.value = postId; loadPostComments(postId); }
    }, 100);
}

// Instagram
document.getElementById('instaForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const btn = e.target.querySelector('button');
    btn.disabled = true;
    btn.textContent = 'Duke publikuar...';
    try {
        const params = new URLSearchParams();
        params.set('caption', document.getElementById('instaCaption').value);
        params.set('mediaUrl', document.getElementById('instaMedia').value);
        params.set('mediaType', document.getElementById('instaMediaType').value);
        const result = await api(`/api/instagram/publish?${params.toString()}`, { method: 'POST' });
        showToast('Publikuar ne Instagram!');
        document.getElementById('instaForm').reset();
    } catch (e) { console.error(e); }
    btn.disabled = false;
    btn.textContent = 'Publiko ne Instagram';
});

async function loadInstagramStatus() {
    const div = document.getElementById('instaStatus');
    try {
        const cfg = await api('/api/config');
        if (cfg.instagramId) {
            div.innerHTML = `<div class="alert alert-info">Instagram Business ID: <strong>${cfg.instagramId}</strong> - Gati per publikim</div>`;
            document.getElementById('instaForm').style.display = 'block';
        } else {
            div.innerHTML = `<div class="alert alert-warning">Instagram nuk eshte i lidhur. Zgjidh nje faqe qe ka Instagram Business te lidhur ne seksionin <a href="#" onclick="navigateTo('pages')">Faqet</a>.</div>`;
            document.getElementById('instaForm').style.display = 'none';
        }
    } catch (e) { div.innerHTML = ''; }
}

// Dashboard
async function loadDashboard() {
    try {
        if (config.pageId) {
            const statusBar = document.getElementById('dashboardStatus');
            statusBar.textContent = 'U rifreskua: ' + new Date().toLocaleTimeString('sq');
            loadPosts();
        }
    } catch (e) {}
}

function escapeHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

// AI Content Generator
document.getElementById('aiGenerateForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const btn = document.getElementById('btnAiGenerate');
    btn.disabled = true;
    btn.textContent = 'Duke gjeneruar...';
    try {
        const topic = document.getElementById('aiTopic').value;
        const tone = document.getElementById('aiTone').value;
        const fileInput = document.getElementById('aiImage');
        let imageBase64 = null;

        if (fileInput.files.length > 0) {
            const file = fileInput.files[0];
            const reader = new FileReader();
            imageBase64 = await new Promise((resolve) => {
                reader.onload = () => resolve(reader.result);
                reader.readAsDataURL(file);
            });
        }

        let result;
        if (imageBase64) {
            result = await api(`/api/ai/generate?topic=${encodeURIComponent(topic)}&tone=${encodeURIComponent(tone)}&imageUrl=${encodeURIComponent(imageBase64)}`, { method: 'POST' });
        } else {
            result = await api(`/api/ai/generate?topic=${encodeURIComponent(topic)}&tone=${encodeURIComponent(tone)}`, { method: 'POST' });
        }

        const div = document.getElementById('aiResult');
        div.style.display = 'block';
        div.innerHTML = `
            <div class="card" style="border-color:var(--primary);">
                <div class="card-header"><h3>Permbajtja e gjeneruar</h3></div>
                <div style="padding:12px;background:var(--surface2);border-radius:6px;margin-bottom:12px;">
                    <div style="font-size:14px;line-height:1.5;">${escapeHtml(result.caption)}</div>
                </div>
                ${result.hashtags && result.hashtags.length ? `
                <div style="margin-bottom:8px;">
                    <strong style="color:var(--primary);">${result.hashtagString || result.hashtags.join(' ')}</strong>
                </div>` : ''}
                ${result.bestTime ? `<div style="color:var(--text2);font-size:12px;">Koha me e mire per postim: ${result.bestTime}</div>` : ''}
                ${result.engagementTip ? `<div style="color:var(--success);font-size:12px;">Tip: ${result.engagementTip}</div>` : ''}
                <button class="btn btn-primary btn-sm" style="margin-top:8px;" onclick="useAiContent()">Perdore kete permbajtje</button>
                <button class="btn btn-outline btn-sm" style="margin-top:8px;" onclick="document.getElementById('aiResult').style.display='none'">Anulo</button>
            </div>
        `;
        window._lastAiResult = result;
        showToast('Permbajtja u gjenerua!');
    } catch (e) { console.error(e); }
    btn.disabled = false;
    btn.textContent = '\u26a1 Gjenero me AI';
});

function useAiContent() {
    const result = window._lastAiResult;
    if (!result) return;
    const msgArea = document.getElementById('postMessage');
    let text = result.caption || '';
    if (result.hashtags && result.hashtags.length > 0) {
        text += '\n\n' + result.hashtags.join(' ');
    }
    msgArea.value = text;
    navigateTo('posts');
    showToast('Permbajtja u vendos ne postim');
}

function openAiGenerator() {
    navigateTo('aisettings');
    document.getElementById('aiTopic')?.focus();
}

// Moderation
async function checkPostModeration() {
    const msg = document.getElementById('postMessage').value.trim();
    if (!msg) { showToast('Shkruaj tekstin fillimisht', 'error'); return; }
    try {
        const result = await api(`/api/ai/moderate?caption=${encodeURIComponent(msg)}&useAi=false`, { method: 'POST' });
        const badge = document.getElementById('moderationBadge');
        if (result.isClean) {
            badge.innerHTML = `<span class="badge badge-success">&#9989; Permbajtja eshte e rregullt (score: ${result.score}%)</span>`;
        } else {
            badge.innerHTML = `<span class="badge badge-danger">&#9888; Probleme te gjetura: ${result.issues.join(', ')}</span>`;
        }
        showToast(result.isClean ? 'Permbajtja eshte e sigurt' : 'Probleme te gjetura', result.isClean ? 'success' : 'error');
    } catch (e) { console.error(e); }
}

async function checkModeration() {
    const text = document.getElementById('moderationText').value.trim();
    if (!text) { showToast('Shkruaj tekstin', 'error'); return; }
    try {
        const result = await api(`/api/ai/moderate?caption=${encodeURIComponent(text)}&useAi=false`, { method: 'POST' });
        const div = document.getElementById('moderationResult');
        div.innerHTML = `
            <div class="${result.isClean ? 'alert alert-info' : 'alert alert-warning'}">
                <strong>${result.isClean ? '&#9989; Kaloi' : '&#9888; Probleme:'}</strong>
                ${result.isClean ? 'Permbajtja eshte ne perputhje me rregulloren.' : result.issues.join('<br>')}
                <br><small>Score: ${result.score}%</small>
            </div>
        `;
    } catch (e) { console.error(e); }
}

// AI Settings
document.getElementById('aiSettingsForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const provider = document.getElementById('aiProvider').value;
    const params = new URLSearchParams();
    params.set('provider', provider);
    if (provider === 'openai') params.set('openAiKey', document.getElementById('aiOpenAiKey').value);
    if (provider === 'gemini') params.set('geminiKey', document.getElementById('aiGeminiKey').value);
    if (provider === 'ollama') params.set('ollamaEndpoint', document.getElementById('aiOllamaEndpoint').value);
    params.set('moderationEnabled', document.getElementById('aiModerationEnabled').checked);

    await api(`/api/ai/settings?${params.toString()}`, { method: 'POST' });
    showToast('Konfigurimi AI u ruajt!');
    loadAiSettings();
});

document.getElementById('aiProvider')?.addEventListener('change', function() {
    document.getElementById('aiOpenAiFields').style.display = this.value === 'openai' ? 'block' : 'none';
    document.getElementById('aiGeminiFields').style.display = this.value === 'gemini' ? 'block' : 'none';
    document.getElementById('aiOllamaFields').style.display = this.value === 'ollama' ? 'block' : 'none';
});

async function loadAiSettings() {
    try {
        const cfg = await api('/api/ai/settings');
        document.getElementById('aiProvider').value = cfg.provider || 'openai';
        document.getElementById('aiProvider').dispatchEvent(new Event('change'));
        document.getElementById('aiOllamaEndpoint').value = cfg.ollamaEndpoint || 'http://localhost:11434';
        document.getElementById('aiModerationEnabled').checked = cfg.moderationEnabled !== false;
    } catch (e) { console.error(e); }
}

// Image preview
document.getElementById('aiImage')?.addEventListener('change', function() {
    const preview = document.getElementById('aiImagePreview');
    const img = document.getElementById('aiImagePreviewImg');
    if (this.files.length > 0) {
        const reader = new FileReader();
        reader.onload = (e) => { img.src = e.target.result; preview.style.display = 'block'; };
        reader.readAsDataURL(this.files[0]);
    } else {
        preview.style.display = 'none';
    }
});

document.getElementById('postMediaFile')?.addEventListener('change', async function() {
    if (this.files.length > 0) {
        const file = this.files[0];
        const reader = new FileReader();
        reader.onload = (e) => {
            document.getElementById('postMedia').value = e.target.result;
            showToast('Foto u ngarkua');
        };
        reader.readAsDataURL(file);
    }
});

// Handle OAuth callback - check URL params after Facebook redirect
function handleOAuthCallback() {
    const params = new URLSearchParams(window.location.search);
    if (params.get('login') === 'success') {
        showToast('\u2705 U lidhe me Facebook! Tani zgjidh nje faqe.');
        // Clean URL without reload
        window.history.replaceState({}, document.title, window.location.pathname);
        setTimeout(() => navigateTo('pages'), 500);
        return true;
    }
    if (params.get('error')) {
        showToast('Gabim: ' + params.get('error'), 'error');
        window.history.replaceState({}, document.title, window.location.pathname);
    }
    return false;
}

function showDebug() {
    const btn = document.getElementById('btnDebug');
    btn.textContent = 'Duke marr informacion...';
    btn.disabled = true;
    fetch('/api/debug')
        .then(r => r.json())
        .then(data => {
            let html = '<div class="card"><div class="card-header"><h3>Debug Info</h3></div><div style="font-size:12px;white-space:pre-wrap;font-family:monospace;">' + JSON.stringify(data, null, 2) + '</div></div>';
            showModal(html);
            btn.textContent = '\ud83d\udd0d Debug API';
            btn.disabled = false;
        })
        .catch(e => { showToast('Gabim: ' + e.message, 'error'); btn.textContent = '\ud83d\udd0d Debug API'; btn.disabled = false; });
}

// Start auto-refresh
function startAutoRefresh() {
    if (refreshInterval) clearInterval(refreshInterval);
    refreshInterval = setInterval(() => {
        const activePage = document.querySelector('.page.active')?.id;
        if (activePage === 'page-dashboard') loadDashboard();
    }, 15000);
}

// Init
(async function init() {
    handleOAuthCallback();
    await loadSettings();
    await loadPages();
    await loadInstagramStatus();
    await loadAiSettings();
    if (config.pageId) await loadPosts();
    startAutoRefresh();
})();
