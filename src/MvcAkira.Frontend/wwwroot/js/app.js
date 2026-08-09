// Lemari Akira — frontend vanilla JS (gaya dod-nerd-sqlite).
// Tanpa Alpine/daisyUI: render DOM manual supaya pagination & list stabil.

var STATE = {
  token: localStorage.getItem('akira_token') || '',
  user: null,
  view: 'dashboard',
  konfig: null,
  rows: [],
  total: 0,
  page: 1,
  limit: 25,
  totalPages: 0,
  q: { search: '', sort: '', dir: 'asc' },
  mode: 'add',
};

var MENU_GROUPS = [
  { label: 'Aturan Umum', items: [
    { route: 'dashboard', nama: 'Dashboard', icon: '&#9636;' },
    { route: 'sampah', nama: 'Sampah', icon: '&#128465;' },
    { route: 'meja_log', nama: 'Log Audit', icon: '&#9644;' },
  ]},
  { label: 'Master Data', items: [
    { route: 'meja_toko', nama: 'Toko', icon: '&#9633;' },
    { route: 'meja_pengguna', nama: 'Pengguna', icon: '&#128100;' },
    { route: 'meja_biodata', nama: 'Biodata', icon: '&#128195;' },
    { route: 'meja_jabatan', nama: 'Jabatan', icon: '&#9733;' },
  ]},
  { label: 'Hak Akses', items: [
    { route: 'meja_target', nama: 'Target', icon: '&#9004;' },
    { route: 'meja_hakakses', nama: 'Hak Akses', icon: '&#128272;' },
  ]},
  { label: 'Keuangan', items: [
    { route: 'meja_keuangan', nama: 'Keuangan', icon: '&#8364;' },
  ]},
];

var TRASH_TABLES = [
  'meja_toko', 'meja_pengguna', 'meja_biodata', 'meja_jabatan',
  'meja_target', 'meja_hakakses', 'meja_keuangan',
];

function $(id) { return document.getElementById(id); }

function toast(msg, type) {
  type = type || 'success';
  var el = document.createElement('div');
  el.className = 'toast toast-' + type;
  el.textContent = msg;
  $('toast-container').appendChild(el);
  setTimeout(function () {
    el.style.opacity = '0';
    el.style.transition = 'all 0.3s ease';
    setTimeout(function () { el.remove(); }, 300);
  }, 2500);
}

function escHtml(s) {
  if (s === null || s === undefined) return '';
  var d = document.createElement('div');
  d.textContent = String(s);
  return d.innerHTML;
}

function debounce(fn, ms) {
  var t;
  return function () {
    clearTimeout(t);
    t = setTimeout(fn, ms);
  };
}

function init() {
  var saved = localStorage.getItem('akira_user');
  if (saved) {
    try { STATE.user = JSON.parse(saved); } catch (e) { STATE.user = null; }
  }
  if (STATE.user && STATE.token) {
    showShell();
  } else {
    showLogin();
  }
  $('login-btn').addEventListener('click', doLogin);
  $('login-password').addEventListener('keydown', function (e) {
    if (e.key === 'Enter') doLogin();
  });
}

function showLogin() {
  $('login-page').classList.remove('hidden');
  $('shell').classList.add('hidden');
}

function showShell() {
  $('login-page').classList.add('hidden');
  $('shell').classList.remove('hidden');
  renderNav();
  renderSidebar();
  go('dashboard');
}

function renderNav() {
  if (!STATE.user) return;
  var u = STATE.user;
  var html = '<span class="navbar-user">' + escHtml(u.nama || u.email);
  if (u.isSuperuser) html += ' <span class="badge badge-blue">SUPERUSER</span>';
  html += '</span>';
  html += '<button class="btn btn-danger btn-sm" onclick="doLogout()">Keluar</button>';
  $('nav-right').innerHTML = html;
}

