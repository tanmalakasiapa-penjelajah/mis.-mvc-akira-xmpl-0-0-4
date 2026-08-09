// Pusat konfigurasi axios instances + interceptor Bearer token.
// READ/WRITE/AUTH didefinisikan SATU kali (global window), dipakai config.js & app.js.
const READ = axios.create({ baseURL: 'http://localhost:5002/api' });
const WRITE = axios.create({ baseURL: 'http://localhost:5003/api' });
const AUTH = axios.create({ baseURL: 'http://localhost:5001/api' });

window.READ = READ;
window.WRITE = WRITE;
window.AUTH = AUTH;

function token() { return localStorage.getItem('akira_token') || ''; }

function attach(instance) {
  instance.interceptors.request.use(cfg => {
    const t = token();
    if (t) cfg.headers.Authorization = 'Bearer ' + t;
    return cfg;
  });
  instance.interceptors.response.use(
    r => r,
    err => {
      if (err.response?.status === 401) {
        localStorage.removeItem('akira_token');
        localStorage.removeItem('akira_user');
        window.location.reload();
      }
      return Promise.reject(err);
    }
  );
}

attach(READ); attach(WRITE); attach(AUTH);