function renderSidebar() {
  var u = STATE.user;
  if (!u) return;
  var html = '';
  MENU_GROUPS.forEach(function (g) {
    html += '<div class="sidebar-label">' + escHtml(g.label) + '</div>';
    g.items.forEach(function (i) {
      var active = STATE.view === i.route ? ' active' : '';
      html += '<button class="sidebar-item' + active + '" onclick="go(\'' + i.route + '\')">' +
        '<span class="sidebar-icon">' + i.icon + '</span>' + escHtml(i.nama) + '</button>';
    });
  });
  html += '<div class="sidebar-user">';
  html += '<div style="font-weight:600">' + escHtml(u.nama || '') + '</div>';
  html += '<div style="color:var(--text-tertiary);margin:2px 0 8px">' + escHtml(u.email || '') + '</div>';
  if (u.tokoNama) html += '<span class="badge badge-gray">' + escHtml(u.tokoNama) + '</span>';
  html += '</div>';
  $('sidebar').innerHTML = html;
}

async function doLogin() {
  var email = $('login-email').value.trim();
  var password = $('login-password').value;
  if (!email || !password) {
    $('login-error').textContent = 'Email dan password wajib diisi';
    return;
  }
  $('login-error').textContent = '';
  var btn = $('login-btn');
  btn.disabled = true; btn.textContent = 'Masuk...';
  try {
    var r = await AUTH.post('auth/login', { email: email, password: password });
    var d = r.data;
    STATE.token = d.token;
    STATE.user = { email: d.email, nama: d.nama, tokoNama: d.tokoName, isSuperuser: !!d.isSuperuser };
    localStorage.setItem('akira_token', STATE.token);
    localStorage.setItem('akira_user', JSON.stringify(STATE.user));
    showShell();
  } catch (e) {
    $('login-error').textContent = (e.response && e.response.data && e.response.data.error) || 'Gagal masuk';
  } finally {
    btn.disabled = false; btn.textContent = 'Masuk';
  }
}

async function doLogout() {
  try { await AUTH.post('auth/logout'); } catch (e) {}
  STATE.token = ''; STATE.user = null;
  localStorage.removeItem('akira_token');
  localStorage.removeItem('akira_user');
  showLogin();
}

function setView(route) {
  STATE.view = route;
  renderSidebar();
}

async function go(route) {
  setView(route);
  if (route === 'dashboard') { await loadDashboard(); return; }
  if (route === 'sampah') { await loadTrash(); return; }
  var k = window.KONFIGURASI[route];
  if (!k) return;
  STATE.konfig = k;
  STATE.q = { search: '', sort: (k.kolom && k.kolom[0] && k.kolom[0].sort) || '', dir: 'asc' };
  STATE.page = 1; STATE.limit = 25;
  await load();
}

function bolehTulis() {
  if (!STATE.user) return false;
  if (STATE.user.isSuperuser) return true;
  if (STATE.konfig && STATE.konfig.sensitif) return false;
  return true;
}

// ---------- Sampah (trash) ----------
var TRASH = { tab: 'meja_toko', page: 1, limit: 25, totalPages: 0, rows: [] };

async function loadTrash() {
  STATE.konfig = null;
  var area = $('content-area');
  area.innerHTML = '<div class="page-title">Sampah</div><div class="loading">Memuat sampah...</div>';
  try {
    var tabs = TRASH_TABLES.map(function (t) {
      var k = window.KONFIGURASI[t];
      return '<button class="trash-tab' + (TRASH.tab === t ? ' active' : '') + '" onclick="goTrash(\'' + t + '\')">' +
        escHtml(k.label) + '</button>';
    }).join('');
    var k = window.KONFIGURASI[TRASH.tab];
    var r = await READ.get(TRASH.tab + '/trash', { params: { page: TRASH.page, limit: TRASH.limit } });
    var d = r.data;
    TRASH.totalPages = d.totalPages || 0;
    TRASH.rows = d.list || [];

    var html = '<div class="page-title">Sampah</div>';
    html += '<div class="trash-tabs">' + tabs + '</div>';
    html += renderTrashTable();
    html += renderTrashPagination();
    area.innerHTML = html;
  } catch (e) {
    var msg = (e.response && e.response.data && (e.response.data.detail || e.response.data.error)) || 'Gagal memuat sampah';
    area.innerHTML = '<div class="card">' + escHtml(msg) + '</div>';
    toast(msg, 'error');
  }
}

function renderTrashTable() {
  var k = window.KONFIGURASI[TRASH.tab];
  var head = '<tr>';
  k.kolom.forEach(function (c) {
    head += '<th style="cursor:default">' + escHtml(c.label) + '</th>';
  });
  head += '<th style="cursor:default">Aksi</th></tr>';

  var body = '';
  if ((TRASH.rows || []).length === 0) {
    body += '<tr><td colspan="' + (k.kolom.length + 1) + '" class="empty-row">Sampah kosong</td></tr>';
  } else {
    TRASH.rows.forEach(function (r) {
      body += '<tr>';
      k.kolom.forEach(function (c) {
        body += '<td>' + escHtml(fmt(c, r)) + '</td>';
      });
      var code = r[k.kodeField];
      body += '<td>';
      body += '<button class="btn btn-success btn-sm" onclick="restoreTrash(\'' + code + '\')">Pulihkan</button> ';
      body += '<button class="btn btn-danger btn-sm" onclick="permanentTrash(\'' + code + '\')">Hapus Selamanya</button>';
      body += '</td></tr>';
    });
  }
  return '<div class="card"><div class="card-title">' + escHtml(k.label) + ' — Sampah</div>' +
    '<div class="table-wrapper"><table><thead>' + head + '</thead><tbody>' + body + '</tbody></table></div></div>';
}

function renderTrashPagination() {
  var html = '<div class="pagination"><span>Halaman ' + TRASH.page + ' dari ' + Math.max(1, TRASH.totalPages) + '</span>';
  html += '<button class="pagination-btn" onclick="goTrashPage(' + (TRASH.page - 1) + ')"' + (TRASH.page <= 1 ? ' disabled' : '') + '>&lsaquo;</button>';
  html += '<button class="pagination-btn" onclick="goTrashPage(' + (TRASH.page + 1) + ')"' + (TRASH.page >= TRASH.totalPages ? ' disabled' : '') + '>&rsaquo;</button>';
  html += '</div>';
  return html;
}

function goTrash(tab) {
  TRASH.tab = tab;
  TRASH.page = 1;
  loadTrash();
}

function goTrashPage(p) {
  if (p < 1 || (TRASH.totalPages && p > TRASH.totalPages)) return;
  TRASH.page = p;
  loadTrash();
}

async function restoreTrash(code) {
  try {
    await WRITE.post(TRASH.tab + '/restore', JSON.stringify(code), { headers: { 'Content-Type': 'application/json' } });
    toast('Dikembalikan');
    await loadTrash();
  } catch (e) {
    toast((e.response && e.response.data && e.response.data.pesan) || 'Gagal mengembalikan', 'error');
  }
}

async function permanentTrash(code) {
  if (!confirm('Hapus permanen? Data tidak bisa dikembalikan.')) return;
  try {
    await WRITE.delete(TRASH.tab + '/permanent', {
      headers: { 'Content-Type': 'application/json' },
      data: JSON.stringify(code),
    });
    toast('Dihapus permanen');
    await loadTrash();
  } catch (e) {
    toast((e.response && e.response.data && e.response.data.pesan) || 'Gagal menghapus', 'error');
  }
}

function fmt(k, r) {
  var v = r ? r[k.field] : '';
  if (k.render) return k.render(v, r);
  return v;
}

function renderContent() {
  var area = $('content-area');
  area.innerHTML = '<div class="page-title">' + escHtml(STATE.konfig.label) + '</div>' + renderToolbar() +
    '<div class="table-wrapper">' + renderTable() + '</div>' + renderPagination();
}

function renderToolbar() {
  var k = STATE.konfig;
  var html = '<div class="toolbar">';
  html += '<div class="search-box"><input id="search-input" placeholder="Cari..." value="' +
    escHtml(STATE.q.search) + '" oninput="onSearch(this.value)"></div>';
  html += '<div class="toolbar-actions">';
  html += '<select id="pagesize" class="form-select" onchange="onPageSize(this.value)">';
  (window.BATAS_HALAMAN || [5, 25, 50, 75, 100]).forEach(function (v) {
    html += '<option value="' + v + '"' + (STATE.limit == v ? ' selected' : '') + '>' + v + '</option>';
  });
  html += '</select>';
  if (k.buat && bolehTulis()) {
    html += '<button class="btn btn-success btn-sm" onclick="openForm()">+ Tambah</button>';
  }
  html += '</div></div>';
  return html;
}

function renderTable() {
  var k = STATE.konfig;
  var head = '<tr>';
  k.kolom.forEach(function (c) {
    var clickable = c.sort ? ' onclick="sortBy(\'' + c.sort + '\')"' : ' style="cursor:default"';
    var arrow = STATE.q.sort === c.sort
      ? (STATE.q.dir === 'asc' ? ' &#8593;' : ' &#8595;')
      : '';
    head += '<th' + clickable + '>' + escHtml(c.label) + arrow + '</th>';
  });
  head += '<th style="cursor:default">Aksi</th></tr>';

  var body = '';
  if (!STATE.rows.length) {
    body += '<tr><td colspan="' + (k.kolom.length + 1) + '" class="empty-row">Tidak ada data</td></tr>';
  } else {
    STATE.rows.forEach(function (r) {
      body += '<tr>';
      k.kolom.forEach(function (c) {
        body += '<td>' + fmt(c, r) + '</td>';
      });
      body += '<td>';
      body += '<button class="btn btn-secondary btn-sm" onclick="openDetail(\'' + r[k.kodeField] + '\')">Lihat</button> ';
      if (k.ubah && bolehTulis()) {
        body += '<button class="btn btn-secondary btn-sm" onclick="openFormEdit(\'' + r[k.kodeField] + '\')">Ubah</button> ';
      }
      if (bolehTulis() && !k.readOnly && k.hapus !== false) {
        body += '<button class="btn btn-danger-ghost btn-sm" onclick="softDelete(\'' + r[k.kodeField] + '\')">Sampah</button>';
      }
      body += '</td></tr>';
    });
  }
  return '<table><thead>' + head + '</thead><tbody>' + body + '</tbody></table>';
}

function renderPagination() {
  var pages = STATE.totalPages;
  var html = '<div class="pagination">';
  var start = Math.max(1, STATE.total ? (STATE.page - 1) * STATE.limit + 1 : 0);
  var end = STATE.total ? Math.min(STATE.page * STATE.limit, STATE.total) : 0;
  html += '<span>' + (STATE.total ? 'Menampilkan ' + start + '-' + end + ' dari ' + STATE.total : '0 data') + ' data</span>';
  if (pages <= 1) return html + '<div class="pagination-btns"></div></div>';
  html += '<div class="pagination-btns">';
  html += '<button class="pagination-btn" onclick="goPage(' + (STATE.page - 1) + ')"' + (STATE.page <= 1 ? ' disabled' : '') + '>&lsaquo;</button>';
  var from = Math.max(1, STATE.page - 4);
  var to = Math.min(pages, from + 9);
  if (from > 1) html += '<button class="pagination-btn" onclick="goPage(1)">1</button><span class="pagination-ellipsis">...</span>';
  for (var p = from; p <= to; p++) {
    html += '<button class="pagination-btn' + (p === STATE.page ? ' active' : '') + '" onclick="goPage(' + p + ')">' + p + '</button>';
  }
  if (to < pages) html += '<span class="pagination-ellipsis">...</span><button class="pagination-btn" onclick="goPage(' + pages + ')">' + pages + '</button>';
  html += '<button class="pagination-btn" onclick="goPage(' + (STATE.page + 1) + ')"' + (STATE.page >= pages ? ' disabled' : '') + '>&rsaquo;</button>';
  html += '</div></div>';
  return html;
}

function onSearch(value) {
  STATE.q.search = value;
  STATE.page = 1;
  debounce(load, 300)();
}

function onPageSize(v) {
  STATE.limit = parseInt(v, 10);
  STATE.page = 1;
  load();
}

function goPage(p) {
  if (p < 1 || p > STATE.totalPages) return;
  STATE.page = p;
  load();
}

function sortBy(field) {
  if (STATE.q.sort === field) {
    STATE.q.dir = STATE.q.dir === 'asc' ? 'desc' : 'asc';
  } else {
    STATE.q.sort = field;
    STATE.q.dir = 'asc';
  }
  STATE.page = 1;
  load();
}

async function load() {
  if (!STATE.konfig) return;
  var params = {
    page: STATE.page,
    limit: STATE.limit,
    search: STATE.q.search || '',
    sort: STATE.q.sort || '',
    dir: STATE.q.dir || 'asc',
  };
  try {
    var r = await READ.get(STATE.konfig.route, { params: params });
    var d = r.data;
    STATE.rows = d.list || [];
    STATE.total = d.total || 0;
    STATE.totalPages = d.totalPages || 0;
    STATE.page = d.page || 1;
    renderContent();
  } catch (e) {
    var msg = (e.response && e.response.data && (e.response.data.detail || e.response.data.error)) || 'Gagal memuat data';
    $('content-area').innerHTML = '<div class="card">' + escHtml(msg) + '</div>';
    toast(msg, 'error');
  }
}

async function loadDashboard() {
  setView('dashboard');
  var area = $('content-area');
  area.innerHTML = '<div class="page-title">Dashboard</div><div class="loading">Memuat data...</div>';
  try {
    var r = await READ.get('dashboard');
    var d = r.data;
    var cards = [
      { icon: '&#9633;', label: 'Toko', val: d.jumlahToko, color: 'var(--accent-blue)' },
      { icon: '&#128100;', label: 'Pengguna', val: d.jumlahPengguna, color: 'var(--accent-green)' },
      { icon: '&#128195;', label: 'Biodata', val: d.jumlahBiodata, color: 'var(--accent-purple)' },
      { icon: '&#9733;', label: 'Jabatan', val: d.jumlahJabatan, color: 'var(--accent-orange)' },
      { icon: '&#9004;', label: 'Target', val: d.jumlahTarget, color: 'var(--accent-teal)' },
    ].map(function (c) {
      return '<div class="stat-card"><div class="stat-icon">' + c.icon + '</div>' +
        '<div class="stat-number" style="color:' + c.color + '">' + escHtml(c.val) + '</div>' +
        '<div class="stat-label">' + escHtml(c.label) + '</div></div>';
    }).join('');

    var html = '<div class="page-title">Dashboard</div><div class="dashboard-grid">' + cards + '</div>';

    if (d.saldoTotal != null) {
      html += '<div class="dashboard-row"><div class="card"><div class="card-title">Saldo Per Tempat</div>';
      var rows = (d.saldoPerTempat || []).map(function (s) {
        return '<div class="keuangan-row"><span>' + escHtml(s.tempat) + '</span><span style="font-weight:600">Rp ' +
          Number(s.saldo).toLocaleString('id-ID') + '</span></div>';
      }).join('') || '<div style="color:var(--text-tertiary);font-size:13px">tidak ada data</div>';
      html += rows + '</div><div class="card"><div class="card-title">Total Saldo</div>' +
        '<div style="font-size:32px;font-weight:700;color:var(--accent-blue)">Rp ' +
        Number(d.saldoTotal).toLocaleString('id-ID') + '</div></div></div>';
    }
    area.innerHTML = html;
  } catch (e) {
    area.innerHTML = '<div class="card">Tidak dapat memuat dashboard</div>';
  }
}

// ---- Modal ----
function openModal(title, bodyHtml) {
  $('modal-title').textContent = title;
  $('modal-body').innerHTML = bodyHtml;
  $('modal-overlay').classList.add('show');
}

function closeModal() {
  $('modal-overlay').classList.remove('show');
}

function openForm() {
  STATE.mode = 'add';
  renderForm({});
}

function openFormEdit(code) {
  var rec = STATE.rows.find(function (r) { return r[STATE.konfig.kodeField] === code; });
  if (!rec) return;
  STATE.mode = 'edit';
  renderForm(rec);
}

function renderForm(rec) {
  var k = STATE.konfig;
  var html = '';
  (k.form || []).forEach(function (f) {
    var val = rec && rec[f.field] !== undefined ? rec[f.field] : '';
    html += '<div class="form-group"><label class="form-label">' + escHtml(f.label) + '</label>';
    if (f.type === 'textarea') {
      html += '<textarea class="form-textarea" id="fld-' + escHtml(f.field) + '">' + escHtml(val) + '</textarea>';
    } else {
      html += '<input class="form-input" id="fld-' + escHtml(f.field) + '" type="' + (f.type || 'text') + '" value="' + escHtml(val) + '">';
    }
    html += '</div>';
  });
  html += '<div class="modal-actions"><button class="btn btn-secondary" onclick="closeModal()">Batal</button>' +
    '<button class="btn btn-primary" onclick="saveForm()">Simpan</button></div>';
  openModal(STATE.mode === 'add' ? 'Tambah ' + k.label : 'Ubah ' + k.label, html);
}

function collectForm() {
  var k = STATE.konfig;
  var f = {};
  (k.form || []).forEach(function (fl) {
    var el = $('fld-' + fl.field);
    if (el) f[fl.field] = fl.type === 'number' ? Number(el.value) : el.value;
  });
  return f;
}

async function saveForm() {
  var k = STATE.konfig;
  if (!k.buat) return;
  try {
    var f = collectForm();
    if (STATE.mode === 'edit') {
      if (!k.ubah) return;
      f[k.kodeField] = f[k.kodeField];
      await k.ubah(f);
    } else {
      await k.buat(f);
    }
    closeModal();
    toast('Tersimpan');
    load();
  } catch (e) {
    toast((e.response && e.response.data && (e.response.data.pesan || e.response.data.error)) || 'Gagal menyimpan', 'error');
  }
}

function openDetail(code) {
  var k = STATE.konfig;
  var rec = STATE.rows.find(function (r) { return r[k.kodeField] === code; });
  if (!rec) return;
  var html = '<div class="detail-grid">';
  k.kolom.forEach(function (c) {
    html += '<div class="detail-item"><div class="detail-label">' + escHtml(c.label) + '</div>' +
      '<div class="detail-value">' + escHtml(fmt(c, rec)) + '</div></div>';
  });
  html += '</div>';
  html += '<div class="modal-actions"><button class="btn" onclick="closeModal()">Tutup</button></div>';
  openModal('Detail ' + k.label, html);
}

async function softDelete(code) {
  var ok = confirm('Hapus (soft)? Data akan masuk ke sampah.');
  if (!ok) return;
  try {
    await WRITE.post(STATE.konfig.route + '/soft-delete', JSON.stringify(code), {
      headers: { 'Content-Type': 'application/json' },
    });
    toast('Dipindah ke sampah');
    load();
  } catch (e) {
    toast((e.response && e.response.data && e.response.data.pesan) || 'Gagal menghapus', 'error');
  }
}

document.addEventListener('keydown', function (e) {
  if (e.key === 'Escape') closeModal();
});

document.addEventListener('DOMContentLoaded', init);
